using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LasHuellasDelSilencio.Enemies;
using LasHuellasDelSilencio.Interaction;
using LasHuellasDelSilencio.Systems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Controlador de combate cuerpo a cuerpo con pala/varilla.
    /// Permite golpear con Clic Izquierdo (orientado hacia el cursor) o teclas [Q], [J], [K], [X],
    /// dañando, empujando y eliminando enemigos hasta hacerlos desaparecer con botín.
    /// </summary>
    public class PlayerCombat2D : MonoBehaviour
    {
        [Header("Combate Cuerpo a Cuerpo")]
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private float attackRange = 1.15f;
        [SerializeField] private float hitRadius = 1.65f;
        [SerializeField] private float closeHitRadius = 1.45f;
        [SerializeField] private float attackCooldown = 0.32f;
        [SerializeField] private float swingDuration = 0.18f;

        [Header("Referencias")]
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private PlayerMovement2D movement;
        [SerializeField] private PlayerNoiseEmitter noiseEmitter;

        private float cooldownTimer = 0f;
        private GameObject slashObj;
        private SpriteRenderer slashRenderer;
        private Coroutine swingCoroutine;
        private AudioSource combatAudioSource;
        private AudioClip swingClip;
        private AudioClip impactClip;

        private void Awake()
        {
            if (playerSpriteRenderer == null) playerSpriteRenderer = GetComponent<SpriteRenderer>();
            if (movement == null) movement = GetComponent<PlayerMovement2D>();
            if (noiseEmitter == null) noiseEmitter = GetComponent<PlayerNoiseEmitter>();

            combatAudioSource = gameObject.AddComponent<AudioSource>();
            combatAudioSource.playOnAwake = false;
            combatAudioSource.spatialBlend = 0f;
            swingClip = CreateToneClip("SlashSfx", 240f, 110f, 0.11f, 0.25f);
            impactClip = CreateToneClip("HitSfx", 150f, 65f, 0.15f, 0.38f);

            BuildSlashEffect();
        }

        private void BuildSlashEffect()
        {
            slashObj = new GameObject("Efecto_Golpe_Pala");
            slashObj.transform.SetParent(transform);
            slashObj.transform.localPosition = new Vector3(1.1f, 0f, 0f);
            slashObj.transform.localScale = new Vector3(1.65f, 1.65f, 1f);

            slashRenderer = slashObj.AddComponent<SpriteRenderer>();
            Sprite slashSpr = SpriteHelper.LoadSprite("Efecto_Golpe_Pala");
            SpriteHelper.ConfigureRenderer(slashRenderer, slashSpr, 55);
            slashObj.SetActive(false);
        }

        private static AudioClip CreateToneClip(string name, float startFreq, float endFreq, float duration, float volume)
        {
            int sampleRate = 22050;
            int sampleCount = Mathf.Max(256, Mathf.RoundToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / sampleRate;
                float env = (1f - t) * (1f - t);
                samples[i] = Mathf.Sin(phase) * volume * env;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (movement != null && !movement.CanMove) return;
            if (cooldownTimer > 0f) return;

            bool attackKey = false;
            bool mouseClick = false;
            Vector2 pointerScreenPos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (
                Keyboard.current.vKey.wasPressedThisFrame ||
                Keyboard.current.qKey.wasPressedThisFrame ||
                Keyboard.current.jKey.wasPressedThisFrame ||
                Keyboard.current.kKey.wasPressedThisFrame ||
                Keyboard.current.xKey.wasPressedThisFrame))
            {
                attackKey = true;
            }
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                mouseClick = true;
                pointerScreenPos = Mouse.current.position.ReadValue();
            }
#else
            if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X))
            {
                attackKey = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                mouseClick = true;
                pointerScreenPos = Input.mousePosition;
            }
#endif

            if (mouseClick && IsPointerOverInteractiveUI(pointerScreenPos))
            {
                mouseClick = false;
            }

            if (mouseClick)
            {
                FaceMouseClickDirection(pointerScreenPos);
                PerformAttack();
            }
            else if (attackKey)
            {
                PerformAttack();
            }
        }

        private static bool IsPointerOverInteractiveUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var r in results)
            {
                if (r.gameObject == null) continue;
                if (r.gameObject.GetComponentInParent<Selectable>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void FaceMouseClickDirection(Vector2 screenPos)
        {
            if (playerSpriteRenderer == null || Camera.main == null) return;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
            float dx = worldPos.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.15f)
            {
                playerSpriteRenderer.flipX = dx < 0f;
            }
        }

        public void PerformAttack()
        {
            cooldownTimer = attackCooldown;
            bool facingLeft = playerSpriteRenderer != null && playerSpriteRenderer.flipX;
            Vector2 facingDir = facingLeft ? Vector2.left : Vector2.right;

            if (swingClip != null && combatAudioSource != null)
            {
                combatAudioSource.PlayOneShot(swingClip);
            }

            if (swingCoroutine != null) StopCoroutine(swingCoroutine);
            swingCoroutine = StartCoroutine(ShowSlashRoutine(facingLeft));

            HashSet<BaseEnemyAI> damagedSet = new HashSet<BaseEnemyAI>();
            Vector2 forwardOrigin = (Vector2)transform.position + facingDir * attackRange;

            CollectTargetsInCircle(forwardOrigin, hitRadius, damagedSet);
            CollectTargetsInCircle(transform.position, closeHitRadius, damagedSet);

            // Respaldo adicional por distancia directa para no fallar golpes cercanos
            foreach (BaseEnemyAI enemy in Object.FindObjectsByType<BaseEnemyAI>(FindObjectsSortMode.None))
            {
                if (enemy == null || enemy.IsDead || damagedSet.Contains(enemy)) continue;
                Vector2 delta = enemy.transform.position - transform.position;
                bool inCloseRange = delta.magnitude <= 1.8f;
                bool inForwardReach = Vector2.Distance(enemy.transform.position, forwardOrigin) <= 1.9f;
                if (inCloseRange || inForwardReach)
                {
                    damagedSet.Add(enemy);
                }
            }

            int hitsLanded = 0;
            foreach (BaseEnemyAI enemy in damagedSet)
            {
                if (enemy == null || enemy.IsDead) continue;

                Vector2 knockDir = ((Vector2)(enemy.transform.position - transform.position)).normalized;
                if (knockDir.sqrMagnitude < 0.01f) knockDir = facingDir;

                SpawnHitSpark(enemy.transform.position);
                enemy.TakeDamage(attackDamage, knockDir);
                hitsLanded++;
            }

            if (noiseEmitter != null)
            {
                noiseEmitter.EmitNoiseBurst(hitsLanded > 0 ? 7.5f : 3.5f);
            }

            if (hitsLanded > 0)
            {
                if (impactClip != null && combatAudioSource != null)
                {
                    combatAudioSource.PlayOneShot(impactClip);
                }
                CameraController2D cam = Object.FindAnyObjectByType<CameraController2D>();
                if (cam != null) cam.Shake(0.18f, 0.15f);
            }

            // Excavar o desenterrar a Laura y sitios cercanos con el golpe de pala
            TryDigNearbyTargets();
        }

        private void TryDigNearbyTargets()
        {
            LauraTreeGrave grave = Object.FindAnyObjectByType<LauraTreeGrave>();
            if (grave != null && grave.CanInteract())
            {
                float dist = Vector2.Distance(transform.position, grave.transform.position);
                if (dist <= 2.2f)
                {
                    grave.Interact(movement);
                    return;
                }
            }

            BuriedRemainsSite[] remains = Object.FindObjectsByType<BuriedRemainsSite>(FindObjectsSortMode.None);
            foreach (var rem in remains)
            {
                if (rem != null && rem.CanInteract())
                {
                    float dist = Vector2.Distance(transform.position, rem.transform.position);
                    if (dist <= 2.2f)
                    {
                        rem.Interact(movement);
                        return;
                    }
                }
            }

            DigSite[] sites = Object.FindObjectsByType<DigSite>(FindObjectsSortMode.None);
            foreach (var site in sites)
            {
                if (site != null && site.CanInteract())
                {
                    float dist = Vector2.Distance(transform.position, site.transform.position);
                    if (dist <= 2.0f)
                    {
                        site.Interact(movement);
                        return;
                    }
                }
            }
        }

        private static void CollectTargetsInCircle(Vector2 center, float radius, HashSet<BaseEnemyAI> targetSet)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                BaseEnemyAI enemy = hit.GetComponent<BaseEnemyAI>();
                if (enemy == null) enemy = hit.GetComponentInParent<BaseEnemyAI>();
                if (enemy != null && !enemy.IsDead)
                {
                    targetSet.Add(enemy);
                }
            }
        }

        private void SpawnHitSpark(Vector3 pos)
        {
            GameObject spark = new GameObject("Chispa_Impacto");
            spark.transform.position = pos + new Vector3(0f, 0.2f, 0f);
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.CreateProceduralColorSprite(new Color(1f, 0.92f, 0.45f, 1f), 20, 20);
            sr.sortingOrder = 58;
            StartCoroutine(AnimateSparkRoutine(spark, sr));
        }

        private static IEnumerator AnimateSparkRoutine(GameObject spark, SpriteRenderer sr)
        {
            float elapsed = 0f;
            float duration = 0.16f;
            while (elapsed < duration && spark != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float s = Mathf.Lerp(0.7f, 1.8f, t);
                spark.transform.localScale = new Vector3(s, s, 1f);
                if (sr != null) sr.color = new Color(1f, 0.85f, 0.3f, 1f - t);
                yield return null;
            }
            if (spark != null) Destroy(spark);
        }

        private IEnumerator ShowSlashRoutine(bool facingLeft)
        {
            if (slashObj == null || slashRenderer == null) yield break;

            float dir = facingLeft ? -1f : 1f;
            slashObj.transform.localPosition = new Vector3(dir * 1.15f, -0.05f, 0f);
            slashRenderer.flipX = facingLeft;
            slashRenderer.color = Color.white;
            slashObj.SetActive(true);

            float elapsed = 0f;
            while (elapsed < swingDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / swingDuration);
                slashObj.transform.localRotation = Quaternion.Euler(0f, 0f, dir * Mathf.Lerp(35f, -42f, t));
                slashRenderer.color = new Color(1f, 1f, 0.85f, 1f - t);
                yield return null;
            }

            slashObj.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            bool facingLeft = playerSpriteRenderer != null && playerSpriteRenderer.flipX;
            Vector2 dir = facingLeft ? Vector2.left : Vector2.right;
            Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.8f);
            Gizmos.DrawWireSphere((Vector2)transform.position + dir * attackRange, hitRadius);
        }
    }
}

