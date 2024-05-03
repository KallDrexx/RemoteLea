using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Meadow;
using Meadow.Hardware;

namespace RemoteLea.Meadow;

public class PinLookup
{
    private readonly Dictionary<string, IPin> _pins = new(StringComparer.OrdinalIgnoreCase);

    public PinLookup(IPinDefinitions pinDefinitions)
    {
        if (pinDefinitions == null)
        {
            throw new ArgumentNullException(nameof(pinDefinitions));
        }
        
        var pins = pinDefinitions.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => typeof(IPin).IsAssignableFrom(x.PropertyType))
            .Select(x => (x.Name, (IPin)x.GetValue(pinDefinitions)))
            .ToArray();

        foreach (var pin in pins)
        {
            _pins[pin.Name] = pin.Item2;
        }

        var allPinNames = _pins.Keys.Aggregate(((x, y) => $"{x}, {y}"));
        Resolver.Log.Debug($"Found pins: {allPinNames}");
    }

    public IPin? Get(string name) => _pins.GetValueOrDefault(name);
}