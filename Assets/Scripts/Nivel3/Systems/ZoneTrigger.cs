using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Systems
{
    /// <summary>
    /// Disparador que actualiza la zona actual en el Level3GameManager
    /// conforme la protagonista avanza por la hacienda.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ZoneTrigger : MonoBehaviour
    {
        [SerializeField] private LevelZone zoneToActivate;
        private bool hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered) return;

            if (other.TryGetComponent<PlayerMovement2D>(out var _))
            {
                hasTriggered = true;
                if (Level3GameManager.Instance != null)
                {
                    Level3GameManager.Instance.SetZone(zoneToActivate);
                }
            }
        }
    }
}

