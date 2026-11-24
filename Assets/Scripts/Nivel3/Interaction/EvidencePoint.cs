using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Punto de interés e indicio en el terreno.
    /// Respeta estrictamente el protocolo ético de las Madres Buscadoras:
    /// No excava ni perturba el lugar; documenta con fotografía y coloca un marcador de evidencia
    /// para canalizar a peritos y personal especializado forense.
    /// </summary>
    public class EvidencePoint : MonoBehaviour, IInteractable
    {
        [Header("Datos del Indicio")]
        [SerializeField] private string evidenceId = "IND-01";
        [SerializeField] private string evidenceTitle = "Prenda de vestir desgastada";
        [TextArea(3, 5)]
        [SerializeField] private string evidenceDescription = "Se observa una prenda protegida entre la maleza. Coincide con prendas descritas en reportes. Se preserva el sitio sin alteración y se coloca un banderín de marcación.";

        [Header("Marcador Visual de Evidencia")]
        [Tooltip("Banderín o estaca amarilla de marcación que se activa al documentar.")]
        [SerializeField] private GameObject markerVisual;

        [Header("Es Indicio Final")]
        [Tooltip("Indica si este indicio corresponde al hallazgo narrativo culminante de la Zona 8.")]
        [SerializeField] private bool isFinalDiscovery = false;

        private bool isDocumented = false;
        public bool IsDocumented => isDocumented;

        private void Start()
        {
            if (markerVisual != null)
            {
                markerVisual.SetActive(false);
            }
        }

        public string GetInteractionPrompt()
        {
            if (isDocumented)
                return "Indicio ya registrado y marcado.";

            return $"Presiona [E] para Documentar y Colocar Marcador: {evidenceTitle}";
        }

        public bool CanInteract()
        {
            return !isDocumented;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isDocumented) return;

            isDocumented = true;

            // Activar visualmente el marcador de evidencia (banderín)
            if (markerVisual != null)
            {
                markerVisual.SetActive(true);
            }

            // Mostrar modal o notificación de documentación
            if (UI.DocumentViewerUI.Instance != null)
            {
                UI.DocumentViewerUI.Instance.DisplayEvidenceNote(evidenceTitle, evidenceDescription);
            }

            // Registrar en el GameManager
            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.RegisterEvidence(evidenceId, isFinalDiscovery);
            }
        }
    }
}

