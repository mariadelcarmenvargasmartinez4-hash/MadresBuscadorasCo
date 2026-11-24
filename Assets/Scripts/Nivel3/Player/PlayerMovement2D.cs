using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Controlador 2D de movimiento para la protagonista (Madre Buscadora).
    /// Compatible con New Input System, Legacy Input y Gamepad.
    /// Incluye caminar, sigilo [C / Ctrl], carrera rápida con energía [Shift]
    /// e impulso de esquive [Espacio] mediante física Rigidbody2D continua.
    /// </summary>
    public class PlayerMovement2D : MonoBehaviour
    {
        [Header("Velocidades")]
        [SerializeField] private float walkSpeed = 4.8f;
        [SerializeField] private float sneakSpeed = 2.6f;
        [SerializeField] private float sprintSpeed = 8.4f;

        [Header("Energía para Correr y Esquive")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 14f;
        [SerializeField] private float staminaRecoverWalk = 24f;
        [SerializeField] private float staminaRecoverIdle = 42f;
        [SerializeField] private float dashSpeed = 13.5f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 1.0f;
        [SerializeField] private float dashStaminaCost = 18f;

        [Header("Referencias")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private PlayerNoiseEmitter noiseEmitter;

        private Rigidbody2D rb;
        private Vector2 moveInput = Vector2.zero;
        private Vector2 lastMoveDir = Vector2.right;
        private bool isSneaking = false;
        private bool isSprinting = false;
        private bool isHidden = false;
        private bool canMove = true;

        private float currentStamina = 100f;
        private bool sprintExhausted = false;
        private float dashTimeRemaining = 0f;
        private float dashCooldownRemaining = 0f;

        public bool IsHidden => isHidden;
        public bool IsSneaking => isSneaking;
        public bool IsSprinting => isSprinting;
        public bool IsDashing => dashTimeRemaining > 0f;
        public bool CanMove => canMove;
        public bool IsMoving => (moveInput.sqrMagnitude > 0.01f || IsDashing) && canMove && !isHidden;
        public Vector2 MoveDirection => moveInput;
        public float StaminaNormalized => maxStamina > 0f ? Mathf.Clamp01(currentStamina / maxStamina) : 1f;

        public event Action<float, bool> OnStaminaChanged;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.linearDamping = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;

            currentStamina = maxStamina;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (noiseEmitter == null)
                noiseEmitter = GetComponent<PlayerNoiseEmitter>();
            if (GetComponent<PlayerCombat2D>() == null)
                gameObject.AddComponent<PlayerCombat2D>();
        }

        private void Start()
        {
            OnStaminaChanged?.Invoke(StaminaNormalized, false);
        }

        private void Update()
        {
            if (!canMove)
            {
                moveInput = Vector2.zero;
                isSprinting = false;
                dashTimeRemaining = 0f;
                return;
            }

            ReadInputUniversal(out bool dashPressed);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveDir = moveInput.normalized;
            }

            if (dashCooldownRemaining > 0f)
                dashCooldownRemaining -= Time.deltaTime;

            if (dashTimeRemaining > 0f)
                dashTimeRemaining -= Time.deltaTime;

            if (dashPressed && dashCooldownRemaining <= 0f && !isHidden && currentStamina >= dashStaminaCost)
            {
                dashTimeRemaining = dashDuration;
                dashCooldownRemaining = dashCooldown;
                currentStamina = Mathf.Max(0f, currentStamina - dashStaminaCost);
                isSneaking = false;
            }

            UpdateStamina();

            // Girar sprite hacia la dirección de movimiento
            if (spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.05f)
            {
                spriteRenderer.flipX = moveInput.x < 0;
            }

            // Emitir sonido para enemigos acústicos (correr genera más ruido que sigilo)
            if (noiseEmitter != null)
            {
                noiseEmitter.EmitMovementNoise(IsMoving, isSneaking, isSprinting || IsDashing);
            }
        }

        private void FixedUpdate()
        {
            if (rb == null) return;

            if (!canMove || isHidden)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (IsDashing)
            {
                rb.linearVelocity = lastMoveDir * dashSpeed;
            }
            else if (moveInput.sqrMagnitude > 0.01f)
            {
                float currentSpeed = walkSpeed;
                if (isSneaking) currentSpeed = sneakSpeed;
                else if (isSprinting) currentSpeed = sprintSpeed;

                rb.linearVelocity = moveInput.normalized * currentSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Mantener dentro del perímetro de la hacienda sin atascos
            Vector2 clamped = rb.position;
            clamped.x = Mathf.Clamp(clamped.x, -16.5f, 205f);
            clamped.y = Mathf.Clamp(clamped.y, -3.9f, 1.8f);
            if (clamped != rb.position)
            {
                rb.position = clamped;
            }
        }

        private void UpdateStamina()
        {
            bool wantsSprint = isSprinting && moveInput.sqrMagnitude > 0.01f && !isHidden;

            if (wantsSprint && !sprintExhausted)
            {
                currentStamina = Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime);
                if (currentStamina <= 0f)
                {
                    sprintExhausted = true;
                    isSprinting = false;
                }
            }
            else
            {
                isSprinting = false;
                float recRate = moveInput.sqrMagnitude > 0.01f ? staminaRecoverWalk : staminaRecoverIdle;
                currentStamina = Mathf.Min(maxStamina, currentStamina + recRate * Time.deltaTime);
                if (sprintExhausted && currentStamina >= 25f)
                {
                    sprintExhausted = false;
                }
            }

            OnStaminaChanged?.Invoke(StaminaNormalized, isSprinting || IsDashing);
        }

        private void ReadInputUniversal(out bool dashPressed)
        {
            float horizontal = 0f;
            float vertical = 0f;
            bool sneakKey = false;
            bool sprintKey = false;
            dashPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

                sneakKey = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed;
                sprintKey = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
                dashPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            }

            if (Gamepad.current != null && Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.sqrMagnitude > 0.05f)
                {
                    horizontal = stick.x;
                    vertical = stick.y;
                }
                if (!sprintKey) sprintKey = Gamepad.current.leftTrigger.isPressed || Gamepad.current.buttonWest.isPressed;
                if (!dashPressed) dashPressed = Gamepad.current.buttonEast.wasPressedThisFrame;
            }
#endif

            // Fallback mediante Input clásico si Keyboard.current no entregó lectura
            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                try
                {
                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
                    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
                    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;

                    if (!sneakKey) sneakKey = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
                    if (!sprintKey) sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    if (!dashPressed) dashPressed = Input.GetKeyDown(KeyCode.Space);
                }
                catch { }
            }

            moveInput = new Vector2(horizontal, vertical);
            isSneaking = sneakKey;
            isSprinting = sprintKey && !sneakKey && !sprintExhausted;

            if (isHidden && moveInput.sqrMagnitude > 0.1f)
            {
                SetHiddenState(false);
            }
        }

        public void SetHiddenState(bool hidden)
        {
            isHidden = hidden;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = hidden ? 0.45f : 1.0f;
                spriteRenderer.color = c;
            }
        }

        public void SetMovementLocked(bool locked)
        {
            canMove = !locked;
            if (locked && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void RestoreFullStamina()
        {
            currentStamina = maxStamina;
            sprintExhausted = false;
            OnStaminaChanged?.Invoke(1f, false);
        }
    }
}
