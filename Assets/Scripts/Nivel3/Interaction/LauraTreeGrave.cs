using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Fosa oculta bajo el Árbol Viejo donde está enterrada Laura.
    /// Requiere excavar en 3 golpes de pala [E] para descubrir solemnemente
    /// los restos y el pañuelo morado de Laura y actualizar el objetivo final.
    /// </summary>
    public class LauraTreeGrave : MonoBehaviour, IInteractable
    {
        [Header("Configuración de Excavación")]
        [SerializeField] private int digStepsRequired = 3;
        [SerializeField] private float stepCooldown = 0.35f;
        [SerializeField] private SpriteRenderer graveRenderer;
        [SerializeField] private Sprite moundSprite;
        [SerializeField] private Sprite uncoveredSprite;

        private int currentStep = 0;
        private float lastDigTime = -10f;
        private bool isLauraFound = false;

        public bool IsLauraFound => isLauraFound;

        private void Awake()
        {
            if (graveRenderer == null) graveRenderer = GetComponent<SpriteRenderer>();
            if (moundSprite == null) moundSprite = SpriteHelper.LoadSprite("Fosa_Laura_Tierra");
            if (uncoveredSprite == null) uncoveredSprite = SpriteHelper.LoadSprite("Fosa_Laura_Descubierta");

            if (graveRenderer != null && moundSprite != null)
            {
                SpriteHelper.ConfigureRenderer(graveRenderer, moundSprite, 8);
            }
        }

        public string GetInteractionPrompt()
        {
            if (isLauraFound)
            {
                return "🕊️ Has encontrado a Laura. Acordona la escena pericial [E / V] junto a la cinta amarilla.";
            }

            if (currentStep == 0)
            {
                return "🌳 Presiona [E / V] para Desenterrar bajo el Árbol Viejo de Laura (0/3)";
            }

            return $"⛏️ Sigue excavando entre las raíces del Árbol de Laura [E / V] ({currentStep}/{digStepsRequired})";
        }

        public bool CanInteract()
        {
            return !isLauraFound;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isLauraFound) return;
            if (Time.time < lastDigTime + stepCooldown) return;
            lastDigTime = Time.time;

            currentStep++;

            CameraController2D cam = Object.FindAnyObjectByType<CameraController2D>();
            if (cam != null) cam.Shake(0.15f, 0.14f);

            if (currentStep < digStepsRequired)
            {
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.ShowTemporaryNotification($"⛏️ Removiendo tierra bajo el Árbol Viejo ({currentStep}/{digStepsRequired})... ¡Asoma un pañuelo morado!");
                }
                return;
            }

            isLauraFound = true;
            if (graveRenderer != null && uncoveredSprite != null)
            {
                SpriteHelper.ConfigureRenderer(graveRenderer, uncoveredSprite, 8);
            }

            if (player != null)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.Heal(1);
            }

            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.MarkLauraFound();
            }

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowTemporaryNotification("🕊️ ¡ENCONTRASTE A LAURA! Enterrada junto a las raíces con su pañuelo. Ahora acordona la escena [E / V].");
                Sprite lauraPhoto = SpriteHelper.LoadSprite("Foto_Cedula_Laura");
                HUDManager.Instance.ShowSearchCard(
                    lauraPhoto,
                    "Laura Elena Morales Garza",
                    "23 años",
                    "19 de Octubre de 2021",
                    "Señas particulares: Portaba su pañuelo morado bordado a mano con iniciales. Localizada bajo las raíces del Árbol Viejo tras incansable búsqueda colectiva.\n\"Hija mía, tu madre jamás te olvidó. Hoy por fin vuelves a la luz y a la memoria.\""
                );
            }
        }
    }
}

