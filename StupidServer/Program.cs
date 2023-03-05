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
            WaveFormat waveFormat = new WaveFormat(48000, 32, 1);
            //WaveFormat waveFormat = new WaveFormat(16000, 16, 1);

            var waveOut = new WaveOut();
            var waveProvider = new BufferedWaveProvider(waveFormat);
            waveOut.Init(waveProvider);
            waveOut.Play();

            TcpListener listener = new TcpListener(IPAddress.Parse("192.168.1.75"), 8550);
            listener.Start();

            Console.WriteLine("Awaiting connection.");
            Socket socket = listener.AcceptSocket();

            byte[] buffer = new byte[1024 * 10];

            Console.WriteLine("Connection established.");

            uint totalBytesRead = 0;
            int bytesRead = 0;
            DateTimeOffset lastReceiveTime = DateTimeOffset.MinValue;
            //using (FileStream fout = File.Create("C:/users/ben/desktop/default.pcm"))
            {
                //for (int c = 0; c < 15; c++)
                while ((bytesRead = socket.Receive(buffer)) > 0)
                {
                    //int bytesRead = socket.Receive(buffer);
                    //DateTimeOffset now = DateTimeOffset.Now;

                    //if (lastReceiveTime != DateTimeOffset.MinValue)
                    //{
                    //    double kbps = bytesRead * 8 / 1000 / (now - lastReceiveTime).TotalSeconds;
                    //    Console.WriteLine($"{kbps} kpbs");
                    //}

                    //fout.Write(buffer, 0, bytesRead);

                    waveProvider.AddSamples(buffer, 0, bytesRead);
                    totalBytesRead += (uint)bytesRead;

                    Console.WriteLine($"Read {totalBytesRead / (48000 * 4)} seconds of audio data");

                    //lastReceiveTime = now;
                }
            }
        }
    }
}
