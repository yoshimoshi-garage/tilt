using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System.Threading.Tasks;

namespace Tilt.F7;

public class FeatherV2App : App<F7FeatherV2>
{
    private AppEngine _engine;
    private RgbLed _onBoardLed;

    public override Task Initialize()
    {
        Device.PlatformOS.NtpClient.TimeChanged += NtpClient_TimeChanged;

        _ = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>().Connect("interwebs", "1234567890");

        var i2c = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);

        var settings = new PlatformSettings
        {
            OptionPin = Device.Pins.D05,
            OptionSelectedAction = OptionAction
        };

        _onBoardLed = new RgbLed(
            Device.Pins.OnboardLedRed,
            Device.Pins.OnboardLedGreen,
            Device.Pins.OnboardLedBlue);

        _engine = new AppEngine(i2c, settings);

        return base.Initialize();
    }

    private void NtpClient_TimeChanged(System.DateTime utcTime)
    {
        Resolver.Log.Info("NTP Time Set");
    }

    private void OptionAction(DebugOption option)
    {
        switch (option)
        {
            case DebugOption.ButtonDetected:
                // the button was down at boot
                // show a blue LED
                _onBoardLed.SetColor(RgbLedColors.Yellow);
                break;
            case DebugOption.OptionNotSelected:
                // no button, or the button was released before the timer elapsed
                // pulse a red LED
                Task.Run(async () =>
                {
                    _onBoardLed.SetColor(RgbLedColors.Red);
                    await Task.Delay(2000);
                    _onBoardLed.IsOn = false;
                });
                break;
            case DebugOption.OptionSelected:
                // the option has been selected
                // blink the green LED
                _onBoardLed.StartBlink(RgbLedColors.Green);
                break;
        }
    }
}