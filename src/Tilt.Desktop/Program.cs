using Meadow;

namespace Tilt.RasPi;

internal class Program : App<Desktop>
{
    private AppEngine _engine;

    private static void Main(string[] args)
    {
        MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        var settings = new PlatformSettings
        {
            Display = Device.Display
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
