using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Controla la Pantalla de Inicio (Menu Principal) de Madres Buscadoras ("Hasta Encontrarles").
/// - Compatible 100% con WebGL, Windows y Movil (carga portada y boton JUGAR desde Resources).
/// - Evita que el clic de "Run Game" del navegador salte automaticamente al Nivel 1.
/// - Garantiza que el Menu Principal con el boton JUGAR sea siempre la primera pantalla al iniciar el juego.
/// </summary>
public class StartScreenManager : MonoBehaviour
{
    private static bool menuMostradoEnEstaSesion = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void GarantizarMenuPrincipalAlInicio()
    {
        Scene escenaActual = SceneManager.GetActiveScene();
        if (escenaActual.name == "MenuInicio")
        {
            menuMostradoEnEstaSesion = true;
            if (Object.FindAnyObjectByType<StartScreenManager>() == null)
            {
                GameObject go = new GameObject("StartScreenManager_Auto");
                go.AddComponent<StartScreenManager>();
            }
            return;
        }

        // Si el juego arranco directamente en Nivel_1 sin haber pasado por el Menu Principal, cargar MenuInicio primero
        if (!menuMostradoEnEstaSesion && escenaActual.name.Contains("Nivel_1"))
        {
            menuMostradoEnEstaSesion = true;
            if (Application.CanStreamedLevelBeLoaded("MenuInicio"))
            {
                SceneManager.LoadScene("MenuInicio");
                return;
            }
        }
    }

    [Header("Escena Objetivo")]
    public string targetSceneName = "Nivel_1";
    public bool loadSceneOnStart = true;

    [Header("Referencias de UI")]
    public CanvasGroup canvasGroup;
    public RectTransform backgroundTransform;
    public Image backgroundImage;
    public Button playButton;
    public Text playButtonText;
    public GameObject hudToEnable;
    public Image buttonGlowImage;

    [Header("Animacion del Boton")]
    public float pulseSpeed = 3.5f;
    public float pulseScaleAmount = 0.05f;
    public float pulseAmount = 0.04f;

    [Header("Respiracion Cinematica")]
    public bool enableBackgroundBreathe = true;
    public float breatheSpeed = 0.6f;
    public float breatheScaleAmount = 0.02f;

    [Header("Particulas de Polvo / Brasas Flotantes")]
    public bool enableEmbers = true;
    public int emberCount = 22;

    [Header("Transicion")]
    public float fadeDuration = 0.35f;

    private bool gameStarted = false;
    private float tiempoEnMenu = 0f;
    private Vector3 buttonBaseScale = Vector3.one;
    private Vector3 bgBaseScale = Vector3.one;
    private bool isHovered = false;

    private static Sprite softCircleSprite;

    private class Ember
    {
        public RectTransform rect;
        public Image img;
        public Vector2 velocity;
        public float swaySpeed;
        public float swayAmount;
        public float seed;
        public float baseAlpha;
    }
    private List<Ember> embers = new List<Ember>();

    void Awake()
    {
        menuMostradoEnEstaSesion = true;
        tiempoEnMenu = 0f;
        gameStarted = false;

        AsegurarEventSystemCompatible();

        Canvas parentCanvas = GetComponentInParent<Canvas>() ?? GetComponent<Canvas>() ?? Object.FindAnyObjectByType<Canvas>();
        if (parentCanvas == null)
        {
            GameObject cGO = new GameObject("Canvas_MenuPrincipal");
            parentCanvas = cGO.AddComponent<Canvas>();
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            parentCanvas.sortingOrder = 100;
            CanvasScaler cs = cGO.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            cs.matchWidthOrHeight = 0.5f;
            cGO.AddComponent<GraphicRaycaster>();
        }

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? parentCanvas.GetComponent<CanvasGroup>();

        // 1. Asegurar referencia del fondo
        if (backgroundImage == null)
        {
            var images = parentCanvas.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject.name.Contains("Background"))
                {
                    backgroundImage = img;
                    backgroundTransform = img.rectTransform;
                    break;
                }
            }
        }
        if (backgroundImage == null)
        {
            GameObject bgGO = new GameObject("Background_Artwork");
            bgGO.transform.SetParent(parentCanvas.transform, false);
            bgGO.transform.SetAsFirstSibling();
            backgroundTransform = bgGO.AddComponent<RectTransform>();
            backgroundTransform.anchorMin = Vector2.zero;
            backgroundTransform.anchorMax = Vector2.one;
            backgroundTransform.offsetMin = Vector2.zero;
            backgroundTransform.offsetMax = Vector2.zero;
            backgroundImage = bgGO.AddComponent<Image>();
        }

        if (backgroundTransform != null)
            bgBaseScale = backgroundTransform.localScale;

        // 2. Cargar portada decorada (Compatible con WebGL mediante Resources.Load)
        EnsureBackgroundLoaded();

        // 3. Reproducir musica de menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDesaparecidosBGM();
        }

        // 4. Configurar Boton JUGAR visible e interactivo
        if (playButton == null)
            playButton = parentCanvas.GetComponentInChildren<Button>(true);
        if (playButton == null)
            playButton = GameObject.Find("Button_START")?.GetComponent<Button>();

        if (playButton == null)
        {
            GameObject btnGO = new GameObject("Button_START");
            btnGO.transform.SetParent(parentCanvas.transform, false);
            btnGO.transform.SetAsLastSibling();
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.35f, 0.14f);
            btnRT.anchorMax = new Vector2(0.65f, 0.30f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;
            Image bImg = btnGO.AddComponent<Image>();
            bImg.color = Color.white;
            playButton = btnGO.AddComponent<Button>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        if (playButton != null)
        {
            if (playButton.transform.localScale == Vector3.zero)
                playButton.transform.localScale = Vector3.one;

            // Posicionar el boton JUGAR en una zona clara y destacada de la portada
            RectTransform rtBtn = playButton.GetComponent<RectTransform>();
            if (rtBtn != null)
            {
                rtBtn.anchorMin = new Vector2(0.36f, 0.12f);
                rtBtn.anchorMax = new Vector2(0.64f, 0.28f);
                rtBtn.anchoredPosition = Vector2.zero;
                rtBtn.sizeDelta = Vector2.zero;
            }

            buttonBaseScale = playButton.transform.localScale;
            playButton.interactable = true;
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(StartGame);

            bool hasDecoratedSprite = false;
            Image btnImg = playButton.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.raycastTarget = true;
                Sprite bSprite = CargarSpriteRecurso("btn_jugar_decorado");
                if (bSprite == null) bSprite = CargarSpriteRecurso("btn_start");
                if (bSprite != null)
                {
                    btnImg.sprite = bSprite;
                    btnImg.color = Color.white;
                    btnImg.preserveAspect = true;
                    hasDecoratedSprite = true;
                }
                else
                {
                    btnImg.color = new Color(0.16f, 0.62f, 0.34f, 0.96f);
                }
            }

            Text btnTxt = playButton.GetComponentInChildren<Text>(true);
            if (hasDecoratedSprite)
            {
                if (btnTxt != null) btnTxt.gameObject.SetActive(false);
            }
            else
            {
                if (btnTxt == null)
                {
                    GameObject txtGO = new GameObject("BtnText");
                    txtGO.transform.SetParent(playButton.transform, false);
                    btnTxt = txtGO.AddComponent<Text>();
                    RectTransform rt = txtGO.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
                Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 32);
                btnTxt.font = f;
                btnTxt.text = "▶ JUGAR";
                btnTxt.fontSize = 40;
                btnTxt.fontStyle = FontStyle.Bold;
                btnTxt.alignment = TextAnchor.MiddleCenter;
                btnTxt.color = Color.white;
                btnTxt.gameObject.SetActive(true);
            }

            var trigger = playButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = playButton.gameObject.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((_) => { isHovered = true; });
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => { isHovered = false; });
            trigger.triggers.Add(exit);
        }

        Time.timeScale = 1f;

        if (enableEmbers)
        {
            CreateProceduralEmbers();
        }
    }

    private void AsegurarEventSystemCompatible()
    {
        EventSystem es = Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            es = esGO.AddComponent<EventSystem>();
        }
#if ENABLE_INPUT_SYSTEM
        StandaloneInputModule legacyMod = es.GetComponent<StandaloneInputModule>();
        if (legacyMod != null) Destroy(legacyMod);
        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>();
#else
        if (es.GetComponent<StandaloneInputModule>() == null)
            es.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private static Sprite CargarSpriteRecurso(string nombreSinExtension)
    {
        Sprite sp = Resources.Load<Sprite>(nombreSinExtension);
        if (sp != null) return sp;

        Texture2D tex = Resources.Load<Texture2D>(nombreSinExtension);
        if (tex != null)
        {
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        return null;
    }

    void EnsureBackgroundLoaded()
    {
        if (backgroundImage == null) return;

        // 1. Cargar desde Resources (Funciona en WebGL, Windows y Movil)
        Sprite spDecorada = CargarSpriteRecurso("pantalla_inicio_decorada");
        if (spDecorada == null) spDecorada = CargarSpriteRecurso("pantalla_inicio_bg");
        if (spDecorada == null) spDecorada = CargarSpriteRecurso("Sprites/poster_22_desaparecidos");

        if (spDecorada != null)
        {
            backgroundImage.sprite = spDecorada;
            backgroundImage.color = Color.white;
            return;
        }

        // 2. Respaldo en Editor por ruta de archivo
        string path = System.IO.Path.Combine(Application.dataPath, "Sprites/UI/pantalla_inicio_decorada.png");
        if (!System.IO.File.Exists(path))
            path = System.IO.Path.Combine(Application.dataPath, "Sprites/UI/pantalla_inicio_bg.png");

        if (System.IO.File.Exists(path))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (texture.LoadImage(fileData))
            {
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                backgroundImage.sprite = newSprite;
                backgroundImage.color = Color.white;
            }
        }
    }

    void CreateProceduralEmbers()
    {
        EnsureSoftCircleSprite();

        Transform parent = transform;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null) parent = parentCanvas.transform;

        GameObject embersRoot = new GameObject("EmbersContainer");
        embersRoot.transform.SetParent(parent, false);
        embersRoot.transform.SetSiblingIndex(Mathf.Max(0, parent.childCount - 2));

        for (int i = 0; i < emberCount; i++)
        {
            GameObject eGO = new GameObject($"Ember_{i}");
            eGO.transform.SetParent(embersRoot.transform, false);

            RectTransform rt = eGO.AddComponent<RectTransform>();
            float size = Random.Range(6f, 16f);
            rt.sizeDelta = new Vector2(size, size);

            float startX = Random.Range(-960f, 960f);
            float startY = Random.Range(-540f, 540f);
            rt.anchoredPosition = new Vector2(startX, startY);

            Image img = eGO.AddComponent<Image>();
            img.sprite = softCircleSprite;
            Color emberColor = Color.Lerp(new Color(1f, 0.72f, 0.18f, 0.85f), new Color(1f, 0.28f, 0.08f, 0.95f), Random.value);
            img.color = emberColor;
            img.raycastTarget = false;

            embers.Add(new Ember
            {
                rect = rt,
                img = img,
                velocity = new Vector2(Random.Range(20f, 50f), Random.Range(35f, 85f)),
                swaySpeed = Random.Range(1.5f, 3.5f),
                swayAmount = Random.Range(20f, 40f),
                seed = Random.Range(0f, 100f),
                baseAlpha = Random.Range(0.45f, 0.95f)
            });
        }
    }

    void EnsureSoftCircleSprite()
    {
        if (softCircleSprite != null) return;

        int res = 32;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(res * 0.5f, res * 0.5f);
        float radius = res * 0.5f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float norm = Mathf.Clamp01(dist / radius);
                float alpha = Mathf.SmoothStep(1f, 0f, norm);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        softCircleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (gameStarted) return;

        tiempoEnMenu += Time.unscaledDeltaTime;
        float t = Time.unscaledTime;

        // 1. Animacion del Boton JUGAR (Pulso + Hover)
        if (playButton != null)
        {
            float targetHoverScale = isHovered ? 1.08f : 1.0f;
            float pulse = Mathf.Sin(t * pulseSpeed) * pulseScaleAmount;
            float s = targetHoverScale + pulse;
            playButton.transform.localScale = Vector3.Lerp(playButton.transform.localScale, buttonBaseScale * s, Time.unscaledDeltaTime * 10f);

            if (buttonGlowImage != null)
            {
                float glowAlpha = 0.4f + Mathf.Sin(t * (pulseSpeed * 1.2f)) * 0.35f;
                if (isHovered) glowAlpha = 0.9f;
                Color gc = buttonGlowImage.color;
                gc.a = glowAlpha;
                buttonGlowImage.color = gc;
            }
        }

        // 2. Respiracion Cinematica del Fondo
        if (enableBackgroundBreathe && backgroundTransform != null)
        {
            float bgS = 1f + Mathf.Sin(t * breatheSpeed) * breatheScaleAmount;
            backgroundTransform.localScale = bgBaseScale * bgS;

            float rot = Mathf.Sin(t * (breatheSpeed * 0.5f)) * 0.25f;
            backgroundTransform.localRotation = Quaternion.Euler(0, 0, rot);
        }

        // 3. Animacion de Brasas / Polvo del Desierto
        if (embers.Count > 0)
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < embers.Count; i++)
            {
                var e = embers[i];
                if (e.rect == null) continue;

                Vector2 pos = e.rect.anchoredPosition;
                pos.x += (e.velocity.x + Mathf.Sin(t * e.swaySpeed + e.seed) * e.swayAmount) * dt;
                pos.y += e.velocity.y * dt;

                if (pos.y > 580f || pos.x > 1050f)
                {
                    pos.y = -580f;
                    pos.x = Random.Range(-960f, 600f);
                }
                e.rect.anchoredPosition = pos;

                if (e.img != null)
                {
                    float alpha = e.baseAlpha * (0.6f + Mathf.PingPong(t * 2f + e.seed, 0.4f));
                    Color c = e.img.color;
                    c.a = alpha;
                    e.img.color = c;
                }
            }
        }

        // 4. Esperar al menos 1.2 segundos para que el clic de "Run Game" del navegador NO salte el menu principal.
        // Despues de 1.2s, el jugador inicia dando clic en el boton JUGAR o presionando Enter / Espacio.
        if (tiempoEnMenu < 1.2f) return;

        bool presionoEnterOEspacio = false;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            presionoEnterOEspacio = true;
#endif
        try
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                presionoEnterOEspacio = true;
        }
        catch { }

        if (presionoEnterOEspacio)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (gameStarted || tiempoEnMenu < 0.35f) return;
        gameStarted = true;
        Debug.Log("[StartScreenManager] ¡Boton JUGAR presionado! Iniciando Nivel 1...");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClickSFX();
        }

        Time.timeScale = 1f;
        string target = string.IsNullOrEmpty(targetSceneName) ? "Nivel_1" : targetSceneName;
#if UNITY_EDITOR
        if (System.IO.File.Exists("Assets/Nivel_1.unity"))
        {
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/Nivel_1.unity",
                new LoadSceneParameters(LoadSceneMode.Single)
            );
            return;
        }
#endif
        try
        {
            SceneManager.LoadScene(target);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[StartScreenManager] No se pudo cargar por nombre '{target}', intentando indice 1: {e.Message}");
            SceneManager.LoadScene(1);
        }
    }
}
