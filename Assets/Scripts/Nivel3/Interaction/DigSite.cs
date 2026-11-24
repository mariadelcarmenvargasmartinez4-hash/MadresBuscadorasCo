using System.Collections;
using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Punto de excavación y prospección con la varilla en T y pala en la hacienda vieja.
    /// Permite a la Madre Buscadora desenterrar objetos personales, llaves, fotografías
    /// y notas protegidas, señalizando el sitio con un banderín de marcación forense.
    /// </summary>
    public class DigSite : MonoBehaviour, IInteractable
    {
        [Header("Datos del Objeto Enterrado")]
        [SerializeField] private string itemName = "Llavero personal oxidado";
        [TextArea(2, 4)]
        [SerializeField] private string itemDescription = "Pertenencia recuperada bajo tierra suelta. Coincide con la descripción de los reportes. Se preserva y se coloca el banderín de evidencia.";
        [SerializeField] private bool givesWaterBottle = false;
        [SerializeField] private bool givesInventoryKey = false;
        [SerializeField] private string keyItemName = "";

        [Header("Tiempos y Efectos")]
        [SerializeField] private float digDuration = 1.4f;
        [SerializeField] private SpriteRenderer soilSpriteRenderer;
        [SerializeField] private Sprite untouchedMoundSprite;
        [SerializeField] private Sprite excavatedHoleSprite;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip diggingSoilSfx;
        [SerializeField] private AudioClip discoverSfx;

        private bool isExcavated = false;
        private bool isDigging = false;

        public bool IsExcavated => isExcavated;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (soilSpriteRenderer == null) soilSpriteRenderer = GetComponent<SpriteRenderer>();

            if (soilSpriteRenderer != null && untouchedMoundSprite != null)
            {
                soilSpriteRenderer.sprite = untouchedMoundSprite;
            }
        }

        public string GetInteractionPrompt()
        {
            if (isExcavated)
                return $"Sitio de prospección ya excavado y marcado [{itemName}]";

            if (isDigging)
                return "Excavando cuidadosamente con varilla y pala...";

            return $"Presiona [E / V] para Excavar Tierra Alterada ({itemName})";
        }

        public bool CanInteract()
        {
            return !isExcavated && !isDigging;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isExcavated || isDigging) return;

            StartCoroutine(DiggingRoutine(player));
        }

        private IEnumerator DiggingRoutine(PlayerMovement2D player)
        {
            isDigging = true;

            // Bloquear movimiento mientras excava
            if (player != null)
            {
                player.SetMovementLocked(true);
            }

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowTemporaryNotification("Clavando varilla de prospección y excavando...");
            }

            if (diggingSoilSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(diggingSoilSfx);
            }

            yield return new WaitForSeconds(digDuration);

            isDigging = false;
            isExcavated = true;

            // Desbloquear movimiento
            if (player != null)
            {
                player.SetMovementLocked(false);
            }

            // Cambiar aspecto a fosa excavada con banderín
            if (soilSpriteRenderer != null && excavatedHoleSprite != null)
            {
                soilSpriteRenderer.sprite = excavatedHoleSprite;
            }

            if (discoverSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(discoverSfx);
            }

            // Entregar recompensas o registrar indicios
            if (givesWaterBottle && player != null)
            {
                if (player.TryGetComponent<PlayerHealth>(out var hp))
                {
                    hp.Heal(1);
                }
            }

            if (givesInventoryKey && !string.IsNullOrEmpty(keyItemName))
            {
                if (Level3GameManager.Instance != null)
                {
                    Level3GameManager.Instance.AddInventoryItem(keyItemName);
                }
            }

            // Registrar indicio y abrir modal informativo
            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.RegisterEvidence(name, false);
            }

            if (UI.DocumentViewerUI.Instance != null)
            {
                UI.DocumentViewerUI.Instance.DisplayEvidenceNote($"DESENTERRADO: {itemName}", itemDescription);
            }
        }
    }
}

