using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Meta física colocada al final del Nivel 3 con Botón Rojo en el suelo.
    /// Al pisar el botón rojo con la Madre Buscadora, se activa la victoria:
    /// "¡PASA AL NIVEL 4 GANADOR!".
    /// </summary>
    public class LevelGoalTrigger : MonoBehaviour
    {
        [Header("Configuración de Meta y Botón Rojo")]
        [SerializeField] private string goalTitle = "¡PASA AL NIVEL 4 GANADOR!";
        [SerializeField] private bool requireLauraFound = false;
        [SerializeField] private SpriteRenderer buttonRenderer;
        [SerializeField] private Sprite pressedSprite;

        private bool hasTriggered = false;

        public bool HasTriggered => hasTriggered;

        public void ConfigureButton(SpriteRenderer renderer, Sprite pressedSpr)
        {
            buttonRenderer = renderer;
            pressedSprite = pressedSpr;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            CheckAndTriggerVictory(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            CheckAndTriggerVictory(other);
        }

        private void CheckAndTriggerVictory(Collider2D other)
        {
            if (hasTriggered) return;

            PlayerMovement2D player = other.GetComponent<PlayerMovement2D>();
            if (player == null) player = other.GetComponentInParent<PlayerMovement2D>();
            bool isPlayerObj = (player != null) || other.CompareTag("Player") || other.name.Contains("MadreBuscadora");

            if (isPlayerObj)
            {
                if (requireLauraFound)
                {
                    LauraTreeGrave laura = Object.FindAnyObjectByType<LauraTreeGrave>();
                    if (laura != null && !laura.IsLauraFound)
                    {
                        if (HUDManager.Instance != null)
                        {
                            HUDManager.Instance.ShowTemporaryNotification("🌳 ¡Primero desentierra a Laura bajo el Árbol Viejo antes de cruzar la meta final!");
                        }
                        return;
                    }
                }

                TriggerVictory();
            }
        }

        public void TriggerVictory()
        {
            if (hasTriggered) return;
            hasTriggered = true;

            Debug.Log($"<color=green><b>[BOTÓN DE META PISADO]</b> ¡El jugador ha pisado el botón rojo! {goalTitle}</color>");

            // Hundir el botón rojo y cambiar a sprite presionado / activo
            if (buttonRenderer != null && pressedSprite != null)
            {
                buttonRenderer.sprite = pressedSprite;
                buttonRenderer.transform.localScale = new Vector3(1.3f, 0.95f, 1f);
            }

            // Sacudida de cámara de celebración
            CameraController2D cam = Object.FindAnyObjectByType<CameraController2D>();
            if (cam != null)
            {
                cam.Shake(0.24f, 0.4f);
            }

            // Notificación grande en HUD
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowTemporaryNotification($"🔴 🏆 ¡BOTÓN ROJO PISADO! {goalTitle}");
            }

            // Mostrar pantalla de victoria final con botón para avanzar al Nivel 4
            GameEndingUI endingUI = Object.FindAnyObjectByType<GameEndingUI>();
            if (endingUI != null)
            {
                endingUI.ShowGameEnding();
            }
        }
    }
}
