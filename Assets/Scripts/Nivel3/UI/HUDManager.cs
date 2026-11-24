using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.UI
{
    /// <summary>
    /// Gestiona la visualización en pantalla del HUD: barra gráfica de vida con corazones,
    /// barra de energía para correr, objetivos, linterna, notificaciones y pantalla de derrota.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("Corazones de Vida")]
        [SerializeField] private Text heartText;
        [SerializeField] private Image[] heartImages;
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;
        [SerializeField] private Color heartNormalColor = Color.white;
        [SerializeField] private Color heartFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Image healthFillImage;

        [Header("Energía / Correr")]
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Text sprintStateText;

        [Header("Textos de Misión")]
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text cluesText;
        [SerializeField] private Text documentsText;
        [SerializeField] private Text enemiesText;
        [SerializeField] private Text interactionPromptText;
        [SerializeField] private GameObject interactionPromptPanel;

        [Header("Barra de Linterna")]
        [SerializeField] private GameObject flashlightPanel;
        [SerializeField] private Slider batterySlider;

        [Header("Notificaciones Rápidas")]
        [SerializeField] private Text notificationText;
        [SerializeField] private CanvasGroup notificationCanvasGroup;

        [Header("Pantalla de Derrota")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button retryButton;

        [Header("Banner de Victoria")]
        [SerializeField] private GameObject victoryBannerObj;
        [SerializeField] private Text victoryBannerText;

        [Header("Cédula de Búsqueda Forense")]
        [SerializeField] private GameObject searchCardModal;
        [SerializeField] private Image searchCardPhoto;
        [SerializeField] private Text searchCardTitleText;
        [SerializeField] private Text searchCardNameText;
        [SerializeField] private Text searchCardAgeText;
        [SerializeField] private Text searchCardDateText;
        [SerializeField] private Text searchCardDetailsText;
        [SerializeField] private Button searchCardCloseButton;

        private Coroutine notificationCoroutine;
        private Action onRetryCallback;
        private int cachedCurrentHearts = 3;
        private int cachedMaxHearts = 3;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (interactionPromptPanel != null)
                interactionPromptPanel.SetActive(false);

            if (searchCardModal != null)
                searchCardModal.SetActive(false);

            if (searchCardCloseButton != null)
            {
                searchCardCloseButton.onClick.RemoveListener(CloseSearchCard);
                searchCardCloseButton.onClick.AddListener(CloseSearchCard);
            }

            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(OnRetryClicked);
                retryButton.onClick.AddListener(OnRetryClicked);
            }
            PlayerHealth health = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.OnHealthChanged += UpdateHearts;
                health.OnDamaged += FlashHeartsOnDamage;
                UpdateHearts(health.CurrentHearts, health.MaxHearts);
            }

            PlayerMovement2D movement = UnityEngine.Object.FindAnyObjectByType<PlayerMovement2D>();
            if (movement != null)
            {
                movement.OnStaminaChanged += UpdateStamina;
                UpdateStamina(movement.StaminaNormalized, movement.IsSprinting || movement.IsDashing);
            }

            PlayerFlashlight flashlight = UnityEngine.Object.FindAnyObjectByType<PlayerFlashlight>();
            if (flashlight != null)
            {
                flashlight.OnBatteryChanged += UpdateBattery;
                flashlight.OnFlashlightToggled += SetFlashlightUIVisible;
                if (flashlightPanel != null)
                {
                    flashlightPanel.SetActive(flashlight.HasFlashlight);
                }
            }
        }

        public void UpdateStamina(float normalized, bool isSprinting)
        {
            if (staminaSlider != null)
            {
                staminaSlider.value = Mathf.Clamp01(normalized);
            }

            if (sprintStateText != null)
            {
                sprintStateText.text = isSprinting
                    ? "⚡ CORRIENDO [Shift]"
                    : $"⚡ ENERGÍA [Shift]: {Mathf.RoundToInt(normalized * 100f)}%";
                sprintStateText.color = isSprinting
                    ? new Color(0.25f, 0.95f, 1f, 1f)
                    : new Color(0.85f, 0.9f, 0.95f, 1f);
            }
        }

        public void UpdateHearts(int current, int max)
        {
            cachedCurrentHearts = current;
            cachedMaxHearts = max;

            if (heartText != null)
            {
                heartText.text = $"LÍNEA DE VIDA: {current}/{max}";
            }

            if (healthFillImage != null && max > 0)
            {
                healthFillImage.fillAmount = Mathf.Clamp01((float)current / max);
            }

            if (heartImages != null)
            {
                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] == null) continue;

                    if (i >= max)
                    {
                        heartImages[i].gameObject.SetActive(false);
                        continue;
                    }

                    heartImages[i].gameObject.SetActive(true);

                    if (i < current)
                    {
                        if (fullHeartSprite != null) heartImages[i].sprite = fullHeartSprite;
                        heartImages[i].color = heartNormalColor;
                    }
                    else
                    {
                        if (emptyHeartSprite != null) heartImages[i].sprite = emptyHeartSprite;
                        heartImages[i].color = new Color(1f, 1f, 1f, 0.45f);
                    }
                }
            }
        }

        public void FlashHeartsOnDamage()
        {
            StartCoroutine(FlashHeartsRoutine());
        }

        private IEnumerator FlashHeartsRoutine()
        {
            if (heartImages == null) yield break;

            for (int i = 0; i < 3; i++)
            {
                foreach (var img in heartImages)
                {
                    if (img != null && img.gameObject.activeSelf)
                        img.color = heartFlashColor;
                }
                yield return new WaitForSeconds(0.1f);
                UpdateHearts(cachedCurrentHearts, cachedMaxHearts);
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void UpdateObjective(string objective)
        {
            if (objectiveText != null)
            {
                objectiveText.text = $"Objetivo: {objective}";
            }
        }

        public void UpdateClues(int current, int total)
        {
            if (cluesText != null)
            {
                cluesText.text = $"Indicios Marcados: {current} / {total}";
            }
        }

        public void UpdateDocuments(int current, int total)
        {
            if (documentsText != null)
            {
                documentsText.text = $"Documentos: {current} / {total}";
            }
        }

        public void UpdateEnemiesDefeated(int defeated)
        {
            if (enemiesText != null)
            {
                enemiesText.text = $"⚔️ Enemigos abatidos: {defeated}";
            }
        }

        public void UpdateBattery(float normalized)
        {
            if (batterySlider != null)
            {
                batterySlider.value = normalized;
            }
        }

        public void SetFlashlightUIVisible(bool visible)
        {
            if (flashlightPanel != null)
            {
                flashlightPanel.SetActive(visible);
            }
        }

        public void ShowInteractionPrompt(string prompt)
        {
            if (interactionPromptPanel != null)
                interactionPromptPanel.SetActive(true);

            if (interactionPromptText != null)
                interactionPromptText.text = prompt;
        }

        public void HideInteractionPrompt()
        {
            if (interactionPromptPanel != null)
                interactionPromptPanel.SetActive(false);
        }

        public void ShowTemporaryNotification(string message)
        {
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            notificationCoroutine = StartCoroutine(NotificationRoutine(message));
        }

        private IEnumerator NotificationRoutine(string message)
        {
            if (notificationText != null) notificationText.text = message;
            if (notificationCanvasGroup != null)
            {
                notificationCanvasGroup.alpha = 1f;
                yield return new WaitForSeconds(2.6f);
                float fade = 1f;
                while (fade > 0f)
                {
                    fade -= Time.deltaTime * 2f;
                    notificationCanvasGroup.alpha = fade;
                    yield return null;
                }
                notificationCanvasGroup.alpha = 0f;
            }
        }

        public void ShowVictoryAnnouncement(string victoryMsg)
        {
            if (victoryBannerObj != null)
            {
                victoryBannerObj.SetActive(true);
                if (victoryBannerText != null) victoryBannerText.text = victoryMsg;
            }
            ShowTemporaryNotification(victoryMsg);
        }

        public void ShowGameOverModal(Action retryAction)
        {
            onRetryCallback = retryAction;
            if (gameOverPanel != null)
            {
                Time.timeScale = 0f;
                gameOverPanel.SetActive(true);
                gameOverPanel.transform.SetAsLastSibling();
            }
            else
            {
                ShowTemporaryNotification("💔 ¡Tu Línea de vida se agotó! Reapareciendo en el punto de control...");
                StartCoroutine(AutoRespawnFallbackRoutine());
            }
        }

        private IEnumerator AutoRespawnFallbackRoutine()
        {
            yield return new WaitForSecondsRealtime(1.6f);
            Time.timeScale = 1f;
            onRetryCallback?.Invoke();
        }

        private void OnRetryClicked()
        {
            Time.timeScale = 1f;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            onRetryCallback?.Invoke();
        }

        public bool IsSearchCardOpen => searchCardModal != null && searchCardModal.activeSelf;

        public void ShowSearchCard(Sprite photo, string name, string age, string disappearanceDate, string details, Action onClose = null)
        {
            if (searchCardModal == null) return;

            if (searchCardPhoto != null && photo != null)
            {
                searchCardPhoto.sprite = photo;
                searchCardPhoto.color = Color.white;
            }

            if (searchCardTitleText != null)
            {
                searchCardTitleText.text = "🕊️ CÉDULA DE IDENTIFICACIÓN Y REGISTRO FORENSE";
            }

            if (searchCardNameText != null)
            {
                searchCardNameText.text = $"NOMBRE: {name.ToUpper()}";
            }

            if (searchCardAgeText != null)
            {
                searchCardAgeText.text = $"EDAD AL DESAPARECER: {age}";
            }

            if (searchCardDateText != null)
            {
                searchCardDateText.text = $"FECHA DE DESAPARICIÓN: {disappearanceDate}";
            }

            if (searchCardDetailsText != null)
            {
                searchCardDetailsText.text = $"DATOS DE LOCALIZACIÓN Y SEÑAS PARTICULARES:\n{details}";
            }

            searchCardModal.SetActive(true);

            if (searchCardCloseButton != null)
            {
                searchCardCloseButton.onClick.RemoveAllListeners();
                searchCardCloseButton.onClick.AddListener(() =>
                {
                    CloseSearchCard();
                    onClose?.Invoke();
                });
            }
        }

        public void CloseSearchCard()
        {
            if (searchCardModal != null)
            {
                searchCardModal.SetActive(false);
            }
        }

        private void Update()
        {
            if (IsSearchCardOpen)
            {
#if ENABLE_INPUT_SYSTEM
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                {
                    CloseSearchCard();
                }
#else
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CloseSearchCard();
                }
#endif
            }
        }
    }
}
