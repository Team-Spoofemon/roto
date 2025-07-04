using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed = 5f;
    public InputActionReference move;

    private Vector3 moveDirection;

    void Start()
    {
        move.action.Enable(); // Make sure input is enabled
    }

    void Update()
    {
        Vector2 input = move.action.ReadValue<Vector2>();
        moveDirection = new Vector3(input.x, 0f, input.y);
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.velocity.y,
            moveDirection.z * moveSpeed
        );
    }
}
