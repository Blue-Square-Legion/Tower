using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 playerMovementInput;
    private Vector2 playerMouseInput;
    private float xRot;
    private float yRot;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed;
    [SerializeField] private float sensitivity;
    [SerializeField] private float maxVerticalRange;
    private float verticalRotation;

    private void Update()
    {
        playerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"),0f, Input.GetAxisRaw("Vertical"));
        playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovePlayer();
        MovePlayerCamera();
    }

    private void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(playerMovementInput);

        if (Input.GetKey(KeyCode.Space)) {
            velocity.y = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity.y = -1f;
        }

        characterController.Move(MoveVector * speed * Time.deltaTime);
        characterController.Move(velocity * speed * Time.deltaTime);

        velocity.y = 0f;
    }

    private void MovePlayerCamera()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            //Rotates the camera left and right
            float xRotation = playerMouseInput.x * sensitivity * Time.deltaTime * 60;
            transform.Rotate(0, xRotation, 0);

            //Rotates the camera Up and Down
            verticalRotation -= playerMouseInput.y * sensitivity * Time.deltaTime * 60;

            //Clamps the rotation within the range
            verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalRange, maxVerticalRange);

            //Applies the vertical rotation
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
        else
            Cursor.lockState = CursorLockMode.None;
    }
}