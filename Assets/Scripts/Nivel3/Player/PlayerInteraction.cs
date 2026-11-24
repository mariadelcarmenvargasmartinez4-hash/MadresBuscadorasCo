using System.Collections.Generic;
using UnityEngine;
using LasHuellasDelSilencio.Interaction;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Maneja la detección e interacción contextual con objetos del entorno (tecla E).
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detección de Interacción")]
        [SerializeField] private float interactionRadius = 1.6f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        private PlayerMovement2D playerMovement;
        private IInteractable currentTarget;

        public IInteractable CurrentTarget => currentTarget;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement2D>();
        }

        private void Update()
        {
            FindClosestInteractable();

            bool interactPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (
                Keyboard.current.eKey.wasPressedThisFrame ||
                Keyboard.current.vKey.wasPressedThisFrame))
            {
                interactPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.V))
            {
                interactPressed = true;
            }
#endif

            if (interactPressed && currentTarget != null && currentTarget.CanInteract())
            {
                currentTarget.Interact(playerMovement);
            }
        }

        private void FindClosestInteractable()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);
            IInteractable bestCandidate = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IInteractable>(out var candidate))
                {
                    if (candidate.CanInteract())
                    {
                        float dist = Vector2.Distance(transform.position, hit.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            bestCandidate = candidate;
                        }
                    }
                }
            }

            currentTarget = bestCandidate;

            // Actualizar prompt en el HUD
            if (UI.HUDManager.Instance != null)
            {
                if (currentTarget != null)
                {
                    UI.HUDManager.Instance.ShowInteractionPrompt(currentTarget.GetInteractionPrompt());
                }
                else
                {
                    UI.HUDManager.Instance.HideInteractionPrompt();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}

