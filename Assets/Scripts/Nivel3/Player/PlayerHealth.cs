using System;
using System.Collections;
using UnityEngine;

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Gestiona el sistema de vida de 3 corazones, daño, recuperación con botellas de agua
    /// y reinicio en el último punto de control según la sección 6 del documento de diseño.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Configuración de Corazones")]
        [SerializeField] private int maxHearts = 3;
        [SerializeField] private int currentHearts = 3;

        [Header("Invulnerabilidad tras Daño")]
        [SerializeField] private float invulnerabilityDuration = 1.4f;
        [SerializeField] private float flashInterval = 0.12f;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip damageHeartbeatSfx;
        [SerializeField] private AudioClip healWaterSfx;
        [SerializeField] private AudioClip defeatSfx;

        [Header("Referencias Visuales")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        public int CurrentHearts => currentHearts;
        public int MaxHearts => maxHearts;
        public bool IsInvulnerable => isInvulnerable;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDamaged;
        public event Action OnDied;

        private bool isInvulnerable = false;
        private bool isDead = false;

        private void Awake()
        {
            currentHearts = maxHearts;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHearts, maxHearts);
            StartCoroutine(SpawnProtectionRoutine(2.0f));
        }

        private IEnumerator SpawnProtectionRoutine(float duration)
        {
            isInvulnerable = true;
            yield return new WaitForSeconds(duration);
            if (!isDead)
            {
                isInvulnerable = false;
                if (spriteRenderer != null) spriteRenderer.enabled = true;
            }
        }

        /// <summary>
        /// Recibe un golpe de un enemigo o peligro, reduciendo 1 corazón.
        /// </summary>
        public void TakeDamage(int amount = 1)
        {
            if (isInvulnerable || isDead) return;

            currentHearts = Mathf.Max(0, currentHearts - amount);
            OnHealthChanged?.Invoke(currentHearts, maxHearts);
            OnDamaged?.Invoke();

            PlaySfx(damageHeartbeatSfx);

            if (currentHearts <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        /// <summary>
        /// Recupera un corazón mediante una botella de agua, sin superar el máximo de 3.
        /// </summary>
        /// <summary>
        /// Incrementa la capacidad de corazones y la vida actual (Recompensa de Acertijo).
        /// </summary>
        public void AddMaxHeart(int amount = 1)
        {
            if (isDead) return;
            maxHearts += amount;
            currentHearts += amount;
            OnHealthChanged?.Invoke(currentHearts, maxHearts);
            PlaySfx(healWaterSfx);
        }

        public bool Heal(int amount = 1)
        {
            if (isDead || currentHearts >= maxHearts) return false;

            currentHearts = Mathf.Min(maxHearts, currentHearts + amount);
            OnHealthChanged?.Invoke(currentHearts, maxHearts);
            PlaySfx(healWaterSfx);
            return true;
        }

        /// <summary>
        /// Ajusta la cantidad actual de corazones (usado tras arresto parcial o eventos especiales).
        /// </summary>
        public void SetHearts(int hearts)
        {
            if (isDead) return;
            currentHearts = Mathf.Clamp(hearts, 1, maxHearts);
            OnHealthChanged?.Invoke(currentHearts, maxHearts);
        }

        private void Die()
        {
            isDead = true;
            PlaySfx(defeatSfx);
            OnDied?.Invoke();

            if (TryGetComponent<PlayerMovement2D>(out var mover))
            {
                mover.SetMovementLocked(true);
            }

            // Notificar al Level3GameManager para mostrar pantalla de reinicio en punto de control
            if (Systems.Level3GameManager.Instance != null)
            {
                Systems.Level3GameManager.Instance.OnPlayerDefeated();
            }
        }

        /// <summary>
        /// Restaura la salud completa y reposiciona a la protagonista en el punto de control.
        /// </summary>
        public void RespawnAt(Vector3 spawnPosition)
        {
            Time.timeScale = 1f;
            StopAllCoroutines();
            transform.position = spawnPosition;
            currentHearts = maxHearts;
            isDead = false;
            isInvulnerable = false;

            if (TryGetComponent<PlayerMovement2D>(out var mover))
            {
                mover.SetMovementLocked(false);
                mover.SetHiddenState(false);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            OnHealthChanged?.Invoke(currentHearts, maxHearts);
            StartCoroutine(SpawnProtectionRoutine(2.0f));
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            float elapsed = 0f;

            while (elapsed < invulnerabilityDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            isInvulnerable = false;
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}

