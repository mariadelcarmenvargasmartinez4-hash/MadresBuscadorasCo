using System.Collections;
using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.Interaction;

namespace LasHuellasDelSilencio.Enemies
{
    public enum EnemyState
    {
        Patrol,
        Investigate,
        Chase,
        SearchLastKnown,
        ReturnToPatrol
    }

    /// <summary>
    /// Clase base para la inteligencia artificial de todos los enemigos
    /// (Vigilante, Perseguidor, Guardián Acústico y Policía).
    /// Maneja patrulla automática con o sin waypoints, cono de visión,
    /// alertas visuales y daño con retroceso al contacto.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BaseEnemyAI : MonoBehaviour
    {
        [Header("Visión y Detección")]
        [SerializeField] protected float viewDistance = 5.5f;
        [Range(30f, 180f)]
        [SerializeField] protected float fieldOfView = 75f;
        [SerializeField] protected LayerMask obstacleMask;
        [SerializeField] protected float detectionIncreaseRate = 85f;
        [SerializeField] protected float detectionDecreaseRate = 38f;

        [Header("Patrulla y Movimiento")]
        [SerializeField] protected float patrolSpeed = 2.0f;
        [SerializeField] protected float chaseSpeed = 3.8f;
        [SerializeField] protected Transform[] waypoints;
        [SerializeField] protected float waypointTolerance = 0.35f;
        [SerializeField] protected float waitAtWaypointTime = 1.2f;

        [Header("Combate / Daño y Vida")]
        [SerializeField] protected int maxHp = 3;
        [SerializeField] protected int damageAmount = 1;
        [SerializeField] protected float attackCooldown = 1.1f;
        [SerializeField] protected float contactDamageRadius = 1.85f;

        [Header("Referencias Visuales")]
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected Rigidbody2D rb;
        protected EnemyState currentState = EnemyState.Patrol;
        protected PlayerMovement2D targetPlayer;
        protected PlayerHealth targetHealth;

        protected int currentHp;
        protected bool isDead = false;
        protected float knockbackTimer = 0f;
        private float hitFlashTimer = 0f;
        private GameObject hpBarRoot;
        private SpriteRenderer hpFillRenderer;
        private Transform hpFillPivot;

        protected float currentDetection = 0f;
        protected int currentWaypointIndex = 0;
        protected float waypointTimer = 0f;
        protected float lastAttackTime = -10f;
        protected Vector2 facingDirection = Vector2.right;
        protected Vector3 lastKnownPosition;
        protected float searchTimer = 0f;

        private Vector3 spawnPosition;
        private float autoMinX;
        private float autoMaxX;
        private bool movingRightAuto = true;
        private Color baseSpriteColor = Color.white;
        private bool hasCapturedBaseColor = false;

        public EnemyState CurrentState => currentState;
        public float DetectionPercentage => Mathf.Clamp01(currentDetection / 100f);
        public bool IsDead => isDead;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                baseSpriteColor = spriteRenderer.color;
                hasCapturedBaseColor = true;
            }

            currentHp = Mathf.Max(1, maxHp);
            spawnPosition = transform.position;
            autoMinX = spawnPosition.x - 4.5f;
            autoMaxX = spawnPosition.x + 4.5f;

            targetPlayer = Object.FindAnyObjectByType<PlayerMovement2D>();
            if (targetPlayer != null)
                targetHealth = targetPlayer.GetComponent<PlayerHealth>();
        }

        protected virtual void Start()
        {
            currentHp = Mathf.Max(1, maxHp);
            BuildHealthBar();
            RefreshHealthBar();
        }

        public void SetPatrolBounds(float minX, float maxX)
        {
            autoMinX = Mathf.Min(minX, maxX);
            autoMaxX = Mathf.Max(minX, maxX);
        }

        protected virtual void Update()
        {
            if (isDead) return;

            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= Time.deltaTime;
            }

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
                UpdateAlertVisuals();
                return;
            }

            if (targetPlayer == null)
            {
                targetPlayer = Object.FindAnyObjectByType<PlayerMovement2D>();
                if (targetPlayer != null)
                    targetHealth = targetPlayer.GetComponent<PlayerHealth>();
                return;
            }

            CheckVision();
            ExecuteStateLogic();
            UpdateAlertVisuals();
            CheckProximityContactDamage();
        }

        protected virtual void UpdateAlertVisuals()
        {
            if (spriteRenderer == null || isDead) return;
            if (!hasCapturedBaseColor)
            {
                baseSpriteColor = spriteRenderer.color;
                hasCapturedBaseColor = true;
            }

            if (hitFlashTimer > 0f)
            {
                spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
                return;
            }

            if (this is PoliceOfficerAI) return;

            if (currentState == EnemyState.Chase)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 12f);
                spriteRenderer.color = Color.Lerp(baseSpriteColor, new Color(1f, 0.35f, 0.35f, 1f), 0.35f + 0.45f * pulse);
            }
            else if (currentState == EnemyState.Investigate || currentState == EnemyState.SearchLastKnown)
            {
                spriteRenderer.color = Color.Lerp(baseSpriteColor, new Color(1f, 0.85f, 0.3f, 1f), 0.45f);
            }
            else
            {
                spriteRenderer.color = baseSpriteColor;
            }
        }

        private void CheckProximityContactDamage()
        {
            if (this is PoliceOfficerAI || isDead || knockbackTimer > 0f) return;
            if (targetPlayer == null || targetHealth == null) return;
            if (targetPlayer.IsHidden || targetPlayer.IsDashing) return;

            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
            if (dist <= contactDamageRadius)
            {
                TryDamagePlayer(targetHealth);
            }
        }

        private void BuildHealthBar()
        {
            if (hpBarRoot != null) return;

            float scaleX = Mathf.Max(0.5f, Mathf.Abs(transform.localScale.x));
            float scaleY = Mathf.Max(0.5f, Mathf.Abs(transform.localScale.y));

            hpBarRoot = new GameObject("Barra_Vida_Enemigo");
            hpBarRoot.transform.SetParent(transform);
            hpBarRoot.transform.localPosition = new Vector3(0f, 1.72f / scaleY, 0f);
            hpBarRoot.transform.localScale = new Vector3(1.15f / scaleX, 1.15f / scaleY, 1f);

            Sprite whiteBar = SpriteHelper.CreateProceduralColorSprite(Color.white, 32, 5);

            GameObject bgObj = new GameObject("Fondo");
            bgObj.transform.SetParent(hpBarRoot.transform);
            bgObj.transform.localPosition = Vector3.zero;
            bgObj.transform.localScale = new Vector3(2.2f, 2.2f, 1f);
            SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = whiteBar;
            bgSr.color = new Color(0.08f, 0.06f, 0.06f, 0.88f);
            bgSr.sortingOrder = 40;

            GameObject pivotObj = new GameObject("FillPivot");
            pivotObj.transform.SetParent(hpBarRoot.transform);
            pivotObj.transform.localPosition = new Vector3(-0.52f, 0f, 0f);
            hpFillPivot = pivotObj.transform;

            GameObject fillObj = new GameObject("Relleno");
            fillObj.transform.SetParent(pivotObj.transform);
            fillObj.transform.localPosition = new Vector3(0.52f, 0f, 0f);
            fillObj.transform.localScale = new Vector3(2.05f, 1.65f, 1f);
            hpFillRenderer = fillObj.AddComponent<SpriteRenderer>();
            hpFillRenderer.sprite = whiteBar;
            hpFillRenderer.color = new Color(0.2f, 0.95f, 0.35f, 1f);
            hpFillRenderer.sortingOrder = 41;

            hpBarRoot.SetActive(!isDead);
        }

        private void RefreshHealthBar()
        {
            if (hpBarRoot == null) BuildHealthBar();
            if (hpBarRoot == null || hpFillPivot == null) return;

            hpBarRoot.SetActive(!isDead);
            float ratio = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;
            hpFillPivot.localScale = new Vector3(ratio, 1f, 1f);

            if (hpFillRenderer != null)
            {
                hpFillRenderer.color = Color.Lerp(new Color(0.95f, 0.2f, 0.2f, 1f), new Color(0.2f, 0.95f, 0.35f, 1f), ratio);
            }
        }

        public virtual void TakeDamage(int damage, Vector2 knockbackDir)
        {
            if (isDead) return;

            currentHp = Mathf.Max(0, currentHp - Mathf.Max(1, damage));
            hitFlashTimer = 0.16f;
            knockbackTimer = 0.28f;
            lastAttackTime = Time.time - attackCooldown * 0.35f;

            if (rb != null)
            {
                Vector2 dir = knockbackDir.sqrMagnitude > 0.01f ? knockbackDir.normalized : -facingDirection;
                rb.linearVelocity = dir * 8.0f;
            }

            RefreshHealthBar();

            if (currentHp <= 0)
            {
                Die();
                return;
            }

            if (targetPlayer != null)
            {
                lastKnownPosition = targetPlayer.transform.position;
                currentDetection = 100f;
                currentState = EnemyState.Chase;

                // Alertar a enemigos cercanos para atacar al jugador en grupo
                foreach (var nearby in Object.FindObjectsByType<BaseEnemyAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    if (nearby != null && !nearby.IsDead && nearby != this && Vector2.Distance(transform.position, nearby.transform.position) <= 7.0f)
                    {
                        nearby.AlertToPosition(transform.position);
                    }
                }
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            if (hpBarRoot != null) hpBarRoot.SetActive(false);
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            foreach (var col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.RegisterEnemyDefeated(gameObject.name.Replace("_", " "));
            }

            SpawnDeathPuffs();
            SpawnDefeatDrop();
            StartCoroutine(DeathFadeRoutine());
        }

        private void SpawnDeathPuffs()
        {
            Sprite puffSprite = SpriteHelper.CreateProceduralColorSprite(new Color(0.95f, 0.92f, 0.85f, 1f), 16, 16);
            for (int i = 0; i < 6; i++)
            {
                GameObject puff = new GameObject($"SmokePuff_{i}");
                Destroy(puff, 0.35f);
                puff.transform.position = transform.position + new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.35f, 0.45f), 0f);
                puff.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                SpriteRenderer sr = puff.AddComponent<SpriteRenderer>();
                sr.sprite = puffSprite;
                sr.sortingOrder = 45;

                Vector2 vel = new Vector2(Random.Range(-2.2f, 2.2f), Random.Range(0.5f, 2.4f));
                StartCoroutine(AnimatePuffRoutine(puff, sr, vel));
            }
        }

        private static IEnumerator AnimatePuffRoutine(GameObject puff, SpriteRenderer sr, Vector2 vel)
        {
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration && puff != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                puff.transform.position += (Vector3)(vel * Time.deltaTime);
                float sc = Mathf.Lerp(0.9f, 1.8f, t);
                puff.transform.localScale = new Vector3(sc, sc, 1f);
                if (sr != null) sr.color = new Color(1f, 0.92f, 0.78f, 1f - t);
                yield return null;
            }
            if (puff != null) Destroy(puff);
        }

        private void SpawnDefeatDrop()
        {
            bool dropsMedkit = (this is GuardianAI || this is PoliceOfficerAI);
            string spriteName = dropsMedkit ? "Botiquin_Auxilios" : "Manzana_Fruta";
            string itemLabel = dropsMedkit ? "Botiquín de Campo" : "Manzana Silvestre";
            HealthItemType type = dropsMedkit ? HealthItemType.FirstAidKit : HealthItemType.WildFruit;
            int hearts = dropsMedkit ? 2 : 1;

            GameObject drop = new GameObject($"Botin_{itemLabel}");
            drop.transform.position = transform.position;
            SpriteRenderer sr = drop.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(sr, SpriteHelper.LoadSprite(spriteName), 26);
            drop.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            CircleCollider2D triggerCol = drop.AddComponent<CircleCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 0.85f;

            HealthPickupItem pickup = drop.AddComponent<HealthPickupItem>();
            pickup.Configure(type, itemLabel, hearts, false);

            // 2. Objeto de persona perdida (huesos o ropa) que aparece al matarlo
            bool dropsBones = (Random.value > 0.5f);
            string missingSpriteName = dropsBones ? "Huesos_Restos" : "Ropa_Persona_Perdida";
            string missingLabel = dropsBones ? "Fragmento Óseo de Víctima" : "Prenda Textil de Persona Desaparecida";
            MissingPersonDropType missingType = dropsBones ? MissingPersonDropType.BonesRemains : MissingPersonDropType.ClothingEvidence;

            GameObject missingDrop = new GameObject($"Indicio_{missingLabel}");
            missingDrop.transform.position = transform.position + new Vector3(Random.Range(-0.4f, 0.4f), 0.15f, 0f);
            SpriteRenderer msr = missingDrop.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(msr, SpriteHelper.LoadSprite(missingSpriteName), 27);
            missingDrop.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            CircleCollider2D mCol = missingDrop.AddComponent<CircleCollider2D>();
            mCol.isTrigger = true;
            mCol.radius = 0.85f;

            MissingPersonDropItem mpItem = missingDrop.AddComponent<MissingPersonDropItem>();
            mpItem.Configure(missingType, missingLabel);
        }

        private IEnumerator DeathFadeRoutine()
        {
            float duration = 0.28f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Color c = spriteRenderer != null ? spriteRenderer.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(startScale, startScale * 0.2f, t);
                transform.rotation = Quaternion.Euler(0f, 0f, 90f * t);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f - t);
                }
                yield return null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            yield return new WaitForSeconds(0.05f);
            Destroy(gameObject);
        }

        protected virtual void CheckVision()
        {
            if (targetPlayer.IsHidden)
            {
                currentDetection = Mathf.Max(0f, currentDetection - detectionDecreaseRate * Time.deltaTime);
                if (currentState == EnemyState.Chase && currentDetection <= 0f)
                {
                    StartSearching();
                }
                return;
            }

            Vector2 dirToPlayer = (targetPlayer.transform.position - transform.position);
            float distToPlayer = dirToPlayer.magnitude;

            if (distToPlayer <= viewDistance)
            {
                float angle = Vector2.Angle(facingDirection, dirToPlayer.normalized);
                bool closeProximity = distToPlayer <= 2.2f;
                if (angle <= fieldOfView * 0.5f || closeProximity)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distToPlayer, obstacleMask);
                    if (hit.collider == null)
                    {
                        float rateMultiplier = closeProximity ? 1.8f : 1.0f;
                        currentDetection = Mathf.Min(100f, currentDetection + detectionIncreaseRate * rateMultiplier * Time.deltaTime);
                        lastKnownPosition = targetPlayer.transform.position;

                        if (currentDetection >= 100f && currentState != EnemyState.Chase)
                        {
                            OnPlayerDetected();
                        }
                        return;
                    }
                }
            }

            currentDetection = Mathf.Max(0f, currentDetection - detectionDecreaseRate * Time.deltaTime);
            if (currentState == EnemyState.Chase && currentDetection <= 0f)
            {
                StartSearching();
            }
        }

        protected virtual void OnPlayerDetected()
        {
            currentState = EnemyState.Chase;
            if (UI.HUDManager.Instance != null && !(this is PoliceOfficerAI))
            {
                UI.HUDManager.Instance.ShowTemporaryNotification($"⚠️ ¡{gameObject.name.Replace("_", " ")} te ha visto! ¡Corre con [Shift] o escóndete!");
            }
        }

        protected virtual void StartSearching()
        {
            currentState = EnemyState.SearchLastKnown;
            searchTimer = 2.8f;
        }

        protected virtual void ExecuteStateLogic()
        {
            switch (currentState)
            {
                case EnemyState.Patrol:
                    PatrolBehavior();
                    break;
                case EnemyState.Investigate:
                    InvestigateBehavior();
                    break;
                case EnemyState.Chase:
                    ChaseBehavior();
                    break;
                case EnemyState.SearchLastKnown:
                    SearchBehavior();
                    break;
                case EnemyState.ReturnToPatrol:
                    ReturnToPatrolBehavior();
                    break;
            }
        }

        protected virtual void PatrolBehavior()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                float targetX = movingRightAuto ? autoMaxX : autoMinX;
                Vector3 targetPos = new Vector3(targetX, spawnPosition.y, 0f);
                Vector2 autoDir = (targetPos - transform.position);

                if (Mathf.Abs(transform.position.x - targetX) <= waypointTolerance)
                {
                    rb.linearVelocity = Vector2.zero;
                    waypointTimer += Time.deltaTime;
                    if (waypointTimer >= waitAtWaypointTime)
                    {
                        waypointTimer = 0f;
                        movingRightAuto = !movingRightAuto;
                    }
                }
                else
                {
                    MoveInDirection(autoDir.normalized, patrolSpeed);
                }
                return;
            }

            Transform targetWp = waypoints[currentWaypointIndex];
            if (targetWp == null) return;
            Vector2 dir = (targetWp.position - transform.position);

            if (dir.magnitude <= waypointTolerance)
            {
                rb.linearVelocity = Vector2.zero;
                waypointTimer += Time.deltaTime;
                if (waypointTimer >= waitAtWaypointTime)
                {
                    waypointTimer = 0f;
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                }
            }
            else
            {
                MoveInDirection(dir.normalized, patrolSpeed);
            }
        }

        protected virtual void ChaseBehavior()
        {
            if (targetPlayer == null) return;

            Vector2 toPlayer = (targetPlayer.transform.position - transform.position);
            float dist = toPlayer.magnitude;
            Vector2 dir = toPlayer.normalized;

            if (dist <= contactDamageRadius)
            {
                TryDamagePlayer(targetHealth);
            }
            else
            {
                MoveInDirection(dir, chaseSpeed);
            }
        }

        protected virtual void InvestigateBehavior()
        {
            Vector2 dir = (lastKnownPosition - transform.position);
            if (dir.magnitude <= 0.45f)
            {
                StartSearching();
            }
            else
            {
                MoveInDirection(dir.normalized, patrolSpeed * 1.35f);
            }
        }

        protected virtual void SearchBehavior()
        {
            rb.linearVelocity = Vector2.zero;
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                currentState = EnemyState.ReturnToPatrol;
            }
        }

        protected virtual void ReturnToPatrolBehavior()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                Vector3 homePos = new Vector3(Mathf.Clamp(transform.position.x, autoMinX, autoMaxX), spawnPosition.y, 0f);
                Vector2 dirHome = (homePos - transform.position);
                if (dirHome.magnitude <= waypointTolerance)
                {
                    currentState = EnemyState.Patrol;
                }
                else
                {
                    MoveInDirection(dirHome.normalized, patrolSpeed);
                }
                return;
            }

            Transform targetWp = waypoints[currentWaypointIndex];
            if (targetWp == null)
            {
                currentState = EnemyState.Patrol;
                return;
            }
            Vector2 dir = (targetWp.position - transform.position);

            if (dir.magnitude <= waypointTolerance)
            {
                currentState = EnemyState.Patrol;
            }
            else
            {
                MoveInDirection(dir.normalized, patrolSpeed);
            }
        }

        protected void MoveInDirection(Vector2 direction, float speed)
        {
            rb.linearVelocity = direction * speed;
            if (direction.sqrMagnitude > 0.01f)
            {
                facingDirection = direction;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }

            Vector3 p = transform.position;
            if (p.y < -3.8f || p.y > 1.6f)
            {
                p.y = Mathf.Clamp(p.y, -3.8f, 1.6f);
                transform.position = p;
            }
        }

        private void TryDamagePlayer(PlayerHealth health)
        {
            if (health == null || health.IsInvulnerable || isDead) return;
            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            Vector2 dirToPlayer = targetPlayer != null
                ? (Vector2)(targetPlayer.transform.position - transform.position).normalized
                : facingDirection;

            // Embestida / lunge de ataque hacia el jugador
            if (rb != null)
            {
                rb.linearVelocity = dirToPlayer * 5.5f;
            }

            SpawnAttackSlashEffect(dirToPlayer);
            health.TakeDamage(damageAmount);

            // Retroceso al jugador
            if (targetPlayer != null && targetPlayer.TryGetComponent<Rigidbody2D>(out var prb))
            {
                prb.linearVelocity = dirToPlayer * 6.0f;
            }

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowTemporaryNotification($"💔 ¡{gameObject.name.Replace("_", " ")} te ha atacado (-{damageAmount} ♥)! ¡Aléjate corriendo con [Shift]!");
            }
        }

        private void SpawnAttackSlashEffect(Vector2 dir)
        {
            GameObject slashObj = new GameObject("EnemyAttackSlash");
            slashObj.transform.position = transform.position + (Vector3)(dir * 0.7f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            slashObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            slashObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();
            Sprite slashSprite = SpriteHelper.LoadSprite("Efecto_Ataque_Enemigo");
            if (slashSprite == null) slashSprite = SpriteHelper.LoadSprite("Efecto_Golpe_Pala");
            SpriteHelper.ConfigureRenderer(sr, slashSprite, 55);

            Destroy(slashObj, 0.22f);
        }

        public void AlertToPosition(Vector3 pos)
        {
            if (isDead) return;
            lastKnownPosition = pos;
            currentDetection = 100f;
            currentState = EnemyState.Chase;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (this is PoliceOfficerAI) return;
            if (collision.gameObject.TryGetComponent<PlayerHealth>(out var health))
            {
                TryDamagePlayer(health);
            }
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (this is PoliceOfficerAI) return;
            if (collision.gameObject.TryGetComponent<PlayerHealth>(out var health))
            {
                TryDamagePlayer(health);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewDistance);

            Vector3 rightRay = Quaternion.Euler(0, 0, fieldOfView * 0.5f) * (Vector3)facingDirection;
            Vector3 leftRay = Quaternion.Euler(0, 0, -fieldOfView * 0.5f) * (Vector3)facingDirection;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rightRay * viewDistance);
            Gizmos.DrawRay(transform.position, leftRay * viewDistance);
        }
    }
}

