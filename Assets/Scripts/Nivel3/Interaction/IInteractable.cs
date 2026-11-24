using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Interfaz para todos los elementos interactivos del nivel:
    /// Indicios, documentos, botellas de agua, escondites, puertas y mecanismos.
    /// </summary>
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(PlayerMovement2D player);
        bool CanInteract();
    }
}

