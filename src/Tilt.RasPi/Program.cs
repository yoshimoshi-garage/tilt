using Meadow;

namespace Tilt.RasPi;

internal class Program : App<RaspberryPi>
{
    private AppEngine _engine;

    private static void Main(string[] args)
    {
        MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        var i2c = Device.CreateI2cBus();

        var settings = new PlatformSettings
        {
            OptionPin = Device.Pins.GPIO27,
            OptionSelectedAction = null
        };
        _engine = new AppEngine(i2c, settings);

        return base.Initialize();
    }
}
