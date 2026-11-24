using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Evento especial de la Zona 5 (Almacén):
    /// Al recoger el último documento clave, se corta la luz, se bloquea la entrada
    /// principal y se habilita la ventana lateral como ruta de escape.
    /// </summary>
    public class WarehouseDarknessEvent : MonoBehaviour
    {
        [Header("Disparador")]
        [SerializeField] private DocumentItem triggerDocument;

        [Header("Elementos del Evento")]
        [SerializeField] private GameObject warehouseLights;
        [SerializeField] private DoorMechanism mainEntranceDoor;
        [SerializeField] private DoorMechanism sideEscapeWindow;
        [SerializeField] private GameObject patrollingEnemySpawn;

        [Header("Efectos")]
        [SerializeField] private AudioClip powerCutSfx;
        [SerializeField] private AudioSource audioSource;

        private bool hasTriggered = false;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void TriggerEvent()
        {
            if (hasTriggered) return;
            hasTriggered = true;

            // Apagar luces del almacén
            if (warehouseLights != null)
            {
                warehouseLights.SetActive(false);
            }

            // Bloquear puerta principal
            if (mainEntranceDoor != null)
            {
                // La puerta principal queda sellada por fuera
                mainEntranceDoor.gameObject.SetActive(true);
            }

            // Habilitar ventana lateral como salida
            if (sideEscapeWindow != null)
            {
                sideEscapeWindow.UnlockAndOpen();
            }

            // Aparece enemigo patrullando el acceso
            if (patrollingEnemySpawn != null)
            {
                patrollingEnemySpawn.SetActive(true);
            }

            if (powerCutSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(powerCutSfx);
            }

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowTemporaryNotification("¡Se cortó la electricidad! La entrada quedó bloqueada, busca la ventana lateral.");
            }

            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.SetZone(LevelZone.CampoDeBusqueda);
            }
        }
    }
}

