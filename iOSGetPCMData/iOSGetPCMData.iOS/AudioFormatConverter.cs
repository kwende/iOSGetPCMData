using AVFoundation;
using System;

namespace iOSGetPCMData.iOS
{
    internal sealed class AudioFormatConverter : IDisposable
    {
        private AVAudioFormat _sourceFormat, _destinationFormat;
        private bool _haveFreshData = false;
        private AVAudioPcmBuffer _latestBuffer;
        private AVAudioConverter _audioConverter;

        public AudioFormatConverter(AVAudioFormat sourceFormat, AVAudioFormat destinationFormat)
        {
            _sourceFormat = sourceFormat;
            _destinationFormat = destinationFormat;

            _audioConverter = new AVAudioConverter(sourceFormat, destinationFormat);
        }

        public AVAudioPcmBuffer Convert(AVAudioPcmBuffer input)
        {
            _latestBuffer = input;
            _haveFreshData = true;

            var convertedAudioBuffer = new AVAudioPcmBuffer(_destinationFormat, (uint)(input.FrameLength * 10));
            var result = _audioConverter.ConvertToBuffer(convertedAudioBuffer, out _, ConverterCallback);

            if (result == AVAudioConverterOutputStatus.HaveData ||
                result == AVAudioConverterOutputStatus.InputRanDry)
            {
                return convertedAudioBuffer;
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            _audioConverter?.Dispose();
            _audioConverter = null;

            _sourceFormat?.Dispose();
            _sourceFormat = null;

            _destinationFormat?.Dispose();
            _destinationFormat = null;
        }

        private AVAudioBuffer ConverterCallback(uint inNumberOfPackets, out AVAudioConverterInputStatus outStatus)
        {
            if (_haveFreshData)
            {
                outStatus = AVAudioConverterInputStatus.HaveData;
                _haveFreshData = false;
                return _latestBuffer;
            }
            else
            {
                outStatus = AVAudioConverterInputStatus.NoDataNow;
                return null;
            }
        }
    }
}