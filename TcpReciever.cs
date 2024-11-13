using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace MediaPipeWebcamModule
{



    public class TcpMessageReceiver
    {
        private readonly TcpListener _listener;
        private TcpClient _currentClient;
        private bool _isListening;
        private readonly ConcurrentQueue<Dictionary<string, float>> _queue;
        private const int QueueSize = 10;
        private Thread _listenerThread;

        public TcpMessageReceiver(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _queue = new ConcurrentQueue<Dictionary<string, float>>();
            Start();
        }

        public void Start()
        {
            _listener.Start();
            _isListening = true;
            _listenerThread = new Thread(ListenForMessages);
            _listenerThread.Start();
        }

        public void Stop()
        {
            _isListening = false;
            _listenerThread.Join();
            _currentClient?.Close();
            _listener.Stop();
        }

        public Dictionary<string, float> GetOldestMessage()
        {
            if (_queue.TryDequeue(out var oldestMessage))
            {
                return oldestMessage;
            }
            return null;
        }

        public int Length()
        {
            return _queue.Count;
        }

        private void ListenForMessages()
        {
            while (_isListening)
            {
                try
                {
                    _currentClient = _listener.AcceptTcpClient();
                    using (var stream = _currentClient.GetStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var message = reader.ReadLine();

                   

                        if (!string.IsNullOrEmpty(message))
                        {
                            message = message.Substring(1, message.Length - 2);
                            message = message.Replace(@"\", "");
                            Dictionary<string, float> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, float>>(message);
                            _queue.Enqueue(jsonDictionary);
                            if (_queue.Count > QueueSize)
                            {
                                _queue.TryDequeue(out _);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error listening for messages: {ex.Message}");
                }
            }
        }
    }

}
