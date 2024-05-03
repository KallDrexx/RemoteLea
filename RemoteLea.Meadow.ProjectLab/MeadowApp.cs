using System;
using System.Linq;
using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;
using Meadow.Logging;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.ProjectLab
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        private MeadowLeaRunner _meadowLeaRunner = null!;

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
            
            var pins = new PinLookup(Device.Pins);
            var operations = CoreOperations.All().Concat(MeadowOperations.All(Device, pins));
            _meadowLeaRunner = new MeadowLeaRunner(operations);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Hello, Meadow Core-Compute!");

            while (true)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}