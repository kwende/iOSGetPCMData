using AVFoundation;
using Foundation;
using System;
using Xamarin.Forms;

namespace GetPCMData
{
    public partial class MainPage : ContentPage
    {
        private AVAudioConverter _converter;
        private AVAudioFormat _desiredFormat;
        private AVAudioMixerNode _avAudioMixer;
        private AVAudioEngine _avAudioEngine;

        public MainPage()
        {
            InitializeComponent();
        }

        private void Tap(AVAudioPcmBuffer buffer, AVAudioTime when)
        {
            Turd2.Text = "Got called!";

            AVAudioPcmBuffer bufferFormatINeed = new AVAudioPcmBuffer(_desiredFormat, 2048);

            NSError error = null;
            if (_converter.ConvertToBuffer(buffer, bufferFormatINeed, out error))
            {

            }
        }

        private void Thing_Clicked(object sender, EventArgs e)
        {
            if (AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Audio) != AVAuthorizationStatus.Authorized)
            {
                AVCaptureDevice.RequestAccessForMediaType(AVAuthorizationMediaType.Audio, (bool granted) =>
                {
                    if (granted)
                    {

                    }
                });
            }

            var instance = AVAudioSession.SharedInstance();

            var available = instance.InputIsAvailable;
            var inputs = instance.AvailableInputs;

            var error = instance.SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
            error = instance.SetActive(true);

            _avAudioEngine = new AVAudioEngine();


            _avAudioMixer = new AVAudioMixerNode();

            var format = new AVAudioFormat(AVAudioCommonFormat.PCMInt32, 44100.0, 1, true);

            //_desiredFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, 16000, 1, true);
            //_converter = new AVAudioConverter(busFormat, _desiredFormat);

            _avAudioEngine.AttachNode(_avAudioMixer);
            _avAudioEngine.Connect(_avAudioEngine.InputNode,
                _avAudioMixer, null);

            _avAudioMixer.InstallTapOnBus(0, 2048, null, Tap);

            NSError nsError = null;
            bool ret = _avAudioEngine.StartAndReturnError(out nsError);
        }
    }
}
