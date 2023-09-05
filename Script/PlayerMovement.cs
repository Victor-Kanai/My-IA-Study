using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;
    Vector3 movementDirection;

    int speed = 5;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementDirection = new Vector3(horizontal, 0, vertical).normalized;
        movementDirection = transform.TransformDirection(movementDirection);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            print(horizontal);
            print(vertical);
        }

        if (!controller.isGrounded)
        {
            movementDirection.y = -1;
        }

        controller.Move(movementDirection * speed * Time.deltaTime);
    }
}
