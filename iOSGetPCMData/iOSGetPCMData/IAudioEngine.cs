using System;

namespace iOSGetPCMData
{
    public interface IAudioEngine
    {
        void Start();

        event Action<int> OnBufferRead;
    }
}
