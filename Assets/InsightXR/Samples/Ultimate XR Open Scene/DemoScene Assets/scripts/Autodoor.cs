using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    public GameObject leftDoor;
    public GameObject rightDoor;
    public Transform player;

    public float detectionRange = 3f;
    public float smoothSpeed = 2f;

    private Vector3 initialLeftDoorPosition;
    private Vector3 initialRightDoorPosition;
    private Vector3 targetLeftDoorPosition;
    private Vector3 targetRightDoorPosition;
    private bool isOpening = false;
    private bool isClosing = false;

    void Start()
    {
        initialLeftDoorPosition = leftDoor.transform.localPosition;
        initialRightDoorPosition = rightDoor.transform.localPosition;
        UpdateTargetPositions();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, player.transform.position.y,transform.position.z), player.position);

        if (distanceToPlayer <= detectionRange && !isOpening)
        {
            isOpening = true;
            isClosing = false;
            UpdateTargetPositions();
        }
        else if (distanceToPlayer > detectionRange && !isClosing)
        {
            isClosing = true;
            isOpening = false;
            UpdateTargetPositions();
        }

        if (isOpening)
        {
            OpenDoor();
        }
        else if (isClosing)
        {
            CloseDoor();
        }
    }

    void UpdateTargetPositions()
    {
        float doorOffset = 2f; // You can adjust this offset as needed
        targetLeftDoorPosition = isOpening ? initialLeftDoorPosition + Vector3.right * doorOffset : initialLeftDoorPosition;
        targetRightDoorPosition = isOpening ? initialRightDoorPosition - Vector3.right * doorOffset : initialRightDoorPosition;
    }

    void OpenDoor()
    {
        float step = smoothSpeed * Time.deltaTime;
        leftDoor.transform.localPosition = Vector3.MoveTowards(leftDoor.transform.localPosition, targetLeftDoorPosition, step);
        rightDoor.transform.localPosition = Vector3.MoveTowards(rightDoor.transform.localPosition, targetRightDoorPosition, step);

        if (Vector3.Distance(leftDoor.transform.localPosition, targetLeftDoorPosition) < 0.01f &&
            Vector3.Distance(rightDoor.transform.localPosition, targetRightDoorPosition) < 0.01f)
        {
            isOpening = false;
        }
    }

    void CloseDoor()
    {
        float step = smoothSpeed * Time.deltaTime;
        leftDoor.transform.localPosition = Vector3.MoveTowards(leftDoor.transform.localPosition, initialLeftDoorPosition, step);
        rightDoor.transform.localPosition = Vector3.MoveTowards(rightDoor.transform.localPosition, initialRightDoorPosition, step);

        if (Vector3.Distance(leftDoor.transform.localPosition, initialLeftDoorPosition) < 0.01f &&
            Vector3.Distance(rightDoor.transform.localPosition, initialRightDoorPosition) < 0.01f)
        {
            isClosing = false;
        }
    }
}
