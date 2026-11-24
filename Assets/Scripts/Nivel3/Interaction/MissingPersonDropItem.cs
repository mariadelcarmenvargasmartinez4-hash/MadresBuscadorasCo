using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    public enum MissingPersonDropType
    {
        BonesRemains,
        ClothingEvidence
    }

    /// <summary>
    /// Objeto personal o resto óseo de persona desaparecida que cae al abatir enemigos.
    /// Puede recolectarse pasando sobre él o presionando [E / V], sumando indicios a la búsqueda
    /// y otorgando energía / salud a la Madre Buscadora.
    /// </summary>
    public class MissingPersonDropItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private MissingPersonDropType dropType = MissingPersonDropType.BonesRemains;
        [SerializeField] private string itemName = "Fragmento Óseo de Víctima";
        [SerializeField] private int heartsToRestore = 1;

        private bool isCollected = false;
        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            if (!isCollected)
            {
                transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * 3.2f) * 0.08f, 0f);
            }
        }

        public void Configure(MissingPersonDropType type, string name)
        {
            dropType = type;
            itemName = name;
        }

        public string GetInteractionPrompt()
        {
            if (isCollected) return "";
            return dropType == MissingPersonDropType.BonesRemains
                ? $"Presiona [E / V] para Asegurar {itemName} (🦴 +1 Indicio Forense)"
                : $"Presiona [E / V] para Recoger {itemName} (👕 +1 Indicio Textil)";
        }

        public bool CanInteract()
        {
            return !isCollected;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (player != null)
            {
                Collect(player.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;
            if (other.CompareTag("Player") || other.GetComponent<PlayerMovement2D>() != null)
            {
                Collect(other.gameObject);
            }
        }

        private void Collect(GameObject playerObj)
        {
            if (isCollected) return;
            isCollected = true;

            PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
            if (health != null && health.CurrentHearts < health.MaxHearts)
            {
                health.Heal(heartsToRestore);
            }
            else
            {
                PlayerMovement2D mov = playerObj.GetComponent<PlayerMovement2D>();
                if (mov != null) mov.RestoreFullStamina();
            }

            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.RegisterEvidence(itemName, false);
            }

            if (HUDManager.Instance != null)
            {
                string icon = (dropType == MissingPersonDropType.BonesRemains) ? "🦴" : "👕";
                HUDManager.Instance.ShowTemporaryNotification($"{icon} ¡Indicio recuperado! {itemName} documentado para peritos (+1 Indicio 🚩).");
            }

            gameObject.SetActive(false);
        }
    }
}

