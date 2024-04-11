using Meadow;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;

namespace Tilt;

public partial class AppEngine
{
    public const int RefreshRate = 50;

    private int _axisLR = 2; // default Z
    private int _axisUD = 1; // default Y
    private bool _invertLR = false;
    private bool _invertUD = false;

    private void InitializeAccelerometer(II2cBus i2c, PlatformSettings settings)
    {
        IAccelerometer? accelerometer = null;
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

        if (accelerometer != null)
        {
            accelerometer.Updated += Gyro_Updated;
            accelerometer.StartUpdating(TimeSpan.FromMilliseconds(RefreshRate));
        }
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
