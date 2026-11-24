using System.Collections.Generic;
using UnityEngine;

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Emite ondas de sonido y vibraciones según el movimiento de la protagonista.
    /// Vital para la mecánica de sigilo acústico frente al Guardián en los túneles subterráneos.
    /// </summary>
    public class PlayerNoiseEmitter : MonoBehaviour
    {
        [Header("Radios de Ruido (Metros)")]
        [Tooltip("Agachada/Sigilo: Ruido nulo, no alerta a enemigos acústicos.")]
        [SerializeField] private float sneakNoiseRadius = 0f;

        [Tooltip("Caminar normal: Ruido moderado.")]
        [SerializeField] private float walkNoiseRadius = 3f;

        [Tooltip("Correr/Sprint: Ruido alto que alerta a gran distancia.")]
        [SerializeField] private float sprintNoiseRadius = 7f;

        [Tooltip("Ruido al patear escombros o tropezar.")]
        [SerializeField] private float debrisNoiseRadius = 8.5f;

        [Header("Capas de Detección")]
        [SerializeField] private LayerMask listenerLayerMask = ~0;

        private float currentRadius = 0f;
        public float CurrentNoiseRadius => currentRadius;

        public void EmitMovementNoise(bool isMoving, bool isSneaking, bool isSprinting)
        {
            if (!isMoving)
            {
                currentRadius = 0f;
                return;
            }

            if (isSneaking)
            {
                currentRadius = sneakNoiseRadius; // 0m
            }
            else if (isSprinting)
            {
                currentRadius = sprintNoiseRadius;
            }
            else
            {
                currentRadius = walkNoiseRadius;
            }

            if (currentRadius > 0.05f)
            {
                BroadcastNoise(transform.position, currentRadius);
            }
        }

        public void TriggerAccidentalNoise()
        {
            currentRadius = debrisNoiseRadius;
            BroadcastNoise(transform.position, debrisNoiseRadius);
        }

        public void EmitNoiseBurst(float radius)
        {
            currentRadius = radius;
            if (radius > 0.05f)
            {
                BroadcastNoise(transform.position, radius);
            }
        }

        private void BroadcastNoise(Vector3 origin, float radius)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, radius, listenerLayerMask);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent<IAcousticListener>(out var listener))
                {
                    listener.OnNoiseHeard(origin, radius);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, walkNoiseRadius);
            Gizmos.color = new Color(1f, 0.2f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, sprintNoiseRadius);
        }
    }
}

