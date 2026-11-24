using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Botella de agua distribuida en el escenario.
    /// Recupera un corazón hasta un máximo de tres (según sección 6 del documento).
    /// </summary>
    public class WaterBottleItemNivel3 : MonoBehaviour, IInteractable
    {
        [SerializeField] private AudioClip drinkSfx;
        private bool isCollected = false;

        public string GetInteractionPrompt()
        {
            return "Presiona [E] para Beber Agua (+1 Corazón)";
        }

        public bool CanInteract()
        {
            return !isCollected;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (player != null)
            {
                TryCollect(player.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;
            if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null)
            {
                TryCollect(other.gameObject);
            }
        }

        private void TryCollect(GameObject playerObj)
        {
            if (isCollected) return;

            if (playerObj.TryGetComponent<PlayerHealth>(out var health))
            {
                isCollected = true;
                if (health.CurrentHearts < health.MaxHearts)
                {
                    health.Heal(1);
                    if (UI.HUDManager.Instance != null)
                    {
                        UI.HUDManager.Instance.ShowTemporaryNotification("💧 ¡Botella de agua bebida! +1 Corazón restaurado en la Línea de vida ♥");
                    }
                }
                else
                {
                    PlayerMovement2D mov = playerObj.GetComponent<PlayerMovement2D>();
                    if (mov != null) mov.RestoreFullStamina();
                    if (UI.HUDManager.Instance != null)
                    {
                        UI.HUDManager.Instance.ShowTemporaryNotification("💧 ¡Botella de agua recolectada! Energía restablecida al 100%.");
                    }
                }
                gameObject.SetActive(false);
            }
        }
    }
}


