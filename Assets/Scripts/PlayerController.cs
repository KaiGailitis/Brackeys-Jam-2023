using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private TrailRenderer trailRenderer;
    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [Header("WallCheck")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private float _inputX;
    private float _moveSpeed = 5.0f;
    private float _playerJumpPower = 10.0f;
    private bool _isFacingRight;
    private float _facingDirection;

    private bool _isWallSliding = false;
    private float _wallSlidingSpeed = 1.0f;

    [Header("Dash")]
    private bool _canDash = true;
    private bool _isDashing;
    private float _dashingPower = 15.0f;
    private float _dashingTime = 0.2f;
    private float _dashingCooldown = 3.0f;

    [Header("Gravity")]
    private float _gravityScale = 2.0f;
    private float _fallingGravityScale = 3.0f;

    private float _apexSpeedModifier = 1.2f;

    [Header("Coyote Time")]
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _coyoteTimeCounter;

    [Header("JumpBuffer")]
    [SerializeField] private float _jumpBufferTime = 0.15f;
    [SerializeField] private float _jumpBufferCounter;

    private bool _jumpedFromWall;
    private bool _isWallJumping;
    private float _wallJumpingDirection;
    private float _wallJumpingTime = 0.2f;
    private float _wallJumpingCounter;
    private float _wallJumpingDuration = 0.4f;
    private Vector2 _wallJumpingPower = new Vector2(2f, 10f);

    void Start()
    {
        
    }

    void Update()
    {
        if (_isDashing)
        {
            return;
        }

        if (!_isWallJumping)
        {
            rigidBody.velocity = new Vector2(_inputX * _moveSpeed, rigidBody.velocity.y);
        }

        // Coyote Timer
        if (IsGrounded())
        {
            _coyoteTimeCounter = _coyoteTime;
            _jumpedFromWall = false;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
        {
            StartCoroutine(Dash());
        }

        // Jump Buffer
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
        else 
        {
            _jumpBufferCounter += Time.deltaTime;
        }

        // Gravity Change At Peak Of Jump
        if (rigidBody.velocity.y >= 0)
        {
            rigidBody.gravityScale = _gravityScale;
        }
        if (rigidBody.velocity.y < 0 && !IsGrounded())
        {
            rigidBody.gravityScale = _fallingGravityScale;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x * _apexSpeedModifier, rigidBody.velocity.y);
        }

        // Player Sprite Flipping
        if (!_isFacingRight && !_isWallJumping && _inputX > 0f)
        {
            Flip();
        }
        else if (_isFacingRight && !_isWallJumping && _inputX < 0f)
        {
            Flip();
        }

        WallSlide();

        WallJump();
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            return;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer)
            || (_jumpedFromWall && Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer));
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && _inputX != 0f)
        {
            _isWallSliding = true;
            _coyoteTimeCounter = 0;
            _jumpBufferCounter = 0;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Clamp(rigidBody.velocity.y, -_wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            _isWallSliding = false;
        }
    }

    private IEnumerator WallJumpCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        _canDash = true;
        _jumpBufferCounter = 0;
    }

    private void WallJump()
    {
        if (_isWallSliding)
        {
            _canDash = false;
            _isWallJumping = false;
            _wallJumpingDirection = -transform.localScale.x;
            _wallJumpingCounter = _wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            _wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && _wallJumpingCounter > 0f && _jumpBufferCounter < 0.15f)
        {
            _isWallJumping = true;
            rigidBody.AddForce(new Vector2(_wallJumpingDirection * _wallJumpingPower.x, _wallJumpingPower.y), ForceMode2D.Impulse);
            _wallJumpingCounter = 0f;
            _jumpBufferCounter = 0f;
            _canDash = false;
            StartCoroutine(WallJumpCooldown());

            if (transform.localScale.x == _wallJumpingDirection)
            {
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            _jumpedFromWall = true;

            Invoke(nameof(StopWallJumping), _wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        _isWallJumping = false;
    }

    private void Flip()
    {
        Vector3 localscale = transform.localScale;
        localscale.x *= -1f;
        transform.localScale = localscale;

        if (localscale.x < 0f)
        {
            _isFacingRight = false;
            _facingDirection = -1f;
        }
        else
        {
            _isFacingRight = true;
            _facingDirection = 1f;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && _coyoteTimeCounter > 0f && !_isWallJumping && !_isDashing)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, _playerJumpPower);
            _jumpBufferCounter = 0;
        }

        if (context.canceled && rigidBody.velocity.y > 0f && !_isWallJumping && !_isDashing)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f);
            _coyoteTimeCounter = 0f;
        }
    }

    public void Move(InputAction.CallbackContext context)
    { 
        _inputX = context.ReadValue<Vector2>().x;
    }

    private IEnumerator Dash()
    {
        if (_isWallJumping || _isWallSliding && _canDash && !_isDashing)
        {
            Debug.Log("WallDash");
            _canDash = false;
            _isDashing = true;
            float originalGravity = rigidBody.gravityScale;
            rigidBody.gravityScale = 0.0f;
            rigidBody.velocity = new Vector2(_facingDirection * -_dashingPower, 0f);
            trailRenderer.emitting = true;
            yield return new WaitForSeconds(_dashingTime);
            trailRenderer.emitting = false;
            rigidBody.gravityScale = originalGravity;
            _isDashing = false;
            yield return new WaitForSeconds(_dashingCooldown);
            _canDash = true;
        }
        else if (_canDash && !_isDashing)
        {
            Debug.Log("Dash");
            _canDash = false;
            _isDashing = true;
            float originalGravity = rigidBody.gravityScale;
            rigidBody.gravityScale = 0.0f;
            rigidBody.velocity = new Vector2(_facingDirection * _dashingPower, 0f);
            trailRenderer.emitting= true;
            yield return new WaitForSeconds(_dashingTime);
            trailRenderer.emitting = false;
            rigidBody.gravityScale = originalGravity;
            _isDashing = false;
            yield return new WaitForSeconds(_dashingCooldown);
            _canDash= true;
        }
    }
}
