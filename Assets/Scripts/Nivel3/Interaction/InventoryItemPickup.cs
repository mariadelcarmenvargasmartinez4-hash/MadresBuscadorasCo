using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Recogible de inventario: Llaves, herramientas y fusibles para desbloquear puertas y mecanismos.
    /// </summary>
    public class InventoryItemPickup : MonoBehaviour, IInteractable
    {
        [Header("Datos del Objeto")]
        [SerializeField] private string itemName = "Llave de la Caseta";
        [TextArea(2, 4)]
        [SerializeField] private string description = "Llave vieja de metal encontrada en la caseta de vigilancia. Abre la reja principal.";

        [Header("Audio")]
        [SerializeField] private AudioClip pickupSfx;

        private bool isPickedUp = false;

        public string Description => description;

        public string GetInteractionPrompt()
        {
            return $"Presiona [E / V] para Recoger {itemName}";
        }

        public bool CanInteract()
        {
            return !isPickedUp;
        }

        public void Interact(PlayerMovement2D player)
        {
            CollectItem();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isPickedUp) return;
            if (other.CompareTag("Player") || other.GetComponent<PlayerMovement2D>() != null)
            {
                CollectItem();
            }
        }

        private void CollectItem()
        {
            if (isPickedUp) return;
            isPickedUp = true;

            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.AddInventoryItem(itemName);
            }

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowTemporaryNotification($"✨ Obtenido y guardado: {itemName}");
            }

            gameObject.SetActive(false);
        }
    }
}

