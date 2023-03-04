using NAudio.Wave;
using System;
using System.Net;
using System.Net.Sockets;

namespace StupidServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WaveFormat waveFormat = new WaveFormat(16000, 16, 1);

            var waveOut = new WaveOut();
            var waveProvider = new BufferedWaveProvider(waveFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();

            TcpListener listener = new TcpListener(IPAddress.Parse("172.17.5.65"), 8550);
            listener.Start();

            Console.WriteLine("Awaiting connection.");
            Socket socket = listener.AcceptSocket();

            byte[] buffer = new byte[1024 * 10];

            Console.WriteLine("Connection established.");

            int bytesRead = 0;
            DateTimeOffset lastReceiveTime = DateTimeOffset.MinValue;
            while ((bytesRead = socket.Receive(buffer)) > 0)
            {
                DateTimeOffset now = DateTimeOffset.Now;

                if (lastReceiveTime != DateTimeOffset.MinValue)
                {
                    double kbps = (bytesRead * 8) / 1000 / (now - lastReceiveTime).TotalSeconds;
                    Console.WriteLine($"{kbps} kpbs");
                }

                waveProvider.AddSamples(buffer, 0, bytesRead);

                lastReceiveTime = now;
            }
        }
    }
}
