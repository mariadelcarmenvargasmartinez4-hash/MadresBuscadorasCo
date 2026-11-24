using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LasHuellasDelSilencio.UI
{
    /// <summary>
    /// Secuencia narrativa final del Nivel 3 según las secciones 14 y 15 del documento:
    /// Presenta el hallazgo con sobriedad, la promesa de la madre buscadora,
    /// la intervención de peritos y el mensaje ético final.
    /// </summary>
    public class NarrativeEndingUI : MonoBehaviour
    {
        public static NarrativeEndingUI Instance { get; private set; }

        [Header("Panel Principal de Cierre")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private CanvasGroup endingCanvasGroup;

        [Header("Elementos de la Escena")]
        [SerializeField] private Image photographImage;
        [SerializeField] private Text protagonistQuoteText;
        [SerializeField] private Text solemnMessageText;
        [SerializeField] private Text badgeText;
        [SerializeField] private Button nextChapterButton;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip finalPianoMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (endingPanel != null)
                endingPanel.SetActive(false);

            if (nextChapterButton != null)
            {
                nextChapterButton.gameObject.SetActive(false);
                nextChapterButton.onClick.AddListener(OnNextChapterClicked);
            }
        }

        public void StartEndingSequence()
        {
            StartCoroutine(EndingRoutine());
        }

        private IEnumerator EndingRoutine()
        {
            if (endingPanel != null)
                endingPanel.SetActive(true);

            if (endingCanvasGroup != null)
            {
                endingCanvasGroup.alpha = 0f;
                while (endingCanvasGroup.alpha < 1f)
                {
                    endingCanvasGroup.alpha += Time.deltaTime * 0.5f;
                    yield return null;
                }
            }

            if (audioSource != null && finalPianoMusic != null)
            {
                audioSource.clip = finalPianoMusic;
                audioSource.Play();
            }

            // Frase de la madre buscadora
            if (protagonistQuoteText != null)
            {
                protagonistQuoteText.text = "«Te prometí que no dejaría de buscarte. No voy a descansar hasta saber la verdad.»";
            }
            yield return new WaitForSeconds(3.5f);

            // Mensaje ético en pantalla
            if (solemnMessageText != null)
            {
                solemnMessageText.text = "Encontrar una pista no significa terminar la búsqueda.\nCada hallazgo merece verdad, dignidad e identidad.\n\nSe ha solicitado la intervención de personal ministerial y forense especializado para la preservación formal del sitio.";
            }
            yield return new WaitForSeconds(3.5f);

            // Insignia obtenida
            if (badgeText != null)
            {
                badgeText.text = "INSIGNIA DESBLOQUEADA:\n★ «La búsqueda no termina» ★";
            }

            if (nextChapterButton != null)
            {
                nextChapterButton.gameObject.SetActive(true);
            }
        }

        private void OnNextChapterClicked()
        {
            Time.timeScale = 1f;
            PlayerPrefs.SetInt("NivelDesbloqueado", Mathf.Max(PlayerPrefs.GetInt("NivelDesbloqueado", 1), 4));
            PlayerPrefs.Save();
#if UNITY_EDITOR
            if (System.IO.File.Exists("Assets/Nivel_4.unity"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                    "Assets/Nivel_4.unity",
                    new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single)
                );
                return;
            }
#endif
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Nivel_4");
            }
            catch
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(4);
            }
        }
    }
}

