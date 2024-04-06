using Meadow;
using Meadow.Devices;
using System.Threading.Tasks;

namespace Tilt.F7;

public class ProjLabApp : App<F7CoreComputeV2>
{
    private AppEngine _engine;

    public override Task Initialize()
    {
        var i2c = Device.CreateI2cBus();
        var projLab = ProjectLab.Create();

        var settings = new PlatformSettings
        {
            Display = projLab.Display,
            Accelerometer = projLab.Accelerometer,
            LeftRightAxis = 0,
            UpDownInvert = true,
            UpDownAxis = 1
        };

        _engine = new AppEngine(i2c, settings);

        return base.Initialize();
    }
}
