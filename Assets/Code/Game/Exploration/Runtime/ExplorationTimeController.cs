using System;
using System.Collections.Generic;
using Code.Config.Pages.Exploration;
using Code.Game.Exploration.Domain;
using Code.Game.Exploration.Logic;

namespace Code.Game.Exploration.Runtime
{
public sealed class ExplorationTimeController
{
    private readonly WorldRuntimeState _worldState;
    private readonly TimeService _timeService;
    private readonly EventResolver _eventResolver;
    private readonly ActionExecutor _actionExecutor;
    private readonly HashSet<string> _flowSources = new(StringComparer.Ordinal);
    private LocationRuntimeState _locationState;
    private EventConfig[] _timeEvents;
    private float _unitsPerSecond;
    private float _carryUnits;

    public ExplorationTimeController(
        WorldRuntimeState worldState,
        TimeService timeService,
        EventResolver eventResolver,
        ActionExecutor actionExecutor)
    {
        _worldState = worldState ?? throw new ArgumentNullException(nameof(worldState));
        _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
        _eventResolver = eventResolver ?? throw new ArgumentNullException(nameof(eventResolver));
        _actionExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
    }

    public bool HasActiveFlow => _flowSources.Count > 0;
    public Action TimeAdvanced { get; set; }

    public void UpdateLocation(LocationConfig locationConfig, LocationRuntimeState locationState)
    {
        if (locationConfig == null)
        {
            throw new ArgumentNullException(nameof(locationConfig));
        }

        _locationState = locationState ?? throw new ArgumentNullException(nameof(locationState));
        _timeEvents = locationConfig.Events ?? Array.Empty<EventConfig>();
    }

    public void SpendUnits(int units)
    {
        if (units <= 0)
        {
            return;
        }

        AdvanceUnits(units);
    }

    public void BeginFlow(string sourceId, float unitsPerSecond)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            throw new ArgumentException("Flow source id must not be empty.", nameof(sourceId));
        }

        if (unitsPerSecond <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(unitsPerSecond), unitsPerSecond, "Flow rate must be greater than 0.");
        }

        _unitsPerSecond = unitsPerSecond;
        _flowSources.Add(sourceId);
    }

    public void EndFlow(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            return;
        }

        _flowSources.Remove(sourceId);
    }

    public void Tick(float deltaSeconds)
    {
        if (!HasActiveFlow || deltaSeconds <= 0f)
        {
            return;
        }

        var units = deltaSeconds * _unitsPerSecond + _carryUnits;
        var wholeUnits = (int)Math.Floor(units);
        _carryUnits = units - wholeUnits;

        if (wholeUnits > 0)
        {
            AdvanceUnits(wholeUnits);
        }
    }

    private void AdvanceUnits(int units)
    {
        _worldState.CurrentTimeUnits += units;
        _timeService.AddUnits(units);

        if (_locationState == null)
        {
            return;
        }

        if (_eventResolver.TryResolve(
                EventTriggerType.Time,
                _timeEvents,
                _worldState,
                _locationState,
                _worldState.CurrentNodeId,
                string.Empty,
                out var resolvedEvent))
        {
            _actionExecutor.Execute(
                resolvedEvent.Actions,
                _worldState,
                _locationState,
                this);
            _eventResolver.MarkTriggered(resolvedEvent, _worldState, _locationState);
        }

        TimeAdvanced?.Invoke();
    }
}
}
