#if UNITY_INPUT_SYSTEM_ENABLE_UI
using UnityEngine.EventSystems;

namespace UnityEngine.InputSystem.UI
{
    internal struct NavigationModel
    {
        public Vector2 move;
        public int consecutiveMoveCount;
        public MoveDirection lastMoveDirection;
        public float lastMoveTime;
        public AxisEventData eventData;

        public void Reset()
        {
            move = Vector2.zero;
        }
    }
}
#endif
