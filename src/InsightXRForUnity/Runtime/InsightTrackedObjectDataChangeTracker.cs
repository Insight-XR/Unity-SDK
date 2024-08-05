using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class InsightTrackedObjectDataChangeTracker
    {
        private readonly Dictionary<uint, bool> _lastObjectActive = new Dictionary<uint, bool>();

        private readonly Dictionary<uint, InsightTrackedObjectData> _lastObjectData =
            new Dictionary<uint, InsightTrackedObjectData>();

        private readonly InsightObjectPool<InsightTrackedObjectData> _insightTrackedObjectDataPool =
            new InsightObjectPool<InsightTrackedObjectData>(() => new InsightTrackedObjectData(),
                TrackingManager.NumTrackedObjectsExpectedUpperEnd *
                TrackingManager.TicksInPipelineExpectedUpperEnd);

        private readonly Dictionary<uint, string> _lastObjectTextContent = new Dictionary<uint, string>();
        private readonly List<(uint, string)> newTexts = new List<(uint, string)>();
        private readonly List<(int, float)> newFloats = new List<(int, float)>();
        private readonly List<(int, int)> newInts = new List<(int, int)>();
        private readonly List<(int, bool)> newBools = new List<(int, bool)>();
        private readonly List<int> newTriggers = new List<int>(); // New list for triggers
        private readonly List<(string, float)> newLeftHandBendOffsets = new List<(string, float)>();
        private readonly List<(string, float)> newRightHandBendOffsets = new List<(string, float)>();

        public void RemoveLastDataFor(uint id)
        {
            if (_lastObjectActive.ContainsKey(id))
            {
                _lastObjectActive.Remove(id);
            }

            if (_lastObjectData.TryGetValue(id, out var insightTrackedObjectData))
            {
                _lastObjectActive.Remove(id);
                _insightTrackedObjectDataPool.Return(insightTrackedObjectData);
            }
        }

        public void ClearLastData()
        {
            _lastObjectActive.Clear();

            foreach (var data in _lastObjectData.Values)
            {
                _insightTrackedObjectDataPool.Return(data);
            }

            _lastObjectData.Clear();
        }

        public (bool newActive, bool newPos, bool newRot, bool newScale, List<(int, float)> newFloats,
        List<(int, int)> newInts, List<(int, bool)> newBools, List<int> newTriggers, List<(uint, string)> newTexts,
        List<(string, float)> newLeftHandBendOffsets, List<(string, float)> newRightHandBendOffsets, float textSize)
        GetTrackedObjectIsNew(InsightTrackedObjectData insightTrackedObjectData)
        {
            var instanceId = insightTrackedObjectData.instanceId;
            var newActive = true;
            if (_lastObjectActive.TryGetValue(instanceId, out var lastActive))
            {
                newActive = lastActive != insightTrackedObjectData.activeInHierarchy;

                _lastObjectActive[instanceId] = insightTrackedObjectData.activeInHierarchy;
            }
            else
            {
                _lastObjectActive.Add(instanceId, insightTrackedObjectData.activeInHierarchy);
            }

            var newPos = true;
            var newRot = true;
            var newScale = true;
            newFloats.Clear();
            newInts.Clear();
            newBools.Clear();
            newTexts.Clear();
            newTriggers.Clear(); // Clear the triggers list
            newLeftHandBendOffsets.Clear();
            newRightHandBendOffsets.Clear();

            if (insightTrackedObjectData.activeInHierarchy)
            {
                if (_lastObjectData.TryGetValue(instanceId, out var lastData))
                {
                    newPos = lastData.position != insightTrackedObjectData.position;
                    newRot = lastData.rotation != insightTrackedObjectData.rotation;
                    newScale = lastData.localScale != insightTrackedObjectData.localScale;

                    lastData.position = insightTrackedObjectData.position;
                    lastData.rotation = insightTrackedObjectData.rotation;
                    lastData.localScale = insightTrackedObjectData.localScale;

                    foreach (var pair in insightTrackedObjectData.animationFloats)
                    {
                        if (lastData.animationFloats.TryGetValue(pair.Key, out var lastValue))
                        {
                            if (lastValue != pair.Value)
                            {
                                newFloats.Add((pair.Key, pair.Value));
                                lastData.animationFloats[pair.Key] = pair.Value;
                            }
                        }
                        else
                        {
                            newFloats.Add((pair.Key, pair.Value));
                            lastData.animationFloats[pair.Key] = pair.Value;
                        }
                    }

                    foreach (var pair in insightTrackedObjectData.animationInts)
                    {
                        if (lastData.animationInts.TryGetValue(pair.Key, out var lastValue))
                        {
                            if (lastValue != pair.Value)
                            {
                                newInts.Add((pair.Key, pair.Value));
                                lastData.animationInts[pair.Key] = pair.Value;
                            }
                        }
                        else
                        {
                            newInts.Add((pair.Key, pair.Value));
                            lastData.animationInts[pair.Key] = pair.Value;
                        }
                    }

                    foreach (var pair in insightTrackedObjectData.animationBools)
                    {
                        if (lastData.animationBools.TryGetValue(pair.Key, out var lastValue))
                        {
                            if (lastValue != pair.Value)
                            {
                                newBools.Add((pair.Key, pair.Value));
                                lastData.animationBools[pair.Key] = pair.Value;
                            }
                        }
                        else
                        {
                            newBools.Add((pair.Key, pair.Value));
                            lastData.animationBools[pair.Key] = pair.Value;
                        }
                    }
                    foreach (var trigger in insightTrackedObjectData.animationTriggers)
                    {
                        if (!lastData.animationTriggers.Contains(trigger))
                        {
                            newTriggers.Add(trigger);
                            lastData.animationTriggers.Add(trigger);
                        }
                    }

                    foreach (var pair in insightTrackedObjectData.leftHandBendOffsets)
                    {
                        if (lastData.leftHandBendOffsets.TryGetValue(pair.Key, out var lastValue))
                        {
                            if (lastValue != pair.Value)
                            {
                                newLeftHandBendOffsets.Add((pair.Key, pair.Value));
                                lastData.leftHandBendOffsets[pair.Key] = pair.Value;
                            }
                        }
                        else
                        {
                            newLeftHandBendOffsets.Add((pair.Key, pair.Value));
                            lastData.leftHandBendOffsets[pair.Key] = pair.Value;
                        }
                    }

                    foreach (var pair in insightTrackedObjectData.rightHandBendOffsets)
                    {
                        if (lastData.rightHandBendOffsets.TryGetValue(pair.Key, out var lastValue))
                        {
                            if (lastValue != pair.Value)
                            {
                                newRightHandBendOffsets.Add((pair.Key, pair.Value));
                                lastData.rightHandBendOffsets[pair.Key] = pair.Value;
                            }
                        }
                        else
                        {
                            newRightHandBendOffsets.Add((pair.Key, pair.Value));
                            lastData.rightHandBendOffsets[pair.Key] = pair.Value;
                        }
                    }
                }
                else
                {
                    var newLastData = _insightTrackedObjectDataPool.Get().Init(insightTrackedObjectData);
                    _lastObjectData.Add(instanceId, newLastData);
                    foreach (var pair in newLastData.animationFloats)
                    {
                        newFloats.Add((pair.Key, pair.Value));
                    }

                    foreach (var pair in newLastData.animationInts)
                    {
                        newInts.Add((pair.Key, pair.Value));
                    }

                    foreach (var pair in newLastData.animationBools)
                    {
                        newBools.Add((pair.Key, pair.Value));
                    }
                    foreach (var trigger in newLastData.animationTriggers)
                    {
                        newTriggers.Add(trigger);
                    }

                    foreach (var pair in newLastData.leftHandBendOffsets)
                    {
                        newLeftHandBendOffsets.Add((pair.Key, pair.Value));
                    }

                    foreach (var pair in newLastData.rightHandBendOffsets)
                    {
                        newRightHandBendOffsets.Add((pair.Key, pair.Value));
                        //Debug.Log($"Right Hand BendOffset Key: {pair.Key}, Value: {pair.Value}");
                    }
                }
            }
            else
            {
                newPos = false;
                newRot = false;
                newScale = false;
            }
            if (insightTrackedObjectData.textContent != null)
            {
                if (_lastObjectTextContent.TryGetValue(instanceId, out var lastText))
                {
                    if (lastText != insightTrackedObjectData.textContent)
                    {
                        newTexts.Add((instanceId, insightTrackedObjectData.textContent));
                        _lastObjectTextContent[instanceId] = insightTrackedObjectData.textContent;
                        //Debug.Log(insightTrackedObjectData.textContent);
                    }
                }
                else
                {
                    newTexts.Add((instanceId, insightTrackedObjectData.textContent));
                    _lastObjectTextContent.Add(instanceId, insightTrackedObjectData.textContent);
                    //Debug.Log(insightTrackedObjectData.textContent);
                }
            }
            var textSize = insightTrackedObjectData.textSize;
            return (newActive, newPos, newRot, newScale, newFloats, newInts, newBools, newTriggers, newTexts,
                newLeftHandBendOffsets, newRightHandBendOffsets, textSize);
        }

    }
}
