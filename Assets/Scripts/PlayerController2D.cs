using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Visuales y Particulas")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private ParticleSystem damageParticles;

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

    private float coyoteTimer;
    private float jumpBufferTimer;
    private int extraJumps;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        audioManager = Object.FindFirstObjectByType<AudioManager>();
        levelManager = Object.FindFirstObjectByType<LevelManager>();

        if (rb != null)
            defaultGravity = rb.gravityScale;

        health = maxHealth > 0 ? maxHealth : 100f;

        // Auto-assign checks if unassigned
        if (groundCheck == null)
        {
            Transform found = transform.Find("GR_Check") ?? transform.Find("GroundCheck") ?? transform.Find("groundCheck");
            if (found != null) groundCheck = found;
        }

        if (wallCheck == null)
        {
            Transform found = transform.Find("Wll_Check") ?? transform.Find("WallCheck") ?? transform.Find("wallCheck");
            if (found != null) wallCheck = found;
        }

        if (dashTrail == null)
        {
            dashTrail = GetComponentInChildren<TrailRenderer>();
        }

        // Safety defaults if Inspector had 0s or broken values
        if (moveSpeed <= 0.1f) moveSpeed = 8f;
        if (acceleration < 10f) acceleration = 50f;
        if (deceleration < 10f) deceleration = 50f;
        if (jumpForce <= 0.1f) jumpForce = 12f;
        if (doubleJumpForce <= 0.1f) doubleJumpForce = 10f;
        if (fallMultiplier <= 0.1f) fallMultiplier = 2.5f;
        if (lowJumpMultiplier < 1f) lowJumpMultiplier = 2f;
        if (coyoteTime <= 0.01f) coyoteTime = 0.15f;
        if (jumpBufferTime <= 0.01f) jumpBufferTime = 0.15f;
        if (wallSlideSpeed <= 0.1f) wallSlideSpeed = 2f;
        if (wallJumpForce == Vector2.zero) wallJumpForce = new Vector2(8f, 12f);
        if (wallJumpDuration > 1f || wallJumpDuration <= 0.01f) wallJumpDuration = 0.2f;
        if (dashSpeed <= 0.1f) dashSpeed = 16f;
        if (dashDuration > 1f || dashDuration <= 0.01f) dashDuration = 0.2f;
        if (dashCooldown > 5f || dashCooldown <= 0.01f) dashCooldown = 0.5f;
        if (invulnerabilityDuration <= 0.01f) invulnerabilityDuration = 1.5f;
        if (flashInterval > 1f || flashInterval <= 0.01f) flashInterval = 0.1f;
        if (groundCheckSize == Vector2.zero) groundCheckSize = new Vector2(0.4f, 0.2f);
        if (wallCheckSize == Vector2.zero) wallCheckSize = new Vector2(0.2f, 0.5f);

        // If no groundLayer selected, use Default and Ground layers
        if (groundLayer.value == 0)
        {
            int defaultLayer = LayerMask.NameToLayer("Default");
            int groundLayerIndex = LayerMask.NameToLayer("Ground");
            int mask = 0;
            if (defaultLayer != -1) mask |= (1 << defaultLayer);
            if (groundLayerIndex != -1) mask |= (1 << groundLayerIndex);
            if (mask != 0) groundLayer = mask;
        }
    }

    private bool receivedInputCallback = false;

    void Update()
    {
        float horizontal = 0f;

        #if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        var gamepad = UnityEngine.InputSystem.Gamepad.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                isJumpHeld = true;
                ProcessJump();
            }
            else if (keyboard.spaceKey.wasReleasedThisFrame || keyboard.wKey.wasReleasedThisFrame || keyboard.upArrowKey.wasReleasedThisFrame)
            {
                isJumpHeld = false;
            }

            if ((keyboard.leftShiftKey.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame) && canDash && !isDashing)
            {
                StartCoroutine(DashRoutine());
            }
        }

        if (gamepad != null)
        {
            float stickX = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.2f) horizontal = stickX;
            if (gamepad.dpad.left.isPressed) horizontal -= 1f;
            if (gamepad.dpad.right.isPressed) horizontal += 1f;

            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                isJumpHeld = true;
                ProcessJump();
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
        #endif

        #if ENABLE_LEGACY_INPUT_MANAGER
        float legacyX = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(legacyX) > 0.05f)
        {
            horizontal = legacyX;
        }

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            isJumpHeld = true;
            ProcessJump();
        }
        else if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            isJumpHeld = false;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K)) && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
        #endif

        if (!receivedInputCallback || Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(moveInput.x) > 0.01f)
        {
            moveInput.x = Mathf.Clamp(horizontal, -1f, 1f);
        }

        if (isDashing) return;

        Vector3 gPos = groundCheck != null ? groundCheck.position : transform.position - new Vector3(0, 0.5f, 0);
        Vector3 wPos = wallCheck != null ? wallCheck.position : transform.position + new Vector3(isFacingRight ? 0.3f : -0.3f, 0, 0);

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
        if (isTouchingWall && !isGrounded && rb != null && rb.linearVelocity.y < 0.1f && (moveInput.x * wallDir > 0.1f))
        {
            isWallSliding = true;
            extraJumps = 1;
        }
        else
        {
            isWallSliding = false;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDashing || rb == null) return;

        if (!isWallJumping)
        {
            float targetSpeed = moveInput.x * moveSpeed;
            float speedRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float movement = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, speedRate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(movement, rb.linearVelocity.y);
        }

        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
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
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetFloat("YVelocity", rb != null ? rb.linearVelocity.y : 0f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsTouchingWall", isTouchingWall);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetBool("IsDashing", isDashing);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        receivedInputCallback = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        receivedInputCallback = true;
    }

    public void OnJump(InputValue value)
    {
        isJumpHeld = value.isPressed;

        if (value.isPressed)
        {
            ProcessJump();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isJumpHeld = true;
            ProcessJump();
        }
        else if (context.canceled)
        {
            isJumpHeld = false;
        }
    }

    private void ProcessJump()
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

    public void OnDash(InputValue value)
    {
        if (value.isPressed && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private void Jump(float force)
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        if (jumpParticles != null) jumpParticles.Play();
        if (animator != null) animator.SetTrigger("Jump");
    }

    private void WallJump()
    {
        if (rb == null) return;
        isWallJumping = true;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;

        float dir = isFacingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * wallJumpForce.x, wallJumpForce.y);

        Flip(dir);
        if (jumpParticles != null) jumpParticles.Play();
        PlaySound(wallJumpSFX);
        if (animator != null) animator.SetTrigger("WallJump");

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
        if (rb != null) rb.gravityScale = 0f;

        float dir = Mathf.Abs(moveInput.x) > 0.1f ? Mathf.Sign(moveInput.x) : (isFacingRight ? 1f : -1f);
        if (rb != null) rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);

        if (dashTrail != null) dashTrail.emitting = true;
        if (dashParticles != null) dashParticles.Play();
        PlaySound(dashSFX);

        yield return new WaitForSeconds(dashDuration);

        if (dashTrail != null) dashTrail.emitting = false;
        if (rb != null) rb.gravityScale = defaultGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
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
        if (damageParticles != null) damageParticles.Play();
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
        health = Mathf.Min(health + amount, maxHealth);
    }

    public void Die()
    {
        health = 0;
        if (animator != null) animator.SetTrigger("Die");
        PlaySound(deathSFX);
        if (levelManager != null) levelManager.PlayerDied();
        gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        if (audioManager == null) audioManager = Object.FindFirstObjectByType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlaySFX(clip, transform.position, Random.Range(0.95f, 1.05f));
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;

        while (elapsed < invulnerabilityDuration)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 gPos = groundCheck != null ? groundCheck.position : transform.position - new Vector3(0, 0.5f, 0);
        Vector3 wPos = wallCheck != null ? wallCheck.position : transform.position + new Vector3(isFacingRight ? 0.3f : -0.3f, 0, 0);

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(gPos, groundCheckSize);

        Gizmos.color = isTouchingWall ? Color.blue : Color.yellow;
        Gizmos.DrawWireCube(wPos, wallCheckSize);
    }
}