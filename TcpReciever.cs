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
        private readonly ConcurrentQueue<Dictionary<string, float>> _queue;
        private int _QueueSize;
        private bool _keepThreadalive;
        private bool _UnexpectedTermination;
        


        public TcpMessageReceiver(int port, int QueueSize = 10)
        {
            
            _listener = new TcpListener(IPAddress.Loopback, port);
            _queue = new ConcurrentQueue<Dictionary<string, float>>();
            _QueueSize = QueueSize;
            _keepThreadalive = true;
            _UnexpectedTermination = false;
            //for (int i = 0; i < 2; i++)
            //{
                Start();
            //}
            
            //Thread t = new Thread(ReconnectIfNeeded);
            //t.Start();
            //^ this looks very dumb and I should probably just spawn threads in Reconn if needed instead

        }


        public void Start()
        {
            _listener.Start();
            Thread t = new Thread(ListenForMessages);
            t.Start();
        }

        //public void ReconnectIfNeeded()
        //{
        //    while (_keepThreadalive)
        //    {
        //        if (_UnexpectedTermination)
        //        {
        //            Start();
        //        }
        //    }

        //}

        public void Stop()
        {
            _keepThreadalive = false;
            _listener.Stop();

        }

        private void ListenForMessages()
        {

            try
            {
                Socket socket = _listener.AcceptSocket();
                Stream stream = new NetworkStream(socket);
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                Console.WriteLine("stream and reader open");
                while (_keepThreadalive)
                {
                    var message = reader.ReadLine();
                    Console.WriteLine("read message");
                    if (!string.IsNullOrEmpty(message))
                    {
                        message = message.Substring(1, message.Length - 2);
                        message = message.Replace(@"\", "");
                        Dictionary<string, float> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, float>>(message);
                        _queue.Enqueue(jsonDictionary);
                        if (_queue.Count > _QueueSize)
                        {
                            _queue.TryDequeue(out _);
                        }
                        Thread.Sleep(9);

                    }
                    else
                    {
                        Start();
                        break;
                    }
                }
                reader.Close();
                stream.Close();
                socket.Close();
                

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listening for messages: {ex.Message}");
            }

            if (_keepThreadalive)
            {
                _UnexpectedTermination = true;
            }
            

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

    }

}
