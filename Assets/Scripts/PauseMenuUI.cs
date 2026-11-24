using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Menú de Pausa Estandarizado (Se abre con tecla 'Esc' o 'P'):
/// - Reanudar juego.
/// - Reiniciar nivel.
/// - Silenciar / Activar audio.
/// - Volver al Menú Principal (MenuInicio).
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    private GameObject pauseRoot;
    private Text muteBtnText;
    private bool isPaused = false;

    private static bool sceneHookRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        if (sceneHookRegistered) return;
        sceneHookRegistered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Contains("Nivel_1") && !scene.name.Contains("Nivel_3")) return;
        if (Object.FindAnyObjectByType<PauseMenuUI>() == null)
        {
            GameObject go = new GameObject("PauseMenuUI");
            go.AddComponent<PauseMenuUI>();
        }
    }

    public static void AutoInit()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.Contains("Nivel_1") && !sceneName.Contains("Nivel_3")) return;

        if (Object.FindAnyObjectByType<PauseMenuUI>() == null)
        {
            GameObject go = new GameObject("PauseMenuUI");
            go.AddComponent<PauseMenuUI>();
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        bool pausePressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            pausePressed = true;
        }
#endif
        if (!pausePressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                {
                    pausePressed = true;
                }
            }
            catch { }
        }

        if (pausePressed)
        {
            // Si el diario está abierto en Nivel 1, dejar que el diario se cierre primero
            if (JournalUI.Instance != null && Time.timeScale == 0f && !isPaused)
            {
                return;
            }
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseRoot == null)
        {
            BuildPauseUI();
        }

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(true);
            pauseRoot.transform.SetAsLastSibling();
        }

        RefreshMuteLabel();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSFX();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseRoot != null)
        {
            pauseRoot.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSFX();
        }
    }

    public void RestartLevel()
    {
        ResumeGame();
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

    public void RestartFromCheckpoint()
    {
        ResumeGame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && SceneManager.GetActiveScene().name.Contains("Nivel_1"))
        {
            player.transform.position = GameState.CurrentCheckpointPosition;
            HealthSystem hs = player.GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.FullHeal();
            }
            ScoreManager.AddScore(0, player.transform.position + Vector3.up * 1.5f, "Reiniciando en Punto de Control");
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ReturnToMainMenu()
    {
        ResumeGame();
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

    private void RefreshMuteLabel()
    {
        if (muteBtnText != null)
        {
            bool muted = (AudioManager.Instance != null && AudioManager.Instance.IsMuted) || AudioListener.pause;
            muteBtnText.text = muted ? "🔇  ACTIVAR AUDIO (SILENCIADO)" : "🔊  SILENCIAR AUDIO";
        }
    }

    private void BuildPauseUI()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject cGO = new GameObject("PauseCanvas_Standard");
            canvas = cGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            CanvasScaler cs = cGO.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cGO.AddComponent<GraphicRaycaster>();
        }

        pauseRoot = new GameObject("PauseMenuUIRoot");
        pauseRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRT = pauseRoot.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        Image overlay = pauseRoot.AddComponent<Image>();
        overlay.color = new Color(0.03f, 0.04f, 0.07f, 0.85f);

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Caja central estandarizada
        GameObject box = new GameObject("PauseFrame");
        box.transform.SetParent(pauseRoot.transform, false);
        RectTransform boxRT = box.AddComponent<RectTransform>();
        boxRT.anchorMin = new Vector2(0.5f, 0.5f);
        boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(520, 440);

        Image boxBg = box.AddComponent<Image>();
        boxBg.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

        // Bordes dorados estandarizados
        CreateGoldBorder(box, "TopBorder", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -2));
        CreateGoldBorder(box, "BottomBorder", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 2));

        // Título
        GameObject titleGO = new GameObject("PauseTitle");
        titleGO.transform.SetParent(box.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1);
        titleRT.anchorMax = new Vector2(0.5f, 1);
        titleRT.anchoredPosition = new Vector2(0, -34);
        titleRT.sizeDelta = new Vector2(460, 38);

        Text title = titleGO.AddComponent<Text>();
        title.text = "⏸ MENÚ DE PAUSA";
        title.font = defaultFont;
        title.fontSize = 26;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(1f, 0.88f, 0.35f, 1f);

        // Subtítulo del nivel
        string sceneName = SceneManager.GetActiveScene().name;
        string levelSubtitle = sceneName.Contains("Nivel_3")
            ? "Nivel 3: Las Huellas del Silencio"
            : "Nivel 1: Madres Buscadoras — La Búsqueda Comienza";

        GameObject subGO = new GameObject("PauseSubtitle");
        subGO.transform.SetParent(box.transform, false);
        RectTransform subRT = subGO.AddComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.5f, 1);
        subRT.anchorMax = new Vector2(0.5f, 1);
        subRT.anchoredPosition = new Vector2(0, -68);
        subRT.sizeDelta = new Vector2(460, 24);

        Text sub = subGO.AddComponent<Text>();
        sub.text = levelSubtitle;
        sub.font = defaultFont;
        sub.fontSize = 14;
        sub.alignment = TextAnchor.MiddleCenter;
        sub.color = new Color(0.80f, 0.88f, 0.96f, 1f);

        // Botones estandarizados
        CreateMenuButton(box, "▶  REANUDAR JUEGO", -120, new Color(0.18f, 0.64f, 0.36f, 1f), ResumeGame, defaultFont);
        CreateMenuButton(box, "🔄  REINICIAR NIVEL", -182, new Color(0.22f, 0.48f, 0.72f, 1f), RestartLevel, defaultFont);
        muteBtnText = CreateMenuButton(box, "🔊  SILENCIAR AUDIO", -244, new Color(0.55f, 0.44f, 0.18f, 1f), () =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.ToggleMute();
            else AudioListener.pause = !AudioListener.pause;
            RefreshMuteLabel();
        }, defaultFont);
        CreateMenuButton(box, "🏠  MENÚ PRINCIPAL", -306, new Color(0.65f, 0.22f, 0.24f, 1f), ReturnToMainMenu, defaultFont);
    }

    private void CreateGoldBorder(GameObject parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos)
    {
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(parent.transform, false);
        RectTransform rt = bar.AddComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.sizeDelta = new Vector2(0, 4);
        rt.anchoredPosition = pos;
        Image img = bar.AddComponent<Image>();
        img.color = new Color(1f, 0.85f, 0.25f, 1f);
    }

    private Text CreateMenuButton(GameObject parent, string label, float posY, Color btnColor, UnityEngine.Events.UnityAction action, Font font)
    {
        GameObject btnGO = new GameObject($"Btn_{label}");
        btnGO.transform.SetParent(parent.transform, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(400, 48);

        Image bg = btnGO.AddComponent<Image>();
        bg.color = btnColor;

        Button btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(action);

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        RectTransform trt = txtGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text t = txtGO.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 15;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        return t;
    }
}
