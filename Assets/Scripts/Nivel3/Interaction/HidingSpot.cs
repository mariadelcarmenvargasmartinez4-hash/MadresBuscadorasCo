using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Punto de escondite (columnas, vehículos abandonados, maleza alta, corrales).
    /// Permite a la protagonista ocultarse de la visión de los vigilantes y perseguidores.
    /// </summary>
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        [Header("Configuración del Escondite")]
        [SerializeField] private string spotName = "Columnas del patio";
        [SerializeField] private Transform hidePosition;

        private bool isPlayerInside = false;
        private PlayerMovement2D currentPlayer;

        public bool IsOccupied => isPlayerInside;

        public string GetInteractionPrompt()
        {
            return isPlayerInside ? "Presiona [E] para Salir del Escondite" : $"Presiona [E] para Ocultarse en {spotName}";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isPlayerInside)
            {
                ExitSpot();
            }
            else
            {
                EnterSpot(player);
            }
        }

        private void EnterSpot(PlayerMovement2D player)
        {
            currentPlayer = player;
            isPlayerInside = true;

            if (hidePosition != null)
            {
                player.transform.position = hidePosition.position;
            }

            player.SetHiddenState(true);

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowTemporaryNotification($"Oculta en {spotName}. Los enemigos no pueden verte.");
            }
        }

        private void ExitSpot()
        {
            if (currentPlayer != null)
            {
                currentPlayer.SetHiddenState(false);
                currentPlayer = null;
            }
            isPlayerInside = false;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (isPlayerInside && other.GetComponent<PlayerMovement2D>() == currentPlayer)
            {
                ExitSpot();
            }
        }
    }
}

