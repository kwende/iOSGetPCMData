using AVFoundation;
using System;
using System.Runtime.InteropServices;

namespace iOSGetPCMData.iOS
{
    public static class AVAudioPCMBufferByteConverter
    {
        public static byte[] PCMBuffer2Bytes(AVAudioPcmBuffer input)
        {
            if (input.Format.StreamDescription.ChannelsPerFrame != 1)
            {
                throw new ArgumentException("Input format must have only one channel", nameof(input));
            }

            if (input.Int16ChannelData == null)
            {
                throw new ArgumentException("Input format must be in Int16", nameof(input));
            }

            byte[] ret = new byte[input.FrameLength * input.Format.StreamDescription.BytesPerFrame];

            IntPtr[] channelPointerArray = new IntPtr[1];

            Marshal.Copy(input.Int16ChannelData, channelPointerArray, 0, 1);
            Marshal.Copy(channelPointerArray[0], ret, 0, ret.Length);

            return ret;
        }

        public static AVAudioPcmBuffer Bytes2PCMBuffer(byte[] input)
        {
            // options: https://stackoverflow.com/questions/31423790/how-to-play-audio-from-avaudiopcmbuffer-converted-from-nsdata

            AVAudioFormat audioFormat = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, 16000, 1, false);
            uint numberOfFramesInData = (uint)(input.Length / audioFormat.StreamDescription.BytesPerFrame);
            AVAudioPcmBuffer ret = new AVAudioPcmBuffer(audioFormat, numberOfFramesInData);

            IntPtr[] channelPtrs = new IntPtr[1];
            Marshal.Copy(ret.Int16ChannelData, channelPtrs, 0, 1);
            Marshal.Copy(input, 0, channelPtrs[0], input.Length);

            ret.FrameLength = numberOfFramesInData;

            return ret;
        }
    }
}