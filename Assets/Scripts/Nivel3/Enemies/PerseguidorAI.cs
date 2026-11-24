using UnityEngine;

namespace LasHuellasDelSilencio.Enemies
{
    /// <summary>
    /// Perseguidor veloz de las Caballerizas (Zona 4).
    /// Dificultad: Alta.
    /// Corre velozmente al detectar a la protagonista y mantiene una persecución agresiva.
    /// El jugador debe romper línea de visión, cambiar de ruta y utilizar obstáculos.
    /// </summary>
    public class PerseguidorAI : BaseEnemyAI
    {
        [Header("Comportamiento Perseguidor")]
        [SerializeField] private float chaseDuration = 8.0f;
        [SerializeField] private AudioClip screamOrChaseSfx;
        [SerializeField] private AudioSource audioSource;

        private float currentChaseTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            // Perseguidor es considerablemente más veloz
            maxHp = 2;
            patrolSpeed = 2.4f;
            chaseSpeed = 5.2f;
            viewDistance = 6.8f;
            fieldOfView = 85f;
            detectionIncreaseRate = 120f;
        }

        protected override void OnPlayerDetected()
        {
            base.OnPlayerDetected();
            currentChaseTime = chaseDuration;

            if (screamOrChaseSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(screamOrChaseSfx);
            }
        }

        protected override void ChaseBehavior()
        {
            base.ChaseBehavior();

            currentChaseTime -= Time.deltaTime;
            // Si el jugador rompe la visión durante varios segundos, el perseguidor se cansa o busca
            if (currentChaseTime <= 0f)
            {
                StartSearching();
            }
        }
    }
}

