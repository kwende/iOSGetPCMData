using AVFoundation;
using Foundation;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace iOSGetPCMData.iOS
{
    public class AppleAudioEngine : IAudioEngine
    {
        private const int SAMPLE_RATE = 16000;

        AVAudioEngine _audioEngine;
        AVAudioConverter _audioConverter;
        AVAudioFormat _destinationFormat;

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

        AVAudioBuffer Turd(uint inNumberOfPackets, out AVAudioConverterInputStatus outStatus)
        {
            if (_haveData)
            {
                outStatus = AVAudioConverterInputStatus.HaveData;
                _haveData = false;
                return _latestReadBuffer;
            }
            else
            {
                outStatus = AVAudioConverterInputStatus.NoDataNow;
                return null;
            }
        }

        AVAudioPcmBuffer _latestReadBuffer;
        private uint _samplesProcessed = 0;
        private bool _haveData = false;

        private void MicrophoneBusTapCallback(AVAudioPcmBuffer buffer, AVAudioTime when)
        {
            _haveData = true;
            _latestReadBuffer = buffer;

            var convertedAudioBuffer = new AVAudioPcmBuffer(_destinationFormat, (uint)(buffer.FrameLength * 2));
            var result = _audioConverter.ConvertToBuffer(convertedAudioBuffer, out _, Turd);

            if (result == AVAudioConverterOutputStatus.HaveData ||
                result == AVAudioConverterOutputStatus.InputRanDry)
            {
                byte[] toSend = new byte[convertedAudioBuffer.FrameLength * convertedAudioBuffer.Format.StreamDescription.BytesPerFrame];

                IntPtr[] channelPointerArray = new IntPtr[1];

                Marshal.Copy(convertedAudioBuffer.Int16ChannelData, channelPointerArray, 0, 1);
                Marshal.Copy(channelPointerArray[0], toSend, 0, toSend.Length);

                _samplesProcessed += convertedAudioBuffer.FrameLength;
                OnBufferRead?.Invoke(toSend);

                Debug.WriteLine($"{convertedAudioBuffer.FrameLength} rate. {_samplesProcessed} samples processed in total. Should be for {_samplesProcessed / 16000} seconds");

            }

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
            //var inputFormat = _audioEngine.InputNode.GetBusOutputFormat(0);

            var desiredReadFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt32, 48000, 1, true);

            _audioEngine.InputNode.InstallTapOnBus(
                0, // 0 = "default bus"
                SAMPLE_RATE * 2, // buffer size, is this right? 
                desiredReadFormat,
                MicrophoneBusTapCallback);

            _destinationFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, SAMPLE_RATE, 1, false);
            _audioConverter = new AVAudioConverter(desiredReadFormat, _destinationFormat);

            _audioEngine.Prepare();

            NSError error = null;
            bool success = _audioEngine.StartAndReturnError(out error);
        }

        public void Start()
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.PlayAndRecord, AVAudioSessionCategoryOptions.DefaultToSpeaker);
            AVAudioSession.SharedInstance().SetActive(true);

            EnsureAudioPermissionsAndTapTheMicrophone();
        }
    }
}