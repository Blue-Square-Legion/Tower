using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 5f;

    private float currentXRotation = 0f;
    private float currentYRotation = 0f;

    // Set rotation limits
    public float minXRotation = -20f;
    public float maxXRotation = 60f;

    void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            currentYRotation += mouseX;
            currentXRotation -= mouseY;

            currentXRotation = Mathf.Clamp(currentXRotation, minXRotation, maxXRotation);
        }

        Quaternion rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0);

        Vector3 desiredPosition = player.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.rotation = rotation;

        transform.LookAt(player);
    }
}