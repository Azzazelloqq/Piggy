using System;
using System.Collections.Generic;
using Azzazelloqq.Config;
using UnityEngine;

namespace Code.Config.Pages.Exploration
{
[CreateAssetMenu(menuName = "Config/Exploration/LocationsConfigPage", fileName = "LocationsConfigPage")]
public sealed class LocationsConfigPage : ScriptableObject, IConfigPage
{
    [SerializeField]
    private LocationConfig[] _locations;

    public LocationConfig[] Locations => _locations;

    public LocationConfig FindLocation(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId) || _locations == null)
        {
            return null;
        }

        foreach (var location in _locations)
        {
            if (location != null && string.Equals(location.Id, locationId, StringComparison.Ordinal))
            {
                return location;
            }
        }

        return null;
    }

    public LocationConfig GetRequiredLocation(string locationId)
    {
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new ArgumentException("Location id must not be empty.", nameof(locationId));
        }

        var location = FindLocation(locationId);
        if (location == null)
        {
            throw new InvalidOperationException($"Location '{locationId}' is missing in {nameof(LocationsConfigPage)}.");
        }

        return location;
    }

    public IReadOnlyDictionary<string, LocationConfig> BuildLookup()
    {
        if (_locations == null)
        {
            throw new InvalidOperationException($"{nameof(LocationsConfigPage)} locations are not configured.");
        }

        if (_locations.Length == 0)
        {
            throw new InvalidOperationException($"{nameof(LocationsConfigPage)} does not contain any locations.");
        }

        var lookup = new Dictionary<string, LocationConfig>(StringComparer.Ordinal);

        for (var i = 0; i < _locations.Length; i++)
        {
            var location = _locations[i];
            if (location == null)
            {
                throw new InvalidOperationException($"{nameof(LocationsConfigPage)} contains an empty location entry at index {i}.");
            }

            if (string.IsNullOrWhiteSpace(location.Id))
            {
                throw new InvalidOperationException($"Location at index {i} has an empty id.");
            }

            if (!lookup.TryAdd(location.Id, location))
            {
                throw new InvalidOperationException($"Duplicate location id '{location.Id}' found in {nameof(LocationsConfigPage)}.");
            }
        }

        return lookup;
    }
}
}
