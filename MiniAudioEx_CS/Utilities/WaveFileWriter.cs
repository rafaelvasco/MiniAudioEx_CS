using System;
using System.IO;

namespace MiniAudioEx;

/// <summary>
    /// A helper class to write PCM data to a 16 bit wave file.
    /// </summary>
    public static unsafe class WaveFileWriter
    {
        public static void Save(string outputFilePath, float[] data, int channels, int sampleRate)
        {
            int payloadSize = data.Length * sizeof(short);
            
            const int headerSize = 44;
            byte[] header = new byte[headerSize];

            const int bitDepth = 16;

            int chunkId = 1179011410;                     //"RIFF
            int chunkSize = headerSize + payloadSize - 8; //File size - 8
            int format = 1163280727;                      //"WAVE"
            int subChunk1Id = 544501094;                  //"fmt "
            int subChunk1Size = 16;
            short audioFormat = 1;
            short numChannels = (short)channels;
            int byteRate = sampleRate * numChannels * bitDepth / 8;
            short blockAlign = (short)(numChannels * bitDepth / 8);
            short bitsPerSample = bitDepth;
            int subChunk2Id = 1635017060;                 //"data"
            int subChunk2Size = payloadSize;

            fixed(byte *pHeader = &header[0])
            {            
                WriteInt32(chunkId, pHeader, 0);
                WriteInt32(chunkSize, pHeader, 4);
                WriteInt32(format, pHeader, 8);
                WriteInt32(subChunk1Id, pHeader, 12);
                WriteInt32(subChunk1Size, pHeader, 16);
                WriteInt16(audioFormat, pHeader, 20);
                WriteInt16(numChannels, pHeader, 22);
                WriteInt32(sampleRate, pHeader, 24);
                WriteInt32(byteRate, pHeader, 28);
                WriteInt16(blockAlign, pHeader, 32);
                WriteInt16(bitsPerSample, pHeader, 34);
                WriteInt32(subChunk2Id, pHeader, 36);
                WriteInt32(subChunk2Size, pHeader, 40);
            }

            using(FileStream stream = new FileStream(outputFilePath, FileMode.Create))
            {
                stream.Write(header, 0, header.Length);

                byte[] buffer = new byte[4096];
                int bytesRemaining = payloadSize;
                int index = 0;

                fixed(byte *pBuffer = &buffer[0])
                {
                    while(bytesRemaining > 0)
                    {
                        int bytesToWrite = Math.Min(buffer.Length, bytesRemaining);
                        int increment = sizeof(short);

                        for(int i = 0; i < bytesToWrite; i+=increment)
                        {
                            WriteInt16((short)(data[index] * short.MaxValue), pBuffer, i);
                            index++;
                        }

                        stream.Write(buffer, 0, bytesToWrite);
                        bytesRemaining -= bytesToWrite;
                    }
                }
            }
        }

        private static void WriteInt16(short value, byte *buffer, int offset)
        {
            byte* src = (byte*)&value;
            Buffer.MemoryCopy(src, (buffer+offset), sizeof(short), sizeof(short));
        }

        private static void WriteInt32(int value, byte *buffer, int offset)
        {
            byte* src = (byte*)&value;
            Buffer.MemoryCopy(src, (buffer+offset), sizeof(int), sizeof(int));
        }
    }