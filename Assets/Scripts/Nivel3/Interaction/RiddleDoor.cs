using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Puerta con Acertijo que se abre al resolver la combinaciÃ³n o enigma.
    /// Al resolver el acertijo y abrirse, recompensa a la protagonista sumÃ¡ndole
    /// un corazÃ³n de vida (+1 CorazÃ³n â™¥).
    /// </summary>
    public class RiddleDoor : MonoBehaviour, IInteractable
    {
        [Header("ConfiguraciÃ³n del Acertijo")]
        [SerializeField] private string doorTitle = "Puerta del Despacho Colonial";
        [SerializeField] private string riddleText = "ACERTIJO DEL GUARDIÃN:\n'Tres nÃºmeros abren este cerrojo:\n1. Los corazones que sostienen tu vida: (3)\n2. Las letras de la palabra LUZ: (3)\n3. La causa por la que buscas: (1)\nCÃ³digo requerido: 3 - 3 - 1'";
        [SerializeField] private int requiredCode = 331;
        [SerializeField] private bool isUnlocked = false;

        [Header("Recompensa de Vida")]
        [SerializeField] private int heartsToRestore = 1;

        [Header("Componentes")]
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private SpriteRenderer doorSprite;

        public bool IsUnlocked => isUnlocked;

        private void Awake()
        {
            if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
            if (doorSprite == null) doorSprite = GetComponent<SpriteRenderer>();
        }

        public string GetInteractionPrompt()
        {
            if (isUnlocked) return $"{doorTitle} (Abierta y Desbloqueada)";
            return $"Presiona [E] para Resolver Acertijo de {doorTitle} (+1 CorazÃ³n)";
        }

        public bool CanInteract()
        {
            return !isUnlocked;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isUnlocked) return;

            // Resolver el acertijo
            isUnlocked = true;

            // Desactivar colisionador para permitir el paso
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }

            // Cambiar color/sprite de la puerta abierta
            if (doorSprite != null)
            {
                doorSprite.color = new Color(0.7f, 1f, 0.7f, 0.7f);
            }

            // SUMAR CORAZONES DE VIDA COMO RECOMPENSA
            if (player != null)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.AddMaxHeart(heartsToRestore);
                }
            }

            // NotificaciÃ³n en pantalla
            HUDManager hud = UnityEngine.Object.FindAnyObjectByType<HUDManager>();
            if (hud != null)
            {
                hud.ShowTemporaryNotification($"[ACERTIJO RESUELTO - CÓDIGO {requiredCode}] {riddleText} Puerta abierta y +{heartsToRestore} Corazón de Vida ♥");
            }

            Debug.Log($"<color=green><b>[Acertijo Resuelto]</b> {doorTitle} desbloqueada con Ã©xito. Vida sumada.</color>");
        }
    }
}
