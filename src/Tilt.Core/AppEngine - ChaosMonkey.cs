using Meadow;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Tilt;

public partial class AppEngine
{
    private Timer? _networkFaultTimer;
    private readonly Random _random = new();

    private void InitializeChaosMonkey(PlatformSettings settings)
    {
        if (settings.InjectRandomNetworkDisconnect)
        {
            // in 2 minutes, we'll start injecting network troubles
            _networkFaultTimer = new Timer(NetworkFaultTimerProc, null, TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(30));
        }
    }

    private void NetworkFaultTimerProc(object _)
    {
        var nic = Resolver.Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

        if (nic == null) return;

        if (nic.IsConnected)
        {
            // 20% chance we'll disconnect
            if (_random.Next(5) == 1)
            {
                Resolver.Log.Info("Disconnecting network");
                nic.Disconnect(false);
            }
        }
        else
        {
            // 25% chance we'll reconnect
            if (_random.Next(4) == 1)
            {
                Resolver.Log.Info("Reconnecting network");
                _ = nic.Connect("interwebs", "1234567890");
            }
        }
    }
}
