using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.EnhancedTouch;

namespace InsightDesk
{
    public class InsightTrackedObjectData
    {
        public uint instanceId;
        public ushort prefabId;
        public ushort parentPrefabId; // New field to store parent prefab ID
        public bool activeInHierarchy;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
        public string textContent;  // New field to store TextMeshPro text
        public float textSize;  // New field to store TextMeshPro text
        public string sceneName;    // New field to store the active scene name

        public Dictionary<int, float> animationFloats = new Dictionary<int, float>(10);
        public Dictionary<int, int> animationInts = new Dictionary<int, int>(10);
        public Dictionary<int, bool> animationBools = new Dictionary<int, bool>(10);
        public List<int> animationTriggers = new List<int>(10); // New list to track trigger parameters

        public Dictionary<string, float> leftHandBendOffsets = new Dictionary<string, float>(); // Add this dictionary
        public Dictionary<string, float> rightHandBendOffsets = new Dictionary<string, float>(); // Add this dictionary

        public InsightTrackedObjectData Init(uint instanceId, ushort prefabId, Transform transform,
            bool activeInHierarchy,
            Animator animator, string sceneName)
        {
            this.instanceId = instanceId;
            this.prefabId = prefabId;
            this.activeInHierarchy = activeInHierarchy;
            this.sceneName = sceneName;

            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;

            // Reset animation dictionaries
            animationFloats.Clear();
            animationInts.Clear();
            animationBools.Clear();
            animationTriggers.Clear();
            leftHandBendOffsets.Clear(); // Reset left hand bendOffsets
            rightHandBendOffsets.Clear(); // Reset right hand bendOffsets

            // Extract animation parameters if the animator is present
            if (animator && animator.runtimeAnimatorController)
            {
                var parameters = animator.parameters;
                for (int p = 0; p < parameters.Length; p++)
                {
                    var parameter = parameters[p];
                    switch (parameter.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            animationFloats.Add(parameter.nameHash, animator.GetFloat(parameter.nameHash));
                            break;
                        case AnimatorControllerParameterType.Int:
                            animationInts.Add(parameter.nameHash, animator.GetInteger(parameter.nameHash));
                            break;
                        case AnimatorControllerParameterType.Bool:
                            animationBools.Add(parameter.nameHash, animator.GetBool(parameter.nameHash));
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            if (animator.GetBool(parameter.nameHash)) // Assuming that a set trigger would be represented by a bool being true
                            {
                                animationTriggers.Add(parameter.nameHash);
                            }
                            break;
                    }
                }
            }

            // Extract text content if a TextMeshPro component is present
            TextMeshPro tmp3D = transform.GetComponent<TextMeshPro>();
            TextMeshProUGUI tmpUI = transform.GetComponent<TextMeshProUGUI>();
            if (tmp3D != null)
            {
                textContent = tmp3D.text;
                textSize = tmp3D.fontSize;
                parentPrefabId = GetParentPrefabId(transform.parent); // Extract parent prefab ID
            }
            else if (tmpUI != null)
            {
                textContent = tmpUI.text;
                textSize = tmpUI.fontSize;
                parentPrefabId = GetParentPrefabId(transform.parent); // Extract parent prefab ID
            }
            else
            {
                textContent = null; // No TextMeshPro component found
                textSize = 0;
                parentPrefabId = 0; // Default value for parent prefab ID
            }

            return this;
        }

        public InsightTrackedObjectData Init(InsightTrackedObjectData insightTrackedObjectData)
        {
            instanceId = insightTrackedObjectData.instanceId;
            prefabId = insightTrackedObjectData.prefabId;
            parentPrefabId = insightTrackedObjectData.parentPrefabId; // Copy the parent prefab ID
            activeInHierarchy = insightTrackedObjectData.activeInHierarchy;
            position = insightTrackedObjectData.position;
            rotation = insightTrackedObjectData.rotation;
            localScale = insightTrackedObjectData.localScale;
            textContent = insightTrackedObjectData.textContent;  // Copy the text content
            textSize = insightTrackedObjectData.textSize;
            sceneName = insightTrackedObjectData.sceneName;      // Copy the scene name

            // Reset and copy animation dictionaries
            animationFloats.Clear();
            animationInts.Clear();
            animationBools.Clear();
            animationTriggers.Clear();
            leftHandBendOffsets.Clear(); // Reset and copy left hand bendOffsets
            rightHandBendOffsets.Clear(); // Reset and copy right hand bendOffsets

            foreach (var pair in insightTrackedObjectData.animationFloats)
            {
                animationFloats.Add(pair.Key, pair.Value);
            }

            foreach (var pair in insightTrackedObjectData.animationInts)
            {
                animationInts.Add(pair.Key, pair.Value);
            }

            foreach (var pair in insightTrackedObjectData.animationBools)
            {
                animationBools.Add(pair.Key, pair.Value);
            }
            foreach (var trigger in insightTrackedObjectData.animationTriggers)
            {
                animationTriggers.Add(trigger);
            }


            foreach (var pair in insightTrackedObjectData.leftHandBendOffsets)
            {
                leftHandBendOffsets.Add(pair.Key, pair.Value);
                //Debug.Log($"Left Hand BendOffset Key: {pair.Key}, Value: {pair.Value}");
            }

            foreach (var pair in insightTrackedObjectData.rightHandBendOffsets)
            {
                rightHandBendOffsets.Add(pair.Key, pair.Value);
                //Debug.Log($"Right Hand BendOffset Key: {pair.Key}, Value: {pair.Value}"); 
            }

            return this;
        }


        private ushort GetParentPrefabId(Transform parentTransform)
        {
            while (parentTransform != null)
            {
                var parentObject = parentTransform.GetComponent<InsightTrackObject>(); // Using InsightTrackObject component
                if (parentObject != null)
                {
                    return parentObject.prefabId;
                }
                parentTransform = parentTransform.parent;
            }
            return 0; // Default value if no parent with prefabId is found
        }
    }
}
