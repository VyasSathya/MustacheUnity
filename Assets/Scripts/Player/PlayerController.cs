using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed of the player
    public float rotationSpeed = 1500f; // Rotation speed for mouse responsiveness

    private Rigidbody rb;
    private Animator animator;

    private float moveX;
    private float moveZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            Debug.LogError("Rigidbody component missing from the player object.");
        }

        if (animator == null)
        {
            Debug.LogError("Animator component missing from the player object.");
        }
    }

    void Update()
    {
        // Get input for movement along X (horizontal) and Z (vertical) axes
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");

        // Normalize the input to ensure diagonal movements have consistent speed
        Vector3 inputVector = new Vector3(moveX, 0f, moveZ);
        inputVector = Vector3.ClampMagnitude(inputVector, 1f);
        moveX = inputVector.x;
        moveZ = inputVector.z;

        // Update animator parameters for movement animations
        if (animator != null)
        {
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveZ", moveZ);
        }

        // Rotate the character horizontally with mouse input
        if (Input.GetMouseButton(1)) // Right-click held down
        {
            RotateCharacterWithMouse();
        }
    }

    void FixedUpdate()
    {
        // Calculate movement vector in local space
        Vector3 movement = transform.right * moveX + transform.forward * moveZ;
        movement = movement.normalized * moveSpeed * Time.fixedDeltaTime;

        // Move the player only if there is input to avoid sliding
        if (movement.magnitude > 0)
        {
            if (rb != null)
            {
                Vector3 targetPosition = transform.position + movement;
                rb.MovePosition(targetPosition);
            }
        }
    }

    void RotateCharacterWithMouse()
    {
        // Get mouse movement along the X-axis
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

        // Rotate the character around the Y-axis based on mouse movement
        transform.Rotate(0, mouseX, 0);
    }
}
