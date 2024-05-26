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
            
            // var red = Device.CreatePwmPort(Device.Pins.D09, new Frequency(100), 0f);
            // red.Start();
            // var green = Device.CreatePwmPort(Device.Pins.D10, new Frequency(100), 0f);
            // green.Start();
            // var blue = Device.CreatePwmPort(Device.Pins.D11, new Frequency(100), 0f);
            // blue.Start();
            //
            // var i2c = Device.CreateI2cBus();
            // var interrupt = Device.CreateDigitalInterruptPort(Device.Pins.A05, InterruptMode.EdgeRising,
            //     ResistorMode.InternalPullDown);
            //
            // var reset = Device.CreateDigitalOutputPort(Device.Pins.D05);
            // _mcp = new Mcp23008(i2c, 0x20, interrupt, reset);
            //
            // var up = _mcp.CreateDigitalInterruptPort(_mcp.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            // var right = _mcp.CreateDigitalInterruptPort(_mcp.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            // var left = _mcp.CreateDigitalInterruptPort(_mcp.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.InternalPullUp);
            // // var up = _mcp.CreateDigitalInputPort(_mcp.Pins.GP0);
            // // var right = _mcp.CreateDigitalInputPort(_mcp.Pins.GP1);
            // // var left = _mcp.CreateDigitalInputPort(_mcp.Pins.GP2);
            //
            // while (true)
            // {
            //     await Task.Delay(100);
            //     if (!up.State)
            //     {
            //         Console.WriteLine("Up");
            //         red.DutyCycle = 0.5f;
            //         while (!up.State)
            //         {
            //             await Task.Delay(100);
            //         }
            //
            //         red.DutyCycle = 0f;
            //     }
            //     else if (!right.State)
            //     {
            //         Console.WriteLine("Right");
            //         green.DutyCycle = 0.5f;
            //         while (!right.State)
            //         {
            //             await Task.Delay(100);
            //         }
            //
            //         green.DutyCycle = 0f;
            //     }
            //     else if (!left.State)
            //     {
            //         Console.WriteLine("Left");
            //         blue.DutyCycle = 0.5f;
            //         while (!left.State)
            //         {
            //             await Task.Delay(100);
            //         }
            //
            //         blue.DutyCycle = 0f;
            //     }
            // }

            // _projectLab = global::Meadow.Devices.ProjectLab.Create();
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
}