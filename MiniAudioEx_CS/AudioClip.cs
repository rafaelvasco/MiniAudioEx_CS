using System;
using System.Runtime.InteropServices;

namespace MiniAudioEx;

/// <summary>
    /// Represents audio data that can be played back or streamed by an AudioSource. Supported file types are WAV/MP3/FlAC.
    /// </summary>
    public sealed class AudioClip : IDisposable
    {
        private string filePath;
        private IntPtr handle;
        private UInt64 dataSize;
        private UInt64 hashCode;
        private bool streamFromDisk;

        /// <summary>
        /// If the constructor with 'string filePath' overloaded is used this will contain the file path, or string.Empty otherwise.
        /// </summary>
        /// <value></value>
        public string FilePath => filePath;

        /// <summary>
        /// If true, data will be streamed from disk. This is useful when a sound is longer than just a couple of seconds. If data is loaded from memory, this property has no effect.
        /// </summary>
        /// <value></value>
        public bool StreamFromDisk => streamFromDisk;

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain a pointer to the allocated memory of the data. Do not manually free!
        /// </summary>
        /// <value></value>
        public IntPtr Handle => handle;

        /// <summary>
        /// Gets the hash code used to identify the data of this AudioClip. Only applicable if the 'byte[] data' overload is used.
        /// </summary>
        /// <value></value>
        public UInt64 Hash => hashCode;

        /// <summary>
        /// If the constructor with 'byte[] data' overload is used this will contain the size of the data in number of bytes.
        /// </summary>
        /// <value></value>
        public UInt64 DataSize
        {
            get
            {
                if(handle != IntPtr.Zero)
                {
                    return dataSize;
                }
                return 0;
            }
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from a file on disk. The file must be in an encoded format.
        /// </summary>
        /// <param name="filePath">The filepath of the encoded audio file (WAV/MP3/FLAC)</param>
        /// <param name="streamFromDisk">If true, streams data from disk rather than loading the entire file into memory for playback. Typically you'd stream from disk if a sound is more than just a couple of seconds long.</param>
        public AudioClip(string filePath, bool streamFromDisk = true)
        {
            this.filePath = filePath;
            this.streamFromDisk = streamFromDisk;
            this.handle = IntPtr.Zero;
            this.hashCode = 0;
        }

        /// <summary>
        /// Creates a new AudioClip instance which gets its data from memory. The data must be in an encoded format.
        /// </summary>
        /// <param name="data">Must be encoded audio data (either WAV/MP3/WAV)</param>
        /// <param name="isUnique">If true, then this clip will not use shared memory. If true, this clip will reuse existing memory if possible.</param>
        public AudioClip(byte[] data, bool isUnique = false)
        {
            this.filePath = string.Empty;
            this.streamFromDisk = false;
            this.dataSize = (UInt64)data.Length;

            if(isUnique)
                this.hashCode = (UInt64)data.GetHashCode();
            else
                this.hashCode = GetHashCode(data, data.Length);

            if(AudioContext.GetAudioClipHandle(hashCode, out IntPtr existingHandle))
            {
                handle = existingHandle;
            }
            else
            {
                handle = Marshal.AllocHGlobal(data.Length);

                if(handle != IntPtr.Zero)
                {            
                    unsafe
                    {
                        byte *ptr = (byte*)handle.ToPointer();
                        for(int i = 0; i < data.Length; i++)
                            ptr[i] = data[i];
                    }

                    AudioContext.Add(this);
                }
            }
        }

        public void Dispose()
        {
            AudioContext.Remove(this);
        }

        /// <summary>
        /// This methods creates a hash of the given data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private UInt64 GetHashCode(byte[] data, int size)
        {
            UInt64 hash = 0;

            for(int i = 0; i < size; i++) 
            {
                hash = data[i] + (hash << 6) + (hash << 16) - hash;
            }

            return hash;            
        }
    }