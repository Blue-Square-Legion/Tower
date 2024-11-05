using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 playerMovementInput;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed;
    [SerializeField] private float sensitivity;

    private void Update()
    {
        playerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (playerMovementInput.magnitude > 0)
        {
            // Convert the player's movement to be relative to the camera's rotation
            Vector3 forward = playerCamera.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = playerCamera.right;
            right.y = 0f;
            right.Normalize();

            Vector3 moveDirection = forward * playerMovementInput.z + right * playerMovementInput.x;

            characterController.Move(moveDirection * speed * Time.deltaTime);
        }
    }
}