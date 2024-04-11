using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using System;

namespace Tilt;

public enum DebugOption
{
    ButtonDetected,
    OptionNotSelected,
    OptionSelected
}

public class PlatformSettings
{
    public IPixelDisplay? Display { get; set; }
    public IAccelerometer? Accelerometer { get; set; }
    public ICurrentSensor? CurrentSensor { get; set; }
    public IPin? OptionPin { get; set; }
    public int? LeftRightAxis { get; set; }
    public bool LeftRightInvert { get; set; }
    public int? UpDownAxis { get; set; }
    public bool UpDownInvert { get; set; }
    public Action<DebugOption>? OptionSelectedAction { get; set; }
    public bool InjectRandomNetworkDisconnect { get; set; }
}
