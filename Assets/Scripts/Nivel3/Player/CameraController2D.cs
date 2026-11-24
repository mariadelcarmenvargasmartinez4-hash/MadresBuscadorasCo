using UnityEngine;

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Cámara 2D que sigue suavemente a la Madre Buscadora a lo largo del nivel.
    /// </summary>
    public class CameraController2D : MonoBehaviour
    {
        [Header("Objetivo")]
        [SerializeField] private Transform target;

        [Header("Suavizado y Límites")]
        [SerializeField] private float smoothSpeed = 5.0f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private bool clampX = true;
        [SerializeField] private float minX = -12f;
        [SerializeField] private float maxX = 120f;

        private float shakeTimer = 0f;
        private float shakeIntensity = 0f;

        private void Start()
        {
            if (target == null)
            {
                var player = Object.FindAnyObjectByType<PlayerMovement2D>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        public void Shake(float intensity = 0.14f, float duration = 0.14f)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
            shakeTimer = Mathf.Max(shakeTimer, duration);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                var player = Object.FindAnyObjectByType<PlayerMovement2D>();
                if (player != null) target = player.transform;
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            if (clampX)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            }

            Vector3 nextPos = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                Vector2 offsetShake = Random.insideUnitCircle * shakeIntensity;
                nextPos += new Vector3(offsetShake.x, offsetShake.y, 0f);
                if (shakeTimer <= 0f) shakeIntensity = 0f;
            }
            transform.position = nextPos;
        }

        public void SetTarget(Transform newTarget, bool snapImmediately = false)
        {
            target = newTarget;
            if (snapImmediately && target != null)
            {
                Vector3 desiredPosition = target.position + offset;
                if (clampX)
                {
                    desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                }
                transform.position = desiredPosition;
            }
        }
    }
}

