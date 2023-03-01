using System;
using Xamarin.Forms;

namespace iOSGetPCMData.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            App.AudioEngine.OnBufferRead += AudioEngine_OnBufferRead;
            App.AudioEngine.Start();
        }

        private void AudioEngine_OnBufferRead(int obj)
        {
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                BufferCount.Text = obj.ToString();
            });
        }
    }
}