using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class InsightTrackHandAnchor : MonoBehaviour
    {
        public enum Hand
        {
            None,
            Left,
            Right
        }

        [SerializeField]
        private bool isAutoHands;

        public bool IsAutoHands
        {
            get { return isAutoHands; }
            set { isAutoHands = value; }
        }

        public Hand hand = Hand.None;

        private static InsightTrackHandAnchor leftHandInstance;
        private static InsightTrackHandAnchor rightHandInstance;

        public static InsightTrackHandAnchor LeftHandInstance => leftHandInstance;
        public static InsightTrackHandAnchor RightHandInstance => rightHandInstance;

        private void Awake()
        {
            if (hand == Hand.Left)
            {
                leftHandInstance = this;
                SetLeftHandAnchor();
            }
            else if (hand == Hand.Right)
            {
                rightHandInstance = this;
                SetRightHandAnchor();
            }
            else if (TrackingManager.instance.logLevel >= TrackingManager.InsightLogLevel.Warning)
            {
                InsightUtility.LogWarning($"Please select hand type on {name}");
            }
        }

        private void SetLeftHandAnchor()
        {
            if (isAutoHands)
            {
                var bendableObjects = new List<GameObject>
                {
                    FindDeepChild(transform, "thumb_01")?.gameObject,
                    FindDeepChild(transform, "index_01")?.gameObject,
                    FindDeepChild(transform, "middle_01")?.gameObject,
                    FindDeepChild(transform, "ring_01")?.gameObject,
                    FindDeepChild(transform, "pinky_01")?.gameObject
                };

                TrackingManager.instance.Left_AutoHandsBendableObjects = bendableObjects;
            }
            TrackingManager.instance.leftHandAnchor = transform;
        }

        private void SetRightHandAnchor()
        {
            if (isAutoHands)
            {
                var bendableObjects = new List<GameObject>
                {
                    FindDeepChild(transform, "thumb_01")?.gameObject,
                    FindDeepChild(transform, "index_01")?.gameObject,
                    FindDeepChild(transform, "middle_01")?.gameObject,
                    FindDeepChild(transform, "ring_01")?.gameObject,
                    FindDeepChild(transform, "pinky_01")?.gameObject
                };

                TrackingManager.instance.Right_AutoHandsBendableObjects = bendableObjects;
            }
            TrackingManager.instance.rightHandAnchor = transform;
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var result = FindDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
