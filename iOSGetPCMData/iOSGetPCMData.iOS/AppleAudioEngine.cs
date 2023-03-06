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
        AVAudioFormat nAudioFormat, _iPhoneFormat;
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
            }

            //byte[] toSend = new byte[convertedAudioBuffer.FrameLength * convertedAudioBuffer.Format.StreamDescription.BytesPerFrame];

            //IntPtr[] channelPointerArray = new IntPtr[1];

            //Marshal.Copy(convertedAudioBuffer.Int16ChannelData, channelPointerArray, 0, 1);
            //Marshal.Copy(channelPointerArray[0], toSend, 0, toSend.Length);

            //_samplesProcessed += convertedAudioBuffer.FrameLength;
            //OnBufferRead?.Invoke(toSend);

            //Debug.WriteLine($"{convertedAudioBuffer.FrameLength} rate. {_samplesProcessed} samples processed in total. Should be for {_samplesProcessed / 16000} seconds");tReadBuffer = buffer;


            //DateTimeOffset now = DateTimeOffset.Now;

            //var audioBuffer = new AVAudioPcmBuffer(_destinationFormat, (uint)(_destinationFormat.SampleRate * 2));
            //_buffer = buffer;

            //var result = _audioConverter.ConvertToBuffer(audioBuffer, out _, Turd);

            //byte[] rawBytes = new byte[audioBuffer.FrameLength * 2];
            //Marshal.Copy(audioBuffer.Int16ChannelData, rawBytes, 0, rawBytes.Length);

            //_processedBits += (rawBytes.Length * 8);

            //if (result == AVAudioConverterOutputStatus.HaveData)
            //{
            //    OnBufferRead?.Invoke(rawBytes);
            //}

            //if (_lastSend == DateTimeOffset.MinValue)
            //{
            //    _lastSend = now;
            //}
            //else
            //{
            //    double totalSeconds = (now - _lastSend).TotalSeconds;

            //    double kbps = _processedBits / 1000 / totalSeconds;

            //    Debug.WriteLine($"{kbps} kbps");

            //    _processedBits = 0;
            //}
        }

        private void TapTheMicrophone()
        {
            _audioEngine = new AVAudioEngine();

            var iPhoneformat = new AVAudioFormat(AVAudioCommonFormat.PCMInt32, IPHONE_SAMPLE_RATE, 1, true);
            var nAudioFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, NAUDIO_SAMPLE_RATE, 1, true);

            _phone2NAudioConverter = new AudioFormatConverter(iPhoneformat, nAudioFormat);
            _nAudio2PhoneConverter = new AudioFormatConverter(nAudioFormat, iPhoneformat);

            _audioEngine.InputNode.InstallTapOnBus(
                0, // 0 = "default bus"
                IPHONE_SAMPLE_RATE * 2, // buffer size, is this right? 
                iPhoneformat,
                MicrophoneBusTapCallback);

            _audioEngine.Prepare();

            NSError error = null;
            bool success = _audioEngine.StartAndReturnError(out error);

            //if (success)
            //{
            //    _phone2NAudioConverter = new AudioFormatConverter(iPhoneformat, nAudioFormat);
            //    _nAudio2PhoneConverter = new AudioFormatConverter(nAudioFormat, iPhoneformat);

            //    _player = new AVAudioPlayerNode();
            //    _audioEngine.AttachNode(_player);

            //    _iPhoneFormat = new AVAudioFormat(
            //        AVAudioCommonFormat.PCMInt32, 48000, (uint)1, false);

            //    _audioEngine.Connect(_player, _audioEngine.MainMixerNode, _iPhoneFormat);
            //}
        }

        public void Start()
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
            AVAudioSession.SharedInstance().SetActive(true);

            EnsureAudioPermissionsAndTapTheMicrophone();

        }

        public void PlayAudioData(byte[] buffer)
        {
            //AVAudioPcmBuffer toPlay = new AVAudioPcmBuffer(nAudioFormat, (uint)(buffer.Length / 2));

            //_player.ScheduleBuffer(toPlay, null);
        }
    }
}