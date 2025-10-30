using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private int _jumpCount;
    private int _maxJumps = 2;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool _isDashing = false;
    private float _dashEndTime = 0f;
    private float _lastDashTime = -999f;
    private int _facingDirection = 1; // 1 = right, -1 = left

    private float _lastAPressedTime = float.NegativeInfinity;
    private float _lastDPressedTime = float.NegativeInfinity;

    [Header("Attack Settings")]
    public bool isMeleeAttack;
    [Range(1,3)] public float attackSpeed = 1;

    private PlayerHealth _playerHealth;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _animator = GetComponentInChildren<Animator>();
        _playerHealth = GetComponent<PlayerHealth>();

        _jumpCount = _maxJumps;

        if (_rb == null) Debug.LogError("Missing Rigidbody2D!");
        if (_animator == null) Debug.LogError("Missing Animator!");
    }

    void Update()
    {
        // Prevent input while knocked back or dashing
        if (_playerHealth != null && _playerHealth.IsKnockedBack())
            return;

        if (!_isDashing)
        {
            HandleMovement();
            HandleJumping();
        }

        FlipPlayerSprite();
        HandleDash();
        DoAttack();
        SetAnimation();
    }

    void HandleMovement()
    {
        float _moveInput = 0f;
        float currentSpeed = SmartMovement(ref _moveInput);

        _rb.linearVelocity = new Vector2(_moveInput * currentSpeed, _rb.linearVelocity.y);

        FlipPlayerSprite(_moveInput);

        if (_moveInput != 0)
            _facingDirection = (int)Mathf.Sign(_moveInput);
    }

    private float SmartMovement(ref float _moveInput)
    {
        float currentSpeed = moveSpeed;

        if (Input.GetKeyDown(KeyCode.A)) _lastAPressedTime = Time.time;
        if (Input.GetKeyDown(KeyCode.D)) _lastDPressedTime = Time.time;

        bool aHeld = Input.GetKey(KeyCode.A);
        bool dHeld = Input.GetKey(KeyCode.D);

        if (aHeld && dHeld)
            _moveInput = (_lastDPressedTime > _lastAPressedTime) ? 1f : -1f;
        else if (aHeld)
            _moveInput = -1f;
        else if (dHeld)
            _moveInput = 1f;

        return currentSpeed;
    }

    private void HandleDash()
    {
        if (!_isDashing && Input.GetButtonDown("Fire3") && Time.time >= _lastDashTime + dashCooldown)
        {
            if (Input.GetKey(KeyCode.A)) _facingDirection = -1;
            else if (Input.GetKey(KeyCode.D)) _facingDirection = 1;

            StartCoroutine(Dash());
        }
    }

    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _jumpCount > 0)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _jumpCount--;
        }
    }

    private IEnumerator Dash()
    {
        _isDashing = true;
        _lastDashTime = Time.time;
        _dashEndTime = Time.time + dashDuration;

        while (Time.time < _dashEndTime)
        {
            _rb.linearVelocity = new Vector2(_facingDirection * dashSpeed, _rb.linearVelocity.y);
            yield return null;
        }

        _isDashing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            _jumpCount = _maxJumps;
    }

    private void FlipPlayerSprite(float _moveInput)
    {
        if (_moveInput == 1 && _spriteRenderer.flipX)
            _spriteRenderer.flipX = false;
        else if (_moveInput == -1 && !_spriteRenderer.flipX)
            _spriteRenderer.flipX = true;
    }

    void FlipPlayerSprite()
    {
        if (Input.GetKeyDown(KeyCode.D) && _spriteRenderer.flipX)
            _spriteRenderer.flipX = false;
        if (Input.GetKeyDown(KeyCode.A) && !_spriteRenderer.flipX)
            _spriteRenderer.flipX = true;
    }

    private void DoAttack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _animator.SetTrigger("meleeAttack");
            isMeleeAttack = true;
        }
    }

    private void SetAnimation()
    {
        _animator.SetBool("isMeleeAttack", isMeleeAttack);
        _animator.SetFloat("attackSpeed", attackSpeed);
    }
}
