using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Escena de Hallazgo Forense y Pericial en Fosa Clandestina.
    /// Una vez desenterrada Laura bajo el Árbol Viejo, acordonar este sitio
    /// activa la secuencia final solemne del juego: "FIN".
    /// </summary>
    public class ForensicCrimeScene : MonoBehaviour, IInteractable
    {
        [Header("Documentación Pericial")]
        [SerializeField] private string sceneName = "Fosa Clandestina y Escena del Crimen";
        [SerializeField] private bool isDocumented = false;

        public bool IsDocumented => isDocumented;

        public string GetInteractionPrompt()
        {
            if (isDocumented) return $"{sceneName} (Sitio asegurado y documentado con estacas)";
            LauraTreeGrave lauraGrave = Object.FindAnyObjectByType<LauraTreeGrave>();
            if (lauraGrave != null && !lauraGrave.IsLauraFound)
            {
                return "🌳 Primero desentierra a Laura bajo el Árbol Viejo [E / V] a unos pasos a la izquierda";
            }
            return $"Presiona [E / V] para Acordonar Escena de Laura y Notificar Peritos 🚩";
        }

        public bool CanInteract()
        {
            return !isDocumented;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isDocumented) return;

            LauraTreeGrave lauraGrave = Object.FindAnyObjectByType<LauraTreeGrave>();
            if (lauraGrave != null && !lauraGrave.IsLauraFound)
            {
                HUDManager hudCheck = Object.FindAnyObjectByType<HUDManager>();
                if (hudCheck != null)
                {
                    hudCheck.ShowTemporaryNotification("🌳 ¡Busca el Árbol Viejo aquí al lado y desentierra a Laura primero con [E / V]!");
                }
                return;
            }

            isDocumented = true;

            Debug.Log("<color=cyan><b>[Hallazgo Forense]</b> Escena del crimen documentada formalmente. Activando secuencia de FIN.</color>");

            // Mostrar notificación
            HUDManager hud = UnityEngine.Object.FindAnyObjectByType<HUDManager>();
            if (hud != null)
            {
                hud.ShowTemporaryNotification("🚩 Hallazgo documentado. Se entrega la escena a la Fiscalía y peritos especializados.");
            }

            // Activar pantalla de FIN
            GameEndingUI endingUI = UnityEngine.Object.FindAnyObjectByType<GameEndingUI>();
            if (endingUI != null)
            {
                endingUI.ShowGameEnding();
            }
        }
    }
}

