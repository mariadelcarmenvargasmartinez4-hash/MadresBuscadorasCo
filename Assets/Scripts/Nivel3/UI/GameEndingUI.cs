using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.UI
{
    /// <summary>
    /// Pantalla estandarizada de Nivel 3 Completado con progresión automática al Nivel 4.
    /// </summary>
    public class GameEndingUI : MonoBehaviour
    {
        [Header("Panel Principal de Fin")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        [Header("Escena Siguiente")]
        public string nextSceneName = "Nivel_4";

        private bool extraButtonsCreated = false;

        private void Start()
        {
            if (endingPanel != null) endingPanel.SetActive(false);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(LoadNextLevel);
            }
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ReturnToMainMenu);
            }
        }

        public void ShowGameEnding()
        {
            if (endingPanel == null)
            {
                GameObject found = GameObject.Find("Pantalla_FIN");
                if (found != null) endingPanel = found;
            }

            if (endingPanel != null)
            {
                Time.timeScale = 0f;
                endingPanel.SetActive(true);
                endingPanel.transform.SetAsLastSibling();
                EnsureStandardButtons();
            }

            PlayerPrefs.SetInt("NivelDesbloqueado", Mathf.Max(PlayerPrefs.GetInt("NivelDesbloqueado", 1), 4));
            PlayerPrefs.Save();

            if (titleText != null)
            {
                titleText.text = "🏆  ¡NIVEL 3 COMPLETADO!  🏆";
            }

            if (subtitleText != null)
            {
                subtitleText.text = "«Las Huellas del Silencio» — Escena Pericial Asegurada";
            }

            if (messageText != null)
            {
                int clues = Level3GameManager.Instance != null ? Level3GameManager.Instance.CluesCount : 0;
                int totalClues = Level3GameManager.Instance != null ? Level3GameManager.Instance.TotalClues : 10;
                int enemiesDefeated = Level3GameManager.Instance != null ? Level3GameManager.Instance.EnemiesDefeated : 0;

                messageText.text = $"¡Enhorabuena! Has superado con valentía la hacienda abandonada, derrotado a los enemigos ({enemiesDefeated} abatidos), documentado los indicios ({clues}/{totalClues}) y desenterrado con profundo respeto y dignidad a Laura bajo el Árbol Viejo.\n\n" +
                                   "La escena pericial ha sido formalmente asegurada.\n" +
                                   "Selecciona «Siguiente nivel» para continuar al Nivel 4.";
            }
        }

        private void EnsureStandardButtons()
        {
            if (extraButtonsCreated || endingPanel == null) return;
            extraButtonsCreated = true;

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(LoadNextLevel);
                Text btnTxt = restartButton.GetComponentInChildren<Text>();
                if (btnTxt != null) btnTxt.text = "▶  SIGUIENTE NIVEL (NIVEL 4)";
                RectTransform rt = restartButton.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.28f);
                    rt.anchorMax = new Vector2(0.5f, 0.28f);
                    rt.sizeDelta = new Vector2(400f, 50f);
                }
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateSecondaryButton(endingPanel.transform, "Btn_Reiniciar_Nivel3", "🔄  REINICIAR NIVEL", new Vector2(0.5f, 0.19f), new Color(0.22f, 0.48f, 0.72f, 1f), RestartGame, font);
            CreateSecondaryButton(endingPanel.transform, "Btn_MenuPrincipal_Nivel3", "🏠  MENÚ PRINCIPAL", new Vector2(0.5f, 0.10f), new Color(0.65f, 0.22f, 0.24f, 1f), ReturnToMainMenu, font);
        }

        private void CreateSecondaryButton(Transform parent, string objName, string label, Vector2 anchor, Color color, UnityEngine.Events.UnityAction action, Font font)
        {
            GameObject btnObj = new GameObject(objName);
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(400f, 46f);
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            GameObject txtObj = new GameObject(objName + "_Texto");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform trt = txtObj.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            trt.sizeDelta = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.text = label;
            txt.font = font;
            txt.fontSize = 16;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
        }

        public void LoadNextLevel()
        {
            Time.timeScale = 1f;
            string target = string.IsNullOrEmpty(nextSceneName) ? "Nivel_4" : nextSceneName;
#if UNITY_EDITOR
            if (System.IO.File.Exists("Assets/Nivel_4.unity"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                    "Assets/Nivel_4.unity",
                    new LoadSceneParameters(LoadSceneMode.Single)
                );
                return;
            }
#endif
            try
            {
                SceneManager.LoadScene(target);
            }
            catch
            {
                SceneManager.LoadScene(4);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            string activePath = SceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(activePath) && System.IO.File.Exists(activePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                    activePath,
                    new LoadSceneParameters(LoadSceneMode.Single)
                );
                return;
            }
#endif
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            if (System.IO.File.Exists("Assets/Scenes/MenuInicio.unity"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                    "Assets/Scenes/MenuInicio.unity",
                    new LoadSceneParameters(LoadSceneMode.Single)
                );
                return;
            }
#endif
            SceneManager.LoadScene("MenuInicio");
        }

        public void ExitGame()
        {
            ReturnToMainMenu();
        }
    }
}
