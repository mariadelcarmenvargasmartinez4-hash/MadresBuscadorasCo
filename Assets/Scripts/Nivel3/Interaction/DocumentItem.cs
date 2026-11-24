using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Documento, libreta, fotografía o fragmento de nota coleccionable.
    /// Aporta contexto narrativo y revela pistas sobre accesos ocultos.
    /// </summary>
    public class DocumentItem : MonoBehaviour, IInteractable
    {
        [Header("Información del Documento")]
        [SerializeField] private string documentId = "DOC-01";
        [SerializeField] private string documentTitle = "Fragmento de carta arrugada";
        [TextArea(4, 8)]
        [SerializeField] private string documentBody = "...no debieron bajar al sótano. Hay una reja sellada con tres símbolos cerca de los corrales...";

        [Header("Propiedades")]
        [SerializeField] private bool countAsMainDocument = true;
        [SerializeField] private bool destroyOnCollect = true;

        private bool isRead = false;

        public string GetInteractionPrompt()
        {
            return $"Presiona [E] para Leer: {documentTitle}";
        }

        public bool CanInteract()
        {
            return !isRead;
        }

        public void Interact(PlayerMovement2D player)
        {
            isRead = true;

            // Abrir visor de documentos
            if (UI.DocumentViewerUI.Instance != null)
            {
                UI.DocumentViewerUI.Instance.DisplayDocument(documentTitle, documentBody);
            }

            // Registrar en el GameManager
            if (countAsMainDocument && Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.RegisterDocument(documentId);
            }

            if (destroyOnCollect)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

