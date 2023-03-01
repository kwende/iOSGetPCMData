using AVFoundation;
using Foundation;
using System;

namespace iOSGetPCMData.iOS
{
    public class AppleAudioEngine : IAudioEngine
    {
        private const int SAMPLE_RATE = 16000;

        AVAudioEngine _audioEngine;
        AVAudioConverter _audioConverter;
        AVAudioFormat _destinationFormat;

        public event Action<int> OnBufferRead;
        private int _bufferCount = 0;

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

        AVAudioBuffer Turd(uint inNumberOfPackets, out AVAudioConverterInputStatus outStatus)
        {
            outStatus = AVAudioConverterInputStatus.HaveData;
            return _buffer;
        }

        AVAudioPcmBuffer _buffer;

        private void MicrophoneBusTapCallback(AVAudioPcmBuffer buffer, AVAudioTime when)
        {
            var audioBuffer = new AVAudioPcmBuffer(_destinationFormat, (uint)(_destinationFormat.SampleRate * 2));
            _buffer = buffer;

            //AVAudioBuffer AVAudioConverterInputHandler(uint inNumberOfPackets, out AVAudioConverterInputStatus outStatus);

            var result = _audioConverter.ConvertToBuffer(audioBuffer, out _, Turd);

            //NSError error = null;
            //bool success = _audioConverter.ConvertToBuffer(audioBuffer, buffer, out error);

            if (result == AVAudioConverterOutputStatus.HaveData)
            {
                OnBufferRead?.Invoke(++_bufferCount);
            }
        }

        private void TapTheMicrophone()
        {
            _audioEngine = new AVAudioEngine();
            var inputFormat = _audioEngine.InputNode.GetBusOutputFormat(0);

            _audioEngine.InputNode.InstallTapOnBus(
                0, // 0 = "default bus"
                SAMPLE_RATE * 2, // buffer size, is this right? 
                inputFormat,
                MicrophoneBusTapCallback);

            _destinationFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, SAMPLE_RATE, 1, true);
            _audioConverter = new AVAudioConverter(inputFormat, _destinationFormat);

            _audioEngine.Prepare();

            NSError error = null;
            bool success = _audioEngine.StartAndReturnError(out error);
        }

        public void Start()
        {
            EnsureAudioPermissionsAndTapTheMicrophone();
        }
    }
}