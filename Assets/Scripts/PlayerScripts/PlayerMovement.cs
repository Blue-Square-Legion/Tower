using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 playerMovementInput;

    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed;

    private void Update()
    {
        playerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 MoveVector = transform.TransformDirection(playerMovementInput);

        characterController.Move(MoveVector * speed * Time.deltaTime);
        characterController.Move(velocity * speed * Time.deltaTime);
    }
}