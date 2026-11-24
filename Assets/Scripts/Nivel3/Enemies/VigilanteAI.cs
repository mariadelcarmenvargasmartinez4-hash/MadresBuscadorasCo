using UnityEngine;

namespace LasHuellasDelSilencio.Enemies
{
    /// <summary>
    /// Vigilante del Patio Principal (Zona 2) y Casa Abandonada (Zona 3).
    /// Dificultad: Baja a media.
    /// Patrulla rutas predecibles, vigila con cono de visión y obliga al jugador a ocultarse.
    /// </summary>
    public class VigilanteAI : BaseEnemyAI
    {
        [Header("Comportamiento Vigilante")]
        [SerializeField] private AudioClip alertSfx;
        [SerializeField] private AudioSource audioSource;

        protected override void Awake()
        {
            base.Awake();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            // Valores de calibración según GDD
            maxHp = 2;
            patrolSpeed = 1.8f;
            chaseSpeed = 3.2f;
            viewDistance = 5.0f;
            fieldOfView = 70f;
        }

        protected override void OnPlayerDetected()
        {
            base.OnPlayerDetected();
            if (alertSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(alertSfx);
            }
        }
    }
}

