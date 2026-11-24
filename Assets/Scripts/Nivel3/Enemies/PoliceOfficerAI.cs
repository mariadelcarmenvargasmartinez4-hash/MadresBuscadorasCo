using System.Collections;
using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Enemies
{
    /// <summary>
    /// Oficial de Policía que vigila las Zonas Privadas de la Hacienda.
    /// Al detectar al jugador activa alerta estroboscópica roja/azul y persecución rápida.
    /// Si lo alcanza, descuenta 1 corazón de vida y lo devuelve al último punto de control.
    /// </summary>
    public class PoliceOfficerAI : BaseEnemyAI
    {
        [Header("Persecución Policial de Zona Privada")]
        [SerializeField] private float arrestDistance = 1.0f;
        [SerializeField] private float sirenFlashSpeed = 8.0f;
        [SerializeField] private bool isChasingPlayer = false;

        private Color policeNavy = new Color(0.88f, 0.92f, 1.0f, 1f);
        private Color redFlash = new Color(1f, 0.25f, 0.25f, 1f);
        private Color blueFlash = new Color(0.3f, 0.6f, 1f, 1f);
        private bool isArresting = false;
        private float sirenTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            maxHp = 2;
            arrestDistance = 0.78f;
            chaseSpeed = 4.6f;
            patrolSpeed = 2.1f;
            viewDistance = 7.2f;
            fieldOfView = 95f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = policeNavy;
            }
        }

        public override void TakeDamage(int damage, Vector2 knockbackDir)
        {
            if (isArresting)
            {
                StopAllCoroutines();
                isArresting = false;
                if (targetPlayer != null)
                {
                    targetPlayer.SetMovementLocked(false);
                }
            }

            base.TakeDamage(damage, knockbackDir);
        }

        protected override void Update()
        {
            if (isDead || isArresting) return;

            base.Update();

            if (isDead || knockbackTimer > 0f) return;

            if (targetPlayer != null && targetPlayer.IsHidden && isChasingPlayer)
            {
                float distToHidden = Vector2.Distance(transform.position, targetPlayer.transform.position);
                if (distToHidden > 1.6f)
                {
                    isChasingPlayer = false;
                    currentDetection = 0f;
                    StartSearching();
                }
            }

            if (currentState == EnemyState.Chase || isChasingPlayer)
            {
                sirenTimer += Time.deltaTime * sirenFlashSpeed;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = (Mathf.Sin(sirenTimer) > 0f) ? redFlash : blueFlash;
                }

                if (targetPlayer != null && !targetPlayer.IsHidden && !targetPlayer.IsDashing)
                {
                    float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
                    if (dist <= arrestDistance)
                    {
                        ArrestPlayer();
                    }
                }
            }
            else
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = policeNavy;
                }
            }
        }

        public void AlertPoliceToPlayer(Vector3 playerPos)
        {
            if (isDead || isArresting) return;
            isChasingPlayer = true;
            currentState = EnemyState.Chase;
            currentDetection = 100f;
            lastKnownPosition = playerPos;

            HUDManager hud = Object.FindAnyObjectByType<HUDManager>();
            if (hud != null)
            {
                hud.ShowTemporaryNotification("🚨 ¡ZONA PRIVADA! ¡CORRE CON [SHIFT] O ESCÓNDETE DE LA POLICÍA! 🚨");
            }
        }

        public void ArrestPlayer()
        {
            if (isArresting) return;
            isArresting = true;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            Debug.Log("<color=red><b>[Policía]</b> ¡Jugador interceptado en zona privada!</color>");

            if (targetPlayer != null)
            {
                targetPlayer.SetMovementLocked(true);
            }

            HUDManager hud = Object.FindAnyObjectByType<HUDManager>();
            if (hud != null)
            {
                hud.ShowTemporaryNotification("⛔ ¡INTERCEPTADA POR LA POLICÍA! (-1 ♥) Volviendo al punto de control...");
            }

            StartCoroutine(ArrestAndRestartRoutine());
        }

        private IEnumerator ArrestAndRestartRoutine()
        {
            yield return new WaitForSeconds(1.5f);

            int remainingHearts = 3;
            if (targetPlayer != null)
            {
                PlayerHealth health = targetPlayer.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    if (health.CurrentHearts > 1)
                    {
                        health.TakeDamage(1);
                        remainingHearts = health.CurrentHearts;
                    }
                    else
                    {
                        remainingHearts = 0;
                    }
                }
                targetPlayer.SetMovementLocked(false);
            }

            Level3GameManager gm = Object.FindAnyObjectByType<Level3GameManager>();
            if (gm != null)
            {
                gm.RespawnPlayer();
                if (remainingHearts > 0 && targetPlayer != null && targetPlayer.TryGetComponent<PlayerHealth>(out var h))
                {
                    h.SetHearts(remainingHearts);
                }
            }

            isArresting = false;
            isChasingPlayer = false;
            currentState = EnemyState.Patrol;
            currentDetection = 0f;
            if (spriteRenderer != null) spriteRenderer.color = policeNavy;
        }
    }
}
