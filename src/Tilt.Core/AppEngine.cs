using Meadow;
using Meadow.Cloud;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Foundation.Sensors.Power;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Linq;

namespace Tilt;

public class SetEmojiCommand : IMeadowCommand
{
    public int Value { get; set; }
}

public class AppEngine
{
    public const int RefreshRate = 50;

    private DisplayService _displayService;
    private int _axisLR = 2; // default Z
    private int _axisUD = 1; // default Y
    private bool _invertLR = false;
    private bool _invertUD = false;
    private CircularBuffer<Current> _currentBuffer = new CircularBuffer<Current>(60);
    private int _measurementNumber = 1;

    private readonly PlatformSettings _platformSettings;

    public SetEmojiCommand Command { get; set; }

    public AppEngine(II2cBus i2c, PlatformSettings settings)
    {
        IAccelerometer? accelerometer = null;
        IPixelDisplay display;
        ICurrentSensor currentSensor;

        _platformSettings = settings;

        if (settings.Accelerometer != null)
        {
            accelerometer = settings.Accelerometer;
        }
        else
        {
            try
            {
                accelerometer = new Mpu6050(i2c);
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Failed to comm with accelerometer: {ex.Message}");
            }
        }

        if (settings.Display != null)
        {
            display = settings.Display;
        }
        else
        {
            display = new Ssd1306(i2c);
        }

        if (settings.CurrentSensor != null)
        {
            currentSensor = settings.CurrentSensor;
        }
        else
        {
            var ina = new Ina219(i2c);
            ina.Configure(
                busVoltageRange: Ina219.BusVoltageRange.Range_16V,
                maxExpectedCurrent: new Current(1.0),
                adcMode: Ina219.ADCModes.ADCMode_4xAvg_2128us);

            currentSensor = ina;
        }

        if (settings.UpDownAxis != null)
        {
            _axisUD = settings.UpDownAxis.Value;
            _invertUD = settings.UpDownInvert;
        }
        if (settings.LeftRightAxis != null)
        {
            _axisLR = settings.LeftRightAxis.Value;
            _invertLR = settings.LeftRightInvert;
        }

        _displayService = new DisplayService(display);

        if (accelerometer != null)
        {
            accelerometer.Updated += Gyro_Updated;
            accelerometer.StartUpdating(TimeSpan.FromMilliseconds(RefreshRate));
        }

        currentSensor.Updated += CurrentSensor_Updated;
        currentSensor.StartUpdating(TimeSpan.FromSeconds(1));

        if (settings.OptionPin != null)
        {
            Resolver.Log.Info($"Connecting Option pin");

            var optionPort = settings.OptionPin.CreateDigitalInterruptPort(
                InterruptMode.EdgeFalling,
                ResistorMode.ExternalPullUp,
                TimeSpan.FromSeconds(1));

            optionPort.Changed += (o, e) =>
            {
                Resolver.Log.Info($"Option pin interrupt");

                _displayService.ToggleView();
            };
        }

        Resolver.MeadowCloudService.SendEvent(
            new CloudEvent
            {
                Description = $"Tilt Started on {Resolver.Device.Information.Platform}"
            });

        Resolver.CommandService.Subscribe<SetEmojiCommand>(c =>
        {
            Resolver.Log.Info($"Command received! {c.Value}");

            var _ = c.Value switch
            {
                > 100 => _displayService.ShowEmoji(Emoji.Laugh),
                > 50 => _displayService.ShowEmoji(Emoji.Happy),
                _ => _displayService.ShowEmoji(Emoji.None)
            };
        });
    }

    private void CurrentSensor_Updated(object sender, IChangeResult<Current> e)
    {
        _currentBuffer.Append(e.New);

        if (_measurementNumber++ % 60 == 0)
        {
            var mean = _currentBuffer.Average(c => c.Milliamps);
            Resolver.Log.Info($"Mean current over the past minute: {mean:N0} mA");
            _displayService.UpdateMeanCurrent(mean);

            Resolver.MeadowCloudService.SendEvent(
                new CloudEvent
                {
                    Description = $"Tilt Power Usage",
                    Measurements = new()
                    {
                        { "MeanCurrent", mean }
                    }
                });
        }

        _displayService.UpdateCurrent(e.New);
    }

    private void Gyro_Updated(object sender, IChangeResult<Acceleration3D> e)
    {
        if (e.New != null)
        {
            var lr = _axisLR switch
            {
                0 => e.New.X.MetersPerSecondSquared,
                1 => e.New.Y.MetersPerSecondSquared,
                _ => e.New.Z.MetersPerSecondSquared
            };
            if (_invertLR) { lr *= -1; }

            var ud = _axisUD switch
            {
                0 => e.New.X.MetersPerSecondSquared,
                1 => e.New.Y.MetersPerSecondSquared,
                _ => e.New.Z.MetersPerSecondSquared
            };
            if (_invertUD) { ud *= -1; }

            if (ud > 1)
            {
                _displayService.MoveCircleUp();
            }
            else if (ud < -1)
            {
                _displayService.MoveCircleDown();
            }

            if (lr > 1)
            {
                _displayService.MoveCircleLeft();
            }
            else if (lr < -1)
            {
                _displayService.MoveCircleRight();
            }
        }
    }
}
