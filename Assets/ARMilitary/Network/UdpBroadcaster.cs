using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ARMilitary.Network
{
    public class UdpBroadcaster : IDisposable
    {
        private const int Port = 7777;

        private UdpClient _client;
        private Thread _receiveThread;
        private bool _running;
        private readonly ConcurrentQueue<string> _inbound = new ConcurrentQueue<string>();

        public string DeviceId { get; } = SystemInfo.deviceUniqueIdentifier;
        public bool IsListening => _running;

        public void StartListening()
        {
            try
            {
                _client = new UdpClient();
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
                _client.EnableBroadcast = true;
                _running = true;
                _receiveThread = new Thread(ReceiveLoop) { IsBackground = true, Name = "ARMilitary.UDP" };
                _receiveThread.Start();
                Debug.Log("[ARMilitary] UDP listening on port " + Port);
            }
            catch (Exception e)
            {
                Debug.LogError("[ARMilitary] UDP start failed: " + e.Message);
            }
        }

        private void ReceiveLoop()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (_running)
            {
                try
                {
                    var data = _client.Receive(ref endpoint);
                    _inbound.Enqueue(Encoding.UTF8.GetString(data));
                }
                catch (SocketException) { break; }
                catch (Exception e) { Debug.LogWarning("[ARMilitary] UDP receive: " + e.Message); }
            }
        }

        public bool TryDequeueRaw(out string json)
        {
            return _inbound.TryDequeue(out json);
        }

        public void Send(string json)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(json);
                using var sender = new UdpClient { EnableBroadcast = true };
                sender.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, Port));
            }
            catch (Exception e)
            {
                Debug.LogError("[ARMilitary] UDP send failed: " + e.Message);
            }
        }

        public void Dispose()
        {
            _running = false;
            try { _client?.Close(); } catch { }
            _receiveThread?.Join(500);
        }
    }
}
