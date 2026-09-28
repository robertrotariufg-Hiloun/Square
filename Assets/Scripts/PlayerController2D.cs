using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController2D : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb;
    private AudioManager audioManager;
    private LevelManager levelManager;

    [Header("Stats y Vida")]
    public float health = 100f;
    public float maxHealth = 100f;
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    private bool isInvulnerable;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 12f);
    [SerializeField] private float wallJumpDuration = 0.2f;
    [SerializeField] private bool invertSpriteOnWall = true;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private TrailRenderer dashTrail;

    [Header("Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.4f, 0.1f);
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.4f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Visuales")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("UI - Vida")]
    [SerializeField] private Image healthFrontFill;
    [SerializeField] private Image healthDelayedFill;
    [SerializeField] private float healthFrontLerpSpeed = 12f;
    [SerializeField] private float healthDelayedLerpSpeed = 2.5f;
    [SerializeField] private float healthDelayBeforeDrop = 0.5f;

    [Header("UI - Dash")]
    [SerializeField] private Image dashBarImage;
    [SerializeField] private Sprite[] dashBarFrames;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip doubleJumpSFX;
    [SerializeField] private AudioClip wallJumpSFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip deathSFX;

    private Vector2 moveInput;
    private float defaultGravity;
    private bool isFacingRight = true;
    private bool isJumpHeld;

    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool isDashing;
    private bool canDash = true;
    public float DashCooldownPercent { get; private set; } = 1f;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private int extraJumps;

    private float healthDelayTimer;
    private float lastHealthPercent = 1f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private bool leftPressed;
    private bool rightPressed;
    private float keyboardMoveX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        dashTrail = GetComponentInChildren<TrailRenderer>();

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        defaultGravity = rb.gravityScale;
        health = maxHealth;

        healthFrontFill.fillAmount = 1f;
        healthDelayedFill.fillAmount = 1f;
    }

    void OnEnable()
    {
        InputActionMap map = inputActions.FindActionMap("Player");

        moveAction = map.FindAction("Move");
        jumpAction = map.FindAction("Jump");
        dashAction = map.FindAction("Dash");

        moveAction.performed += OnMoveInput;
        moveAction.canceled += OnMoveInput;
        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;
        dashAction.performed += OnDashPerformed;

        map.Enable();
    }

    void OnDisable()
    {
        moveAction.performed -= OnMoveInput;
        moveAction.canceled -= OnMoveInput;
        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;
        dashAction.performed -= OnDashPerformed;

        inputActions.FindActionMap("Player").Disable();
    }

    private void OnMoveInput(InputAction.CallbackContext ctx)
    {
        bool pressed = ctx.ReadValueAsButton();

        if (ctx.control.name == "a")
        {
            leftPressed = pressed;
        }
        else if (ctx.control.name == "d")
        {
            rightPressed = pressed;
        }

        keyboardMoveX = (rightPressed ? 1f : 0f) - (leftPressed ? 1f : 0f);
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        isJumpHeld = true;
        NormalJump();
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        isJumpHeld = false;
    }

    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        if (canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void Update()
    {
        float horizontal = keyboardMoveX;

        var gamepad = Gamepad.current;

        if (gamepad != null)
        {
            float stickX = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.2f) horizontal = stickX;
            if (gamepad.dpad.left.isPressed) horizontal -= 1f;
            if (gamepad.dpad.right.isPressed) horizontal += 1f;

            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                isJumpHeld = true;
                NormalJump();
            }
            else if (gamepad.buttonSouth.wasReleasedThisFrame)
            {
                isJumpHeld = false;
            }

            if ((gamepad.rightTrigger.wasPressedThisFrame || gamepad.buttonWest.wasPressedThisFrame) && canDash && !isDashing)
            {
                StartCoroutine(DashRoutine());
            }
        }

        moveInput.x = Mathf.Clamp(horizontal, -1f, 1f);

        UpdateHealthBar();
        UpdateDashBar();

        if (isDashing) return;

        Vector3 gPos = groundCheck.position;
        Vector3 wPos = wallCheck.position;

        isGrounded = Physics2D.OverlapBox(gPos, groundCheckSize, 0f, groundLayer);
        isTouchingWall = Physics2D.OverlapBox(wPos, wallCheckSize, 0f, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            extraJumps = 1;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (Mathf.Abs(moveInput.x) > 0.1f && !isWallJumping)
        {
            Flip(moveInput.x);
        }

        int wallDir = isFacingRight ? 1 : -1;
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0.1f && (moveInput.x * wallDir > 0.1f))
        {
            isWallSliding = true;
            extraJumps = 1;
        }
        else
        {
            isWallSliding = false;
        }

        spriteRenderer.flipX = invertSpriteOnWall && (isWallSliding || (isTouchingWall && !isGrounded));

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        if (!isWallJumping)
        {
            float targetSpeed = moveInput.x * moveSpeed;
            float speedRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float movement = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, speedRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(movement, rb.linearVelocity.y);
        }

        if (isWallSliding)
        {
            rb.gravityScale = defaultGravity;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            return;
        }

        if (rb.linearVelocity.y < -0.1f)
        {
            rb.gravityScale = defaultGravity * fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0.1f && !isJumpHeld)
        {
            rb.gravityScale = defaultGravity * lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravity;
        }
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsTouchingWall", isTouchingWall);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetBool("IsDashing", isDashing);
    }

    private void UpdateHealthBar()
    {
        float targetPercent = health / maxHealth;

        healthFrontFill.fillAmount = Mathf.MoveTowards(healthFrontFill.fillAmount, targetPercent, healthFrontLerpSpeed * Time.deltaTime);

        if (targetPercent < lastHealthPercent)
        {
            healthDelayTimer = healthDelayBeforeDrop;
        }
        else
        {
            healthDelayedFill.fillAmount = targetPercent;
        }

        lastHealthPercent = targetPercent;

        if (healthDelayTimer > 0f)
        {
            healthDelayTimer -= Time.deltaTime;
        }
        else
        {
            healthDelayedFill.fillAmount = Mathf.MoveTowards(healthDelayedFill.fillAmount, targetPercent, healthDelayedLerpSpeed * Time.deltaTime);
        }
    }

    private void UpdateDashBar()
    {
        int index = Mathf.RoundToInt(Mathf.Clamp01(DashCooldownPercent) * (dashBarFrames.Length - 1));
        dashBarImage.sprite = dashBarFrames[index];
    }

    private void NormalJump()
    {
        jumpBufferTimer = jumpBufferTime;

        if (isWallSliding || (isTouchingWall && !isGrounded))
        {
            WallJump();
        }
        else if (coyoteTimer > 0f)
        {
            Jump(jumpForce);
            PlaySound(jumpSFX);
        }
        else if (extraJumps > 0 && !isGrounded)
        {
            extraJumps--;
            Jump(doubleJumpForce);
            PlaySound(doubleJumpSFX);
        }
    }

    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        animator.SetTrigger("Jump");
    }

    private void WallJump()
    {
        isWallJumping = true;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;

        float dir = isFacingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * wallJumpForce.x, wallJumpForce.y);

        Flip(dir);
        PlaySound(wallJumpSFX);
        animator.SetTrigger("WallJump");

        StopCoroutine(nameof(ResetWallJump));
        StartCoroutine(nameof(ResetWallJump));
    }

    private IEnumerator ResetWallJump()
    {
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        canDash = false;
        DashCooldownPercent = 0f;
        rb.gravityScale = 0f;
        animator.SetTrigger("Dash");

        float dir = Mathf.Abs(moveInput.x) > 0.1f ? Mathf.Sign(moveInput.x) : (isFacingRight ? 1f : -1f);
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);

        dashTrail.emitting = true;
        PlaySound(dashSFX);

        yield return new WaitForSeconds(dashDuration);

        dashTrail.emitting = false;
        rb.gravityScale = defaultGravity;
        isDashing = false;

        float cooldownElapsed = 0f;
        while (cooldownElapsed < dashCooldown)
        {
            cooldownElapsed += Time.deltaTime;
            DashCooldownPercent = cooldownElapsed / dashCooldown;
            yield return null;
        }

        DashCooldownPercent = 1f;
        canDash = true;
    }

    private void Flip(float horizontal)
    {
        if (horizontal > 0 && !isFacingRight || horizontal < 0 && isFacingRight)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        health -= damage;
        PlaySound(damageSFX);

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    public void Heal(float amount)
    {
        if (health >= maxHealth) return;
        health = Mathf.Min(health + amount, maxHealth);
    }

    public void Die()
    {
        health = 0;
        animator.SetTrigger("Die");
        PlaySound(deathSFX);
        levelManager.PlayerDied();
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        audioManager.PlaySFX(clip, transform.position, Random.Range(0.95f, 1.05f));
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gPos = groundCheck.position;
        Vector3 wPos = wallCheck.position;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(gPos, groundCheckSize);

        Gizmos.color = isTouchingWall ? Color.blue : Color.yellow;
        Gizmos.DrawWireCube(wPos, wallCheckSize);
    }
}