using System;
using System.Net.Sockets;
using Xamarin.Forms;

namespace iOSGetPCMData.Views
{
    public partial class AboutPage : ContentPage
    {
        private TcpClient _tcpClient;
        private int _batchCount = 0;

        public AboutPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect("172.17.5.65", 8550);

            App.AudioEngine.OnBufferRead += AudioEngine_OnBufferRead;
            App.AudioEngine.Start();
        }

        private void AudioEngine_OnBufferRead(byte[] obj)
        {
            _tcpClient.GetStream().Write(obj, 0, obj.Length);

            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                BufferCount.Text = (++_batchCount).ToString();
            });
        }
    }
}