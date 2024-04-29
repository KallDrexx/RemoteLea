using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using System;
using System.Linq;
using System.Threading.Tasks;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.F7Feather
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private MeadowLeaRunner _meadowLeaRunner = null!;
        
        public override Task Initialize()
        {
            Resolver.Log.Info("Starting RemoteLea Feather Device");

            var operations = CoreOperations.All().Concat(MeadowOperations.All(Device, Device.Pins));
            _meadowLeaRunner = new MeadowLeaRunner(operations);

            return base.Initialize();
        }

        public override async Task Run()
        {
            while (true)
            {
                await Task.Delay(1000);
            }
        }
    }
}