package com.ARMilitary.trainer;

import android.net.wifi.WifiManager;
import android.os.Bundle;
import com.unity3d.player.UnityPlayerActivity;

/**
 * Acquires a WiFi Multicast Lock so UDP broadcast packets
 * are not silently dropped by Android on WiFi networks.
 * Required for Android 10+ where multicast is restricted.
 */
public class ARMilitaryMainActivity extends UnityPlayerActivity
{
    private WifiManager.MulticastLock _multicastLock;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        WifiManager wifi = (WifiManager) getApplicationContext()
                .getSystemService(WIFI_SERVICE);
        if (wifi != null)
        {
            _multicastLock = wifi.createMulticastLock("ARMilitaryUDP");
            _multicastLock.setReferenceCounted(true);
            _multicastLock.acquire();
        }
    }

    @Override
    protected void onDestroy()
    {
        if (_multicastLock != null && _multicastLock.isHeld())
            _multicastLock.release();
        super.onDestroy();
    }
}
