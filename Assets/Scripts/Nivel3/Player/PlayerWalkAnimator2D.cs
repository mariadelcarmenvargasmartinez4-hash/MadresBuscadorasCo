using UnityEngine;

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Animación procedural de caminata y carrera para la Madre Buscadora.
    /// Aplica balanceo rítmico e inclinación al correr o esquivar.
    /// </summary>
    public class PlayerWalkAnimator2D : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private PlayerMovement2D movement;

        [Header("Cadencia y Balanceo de Pasos")]
        [SerializeField] private float walkFrequency = 9.0f;
        [SerializeField] private float sprintFrequency = 16.0f;
        [SerializeField] private float swayAngle = 4.0f;

        private float stepTimer = 0f;

        private void Awake()
        {
            if (movement == null) movement = GetComponent<PlayerMovement2D>();
        }

        private void Update()
        {
            if (movement == null) return;

            if (movement.IsMoving)
            {
                float freq = (movement.IsSprinting || movement.IsDashing) ? sprintFrequency : walkFrequency;
                stepTimer += Time.deltaTime * freq;

                float amplitude = movement.IsSprinting ? swayAngle * 1.65f : swayAngle;
                if (movement.IsDashing) amplitude = swayAngle * 2.2f;

                float sway = Mathf.Sin(stepTimer * 0.5f) * amplitude;
                transform.localRotation = Quaternion.Euler(0f, 0f, sway);
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            }
        }
    }
}
