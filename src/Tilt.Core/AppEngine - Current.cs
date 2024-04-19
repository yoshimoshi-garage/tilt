using Meadow;
using Meadow.Cloud;
using Meadow.Foundation.Sensors.Power;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Linq;

namespace Tilt;

public partial class AppEngine
{
    private readonly CircularBuffer<Current> _currentBuffer = new CircularBuffer<Current>(60);
    private int _measurementNumber = 1;

    private void InitializeCurrentSensor(II2cBus i2c, PlatformSettings settings)
    {
        ICurrentSensor currentSensor;

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

        currentSensor.Updated += CurrentSensor_Updated;
        currentSensor.StartUpdating(TimeSpan.FromSeconds(1));
    }


    private void CurrentSensor_Updated(object sender, IChangeResult<Current> e)
    {
        Resolver.Log.Info($"Current {e.New.Milliamps:N0} mA");

        _currentBuffer.Append(e.New);

        if (_measurementNumber++ % 60 == 0)
        {
            var mean = _currentBuffer.Average(c => c.Milliamps);
            Resolver.Log.Info($"Mean current over the past minute: {mean:N0} mA");
            _displayService?.UpdateMeanCurrent(mean);

            try
            {
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
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to send Current data to Cloud: {ex.Message}");
            }
        }

        _displayService?.UpdateCurrent(e.New);
    }
}
