using Meadow;
using Meadow.Hardware;
using System;

namespace Tilt;

public partial class AppEngine
{
    private void InitializeOptionPin(PlatformSettings settings)
    {
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
    }
}
