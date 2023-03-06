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
            throw new NotImplementedException();
        }
    }
}