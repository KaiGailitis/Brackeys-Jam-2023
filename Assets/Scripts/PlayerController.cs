using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private float _inputX;
    private float _moveSpeed = 5.0f;
    private float _playerJumpPower = 10.0f;
    private bool _isFacingRight;

    private float _gravityScale = 2.0f;
    private float _fallingGravityScale = 4.0f;

    [Header("Coyote Time")]
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _coyoteTimeCounter;

    void Start()
    {
        
    }

    void Update()
    {
        rigidBody.velocity = new Vector2(_inputX * _moveSpeed, rigidBody.velocity.y);

        if (IsGrounded())
        {
            _coyoteTimeCounter = _coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (rigidBody.velocity.y >= 0)
        {
            rigidBody.gravityScale = _gravityScale;
        }
        if (rigidBody.velocity.y < 0 && IsGrounded())
        {
            rigidBody.gravityScale = _fallingGravityScale;
        }

        if (!_isFacingRight && _inputX > 0f)
        {
            Flip();
        }
        else if (_isFacingRight && _inputX < 0f)
        {
            Flip();
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        _isFacingRight = false;
        Vector3 localscale = transform.localScale;
        localscale.x *= -1f;
        transform.localScale = localscale;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && _coyoteTimeCounter > 0f)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, _playerJumpPower);
        }

        if (context.canceled && rigidBody.velocity.y > 0f)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f);
            _coyoteTimeCounter = 0f;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        _inputX = context.ReadValue<Vector2>().x;
    }
}
