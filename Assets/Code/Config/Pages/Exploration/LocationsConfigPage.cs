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

        for (var i = 0; i < _locations.Length; i++)
        {
            var location = _locations[i];
            if (location != null && string.Equals(location.Id, locationId, StringComparison.Ordinal))
            {
                return location;
            }
        }

        return null;
    }

    public IReadOnlyDictionary<string, LocationConfig> BuildLookup()
    {
        var lookup = new Dictionary<string, LocationConfig>(StringComparer.Ordinal);
        if (_locations == null)
        {
            return lookup;
        }

        for (var i = 0; i < _locations.Length; i++)
        {
            var location = _locations[i];
            if (location == null || string.IsNullOrWhiteSpace(location.Id))
            {
                continue;
            }

            lookup[location.Id] = location;
        }

        return lookup;
    }
}
}
