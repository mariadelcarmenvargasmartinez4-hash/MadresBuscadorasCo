using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Enemies
{
    /// <summary>
    /// Guardián subterráneo de los Túneles y Sótano (Zona 7).
    /// Dificultad: Muy alta.
    /// Posee audición hipersensible: reacciona a los pasos normales, carreras y escombros.
    /// Obliga a la protagonista a usar el sigilo acústico (agacharse) y evitar pisadas ruidosas.
    /// </summary>
    public class GuardianAI : BaseEnemyAI, IAcousticListener
    {
        [Header("Sensibilidad Acústica")]
        [SerializeField] private float hearingSensitivity = 1.2f;
        [SerializeField] private AudioClip soundDetectedSfx;
        [SerializeField] private AudioSource audioSource;

        protected override void Awake()
        {
            base.Awake();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            // Configuración en túneles oscuros
            maxHp = 3;
            patrolSpeed = 1.9f;
            chaseSpeed = 4.4f;
            viewDistance = 4.5f; // Visión más corta por la penumbra de los túneles
            fieldOfView = 80f;
            detectionIncreaseRate = 100f;
        }

        /// <summary>
        /// Recibe las ondas de sonido emitidas por el PlayerNoiseEmitter.
        /// Si el sonido es perceptible, se pone en alerta y acude a inspeccionar el origen.
        /// </summary>
        public void OnNoiseHeard(Vector3 noiseOrigin, float intensity)
        {
            float dist = Vector3.Distance(transform.position, noiseOrigin);
            if (dist <= intensity * hearingSensitivity)
            {
                // Solo investiga si no está ya en persecución directa
                if (currentState != EnemyState.Chase)
                {
                    lastKnownPosition = noiseOrigin;
                    currentState = EnemyState.Investigate;

                    if (soundDetectedSfx != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(soundDetectedSfx);
                    }

                    if (UI.HUDManager.Instance != null)
                    {
                        UI.HUDManager.Instance.ShowTemporaryNotification("¡El Guardián ha escuchado un ruido y se aproxima!");
                    }
                }
            }
        }
    }
}

