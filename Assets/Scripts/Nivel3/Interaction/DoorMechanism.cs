using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Mecanismo de puertas, rejas y accesos bloqueados.
    /// Soporta apertura mediante llaves, herramientas, fusibles o activación por eventos.
    /// </summary>
    public class DoorMechanism : MonoBehaviour, IInteractable
    {
        public enum UnlockRequirement
        {
            None,
            RequiresKey,
            RequiresTool,
            RequiresFuses,
            RequiresPuzzleSolved
        }

        [Header("Identificación y Requisitos")]
        [SerializeField] private string doorName = "Reja oxidada de la entrada";
        [SerializeField] private UnlockRequirement requirement = UnlockRequirement.RequiresKey;
        [SerializeField] private string requiredItemName = "Llave de la Caseta";

        [Header("Estado de la Puerta")]
        [SerializeField] private bool isLocked = true;
        [SerializeField] private bool isOpen = false;

        [Header("Referencias Visuales y Físicas")]
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private SpriteRenderer doorSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private AudioClip openSfx;
        [SerializeField] private AudioClip lockedSfx;
        [SerializeField] private AudioSource audioSource;

        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
            if (doorSprite == null) doorSprite = GetComponent<SpriteRenderer>();
        }

        public string GetInteractionPrompt()
        {
            if (isOpen)
                return $"{doorName} (Abierta)";

            if (isLocked)
            {
                return $"Presiona [E] para Abrir {doorName} [{requiredItemName}]";
            }

            return $"Presiona [E] para Abrir {doorName}";
        }

        public bool CanInteract()
        {
            return !isOpen;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isOpen) return;

            if (isLocked)
            {
                // Verificar en el inventario/progreso del GameManager si se tiene el requisito
                bool hasRequirement = CheckRequirement();

                if (!hasRequirement)
                {
                    PlayAudio(lockedSfx);
                    if (UI.HUDManager.Instance != null)
                    {
                        UI.HUDManager.Instance.ShowTemporaryNotification($"Bloqueada. Necesitas: {requiredItemName}");
                    }
                    return;
                }
            }

            OpenDoor();
        }

        private bool CheckRequirement()
        {
            if (requirement == UnlockRequirement.None) return true;

            if (Systems.Level3GameManager.Instance != null)
            {
                return Systems.Level3GameManager.Instance.HasItemOrRequirement(requiredItemName);
            }

            return false;
        }

        public void UnlockAndOpen()
        {
            isLocked = false;
            OpenDoor();
        }

        private void OpenDoor()
        {
            isOpen = true;
            isLocked = false;

            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }

            if (doorSprite != null && openSprite != null)
            {
                doorSprite.sprite = openSprite;
            }
            else if (doorSprite != null)
            {
                // Si no hay sprite de abierto, desvanecer ligeramente
                Color c = doorSprite.color;
                c.a = 0.3f;
                doorSprite.color = c;
            }

            PlayAudio(openSfx);

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowTemporaryNotification($"{doorName} abierta.");
            }
        }

        private void PlayAudio(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}

