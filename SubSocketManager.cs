using System;
using System.Collections.Generic;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace MediaPipeWebcam
{
    public class SubSocketManager : IDisposable
{
    private readonly string _socketAddress;
    private readonly int _maxQueueLength;
    private readonly Queue<Dictionary<string, object>> _messageQueue;
    private readonly object _queueLock = new object();
    private readonly Thread _receiverThread;
    private bool _running;
    private bool _disposed;

    public SubSocketManager(string socketAddress, int maxQueueLength)
    {
        _socketAddress = socketAddress;
        _maxQueueLength = maxQueueLength;
        _messageQueue = new Queue<Dictionary<string, object>>();
        _receiverThread = new Thread(ReceiveMessages)
        {
            IsBackground = true
        };
        _running = true;
        _receiverThread.Start();
    }

    private void ReceiveMessages()
    {
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Connect(_socketAddress);
            subSocket.Subscribe(""); // Subscribe to all messages

            while (_running)
            {
                try
                {
                    string message = subSocket.ReceiveFrameString();
                    var jsonDictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                    EnqueueMessage(ConvertValuesToNumbers(jsonDictionary));
                    //Console.WriteLine("recieved message");
                }
                catch (Exception ex)
                {
                    // Log or handle the error according to your needs
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                }
            }
        }
    }

    private Dictionary<string, object> ConvertValuesToNumbers(Dictionary<string, object> original)
    {
        var transformed = new Dictionary<string, object>();
        foreach (var kvp in original)
        {
            if (kvp.Value is long longValue) // JSON deserializes numbers as long
            {
                transformed[kvp.Key] = (float)longValue; // Cast to float
            }
            else if (kvp.Value is double doubleValue) // Handle double values
            {
                transformed[kvp.Key] = (float)doubleValue; // Cast to float
            }
            else if (kvp.Value is string stringValue)
            {
                transformed[kvp.Key] = stringValue; // Keep it as string
            }
            else
            {
                transformed[kvp.Key] = kvp.Value; // or handle the default case
            }
        }
        return transformed;
    }

    private void EnqueueMessage(Dictionary<string, object> message)
    {
        lock (_queueLock)
        {
            if (_messageQueue.Count >= _maxQueueLength)
            {
                _messageQueue.Dequeue(); // Remove the oldest entry
            }
            _messageQueue.Enqueue(message);
        }
    }

    public Dictionary<string, object> GetLatestData()
    {
        lock (_queueLock)
        {
            return _messageQueue.Count > 0 ? _messageQueue.Peek() : null;
        }
    }

    public Dictionary<string, object> GetOldestData()
    {
        lock (_queueLock)
        {
            return _messageQueue.Count > 0 ? _messageQueue.Dequeue() : null;
        }
    }

    public void FlushQueue()
    {
        lock (_queueLock)
        {
            _messageQueue.Clear();
        }
    }

    public void Stop()
    {
        _running = false;
        _receiverThread.Join();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            NetMQ.NetMQConfig.Cleanup();
            _disposed = true;
        }
    }

    ~SubSocketManager()
    {
        Dispose();
    }
}



}
