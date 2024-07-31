using System;
using System.IO;
using UnityEngine;

namespace InsightDesk
{
    public class TickEntry
    {
        public long timeTicks;
        public float unscaledTime;
        public float deltaTime;
        public float handleTickTime;
        public ushort numObjects;
        public ushort numDeleted;
        public string sceneName;
        public bool isEvent; // New field
        public string eventName; // New field
        public bool newSkybox; // New field
        public string skyboxName; // New field
        public bool newfps; // New field
        public int fpsnow; // New field

        public static void Write(InsightBuffer buffer, long timeTicks, float unscaledTime, float deltaTime,
            float handleTickTime, ushort numObjects, ushort numDeleted, bool newScene, string sceneName, bool isImmersion, bool isEvent, string eventName, bool newSkybox, string skyboxName, bool newfps, int fpsnow)
        {
            byte flags = 0;
            flags |= (byte)((newScene ? 1 : 0) << 0);
            flags |= (byte)((isImmersion ? 1 : 0) << 1);
            flags |= (byte)((isEvent ? 1 : 0) << 2); // New flag for isEvent
            flags |= (byte)((newSkybox ? 1 : 0) << 3); // New flag for newSkybox
            flags |= (byte)((newfps ? 1 : 0) << 4); // New flag for newfps

            buffer.Write(flags);
            buffer.Write(timeTicks);
            buffer.Write(unscaledTime);
            buffer.Write(deltaTime);
            buffer.Write(handleTickTime);
            buffer.Write(numObjects);
            buffer.Write(numDeleted);

            if (newScene)
            {
                buffer.Write(sceneName);
            }

            if (isEvent)
            {
                buffer.Write(eventName);
                // Debug.Log("event " + eventName);
            }

            if (newSkybox)
            {
                buffer.Write(skyboxName);
                // Debug.Log("skybox " + skyboxName);
            }

            if (newfps)
            {
                buffer.Write(fpsnow);
                // Debug.Log("FPS now from tickentry: " + fpsnow);
            }
        }

        public TickEntry(BinaryReader binaryReader)
        {
            byte flags = binaryReader.ReadByte();
            var newScene = (flags & (1 << 0)) != 0;
            var isImmersion = (flags & (1 << 1)) != 0;
            isEvent = (flags & (1 << 2)) != 0; // Read the isEvent flag
            newSkybox = (flags & (1 << 3)) != 0; // Read the newSkybox flag
            newfps = (flags & (1 << 4)) != 0; // Read the newfps flag

            timeTicks = binaryReader.ReadInt64();
            unscaledTime = binaryReader.ReadSingle();
            deltaTime = binaryReader.ReadSingle();
            handleTickTime = binaryReader.ReadSingle();
            numObjects = binaryReader.ReadUInt16();
            numDeleted = binaryReader.ReadUInt16();

            if (newScene)
            {
                sceneName = binaryReader.ReadString();
            }

            if (isEvent)
            {
                eventName = binaryReader.ReadString();
                // Debug.Log("event " + eventName);
            }

            if (newSkybox)
            {
                skyboxName = binaryReader.ReadString();
                // Debug.Log("skybox " + skyboxName);
            }

            if (newfps)
            {
                fpsnow = binaryReader.ReadInt32();
                // Debug.Log("FPS now: " + fpsnow);
            }
        }
    }
}
