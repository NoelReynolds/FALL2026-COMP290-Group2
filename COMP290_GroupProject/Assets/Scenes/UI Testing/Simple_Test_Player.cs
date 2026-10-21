using System;
using UnityEngine;

public class Simple_Test_Player : MonoBehaviour
{
    [SerializeField] public bool isPossessed = true;

    [SerializeField] private Vector2 movement;

    [SerializeField] private Rigidbody2D rb;

    private float speed = 5.0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.linearVelocity = movement * speed;
    }
}
