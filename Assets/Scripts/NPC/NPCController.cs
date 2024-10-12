using UnityEngine;

public class NPCController : MonoBehaviour
{
    public float interactionDistance = 3f; // Adjust as needed
    public float interactionAngle = 45f;   // Degrees
    public GameObject player; // Assign this manually in the Inspector
    private DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = GetComponent<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager component missing from NPC.");
        }

        if (player == null)
        {
            Debug.LogError("Player reference is not assigned in the Inspector. Please assign it.");
        }
    }

    void Update()
    {
        if (player == null)
        {
            return; // Exit if player reference is missing
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsPlayerInRange() && IsPlayerFacingNPC())
            {
                EngageConversation("Hello! How can I help you today?");
            }
        }
    }

    private bool IsPlayerInRange()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        return distance <= interactionDistance;
    }

    private bool IsPlayerFacingNPC()
    {
        Vector3 directionToNPC = (transform.position - player.transform.position).normalized;
        float angle = Vector3.Angle(player.transform.forward, directionToNPC);
        return angle <= interactionAngle;
    }

    private void EngageConversation(string message)
    {
        Debug.Log(message); // Output the conversation to the Unity console for testing
    }
}
