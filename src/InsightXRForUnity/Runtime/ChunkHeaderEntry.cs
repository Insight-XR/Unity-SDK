using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace InsightDesk
{
    public class ChunkHeaderEntry
    {
        public short endianness;
        public short version;
        public string appVersion;
        public short tickRate;
        public int numTicksInChunk;

        public static void Write(InsightBuffer buffer, short version, string appVersion, short tickRate,
            int numTicksInChunk)
        {
            short endianness = 1; // when read, if endianness is not 1, all reads need to be converted
            buffer.Write(endianness);

            buffer.Write(version);
            buffer.Write(appVersion);
            buffer.Write(tickRate);
            buffer.Write(numTicksInChunk);
        }

        public ChunkHeaderEntry(BinaryReader binaryReader)
        {
            endianness = binaryReader.ReadInt16();
            version = binaryReader.ReadInt16();
            appVersion = binaryReader.ReadString();
            tickRate = binaryReader.ReadInt16();
            numTicksInChunk = binaryReader.ReadInt32();
            // Debug log
            // Debug.Log($"Constructor - Endianness: {endianness}, Version: {version}, App Version: {appVersion}, Tick Rate: {tickRate}, Num Ticks In Chunk: {numTicksInChunk}");
        }

    }
}