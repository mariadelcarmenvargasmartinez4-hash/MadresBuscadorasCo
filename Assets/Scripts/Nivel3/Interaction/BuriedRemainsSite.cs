using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Systems;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Fosa clandestina con restos humanos enterrados localizados mediante prospección.
    /// Permite a la Madre Buscadora remover la tierra con la pala [V / E] hasta
    /// descubrir y documentar los restos óseos con solemnidad y colocar un banderín forense.
    /// </summary>
    public class BuriedRemainsSite : MonoBehaviour, IInteractable
    {
        [Header("Datos de los Restos")]
        [SerializeField] private string remainsTitle = "Fosa Clandestina con Restos Óseos";
        [TextArea(2, 4)]
        [SerializeField] private string description = "Osamenta humana y prendas civiles recuperadas en estrato de tierra alterada. Se documenta formalmente para identificación pericial.";
        [SerializeField] private int digStepsRequired = 2;
        [SerializeField] private float stepCooldown = 0.35f;

        [Header("Cédula de Persona Desaparecida")]
        [SerializeField] private Sprite personPhoto;
        [SerializeField] private string personName = "Persona Sin Identificar";
        [SerializeField] private string personAge = "Edad desconocida";
        [SerializeField] private string disappearanceDate = "Fecha indeterminada";
        [TextArea(2, 4)]
        [SerializeField] private string additionalDetails = "Indicios recuperados en cuadrante pericial de la hacienda.";

        [Header("Referencias Visuales")]
        [SerializeField] private SpriteRenderer graveRenderer;
        [SerializeField] private Sprite buriedMoundSprite;
        [SerializeField] private Sprite uncoveredRemainsSprite;

        private int currentStep = 0;
        private float lastDigTime = -10f;
        private bool isExcavated = false;

        public bool IsExcavated => isExcavated;
        public string RemainsTitle => remainsTitle;

        private void Awake()
        {
            if (graveRenderer == null) graveRenderer = GetComponent<SpriteRenderer>();
            if (buriedMoundSprite == null) buriedMoundSprite = SpriteHelper.LoadSprite("Fosa_Restos_Enterrados");
            if (uncoveredRemainsSprite == null) uncoveredRemainsSprite = SpriteHelper.LoadSprite("Fosa_Restos_Descubiertos");

            if (graveRenderer != null && buriedMoundSprite != null)
            {
                SpriteHelper.ConfigureRenderer(graveRenderer, buriedMoundSprite, 8);
            }
        }

        public void Configure(string title, string desc, int steps)
        {
            remainsTitle = title;
            description = desc;
            digStepsRequired = Mathf.Max(1, steps);
        }

        public void ConfigureSearchCard(Sprite photo, string name, string age, string date, string details)
        {
            personPhoto = photo;
            personName = name;
            personAge = age;
            disappearanceDate = date;
            additionalDetails = details;
        }

        public string GetInteractionPrompt()
        {
            if (isExcavated)
            {
                return $"🕊️ {remainsTitle} (Sitio pericial documentado)";
            }

            if (currentStep == 0)
            {
                return $"⚰️ Presiona [V / E] para Desenterrar {remainsTitle} (0/{digStepsRequired})";
            }

            return $"⛏️ Sigue excavando restos bajo tierra [V / E] ({currentStep}/{digStepsRequired})";
        }

        public bool CanInteract()
        {
            return !isExcavated;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isExcavated) return;
            if (Time.time < lastDigTime + stepCooldown) return;
            lastDigTime = Time.time;

            currentStep++;

            CameraController2D cam = Object.FindAnyObjectByType<CameraController2D>();
            if (cam != null) cam.Shake(0.15f, 0.12f);

            if (currentStep < digStepsRequired)
            {
                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.ShowTemporaryNotification($"⛏️ Removiendo tierra ({currentStep}/{digStepsRequired})... ¡Asoman fragmentos óseos e indicios textiles!");
                }
                return;
            }

            // Descubrimiento completo de los restos
            isExcavated = true;
            if (graveRenderer != null && uncoveredRemainsSprite != null)
            {
                SpriteHelper.ConfigureRenderer(graveRenderer, uncoveredRemainsSprite, 8);
            }

            if (player != null)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.Heal(1);
            }

            if (Level3GameManager.Instance != null)
            {
                Level3GameManager.Instance.RegisterEvidence(remainsTitle, false);
            }

            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.ShowTemporaryNotification($"🕊️ ¡RESTOS LOCALIZADOS! {remainsTitle} documentados con banderín forense (+1 Salud ♥).");
                HUDManager.Instance.ShowSearchCard(personPhoto, personName, personAge, disappearanceDate, additionalDetails);
            }
        }
    }
}

