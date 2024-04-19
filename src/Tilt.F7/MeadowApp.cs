using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Logging;
using Meadow.Peripherals.Leds;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tilt.F7;

public class FeatherV2App : App<F7FeatherV2>
{
    private AppEngine _engine;
    private RgbLed _onBoardLed;
    private UdpLogger? _udpLogger;

    public override Task Initialize()
    {
        Device.PlatformOS.NtpClient.TimeChanged += NtpClient_TimeChanged;

        var nic = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
        if (nic != null)
        {
            nic.NetworkConnected += (s, e) =>
            {
                if (_udpLogger == null)
                {
                    _udpLogger = new UdpLogger();
                    Resolver.Log.Info("Adding UDP logger");
                    Resolver.Log.AddProvider(_udpLogger);
                }
            };

            _ = nic.Connect("interwebs", "1234567890");
        }

        var i2c = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);

        var settings = new PlatformSettings
        {
            OptionPin = Device.Pins.D05,
            OptionSelectedAction = OptionAction,
            InjectRandomNetworkDisconnect = false
        };

        _onBoardLed = new RgbLed(
            Device.Pins.OnboardLedRed,
            Device.Pins.OnboardLedGreen,
            Device.Pins.OnboardLedBlue);

        _engine = new AppEngine(i2c, settings);

        return base.Initialize();
    }

    public override void OnBootFromCrash(IEnumerable<string> crashReports)
    {
        Resolver.Log.Warn("Booting up from crash");
        foreach (var report in crashReports)
        {
            Resolver.Log.Warn(report);
        }
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