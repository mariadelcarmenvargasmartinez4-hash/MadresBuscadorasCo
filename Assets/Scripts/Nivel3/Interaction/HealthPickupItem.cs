using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    public enum HealthItemType
    {
        WaterBottle,
        FirstAidKit,
        WildFruit,
        HopeAmulet
    }

    /// <summary>
    /// Objeto interactivo que restaura o incrementa corazones de vida en la barra de salud.
    /// Soporta recolección por contacto (Trigger) y por interacción directa [E].
    /// </summary>
    public class HealthPickupItem : MonoBehaviour, IInteractable
    {
        [Header("Configuración de Salud")]
        [SerializeField] private HealthItemType itemType = HealthItemType.WaterBottle;
        [SerializeField] private string itemName = "Botiquín Médico";
        [SerializeField] private int heartsToRestore = 1;
        [SerializeField] private bool increasesMaxCapacity = false;

        [Header("Efecto de Flotación Suave")]
        [SerializeField] private bool floatAnimation = true;
        [SerializeField] private float floatSpeed = 2.8f;
        [SerializeField] private float floatHeight = 0.08f;

        private Vector3 startPos;
        private bool isCollected = false;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            if (floatAnimation && !isCollected)
            {
                transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * floatSpeed) * floatHeight, 0f);
            }
        }

        public string GetInteractionPrompt()
        {
            if (isCollected) return "";
            if (increasesMaxCapacity) return $"Presiona [E] para Recoger {itemName} (+1 Corazón Máximo a la Línea de vida)";
            return $"Presiona [E] para Tomar {itemName} (+{heartsToRestore} en tu Línea de vida)";
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

        public HealthItemType ItemType => itemType;

        public void Configure(HealthItemType type, string name, int hearts, bool expandMax)
        {
            itemType = type;
            itemName = name;
            heartsToRestore = hearts;
            increasesMaxCapacity = expandMax;
        }

        private void TryCollect(GameObject playerObj)
        {
            if (isCollected) return;

            PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
            if (health == null) return;

            if (increasesMaxCapacity)
            {
                health.AddMaxHeart(heartsToRestore);
                isCollected = true;
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.ShowTemporaryNotification($"✨ ¡{itemName} obtenido! Capacidad de la Línea de vida incrementada (+{heartsToRestore} Corazón Máximo ♥)");
                }
                gameObject.SetActive(false);
            }
            else
            {
                isCollected = true;
                if (health.CurrentHearts < health.MaxHearts)
                {
                    health.Heal(heartsToRestore);
                    if (HUDManager.Instance != null)
                    {
                        HUDManager.Instance.ShowTemporaryNotification($"💚 {itemName}: +{heartsToRestore} Corazón restaurado en la Línea de vida ♥");
                    }
                }
                else
                {
                    PlayerMovement2D mov = playerObj.GetComponent<PlayerMovement2D>();
                    if (mov != null) mov.RestoreFullStamina();
                    if (HUDManager.Instance != null)
                    {
                        HUDManager.Instance.ShowTemporaryNotification($"🎒 ¡{itemName} recolectado! Mochila de suministros abastecida y energía restaurada al 100%.");
                    }
                }
                gameObject.SetActive(false);
            }
        }
    }
}
