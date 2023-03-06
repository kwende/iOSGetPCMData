using AVFoundation;
using Foundation;
using System;

namespace iOSGetPCMData.iOS
{
    public class AppleAudioEngine : IAudioEngine
    {
        private const int NAUDIO_SAMPLE_RATE = 16000;
        private const int IPHONE_SAMPLE_RATE = 48000;

        AVAudioEngine _audioEngine;
        AudioFormatConverter _phone2NAudioConverter, _nAudio2PhoneConverter;
        AVAudioPlayerNode _player;

        public event Action<byte[]> OnBufferRead;

        private DateTimeOffset _lastSend = DateTimeOffset.MinValue;

        private void EnsureAudioPermissionsAndTapTheMicrophone()
        {
            if (AVAudioSession.SharedInstance().RecordPermission != AVAudioSessionRecordPermission.Granted)
            {
                AVAudioSession.SharedInstance().RequestRecordPermission((bool granted) =>
                {
                    if (granted)
                    {
                        TapTheMicrophone();
                    }
                });
            }
            else
            {
                TapTheMicrophone();
            }
        }

        AVAudioPcmBuffer _latestReadBuffer;
        private uint _samplesProcessed = 0;
        private bool _haveData = false;

        private void MicrophoneBusTapCallback(AVAudioPcmBuffer audioBufferFromPhone, AVAudioTime when)
        {
            var audioBuffer4NAudio = _phone2NAudioConverter.Convert(audioBufferFromPhone);

            if (audioBuffer4NAudio != null)
            {
                OnBufferRead?.Invoke(AVAudioPCMBufferByteConverter.PCMBuffer2Bytes(audioBuffer4NAudio));

                var toPlay = _nAudio2PhoneConverter.Convert(audioBuffer4NAudio);
                _player.ScheduleBuffer(toPlay, null);
            }
        }

        private void TapTheMicrophone()
        {
            _audioEngine = new AVAudioEngine();

            var iPhone32BitFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt32, IPHONE_SAMPLE_RATE, 1, false);
            var nAudioFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, NAUDIO_SAMPLE_RATE, 1, false);
            var mainMixerNodeFormat = _audioEngine.MainMixerNode.GetBusOutputFormat(0);

            _phone2NAudioConverter = new AudioFormatConverter(iPhone32BitFormat, nAudioFormat);
            _nAudio2PhoneConverter = new AudioFormatConverter(nAudioFormat, mainMixerNodeFormat);

            _audioEngine.InputNode.InstallTapOnBus(
                0, // 0 = "default bus"
                IPHONE_SAMPLE_RATE * 2, // buffer size, is this right? 
                iPhone32BitFormat,
                MicrophoneBusTapCallback);

            _audioEngine.Prepare();

            NSError error = null;
            bool success = _audioEngine.StartAndReturnError(out error);

            if (success)
            {
                _player = new AVAudioPlayerNode();
                _audioEngine.AttachNode(_player);

                _audioEngine.Connect(_player, _audioEngine.MainMixerNode, mainMixerNodeFormat);
                _player.Play();
            }
        }

        public void Start()
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
            AVAudioSession.SharedInstance().SetActive(true);

            EnsureAudioPermissionsAndTapTheMicrophone();

        }

        public void PlayAudioData(byte[] buffer)
        {
            //AVAudioPcmBuffer toPlay16Bit = AVAudioPCMBufferByteConverter.Bytes2PCMBuffer(buffer);
            //AVAudioPcmBuffer mainMixerNodeBuffer = _nAudio2PhoneConverter.Convert(toPlay16Bit);
            //_player.ScheduleBuffer(mainMixerNodeBuffer, () =>
            //{

            //});
        }
    }
}