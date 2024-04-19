using Meadow;
using Meadow.Foundation.Sensors;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Peripherals.Sensors;

namespace Tilt.Desktop;

internal class Program : App<Meadow.Desktop>
{
    private AppEngine _engine;

    private static void Main(string[] args)
    {
        MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        Device.Display?.Resize(320, 240, 3);

        var keyboard = new Keyboard();

        var currentSensor = new SimulatedCurrentSensor();
        currentSensor.StartSimulation(SimulationBehavior.RandomWalk);
        var accelerometer = new SimulatedAccelerometer();
        accelerometer.StartSimulation(SimulationBehavior.RandomWalk);

        var settings = new PlatformSettings
        {
            Display = Device.Display,
            Accelerometer = accelerometer,
            CurrentSensor = currentSensor,
            OptionPin = keyboard.Pins.Tilde
        };

        _engine = new AppEngine(null, settings);

        return base.Initialize();
    }

    public override Task Run()
    {
        System.Windows.Forms.Application.Run(Device.Display as Form);
        return base.Run();
    }
}
