using Meadow.Foundation.Displays;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Tilt;

public partial class AppEngine
{
    private DisplayService _displayService;

    private void InitializeDisplay(II2cBus i2c, PlatformSettings settings)
    {
        IPixelDisplay display;

        if (settings.Display != null)
        {
            display = settings.Display;
        }
        else
        {
            display = new Ssd1306(i2c);
        }
        _displayService = new DisplayService(display);
    }
}
