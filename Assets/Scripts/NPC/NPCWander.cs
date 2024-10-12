using UnityEngine;

public class NPCWander : MonoBehaviour
{
    public float wanderRadius = 5f; // Radius within which the NPC can move
    public float moveSpeed = 2f; // Speed of the NPC
    public float changeDirectionTime = 3f; // Time in seconds before changing direction

    private Animator animator;
    private Vector3 targetPosition;
    private float timer;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetNewTargetPosition();
        timer = changeDirectionTime;

        if (animator == null)
        {
            Debug.LogError("Animator component missing from the NPC.");
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SetNewTargetPosition();
            timer = changeDirectionTime;
        }

        MoveTowardsTarget();
    }

    private void SetNewTargetPosition()
    {
        // Generate a random point within the wander radius
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        targetPosition = new Vector3(randomPoint.x + transform.position.x, transform.position.y, randomPoint.y + transform.position.z);
    }

    private void MoveTowardsTarget()
    {
        // Calculate the direction to move in
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Update Animator parameters to trigger movement animations in blend tree
        if (animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveZ", direction.z);
        }

        // Move the NPC towards the target
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
