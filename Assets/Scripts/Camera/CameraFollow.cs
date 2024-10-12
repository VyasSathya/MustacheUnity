using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player's transform
    public Vector3 offset;   // Offset between camera and player
    public float smoothSpeed = 0.125f; // Adjust for smoothness of movement
    public float pitchSpeed = 300f; // Increased speed for camera pitch control
    public float rotationSpeed = 300f; // Increased speed for camera horizontal rotation
    public float minPitch = -30f; // Minimum pitch angle
    public float maxPitch = 60f; // Maximum pitch angle

    private float currentPitch = 0f; // Current pitch angle

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogError("Target (player) is not assigned in the CameraFollow script.");
            return;
        }

        // Rotate the camera up and down, and left and right when holding right mouse button
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * pitchSpeed * Time.deltaTime;

            // Update pitch (up and down) rotation and clamp it
            currentPitch = Mathf.Clamp(currentPitch - mouseY, minPitch, maxPitch);

            // Rotate target (left and right) for horizontal camera rotation
            target.Rotate(Vector3.up * mouseX);
        }

        // Set camera position behind the player with the given offset, respecting the current pitch
        Vector3 desiredPosition = target.position + Quaternion.Euler(currentPitch, target.eulerAngles.y, 0) * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Keep the camera looking at the player
        transform.LookAt(target.position + Vector3.up * 1.5f); // Adjust height to match player's head height
    }
}
