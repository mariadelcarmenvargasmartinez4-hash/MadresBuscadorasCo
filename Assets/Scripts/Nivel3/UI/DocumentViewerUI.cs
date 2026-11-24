using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LasHuellasDelSilencio.UI
{
    /// <summary>
    /// Ventana modal para lectura de documentos, notas, libretas y fotografías encontradas.
    /// </summary>
    public class DocumentViewerUI : MonoBehaviour
    {
        public static DocumentViewerUI Instance { get; private set; }

        [Header("Elementos de la Ventana")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button closeButton;

        private bool isOpen = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (modalPanel != null)
                modalPanel.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseDocument);
        }

        private void Update()
        {
            if (!isOpen) return;

            bool closePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                closePressed = true;
#else
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
                closePressed = true;
#endif

            if (closePressed)
            {
                CloseDocument();
            }
        }

        public void DisplayDocument(string title, string content)
        {
            isOpen = true;
            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = content;

            if (modalPanel != null)
                modalPanel.SetActive(true);

            Time.timeScale = 0f; // Pausa para lectura
        }

        public void DisplayEvidenceNote(string title, string report)
        {
            DisplayDocument($"[REGISTRO DE INDICIO] - {title}", report);
        }

        public void CloseDocument()
        {
            isOpen = false;
            if (modalPanel != null)
                modalPanel.SetActive(false);

            Time.timeScale = 1f;
        }
    }
}

