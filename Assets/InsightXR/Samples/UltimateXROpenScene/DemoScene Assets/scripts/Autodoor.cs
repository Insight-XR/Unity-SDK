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
    private bool isOpen = false;
    private InsightXRAPI API;
    public string openmessage;
    public string closemessage;

    void Start()
    {
        API = FindObjectOfType<InsightXRAPI>();
        initialLeftDoorPosition = leftDoor.transform.localPosition;
        initialRightDoorPosition = rightDoor.transform.localPosition;
        UpdateTargetPositions();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(new Vector3(transform.position.x, player.transform.position.y, transform.position.z), player.position);

        if (distanceToPlayer <= detectionRange && !isOpen)
        {
            isOpen = true;
            UpdateTargetPositions();
            // Debug.Log("Door Opening");
            API.InsightLogEvent(openmessage);
        }
        else if (distanceToPlayer > detectionRange && isOpen)
        {
            isOpen = false;
            UpdateTargetPositions();
            // Debug.Log("Door Closing");
            API.InsightLogEvent(closemessage);
        }

        if (isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    void UpdateTargetPositions()
    {
        float doorOffset = 2f; // You can adjust this offset as needed
        targetLeftDoorPosition = isOpen ? initialLeftDoorPosition + Vector3.right * doorOffset : initialLeftDoorPosition;
        targetRightDoorPosition = isOpen ? initialRightDoorPosition - Vector3.right * doorOffset : initialRightDoorPosition;
    }

    void OpenDoor()
    {
        float step = smoothSpeed * Time.deltaTime;
        leftDoor.transform.localPosition = Vector3.MoveTowards(leftDoor.transform.localPosition, targetLeftDoorPosition, step);
        rightDoor.transform.localPosition = Vector3.MoveTowards(rightDoor.transform.localPosition, targetRightDoorPosition, step);
    }

    void CloseDoor()
    {
        float step = smoothSpeed * Time.deltaTime;
        leftDoor.transform.localPosition = Vector3.MoveTowards(leftDoor.transform.localPosition, initialLeftDoorPosition, step);
        rightDoor.transform.localPosition = Vector3.MoveTowards(rightDoor.transform.localPosition, initialRightDoorPosition, step);
    }
}
