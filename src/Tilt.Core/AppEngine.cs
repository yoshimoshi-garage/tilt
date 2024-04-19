using Meadow.Hardware;

namespace Tilt;

public partial class AppEngine
{
    public AppEngine(II2cBus i2c, PlatformSettings settings)
    {
        InitializeDisplay(i2c, settings);
        InitializeAccelerometer(i2c, settings);
        InitializeCurrentSensor(i2c, settings);
        InitializeOptionPin(settings);
        InitializeCloud();
        InitializeChaosMonkey(settings);
    }
}
