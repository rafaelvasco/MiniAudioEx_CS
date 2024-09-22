using System;
using System.IO;

namespace MiniAudioEx;

public sealed class AudioRecorder : IAudioEffect
{
        private enum State
        {
            Idle,
            WriteHeader,
            WriteData,
            CloseFile
        }

        private byte[] outputBuffer = new byte[4096];
        private ulong bytesWritten;
        private State state = State.Idle;
        private readonly object stateLock = new();
        private string _currentFileName;
        private FileStream? _stream;

        public AudioRecorder(string currentFileName, FileStream stream)
        {
            _currentFileName = currentFileName;
            _stream = stream;
        }

        public void Start()
        {
            if(GetState() != State.Idle) 
                return;

            SetState(State.WriteHeader);
        }

        public void Stop()
        {
            CloseFile();
        }

        public void OnProcess(AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            WriteHeader();
            WriteData(framesOut, frameCount, channels);
        }

        public void OnDestroy()
        {
            CloseFile();
        }

        private State GetState()
        {
            lock (stateLock)
            {
                return state;
            }
        }

        private void SetState(State newState)
        {
            lock (stateLock)
            {
                state = newState;
            }
        }

        private void WriteHeader()
        {
            if(GetState() != State.WriteHeader)
                return;

            if(bytesWritten > 0)
                return;

            if(_stream != null)
                return;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Recording");
            Console.ForegroundColor = ConsoleColor.White;

            _currentFileName = "res/recordings/" + DateTime.Now.Ticks.ToString() + ".wav";

            byte[] header = new byte[44];

            const int bitDepth = 16;

            int chunkId = 1179011410;           //"RIFF
            int chunkSize = 0;
            int format = 1163280727;            //"WAVE"
            int subChunk1Id = 544501094;        //"fmt "
            int subChunk1Size = 16;
            short audioFormat = 1;
            short numChannels = (short)AudioContext.Channels;
            int sampleRate = AudioContext.SampleRate;
            int byteRate = sampleRate * numChannels * bitDepth / 8;
            short blockAlign = (short)(numChannels * bitDepth / 8);
            short bitsPerSample = bitDepth;
            int subChunk2Id = 1635017060;       //"data"
            int subChunk2Size = 0;
            
            WriteInt32(chunkId, header, 0);
            WriteInt32(chunkSize, header, 4);
            WriteInt32(format, header, 8);
            WriteInt32(subChunk1Id, header, 12);
            WriteInt32(subChunk1Size, header, 16);
            WriteInt16(audioFormat, header, 20);
            WriteInt16(numChannels, header, 22);
            WriteInt32(sampleRate, header, 24);
            WriteInt32(byteRate, header, 28);
            WriteInt16(blockAlign, header, 32);
            WriteInt16(bitsPerSample, header, 34);
            WriteInt32(subChunk2Id, header, 36);
            WriteInt32(subChunk2Size, header, 40);

            _stream = new FileStream(_currentFileName, FileMode.CreateNew);
            _stream.Write(header, 0, header.Length);

            bytesWritten = 0;

            SetState(State.WriteData);
        }

        private void WriteData(AudioBuffer<float> framesOut, ulong frameCount, int channels)
        {
            if(GetState() != State.WriteData)
                return;

            if(_stream == null)
                return;

            uint byteSize = (uint)(framesOut.Length * sizeof(short));

            if(byteSize == 0)
                return;

            if(outputBuffer.Length < byteSize)
                outputBuffer = new byte[byteSize];

            int index = 0;

            unsafe
            {
                fixed(byte *pOutputBuffer = &outputBuffer[0])
                {
                    short *pBuffer = (short*)pOutputBuffer;
                    for(int i = 0; i < framesOut.Length; i++)
                    {
                        pBuffer[index] = (short)(framesOut[i] * short.MaxValue);
                        index++;
                    }
                }
            }

            _stream.Write(outputBuffer, 0, (int)byteSize);

            bytesWritten += byteSize;
        }

        private void CloseFile()
        {
            if(_stream != null)
            {
                if(bytesWritten > 0)
                {
                    const int headerSize = 44;
                    int chunkSize = headerSize + (int)(bytesWritten - 8);//file size - 8;
                    int subChunk2Size = (int)bytesWritten;

                    WriteInt32(chunkSize, outputBuffer, 0);
                    _stream.Seek(4, SeekOrigin.Begin);
                    _stream.Write(outputBuffer, 0, sizeof(int));
                    
                    WriteInt32(subChunk2Size, outputBuffer, 0);
                    _stream.Seek(40, SeekOrigin.Begin);
                    _stream.Write(outputBuffer, 0, sizeof(int));

                    bytesWritten = 0;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Recording stopped");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                _stream.Dispose();
                _stream = null;
            }

            SetState(State.Idle);
        }

        private unsafe void WriteInt16(short value, byte[] buffer, int offset)
        {
            fixed(byte *dst = &buffer[offset])
            {
                byte* src = (byte*)&value;
                Buffer.MemoryCopy(src, dst, sizeof(short), sizeof(short));
            }
        }

        private unsafe void WriteInt32(int value, byte[] buffer, int offset)
        {
            fixed(byte *dst = &buffer[offset])
            {
                byte* src = (byte*)&value;
                Buffer.MemoryCopy(src, dst, sizeof(int), sizeof(int));
            }
        }

        private unsafe void WriteFloat(float value, byte[] buffer, int offset)
        {
            fixed(byte *dst = &buffer[offset])
            {
                byte* src = (byte*)&value;
                Buffer.MemoryCopy(src, dst, sizeof(float), sizeof(float));
            }
        }
    }