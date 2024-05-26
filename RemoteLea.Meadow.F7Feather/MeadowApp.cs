using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Linq;
using System.Threading.Tasks;
using Meadow.Logging;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.F7Feather;

public class MeadowApp : App<F7FeatherV2>
{
    public override Task Initialize()
    {
        Device.NetworkConnected += (sender, args) => Resolver.Log.Info($"Wifi connected {args.IpAddress}");
        Device.NetworkDisconnected += (sender, args) => Resolver.Log.Info($"Wifi disconnected: {args.Reason}");

        var cloudLogger = new CloudLogger();

        Resolver.MeadowCloudService.ConnectionStateChanged += (sender, state) =>
            Resolver.Log.Info($"Cloud connection state changed: {state}");

        Resolver.Log.AddProvider(cloudLogger);
        Resolver.Services.Add(cloudLogger);
        Resolver.Log.Info("Initialize...");
        
        return Task.CompletedTask;
    }

    public override async Task Run()
    {
        var pins = new PinLookup(Device.Pins);
        var operations = CoreOperations.All().Concat(MeadowOperations.All(Device, pins));
        _ = new MeadowLeaRunner(operations);
        
        Resolver.Log.Info("Hello, Meadow Core-Compute!");

        while (true)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}