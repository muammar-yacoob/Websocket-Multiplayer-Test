using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed = 6.0f;
    private float jumpSpeed = 8.0f;
    private float gravity = 9.8f*2;

    private Vector3 startPos;
    private Vector3 moveDirection;
    private CharacterController controller;

    private Camera cam;

    private void Start()
    {
        startPos = transform.position;
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            moveDirection = cam.transform.right * h + cam.transform.forward * v;
           //moveDirection = new Vector3(h 0, v);

            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
        if (transform.position.y < -50)
            transform.position = startPos;
    }


}