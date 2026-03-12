using Code.Config.Pages.Exploration;

namespace Code.Game.Exploration.Runtime
{
public sealed class ExplorationSession
{
    public ExplorationSession(
        WorldRuntimeState worldState,
        LocationConfig currentLocationConfig,
        LocationRuntimeState currentLocationState,
        TimeService timeService)
    {
        WorldState = worldState;
        CurrentLocationConfig = currentLocationConfig;
        CurrentLocationState = currentLocationState;
        TimeService = timeService;
    }

    public WorldRuntimeState WorldState { get; }
    public LocationConfig CurrentLocationConfig { get; }
    public LocationRuntimeState CurrentLocationState { get; }
    public TimeService TimeService { get; }
}
}