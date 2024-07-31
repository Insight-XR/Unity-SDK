using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace InsightDesk
{
    public class ObjectEntry
    {
        public uint instanceId;
        public ushort prefabId;
        public ushort parentPrefabId; // New field to store parent prefab ID
        public bool activeInHierarchy;
        public bool newPos;
        public bool newRot;
        public bool newScale;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
        public List<(int, float)> newAnimationFloats = new List<(int, float)>();
        public List<(int, int)> newAnimationInts = new List<(int, int)>();
        public List<(int, bool)> newAnimationBools = new List<(int, bool)>();
        public List<(uint, string, float)> newTexts = new List<(uint, string, float)>();
        public List<int> newAnimationTriggers = new List<int>(); // New list for triggers
        public List<(string, float)> newLeftHandBendOffsets = new List<(string, float)>();
        public List<(string, float)> newRightHandBendOffsets = new List<(string, float)>();

        TrackingManagerWorker trackingManagerWorker;

        public ObjectEntry(ObjectEntry insightObjectEntry)
        {
            instanceId = insightObjectEntry.instanceId;
            prefabId = insightObjectEntry.prefabId;
            parentPrefabId = insightObjectEntry.parentPrefabId; // Copy the parent prefab ID
            activeInHierarchy = insightObjectEntry.activeInHierarchy;
            newPos = insightObjectEntry.newPos;
            newRot = insightObjectEntry.newRot;
            newScale = insightObjectEntry.newScale;

            position = insightObjectEntry.position;
            rotation = insightObjectEntry.rotation;
            localScale = insightObjectEntry.localScale;
            newAnimationFloats = new List<(int, float)>(insightObjectEntry.newAnimationFloats);
            newAnimationInts = new List<(int, int)>(insightObjectEntry.newAnimationInts);
            newAnimationBools = new List<(int, bool)>(insightObjectEntry.newAnimationBools);
            newAnimationTriggers = new List<int>(insightObjectEntry.newAnimationTriggers); // Copy triggers
            newTexts = new List<(uint, string, float)>(insightObjectEntry.newTexts);
            newLeftHandBendOffsets = new List<(string, float)>(insightObjectEntry.newLeftHandBendOffsets);
            newRightHandBendOffsets = new List<(string, float)>(insightObjectEntry.newRightHandBendOffsets);
        }

        public static void Write(InsightBuffer buffer, uint instanceId, ushort prefabId, ushort parentPrefabId,
            InsightTrackedObjectData insightTrackedObjectData,
            bool newPos, bool newRot, bool newScale, List<(int, float)> newFloats, List<(int, int)> newInts,
            List<(int, bool)> newBools, List<int> newTriggers, List<(uint, string)> newTexts, float textSize,
            List<(string, float)> newLeftHandBendOffsets, List<(string, float)> newRightHandBendOffsets, TrackingManager.InsightLogLevel logLevel, bool isAutoHands)
        {
            var activeInHierarchy = insightTrackedObjectData.activeInHierarchy;

            ushort flags = 0;

            flags |= (ushort)((activeInHierarchy ? 1 : 0) << 0);

            var hasAnimationParameters = newFloats.Count > 0 || newInts.Count > 0 || newBools.Count > 0 || newTriggers.Count > 0;
            flags |= (ushort)((hasAnimationParameters ? 1 : 0) << 1);

            flags |= (ushort)((newPos ? 1 : 0) << 2);
            flags |= (ushort)((newRot ? 1 : 0) << 3);
            flags |= (ushort)((newScale ? 1 : 0) << 4);

            var hasTexts = newTexts.Count > 0;
            flags |= (ushort)((hasTexts ? 1 : 0) << 5);

            var hasLeftHandBendOffsets = newLeftHandBendOffsets.Count > 0 && isAutoHands;
            flags |= (ushort)((hasLeftHandBendOffsets ? 1 : 0) << 6);

            var hasRightHandBendOffsets = newRightHandBendOffsets.Count > 0 && isAutoHands;
            flags |= (ushort)((hasRightHandBendOffsets ? 1 : 0) << 7);

            var position = insightTrackedObjectData.position;
            var rotation = insightTrackedObjectData.rotation;
            var localScale = insightTrackedObjectData.localScale;

            var defaultPos = position == Vector3.zero;
            var defaultRot = rotation == Quaternion.identity;
            var defaultScale = localScale == Vector3.one;
            flags |= (ushort)((defaultPos ? 1 : 0) << 8);
            flags |= (ushort)((defaultRot ? 1 : 0) << 9);
            flags |= (ushort)((defaultScale ? 1 : 0) << 10);
            buffer.Write(flags);
            buffer.Write(instanceId);
            buffer.Write(prefabId);
            buffer.Write(parentPrefabId); // Write the parent prefab ID

            //Debug.Log($"Writing FPS: {fps} for Object ID: {instanceId}");

            if (!activeInHierarchy)
            {
                return;
            }

            if (newPos && !defaultPos)
            {
                buffer.Write(position.x);
                buffer.Write(position.y);
                buffer.Write(position.z);
            }

            if (newRot && !defaultRot)
            {
                buffer.Write(rotation.x);
                buffer.Write(rotation.y);
                buffer.Write(rotation.z);
                buffer.Write(rotation.w);
            }

            if (newScale && !defaultScale)
            {
                buffer.Write(localScale.x);
                buffer.Write(localScale.y);
                buffer.Write(localScale.z);
            }

            if (hasAnimationParameters)
            {
                byte animationParametersFlags = 0;
                animationParametersFlags |= (byte)((newFloats.Count > 0 ? 1 : 0) << 0);
                animationParametersFlags |= (byte)((newInts.Count > 0 ? 1 : 0) << 1);
                animationParametersFlags |= (byte)((newBools.Count > 0 ? 1 : 0) << 2);
                animationParametersFlags |= (byte)((newTriggers.Count > 0 ? 1 : 0) << 3);
                buffer.Write(animationParametersFlags);

                if ((newFloats.Count > byte.MaxValue || newInts.Count > byte.MaxValue ||
                     newBools.Count > byte.MaxValue) && logLevel >= TrackingManager.InsightLogLevel.Warning)
                {
                    InsightUtility.LogWarning(
                        $"Insight Tracker only supports up to {byte.MaxValue} of each animation parameter.");
                }

                if (newFloats.Count > 0)
                {
                    byte floatsLength = newFloats.Count > byte.MaxValue
                        ? byte.MaxValue
                        : (byte)newFloats.Count;
                    buffer.Write(floatsLength);
                    for (int i = 0; i < floatsLength; i++)
                    {
                        var parameter = newFloats[i];
                        buffer.Write(parameter.Item1);
                        buffer.Write(parameter.Item2);
                    }
                }

                if (newInts.Count > 0)
                {
                    byte intsLength = newInts.Count > byte.MaxValue
                        ? byte.MaxValue
                        : (byte)newInts.Count;
                    buffer.Write(intsLength);
                    for (int i = 0; i < intsLength; i++)
                    {
                        var parameter = newInts[i];
                        buffer.Write(parameter.Item1);
                        buffer.Write(parameter.Item2);
                    }
                }

                if (newBools.Count > 0)
                {
                    byte boolsLength = newBools.Count > byte.MaxValue
                        ? byte.MaxValue
                        : (byte)newBools.Count;
                    buffer.Write(boolsLength);
                    for (int i = 0; i < boolsLength; i++)
                    {
                        var parameter = newBools[i];
                        buffer.Write(parameter.Item1);
                        buffer.Write(parameter.Item2);
                    }
                }
                if (newTriggers.Count > 0)
                {
                    byte triggersLength = newTriggers.Count > byte.MaxValue
                        ? byte.MaxValue
                        : (byte)newTriggers.Count;
                    buffer.Write(triggersLength);
                    for (int i = 0; i < triggersLength; i++)
                    {
                        buffer.Write(newTriggers[i]);
                    }
                }
            }

            if (hasTexts)
            {
                byte textsLength = newTexts.Count > byte.MaxValue
                    ? byte.MaxValue
                    : (byte)newTexts.Count;
                buffer.Write(textsLength);
                for (int i = 0; i < textsLength; i++)
                {
                    var text = newTexts[i];
                    buffer.Write(text.Item1);
                    buffer.Write(text.Item2);
                    buffer.Write(textSize);
                    // Debug.Log("yoyo " + textSize);
                }
            }

            if (hasLeftHandBendOffsets && isAutoHands)
            {
                byte leftHandBendOffsetsLength = newLeftHandBendOffsets.Count > byte.MaxValue
                    ? byte.MaxValue
                    : (byte)newLeftHandBendOffsets.Count;
                buffer.Write(leftHandBendOffsetsLength);
                for (int i = 0; i < leftHandBendOffsetsLength; i++)
                {
                    var offset = newLeftHandBendOffsets[i];
                    buffer.Write(offset.Item1);
                    buffer.Write(offset.Item2);
                }
            }

            if (hasRightHandBendOffsets && isAutoHands)
            {
                byte rightHandBendOffsetsLength = newRightHandBendOffsets.Count > byte.MaxValue
                    ? byte.MaxValue
                    : (byte)newRightHandBendOffsets.Count;
                buffer.Write(rightHandBendOffsetsLength);
                for (int i = 0; i < rightHandBendOffsetsLength; i++)
                {
                    var offset = newRightHandBendOffsets[i];
                    buffer.Write(offset.Item1);
                    buffer.Write(offset.Item2);
                }
            }

        }


        public ObjectEntry(BinaryReader binaryReader)
        {
            try
            {
                ushort flags = binaryReader.ReadUInt16();
                activeInHierarchy = (flags & (1 << 0)) != 0;
                var hasAnimationParameters = (flags & (1 << 1)) != 0;
                var hasTexts = (flags & (1 << 5)) != 0;
                var hasLeftHandBendOffsets = (flags & (1 << 6)) != 0;
                var hasRightHandBendOffsets = (flags & (1 << 7)) != 0;

                newPos = (flags & (1 << 2)) != 0;
                newRot = (flags & (1 << 3)) != 0;
                newScale = (flags & (1 << 4)) != 0;

                var defaultPos = (flags & (1 << 8)) != 0;
                var defaultRot = (flags & (1 << 9)) != 0;
                var defaultScale = (flags & (1 << 10)) != 0;
                instanceId = binaryReader.ReadUInt32();
                prefabId = binaryReader.ReadUInt16();
                parentPrefabId = binaryReader.ReadUInt16();

                if (!activeInHierarchy)
                {
                    position = Vector3.zero;
                    rotation = Quaternion.identity;
                    localScale = Vector3.one;
                    return;
                }

                if (defaultPos || !newPos)
                {
                    position = Vector3.zero;
                }
                else
                {
                    position = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                }

                if (defaultRot || !newRot)
                {
                    rotation = Quaternion.identity;
                }
                else
                {
                    rotation = new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                }

                if (defaultScale || !newScale)
                {
                    localScale = Vector3.one;
                }
                else
                {
                    localScale = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                }

                if (hasAnimationParameters)
                {
                    var animationParametersFlags = binaryReader.ReadByte();
                    var hasFloats = (animationParametersFlags & (1 << 0)) != 0;
                    var hasInts = (animationParametersFlags & (1 << 1)) != 0;
                    var hasBools = (animationParametersFlags & (1 << 2)) != 0;
                    var hasTriggers = (animationParametersFlags & (1 << 3)) != 0;

                    if (hasFloats)
                    {
                        var numFloats = binaryReader.ReadByte();
                        for (int i = 0; i < numFloats; i++)
                        {
                            newAnimationFloats.Add((binaryReader.ReadInt32(), binaryReader.ReadSingle()));
                        }
                    }

                    if (hasInts)
                    {
                        var numInts = binaryReader.ReadByte();
                        for (int i = 0; i < numInts; i++)
                        {
                            newAnimationInts.Add((binaryReader.ReadInt32(), binaryReader.ReadInt32()));
                        }
                    }

                    if (hasBools)
                    {
                        var numBools = binaryReader.ReadByte();
                        for (int i = 0; i < numBools; i++)
                        {
                            newAnimationBools.Add((binaryReader.ReadInt32(), binaryReader.ReadBoolean()));
                        }
                    }
                    if (hasTriggers)
                    {
                        var numTriggers = binaryReader.ReadByte();
                        for (int i = 0; i < numTriggers; i++)
                        {
                            newAnimationTriggers.Add(binaryReader.ReadInt32());
                        }
                    }
                }

                if (hasTexts)
                {
                    var numTexts = binaryReader.ReadByte();
                    for (int i = 0; i < numTexts; i++)
                    {
                        var id = binaryReader.ReadUInt32();
                        var text = binaryReader.ReadString();
                        var textSize = binaryReader.ReadSingle();
                        // Debug.Log("yoyo " + textSize);
                        newTexts.Add((id, text, textSize));
                    }
                }

                if (hasLeftHandBendOffsets)
                {
                    var numLeftHandBendOffsets = binaryReader.ReadByte();
                    for (int i = 0; i < numLeftHandBendOffsets; i++)
                    {
                        newLeftHandBendOffsets.Add((binaryReader.ReadString(), binaryReader.ReadSingle()));
                    }
                }

                if (hasRightHandBendOffsets)
                {
                    var numRightHandBendOffsets = binaryReader.ReadByte();
                    for (int i = 0; i < numRightHandBendOffsets; i++)
                    {
                        newRightHandBendOffsets.Add((binaryReader.ReadString(), binaryReader.ReadSingle()));
                    }
                }
            }
            catch (EndOfStreamException ex)
            {
                Debug.LogError("End of stream reached unexpectedly: " + ex.Message);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception in ObjectEntry constructor: {e.Message}");
                throw;
            }
        }

    }
}