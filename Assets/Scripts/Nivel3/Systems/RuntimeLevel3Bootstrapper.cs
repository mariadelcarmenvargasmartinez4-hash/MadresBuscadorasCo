using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Enemies;
using LasHuellasDelSilencio.Interaction;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Systems
{
    /// <summary>
    /// Constructor canónico y Bootstrapper automático del Nivel 3: La Hacienda Abandonada.
    /// Usado tanto por el Editor (Level3SceneBuilder) como en tiempo de ejecución (Runtime).
    /// Configura movimiento fluido con carrera/esquive, barra de vida con corazones,
    /// barra de energía, múltiples enemigos con sprites propios (Vigilantes, Perseguidores,
    /// Guardián Acústico y 4 Patrullas Policiales), puerta con acertijo, fosa clandestina y FIN.
    /// </summary>
    public static class RuntimeLevel3Bootstrapper
    {
        private static bool sceneHookRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadHook()
        {
            if (sceneHookRegistered) return;
            sceneHookRegistered = true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "Nivel_3" || scene.name.Contains("Nivel_3") || scene.name.Contains("Level3"))
            {
                Time.timeScale = 1f;
                InitializeLevelIfEmpty();
            }
        }

        public static void InitializeLevelIfEmpty()
        {
            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeSceneName != "Nivel_3" && !activeSceneName.Contains("Nivel_3") && !activeSceneName.Contains("Level3"))
            {
                return;
            }

            CleanupStrayMenuSceneObjects();

            GameObject existingWorld = GameObject.Find("Hacienda_Vieja_Entorno");
            GameObject existingPlayer = GameObject.Find("MadreBuscadora_Player");

            bool hasStrayCanvas = false;
            foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (c != null && c.gameObject.name != "Canvas_HUD" && !c.gameObject.name.Contains("PauseMenu")) { hasStrayCanvas = true; break; }
            }

            if (existingWorld != null && existingPlayer != null && !hasStrayCanvas)
            {
                SpriteRenderer pSr = existingPlayer.GetComponent<SpriteRenderer>();
                bool hasImprovedSystems = GameObject.Find("LineaDeVida_V2_OK") != null
                    && GameObject.Find("Objetos_Vida_Salud") != null
                    && GameObject.Find("Etiqueta_Linea_Vida") != null
                    && GameObject.Find("Barra_Vida_Contenedor") != null
                    && GameObject.Find("Barra_Energia_Correr") != null
                    && GameObject.Find("Panel_GameOver") != null
                    && GameObject.Find("Puerta_Con_Acertijo") != null
                    && GameObject.Find("Tumba_De_Laura") != null
                    && GameObject.Find("Escena_Fosa_Clandestina") != null
                    && GameObject.Find("Fosas_Restos_Enterrados") != null
                    && GameObject.Find("Vigilante_Patio") != null
                    && GameObject.Find("MadreBuscadora_Player") != null
                    && GameObject.Find("MadreBuscadora_Player").GetComponent<PlayerCombat2D>() != null
                    && GameObject.Find("Modal_Cedula_Busqueda") != null
                    && GameObject.Find("Meta_Final_Nivel3") != null
                    && GameObject.Find("Vigilante_Patio_Sur") != null
                    && GameObject.Find("Vigilante_Caseta_Norte") != null;

                if (pSr != null && pSr.sortingOrder == 50 && pSr.sprite != null && hasImprovedSystems)
                {
                    return;
                }
            }

            Debug.Log("<color=cyan><b>[Las Huellas del Silencio]</b> Configurando Nivel 3 completo en Runtime...</color>");
            BuildCanonicalLevel(null);
            Debug.Log("<color=green><b>[Las Huellas del Silencio]</b> Escenario en Runtime listo con Correr, Vida, 24 Enemigos, Cédula de Búsqueda, Policía y FIN.</color>");
        }

        public static void BuildCanonicalLevel(Transform optionalRoot)
        {
            // 1. Limpiar objetos previos para evitar duplicados
            string[] cleanupNames = new string[]
            {
                "--- NIVEL 3: LAS HUELLAS DEL SILENCIO ---",
                "Hacienda_Vieja_Entorno",
                "MadreBuscadora_Player",
                "Level3_GameManager",
                "Puntos_De_Control",
                "Botellas_Agua_Visibles",
                "Botellas_De_Agua",
                "Objetos_Vida_Salud",
                "Zonas_Privadas_Triggers",
                "Zona_Privada_Trigger",
                "Enemigos",
                "Meta_Final_Nivel3",
                "Canvas_HUD"
            };

            foreach (var name in cleanupNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    obj.SetActive(false);
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }

            CleanupStrayMenuSceneObjects();

            // 2. Configurar Cámara Principal
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4.8f;
            mainCam.backgroundColor = new Color(0.12f, 0.10f, 0.08f, 1f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.cullingMask = -1;
            mainCam.depth = 0;
            mainCam.transform.position = new Vector3(-8f, -0.2f, -10f);

            // 3. GameManager
            GameObject gmObj = CreateChild("Level3_GameManager", optionalRoot);
            gmObj.AddComponent<Level3GameManager>();

            // 4. Jugador (Madre Buscadora) - Primer plano (sortingOrder = 50)
            GameObject player = CreateChild("MadreBuscadora_Player", optionalRoot);
            player.transform.position = new Vector3(-8f, -0.8f, 0f);

            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            Sprite playerSprite = SpriteHelper.LoadSprite("MadreBuscadora_Protagonista");
            SpriteHelper.ConfigureRenderer(sr, playerSprite, 50);
            player.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = player.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.offset = new Vector2(0f, -0.3f);

            player.AddComponent<PlayerNoiseEmitter>();
            player.AddComponent<PlayerMovement2D>();
            player.AddComponent<PlayerCombat2D>();
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerFlashlight>();
            player.AddComponent<PlayerInteraction>();
            player.AddComponent<PlayerWalkAnimator2D>();

            CameraController2D camCtrl = mainCam.GetComponent<CameraController2D>();
            if (camCtrl == null) camCtrl = mainCam.gameObject.AddComponent<CameraController2D>();
            SetPrivateField(camCtrl, "maxX", 215f);
            SetPrivateField(camCtrl, "offset", new Vector3(0f, 0.6f, -10f));
            camCtrl.SetTarget(player.transform, true);

            // 5. Escenario
            BuildWorldHierarchy(optionalRoot);

            // 6. Checkpoints
            GameObject cpRoot = CreateChild("Puntos_De_Control", optionalRoot);
            CreateCheckpoint(cpRoot.transform, "Checkpoint 1 · Entrada / Patio", new Vector3(12f, -1f, 0f));
            CreateCheckpoint(cpRoot.transform, "Checkpoint 2 · Caballerizas", new Vector3(75f, -1f, 0f));
            CreateCheckpoint(cpRoot.transform, "Checkpoint 3 · Acceso a Túneles", new Vector3(155f, -1f, 0f));

            // 7. Objetos que Suman Vida
            GameObject healthItemsRoot = CreateChild("Objetos_Vida_Salud", optionalRoot);

            CreateHealthPickup(healthItemsRoot.transform, "Botiquin_Auxilios_Patio", "Botiquin_Auxilios", new Vector3(22f, -0.6f, 0f), HealthItemType.FirstAidKit, 2, false);
            CreateHealthPickup(healthItemsRoot.transform, "Botiquin_Auxilios_Caballerizas", "Botiquin_Auxilios", new Vector3(96f, -1.8f, 0f), HealthItemType.FirstAidKit, 2, false);
            CreateHealthPickup(healthItemsRoot.transform, "Botiquin_Auxilios_Campo", "Botiquin_Auxilios", new Vector3(142f, -0.5f, 0f), HealthItemType.FirstAidKit, 2, false);
            CreateHealthPickup(healthItemsRoot.transform, "Botiquin_Auxilios_Perimetro", "Botiquin_Auxilios", new Vector3(182f, -2.0f, 0f), HealthItemType.FirstAidKit, 2, false);

            CreateHealthPickup(healthItemsRoot.transform, "Manzana_Carretera", "Manzana_Fruta", new Vector3(-10f, -2.5f, 0f), HealthItemType.WildFruit, 1, false);
            CreateHealthPickup(healthItemsRoot.transform, "Manzana_CasaGrande", "Manzana_Fruta", new Vector3(50f, -2.2f, 0f), HealthItemType.WildFruit, 1, false);
            CreateHealthPickup(healthItemsRoot.transform, "Manzana_Arbol_Almacen", "Manzana_Fruta", new Vector3(122f, -2.4f, 0f), HealthItemType.WildFruit, 1, false);
            CreateHealthPickup(healthItemsRoot.transform, "Manzana_Matorrales", "Manzana_Fruta", new Vector3(148f, -2.2f, 0f), HealthItemType.WildFruit, 1, false);

            CreateHealthPickup(healthItemsRoot.transform, "Amuleto_Esperanza_Casa", "Amuleto_Esperanza", new Vector3(62f, 0.2f, 0f), HealthItemType.HopeAmulet, 1, true);
            CreateHealthPickup(healthItemsRoot.transform, "Amuleto_Esperanza_Tuneles", "Amuleto_Esperanza", new Vector3(138f, 0.5f, 0f), HealthItemType.HopeAmulet, 1, true);

            Vector3[] waterPositions = new Vector3[]
            {
                new Vector3(-5f, -1.8f, 0f),
                new Vector3(38f, -0.5f, 0f),
                new Vector3(84f, -2.2f, 0f),
                new Vector3(108f, -1.2f, 0f),
                new Vector3(168f, -2.0f, 0f)
            };
            for (int i = 0; i < waterPositions.Length; i++)
            {
                CreateHealthPickup(healthItemsRoot.transform, $"Botella_Agua_{i + 1}", "Botella_Agua", waterPositions[i], HealthItemType.WaterBottle, 1, false);
            }

            // 8. Enemigos y Patrullas Policiales (con sprites propios y rutas activas)
            GameObject enemyRoot = CreateChild("Enemigos", optionalRoot);
            GameObject privateZonesRoot = CreateChild("Zonas_Privadas_Triggers", optionalRoot);

            // 4 Patrullas Policiales en Zonas Privadas
            PoliceOfficerAI pol1 = CreatePoliceOfficer(enemyRoot.transform, "Policia_Patrulla_1_Patio", new Vector3(32f, -1.2f, 0f), 20f, 44f);
            CreatePrivateZoneTrigger(privateZonesRoot.transform, "Trigger_Zona_Privada_Patio", new Vector3(30f, -1.0f, 0f), new Vector2(26f, 6.0f), pol1);

            PoliceOfficerAI pol2 = CreatePoliceOfficer(enemyRoot.transform, "Policia_Patrulla_2_Caballerizas", new Vector3(82f, -1.0f, 0f), 70f, 94f);
            CreatePrivateZoneTrigger(privateZonesRoot.transform, "Trigger_Zona_Privada_Caballerizas", new Vector3(82f, -1.0f, 0f), new Vector2(24f, 6.0f), pol2);

            PoliceOfficerAI pol3 = CreatePoliceOfficer(enemyRoot.transform, "Policia_Patrulla_3_Almacen", new Vector3(118f, -1.2f, 0f), 104f, 132f);
            CreatePrivateZoneTrigger(privateZonesRoot.transform, "Trigger_Zona_Privada_Almacen", new Vector3(118f, -1.2f, 0f), new Vector2(28f, 6.0f), pol3);

            PoliceOfficerAI pol4 = CreatePoliceOfficer(enemyRoot.transform, "Policia_Patrulla_4_Tuneles", new Vector3(164f, -0.8f, 0f), 150f, 178f);
            CreatePrivateZoneTrigger(privateZonesRoot.transform, "Trigger_Zona_Privada_Tuneles", new Vector3(164f, -0.8f, 0f), new Vector2(28f, 6.0f), pol4);

            // Vigilantes, Perseguidores veloces y Guardián Acústico con rutas de patrulla (dejando libre la zona de inicio x=-8)
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Caseta", "Enemigo_Vigilante", new Vector3(6f, -1.0f, 0f), 2f, 12f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Patio", "Enemigo_Vigilante", new Vector3(22f, -2.0f, 0f), 14f, 32f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Huertas", "Enemigo_Perseguidor", new Vector3(45f, -1.8f, 0f), 38f, 56f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_CasaGrande", "Enemigo_Vigilante", new Vector3(64f, -1.5f, 0f), 54f, 74f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Caballerizas", "Enemigo_Perseguidor", new Vector3(88f, -1.0f, 0f), 76f, 100f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Almacen", "Enemigo_Vigilante", new Vector3(106f, -1.5f, 0f), 96f, 116f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Campo", "Enemigo_Perseguidor", new Vector3(136f, -1.6f, 0f), 126f, 148f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Pozo", "Enemigo_Guardian", new Vector3(146f, -1.2f, 0f), 138f, 154f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Tuneles", "Enemigo_Guardian", new Vector3(172f, -0.8f, 0f), 160f, 186f);

            // 15 Enemigos adicionales agregados (Total 24 enemigos patrulleros + 4 policías = 28)
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Entrada_2", "Enemigo_Vigilante", new Vector3(13f, -1.2f, 0f), 7f, 18f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Porton", "Enemigo_Perseguidor", new Vector3(8f, -1.8f, 0f), 2f, 15f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Patio_Norte", "Enemigo_Vigilante", new Vector3(18f, -0.6f, 0f), 10f, 26f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Corredor", "Enemigo_Perseguidor", new Vector3(30f, -1.5f, 0f), 22f, 38f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Jardines", "Enemigo_Guardian", new Vector3(40f, -1.0f, 0f), 32f, 48f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Huertas_2", "Enemigo_Vigilante", new Vector3(50f, -2.2f, 0f), 42f, 58f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Muros", "Enemigo_Perseguidor", new Vector3(58f, -0.8f, 0f), 48f, 68f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Capilla", "Enemigo_Vigilante", new Vector3(70f, -1.6f, 0f), 60f, 80f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Establos", "Enemigo_Perseguidor", new Vector3(80f, -2.0f, 0f), 70f, 90f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Bodega", "Enemigo_Guardian", new Vector3(95f, -1.2f, 0f), 86f, 104f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Bodegas_2", "Enemigo_Vigilante", new Vector3(112f, -1.8f, 0f), 102f, 122f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Cercos", "Enemigo_Perseguidor", new Vector3(125f, -1.4f, 0f), 114f, 134f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Ruinas", "Enemigo_Vigilante", new Vector3(152f, -1.8f, 0f), 142f, 162f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Sendero", "Enemigo_Perseguidor", new Vector3(165f, -1.2f, 0f), 154f, 175f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Arbol_Viejo", "Enemigo_Guardian", new Vector3(182f, -1.0f, 0f), 172f, 190f);

            // 12 Nuevos Enemigos Tácticos Adicionales (Total: 36 enemigos patrulleros + 4 policías = 40)
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Patio_Sur", "Enemigo_Vigilante", new Vector3(25f, -1.2f, 0f), 16f, 34f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Fuente", "Enemigo_Perseguidor", new Vector3(34f, -2.2f, 0f), 24f, 44f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Huertas_Norte", "Enemigo_Guardian", new Vector3(47f, -1.0f, 0f), 38f, 56f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Arcadas", "Enemigo_Vigilante", new Vector3(62f, -2.4f, 0f), 52f, 72f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Caballerizas_2", "Enemigo_Perseguidor", new Vector3(75f, -1.2f, 0f), 65f, 85f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Establos_Norte", "Enemigo_Guardian", new Vector3(84f, -1.8f, 0f), 74f, 94f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Galpon", "Enemigo_Vigilante", new Vector3(99f, -2.0f, 0f), 90f, 108f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Bodega_Central", "Enemigo_Perseguidor", new Vector3(108f, -1.0f, 0f), 98f, 118f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Vias", "Enemigo_Vigilante", new Vector3(120f, -2.2f, 0f), 110f, 130f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Zanjas", "Enemigo_Perseguidor", new Vector3(130f, -1.2f, 0f), 120f, 140f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Ruinas_Ocultas", "Enemigo_Guardian", new Vector3(158f, -1.0f, 0f), 148f, 168f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Avanzada", "Enemigo_Perseguidor", new Vector3(178f, -1.5f, 0f), 168f, 188f);

            // 12 Enemigos Más para Máxima Acción y Peligro (Total: 48 patrulleros + 4 policías = 52)
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Caseta_Norte", "Enemigo_Vigilante", new Vector3(8f, -2.4f, 0f), 2f, 15f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Calzada", "Enemigo_Perseguidor", new Vector3(18f, -1.0f, 0f), 10f, 26f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Pozo_Seco", "Enemigo_Guardian", new Vector3(38f, -2.4f, 0f), 28f, 46f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Rosales", "Enemigo_Vigilante", new Vector3(54f, -1.2f, 0f), 45f, 62f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Porton_Este", "Enemigo_Perseguidor", new Vector3(68f, -2.5f, 0f), 60f, 78f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Granero", "Enemigo_Vigilante", new Vector3(88f, -1.5f, 0f), 80f, 96f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Silos", "Enemigo_Guardian", new Vector3(104f, -2.2f, 0f), 95f, 112f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Herreria", "Enemigo_Perseguidor", new Vector3(115f, -1.0f, 0f), 106f, 124f);
            CreatePatrolEnemy<VigilanteAI>(enemyRoot.transform, "Vigilante_Acueducto", "Enemigo_Vigilante", new Vector3(138f, -2.3f, 0f), 128f, 146f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Matorrales", "Enemigo_Perseguidor", new Vector3(150f, -1.2f, 0f), 140f, 158f);
            CreatePatrolEnemy<GuardianAI>(enemyRoot.transform, "Guardian_Fosa_Vieja", "Enemigo_Guardian", new Vector3(166f, -2.0f, 0f), 156f, 174f);
            CreatePatrolEnemy<PerseguidorAI>(enemyRoot.transform, "Perseguidor_Ultimo_Recodo", "Enemigo_Perseguidor", new Vector3(186f, -1.4f, 0f), 176f, 194f);

            // 9. HUD Completo (Corazones, Energía de Correr, Derrota y FIN)
            CreateHUDCanvas(optionalRoot);
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);
            return go;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null && target.GetType().BaseType != null)
            {
                field = target.GetType().BaseType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }
            if (field != null) field.SetValue(target, value);
        }

        private static void BuildWorldHierarchy(Transform optionalRoot)
        {
            GameObject worldRoot = CreateChild("Hacienda_Vieja_Entorno", optionalRoot);

            Sprite sueloSprite = SpriteHelper.LoadSprite("Hacienda_Suelo_Patio");
            Sprite muroSprite = SpriteHelper.LoadSprite("Hacienda_Fondo_Muro");
            Sprite fountainSprite = SpriteHelper.LoadSprite("Fuente_Antigua_Seca");
            Sprite moundSprite = SpriteHelper.LoadSprite("Tierra_Alterada_Spot");
            Sprite excavatedSprite = SpriteHelper.LoadSprite("Tierra_Excavada_Descubierta");
            Sprite treeSprite = SpriteHelper.LoadSprite("Arbol_Rural");
            Sprite casa1Sprite = SpriteHelper.LoadSprite("Casa_Rural_1");
            Sprite casetaSprite = SpriteHelper.LoadSprite("Casa_Rural_2");
            Sprite cercaSprite = SpriteHelper.LoadSprite("Cerca_Madera");
            Sprite carreteraSprite = SpriteHelper.LoadSprite("Carretera_Asfalto");
            Sprite columnaSprite = SpriteHelper.LoadSprite("Columna_Cantera");
            Sprite rejaSprite = SpriteHelper.LoadSprite("Puerta_Reja_Hierro");
            Sprite puertaAcertijoSprite = SpriteHelper.LoadSprite("Puerta_Acertijo");
            Sprite escenaCrimenSprite = SpriteHelper.LoadSprite("Escena_Hallazgo_Pericial");
            Sprite cajaSprite = SpriteHelper.LoadSprite("Caja_Madera_Almacen");
            Sprite matorralSprite = SpriteHelper.LoadSprite("Matorral_Seco");

            // 1. Suelo (sortingOrder = 0)
            GameObject floorRoot = CreateChild("Suelo_Patio_Y_Camino", worldRoot.transform);
            for (float x = -18f; x <= 208f; x += 3.8f)
            {
                for (float y = -4.5f; y <= 1.2f; y += 3.8f)
                {
                    GameObject tile = CreateChild($"Floor_{x:F0}_{y:F0}", floorRoot.transform);
                    tile.transform.position = new Vector3(x, y, 0f);
                    SpriteRenderer tsr = tile.AddComponent<SpriteRenderer>();
                    Sprite tileSpr = (x < 6f && y > -2.5f && y < 1.0f) ? carreteraSprite : sueloSprite;
                    SpriteHelper.ConfigureRenderer(tsr, tileSpr, 0);
                }
            }

            // 2. Muros Coloniales Norte (sortingOrder = 12)
            GameObject wallsRoot = CreateChild("Muros_Coloniales_Norte", worldRoot.transform);
            for (float x = -15f; x <= 208f; x += 5.8f)
            {
                GameObject wall = CreateChild($"Muro_Colonial_{x:F0}", wallsRoot.transform);
                wall.transform.position = new Vector3(x, 2.7f, 0f);
                SpriteRenderer wsr = wall.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(wsr, muroSprite, 12);
                BoxCollider2D wcol = wall.AddComponent<BoxCollider2D>();
                wcol.size = new Vector2(5.5f, 1.2f);
                wcol.offset = new Vector2(0f, -0.6f);
            }

            // 3. Cercas Sur (sortingOrder = 14)
            GameObject southWalls = CreateChild("Cercas_Sur", worldRoot.transform);
            for (float x = -15f; x <= 208f; x += 3.5f)
            {
                GameObject fence = CreateChild($"Cerca_Sur_{x:F0}", southWalls.transform);
                fence.transform.position = new Vector3(x, -4.5f, 0f);
                SpriteRenderer fsr = fence.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(fsr, cercaSprite, 14);
                fence.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                BoxCollider2D fcol = fence.AddComponent<BoxCollider2D>();
                fcol.size = new Vector2(2.5f, 0.6f);
            }

            // 4. Caseta y Reja Entrada
            GameObject caseta = CreateChild("Caseta_Vigilancia_Entrada", worldRoot.transform);
            caseta.transform.position = new Vector3(-5f, 0.8f, 0f);
            SpriteRenderer cSr = caseta.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(cSr, casetaSprite, 16);
            caseta.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            BoxCollider2D cCol = caseta.AddComponent<BoxCollider2D>();
            cCol.size = new Vector2(2.2f, 2.0f);

            GameObject reja = CreateChild("Reja_Acceso_Patio", worldRoot.transform);
            reja.transform.position = new Vector3(5f, -0.8f, 0f);
            SpriteRenderer rSr = reja.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(rSr, rejaSprite, 17);
            reja.transform.localScale = new Vector3(1.1f, 1.3f, 1f);
            BoxCollider2D rCol = reja.AddComponent<BoxCollider2D>();
            rCol.size = new Vector2(1.0f, 2.8f);
            DoorMechanism doorMech = reja.AddComponent<DoorMechanism>();
            SetPrivateField(doorMech, "requiredKeyName", "Llave de la Caseta");

            // 5. Puerta con Acertijo (+1 Corazón de Vida) en X: 46f
            GameObject riddleDoorObj = CreateChild("Puerta_Con_Acertijo", worldRoot.transform);
            riddleDoorObj.transform.position = new Vector3(46f, -0.8f, 0f);
            SpriteRenderer rdSr = riddleDoorObj.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(rdSr, puertaAcertijoSprite, 17);
            riddleDoorObj.transform.localScale = new Vector3(1.1f, 1.3f, 1f);
            BoxCollider2D rdCol = riddleDoorObj.AddComponent<BoxCollider2D>();
            rdCol.size = new Vector2(1.0f, 2.8f);
            riddleDoorObj.AddComponent<RiddleDoor>();

            // 6. Patio con Fuente y Columnas
            GameObject fuente = CreateChild("Fuente_Colonial_Seca", worldRoot.transform);
            fuente.transform.position = new Vector3(25f, -1.2f, 0f);
            SpriteRenderer fSr = fuente.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(fSr, fountainSprite, 18);
            fuente.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            CircleCollider2D fCol = fuente.AddComponent<CircleCollider2D>();
            fCol.radius = 1.0f;
            fuente.AddComponent<HidingSpot>();

            float[] colX = new float[] { 14f, 20f, 32f, 38f };
            for (int i = 0; i < colX.Length; i++)
            {
                GameObject colObj = CreateChild($"Columna_Patio_{i + 1}", worldRoot.transform);
                colObj.transform.position = new Vector3(colX[i], 0.6f, 0f);
                SpriteRenderer colSr = colObj.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(colSr, columnaSprite, 18);
                colObj.transform.localScale = new Vector3(1.0f, 1.1f, 1f);
                colObj.AddComponent<HidingSpot>();
            }

            // 7. Casa Grande
            GameObject casa = CreateChild("Fachada_Casa_Grande", worldRoot.transform);
            casa.transform.position = new Vector3(56f, 1.2f, 0f);
            SpriteRenderer cgSr = casa.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(cgSr, casa1Sprite, 16);
            casa.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            BoxCollider2D cgCol = casa.AddComponent<BoxCollider2D>();
            cgCol.size = new Vector2(3.0f, 2.4f);

            // 8. Caballerizas
            GameObject corrales = CreateChild("Corrales_Caballerizas", worldRoot.transform);
            for (float x = 70f; x < 95f; x += 4.5f)
            {
                GameObject fencePost = CreateChild($"Cerca_Corral_{x:F0}", corrales.transform);
                fencePost.transform.position = new Vector3(x, 0.4f, 0f);
                SpriteRenderer cfsr = fencePost.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(cfsr, cercaSprite, 15);
                fencePost.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                BoxCollider2D cfcol = fencePost.AddComponent<BoxCollider2D>();
                cfcol.size = new Vector2(2.0f, 0.6f);
            }

            // 9. Almacén con Cajas
            GameObject cajasRoot = CreateChild("Cajas_Almacen", worldRoot.transform);
            Vector3[] cajaPos = new Vector3[]
            {
                new Vector3(98f, 0.5f, 0f),
                new Vector3(101f, 0.3f, 0f),
                new Vector3(108f, -2.5f, 0f),
                new Vector3(112f, 0.4f, 0f)
            };
            for (int i = 0; i < cajaPos.Length; i++)
            {
                GameObject caja = CreateChild($"Caja_Madera_{i + 1}", cajasRoot.transform);
                caja.transform.position = cajaPos[i];
                SpriteRenderer cjSr = caja.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(cjSr, cajaSprite, 19);
                caja.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
                caja.AddComponent<HidingSpot>();
            }

            // 10. Árboles y Matorrales
            GameObject nature = CreateChild("Vegetacion_Campo", worldRoot.transform);
            float[] treeX = new float[] { -12f, 18f, 48f, 78f, 120f, 132f, 144f, 185f, 198f };
            float[] treeY = new float[] { -3.8f, -4.0f, -3.9f, -3.8f, 0.5f, -3.6f, 0.6f, -3.5f, 0.4f };
            for (int i = 0; i < treeX.Length; i++)
            {
                GameObject tree = CreateChild($"Arbol_Rural_{i + 1}", nature.transform);
                tree.transform.position = new Vector3(treeX[i], treeY[i], 0f);
                SpriteRenderer trSr = tree.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(trSr, treeSprite, 22);
                tree.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            }

            for (float x = 118f; x <= 152f; x += 5.0f)
            {
                GameObject mat = CreateChild($"Matorral_{x:F0}", nature.transform);
                mat.transform.position = new Vector3(x, (x % 2 == 0) ? -2.2f : -0.2f, 0f);
                SpriteRenderer mSr = mat.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(mSr, matorralSprite, 19);
                mat.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }

            // 11. Reja Túnel
            GameObject rejaTunel = CreateChild("Reja_Acceso_Tuneles", worldRoot.transform);
            rejaTunel.transform.position = new Vector3(158f, -0.8f, 0f);
            SpriteRenderer rtSr = rejaTunel.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(rtSr, rejaSprite, 17);
            rejaTunel.transform.localScale = new Vector3(1.1f, 1.3f, 1f);
            BoxCollider2D rtCol = rejaTunel.AddComponent<BoxCollider2D>();
            rtCol.size = new Vector2(1.0f, 2.8f);
            rejaTunel.AddComponent<SymbolPuzzle>();

            // 12A. El Árbol Viejo donde está enterrada Laura (X: 188f)
            Sprite arbolLauraSprite = SpriteHelper.LoadSprite("Arbol_Laura");
            Sprite fosaTierraSprite = SpriteHelper.LoadSprite("Fosa_Laura_Tierra");
            Sprite fosaDescubiertaSprite = SpriteHelper.LoadSprite("Fosa_Laura_Descubierta");

            GameObject arbolLauraObj = CreateChild("Arbol_De_Laura", worldRoot.transform);
            arbolLauraObj.transform.position = new Vector3(188f, 0.35f, 0f);
            SpriteRenderer alSr = arbolLauraObj.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(alSr, arbolLauraSprite, 22);
            arbolLauraObj.transform.localScale = new Vector3(1.55f, 1.55f, 1f);

            GameObject lauraGraveObj = CreateChild("Tumba_De_Laura", worldRoot.transform);
            lauraGraveObj.transform.position = new Vector3(188f, -1.15f, 0f);
            SpriteRenderer lgSr = lauraGraveObj.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(lgSr, fosaTierraSprite, 8);
            lauraGraveObj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            CircleCollider2D lgCol = lauraGraveObj.AddComponent<CircleCollider2D>();
            lgCol.isTrigger = true;
            lgCol.radius = 1.9f;
            LauraTreeGrave lauraComp = lauraGraveObj.AddComponent<LauraTreeGrave>();
            SetPrivateField(lauraComp, "moundSprite", fosaTierraSprite);
            SetPrivateField(lauraComp, "uncoveredSprite", fosaDescubiertaSprite);

            // 12B. Escena Forense de Fosa Clandestina en X: 196f
            GameObject crimeSceneObj = CreateChild("Escena_Fosa_Clandestina", worldRoot.transform);
            crimeSceneObj.transform.position = new Vector3(196f, -1.0f, 0f);
            SpriteRenderer csSr = crimeSceneObj.AddComponent<SpriteRenderer>();
            SpriteHelper.ConfigureRenderer(csSr, escenaCrimenSprite, 18);
            crimeSceneObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            CircleCollider2D csCol = crimeSceneObj.AddComponent<CircleCollider2D>();
            csCol.isTrigger = true;
            csCol.radius = 1.6f;
            crimeSceneObj.AddComponent<ForensicCrimeScene>();

            // 12D. Meta Final del Nivel 3 en X: 200f con Arco y Botón Rojo en el suelo
            GameObject metaObj = CreateChild("Meta_Final_Nivel3", worldRoot.transform);
            metaObj.transform.position = new Vector3(200f, 0.4f, 0f);
            SpriteRenderer metaSr = metaObj.AddComponent<SpriteRenderer>();
            Sprite metaSprite = SpriteHelper.LoadSprite("Meta_Arco_Final");
            SpriteHelper.ConfigureRenderer(metaSr, metaSprite, 22);
            metaObj.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            // Botón Rojo en el suelo bajo el arco (al pisar dice "¡PASA AL NIVEL 4 GANADOR!")
            GameObject btnObj = CreateChild("Boton_Rojo_Meta_Suelo", metaObj.transform);
            btnObj.transform.localPosition = new Vector3(0f, -0.92f, 0f);
            btnObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            SpriteRenderer btnSr = btnObj.AddComponent<SpriteRenderer>();
            Sprite btnSprite = SpriteHelper.LoadSprite("Boton_Rojo_Meta");
            Sprite btnPressedSprite = SpriteHelper.LoadSprite("Boton_Rojo_Meta_Presionado");
            SpriteHelper.ConfigureRenderer(btnSr, btnSprite, 25);

            BoxCollider2D metaCol = metaObj.AddComponent<BoxCollider2D>();
            metaCol.isTrigger = true;
            metaCol.size = new Vector2(8.0f, 10.0f);
            LevelGoalTrigger goalTrigger = metaObj.AddComponent<LevelGoalTrigger>();
            goalTrigger.ConfigureButton(btnSr, btnPressedSprite);

            // 12C. Sitios de Fosas Clandestinas con Restos Humanos Enterrados
            GameObject remainsRoot = CreateChild("Fosas_Restos_Enterrados", worldRoot.transform);
            Sprite fosaRestosTierra = SpriteHelper.LoadSprite("Fosa_Restos_Enterrados");
            Sprite fosaRestosDesc = SpriteHelper.LoadSprite("Fosa_Restos_Descubiertos");

            var remainsData = new (float x, float y, string title, string desc, int steps, string photoName, string personName, string personAge, string date, string details)[]
            {
                (28f, -1.8f, "Fosa Clandestina #1: Carlos Méndez Ramos", "Osamenta humana y cal viva recuperadas en el patio colonial.", 2,
                 "Foto_Cedula_Carlos", "Carlos Méndez Ramos", "28 años", "4 de Mayo de 2020",
                 "Señas particulares: Pulsera de tela roja y calzado industrial de trabajo. Localizado en estrato pericial alterado con cal viva en el patio colonial."),

                (72f, -1.5f, "Fosa Clandestina #2: Sofía Arriaga Téllez", "Restos óseos con amarres y prendas textiles preservadas junto a las caballerizas.", 2,
                 "Foto_Cedula_Sofia", "Sofía Arriaga Téllez", "21 años", "12 de Febrero de 2022",
                 "Señas particulares: Suéter morado y cadena de plata con dije de colibrí. Localizada e identificada junto a las antiguas caballerizas."),

                (140f, -2.1f, "Fosa Clandestina #3: Miguel Ángel Castro Ruiz", "Osamenta en fosa oculta con cartera y cédula de búsqueda.", 3,
                 "Foto_Cedula_Miguel", "Miguel Ángel Castro Ruiz", "34 años", "30 de Noviembre de 2019",
                 "Señas particulares: Cartera de cuero con credencial de elector y reloj plateado. Localizado e identificado en la fosa perimetral de los túneles.")
            };

            for (int rIdx = 0; rIdx < remainsData.Length; rIdx++)
            {
                var rd = remainsData[rIdx];
                GameObject rObj = CreateChild($"Fosa_Restos_{rIdx + 1}", remainsRoot.transform);
                rObj.transform.position = new Vector3(rd.x, rd.y, 0f);
                rObj.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

                SpriteRenderer remSr = rObj.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(remSr, fosaRestosTierra, 8);

                CircleCollider2D remCol = rObj.AddComponent<CircleCollider2D>();
                remCol.isTrigger = true;
                remCol.radius = 1.8f;

                BuriedRemainsSite rComp = rObj.AddComponent<BuriedRemainsSite>();
                rComp.Configure(rd.title, rd.desc, rd.steps);
                Sprite personPhotoSpr = SpriteHelper.LoadSprite(rd.photoName);
                rComp.ConfigureSearchCard(personPhotoSpr, rd.personName, rd.personAge, rd.date, rd.details);
                SetPrivateField(rComp, "buriedMoundSprite", fosaRestosTierra);
                SetPrivateField(rComp, "uncoveredRemainsSprite", fosaRestosDesc);
            }

            // 13. 10 Sitios de Excavación
            GameObject digSitesRoot = CreateChild("Puntos_De_Excavacion", worldRoot.transform);
            var digData = new (float x, float y, string name, string desc, bool isKey, string keyName, bool isWater)[]
            {
                (-4.0f, -0.5f, "Llave de la Caseta", "Llave antigua forjada que abre el portón colonial hacia el patio principal.", true, "Llave de la Caseta", false),
                (17.0f, -2.2f, "Llavero con iniciales familiares", "Llavero oxidado con las iniciales coincidentes con el reporte familiar.", false, "", false),
                (35.0f, 0.2f, "Fotografía en funda impermeable", "Fotografía protegida bajo tierra que aporta indicios concluyentes.", false, "", false),
                (58.0f, -1.8f, "Reloj de bolsillo grabado", "Reloj conmemorativo con fecha. Se documenta formalmente sin perturbar el estrato.", false, "", false),
                (84.0f, -2.0f, "Herramienta de palanca / barreta", "Herramienta de acero necesaria para abrir la compuerta trancada de caballerizas.", true, "Herramienta de palanca", false),
                (104.0f, 0.0f, "Botella de agua de brigada", "Botella sellada útil para recuperar energía.", false, "", true),
                (122.0f, -2.5f, "Prenda de vestir protegida", "Prenda textil con etiqueta de fábrica. Se preserva el cuadrante pericial.", false, "", false),
                (136.0f, 0.0f, "Medallón familiar con cadena", "Medallón de valor afectivo idéntico al de las fotografías familiares.", false, "", false),
                (148.0f, -1.5f, "Mochila con pertenencias personales", "Mochila asegurada e inventariada para el equipo forense especializado.", false, "", false),
                (192.0f, -1.0f, "Hallazgo final determinante", "Pertenencia personal confirmatoria. Se solicita la intervención inmediata de la brigada pericial.", false, "", false)
            };

            for (int i = 0; i < digData.Length; i++)
            {
                var d = digData[i];
                GameObject digObj = CreateChild($"Sitio_Excavacion_{i + 1}_{d.name}", digSitesRoot.transform);
                digObj.transform.position = new Vector3(d.x, d.y, 0f);

                SpriteRenderer dsr = digObj.AddComponent<SpriteRenderer>();
                SpriteHelper.ConfigureRenderer(dsr, moundSprite, 5);
                digObj.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                CircleCollider2D dcol = digObj.AddComponent<CircleCollider2D>();
                dcol.isTrigger = true;
                dcol.radius = 0.9f;

                DigSite digSiteComp = digObj.AddComponent<DigSite>();
                SetPrivateField(digSiteComp, "untouchedMoundSprite", moundSprite);
                SetPrivateField(digSiteComp, "excavatedHoleSprite", excavatedSprite);
                SetPrivateField(digSiteComp, "itemName", d.name);
                SetPrivateField(digSiteComp, "itemDescription", d.desc);
                SetPrivateField(digSiteComp, "givesInventoryKey", d.isKey);
                SetPrivateField(digSiteComp, "keyItemName", d.keyName);
                SetPrivateField(digSiteComp, "givesWaterBottle", d.isWater);
            }
        }

        private static void CreateCheckpoint(Transform parent, string name, Vector3 pos)
        {
            GameObject cp = CreateChild(name, parent);
            cp.transform.position = pos;
            BoxCollider2D bc = cp.AddComponent<BoxCollider2D>();
            bc.isTrigger = true;
            bc.size = new Vector2(2.5f, 8f);
            cp.AddComponent<Checkpoint>();
        }

        private static void AssignPatrolWaypoints(BaseEnemyAI enemyAI, Transform enemyTransform, string name, float minX, float maxX, float y)
        {
            enemyAI.SetPatrolBounds(minX, maxX);

            GameObject wp1 = new GameObject($"{name}_WP1");
            wp1.transform.position = new Vector3(minX, y, 0f);
            wp1.transform.SetParent(enemyTransform);

            GameObject wp2 = new GameObject($"{name}_WP2");
            wp2.transform.position = new Vector3(maxX, y, 0f);
            wp2.transform.SetParent(enemyTransform);

            SetPrivateField(enemyAI, "waypoints", new Transform[] { wp1.transform, wp2.transform });
        }

        private static T CreatePatrolEnemy<T>(Transform parent, string name, string spriteName, Vector3 pos, float minX, float maxX) where T : BaseEnemyAI
        {
            GameObject enemy = CreateChild(name, parent);
            enemy.transform.position = pos;

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            Sprite enemySprite = SpriteHelper.LoadSprite(spriteName);
            SpriteHelper.ConfigureRenderer(sr, enemySprite, 30);
            enemy.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
            col.radius = 0.65f;

            T ai = enemy.AddComponent<T>();
            AssignPatrolWaypoints(ai, enemy.transform, name, minX, maxX, pos.y);
            return ai;
        }

        private static PoliceOfficerAI CreatePoliceOfficer(Transform parent, string name, Vector3 pos, float minX, float maxX)
        {
            GameObject policeObj = CreateChild(name, parent);
            policeObj.transform.position = pos;

            SpriteRenderer polSr = policeObj.AddComponent<SpriteRenderer>();
            Sprite polSprite = SpriteHelper.LoadSprite("Policia_Patrulla");
            SpriteHelper.ConfigureRenderer(polSr, polSprite, 30);
            policeObj.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            Rigidbody2D polRb = policeObj.AddComponent<Rigidbody2D>();
            polRb.gravityScale = 0f;
            polRb.freezeRotation = true;

            CircleCollider2D polCol = policeObj.AddComponent<CircleCollider2D>();
            polCol.radius = 0.45f;

            PoliceOfficerAI policeAI = policeObj.AddComponent<PoliceOfficerAI>();
            AssignPatrolWaypoints(policeAI, policeObj.transform, name, minX, maxX, pos.y);
            return policeAI;
        }

        private static void CreateHealthPickup(Transform parent, string name, string spriteName, Vector3 pos, HealthItemType type, int hearts, bool expandMax)
        {
            GameObject itemObj = CreateChild(name, parent);
            itemObj.transform.position = pos;

            SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();
            Sprite spr = SpriteHelper.LoadSprite(spriteName);
            SpriteHelper.ConfigureRenderer(sr, spr, 10);
            itemObj.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            CircleCollider2D col = itemObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            HealthPickupItem pickup = itemObj.AddComponent<HealthPickupItem>();
            pickup.Configure(type, name, hearts, expandMax);
        }

        private static void CreatePrivateZoneTrigger(Transform parent, string name, Vector3 pos, Vector2 size, PoliceOfficerAI officer)
        {
            GameObject pzObj = CreateChild(name, parent);
            pzObj.transform.position = pos;

            BoxCollider2D pzCol = pzObj.AddComponent<BoxCollider2D>();
            pzCol.isTrigger = true;
            pzCol.size = size;

            PrivateZoneTrigger pzTrigger = pzObj.AddComponent<PrivateZoneTrigger>();
            SetPrivateField(pzTrigger, "policeOfficer", officer);
        }

        private static void CleanupStrayMenuSceneObjects()
        {
            // 1. Destruir cualquier Canvas que NO sea Canvas_HUD ni PauseMenu
            Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                if (c == null || c.gameObject.name == "Canvas_HUD" || c.gameObject.name.Contains("PauseMenu")) continue;
                if (Application.isPlaying) Object.Destroy(c.gameObject);
                else Object.DestroyImmediate(c.gameObject);
            }

            // 2. Destruir objetos residuales de menús de escenas previas
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject go in allObjects)
            {
                if (go == null) continue;
                string lower = go.name.ToLowerInvariant();
                if (lower.StartsWith("controlador")
                    || lower.Contains("darkoverlay")
                    || lower.Contains("botonvolver")
                    || lower.Contains("titulonivel")
                    || (lower == "fondo" && go.GetComponent<Image>() != null)
                    || (lower.StartsWith("canvas") && go.name != "Canvas_HUD"))
                {
                    if (go.transform.root == null || go.transform.root.name != "Canvas_HUD")
                    {
                        if (Application.isPlaying) Object.Destroy(go);
                        else Object.DestroyImmediate(go);
                    }
                }
            }
        }

        private static void EnsureEventSystemExists(Transform optionalRoot)
        {
            EventSystem[] systems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            GameObject esObj;
            if (systems != null && systems.Length > 0)
            {
                esObj = systems[0].gameObject;
                for (int i = 1; i < systems.Length; i++)
                {
                    if (systems[i] == null) continue;
                    if (Application.isPlaying) Object.Destroy(systems[i].gameObject);
                    else Object.DestroyImmediate(systems[i].gameObject);
                }
            }
            else
            {
                esObj = CreateChild("EventSystem", optionalRoot);
                esObj.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule legacyModule = esObj.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                if (Application.isPlaying) Object.Destroy(legacyModule);
                else Object.DestroyImmediate(legacyModule);
            }
            BaseInput baseInput = esObj.GetComponent<BaseInput>();
            if (baseInput != null)
            {
                if (Application.isPlaying) Object.Destroy(baseInput);
                else Object.DestroyImmediate(baseInput);
            }
            if (esObj.GetComponent<InputSystemUIInputModule>() == null)
            {
                esObj.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (esObj.GetComponent<StandaloneInputModule>() == null)
            {
                esObj.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private static GameObject CreateFullScreenModal(Transform parent, string name, Color bgColor, string title, Color titleColor, string subtitle, string message, string buttonLabel, Color btnColor, out Button actionButton)
        {
            GameObject panel = CreateChild(name, parent);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = bgColor;

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject titleObj = CreateChild(name + "_Titulo", panel.transform);
            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 0.72f);
            tRect.anchorMax = new Vector2(0.5f, 0.72f);
            tRect.anchoredPosition = Vector2.zero;
            tRect.sizeDelta = new Vector2(900f, 90f);
            Text tText = titleObj.AddComponent<Text>();
            tText.text = title;
            tText.font = defaultFont;
            tText.fontSize = 46;
            tText.fontStyle = FontStyle.Bold;
            tText.color = titleColor;
            tText.alignment = TextAnchor.MiddleCenter;
            tText.raycastTarget = false;

            GameObject subObj = CreateChild(name + "_Subtitulo", panel.transform);
            RectTransform sRect = subObj.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.58f);
            sRect.anchorMax = new Vector2(0.5f, 0.58f);
            sRect.anchoredPosition = Vector2.zero;
            sRect.sizeDelta = new Vector2(860f, 40f);
            Text sText = subObj.AddComponent<Text>();
            sText.text = subtitle;
            sText.font = defaultFont;
            sText.fontSize = 22;
            sText.fontStyle = FontStyle.Bold;
            sText.color = Color.white;
            sText.alignment = TextAnchor.MiddleCenter;
            sText.raycastTarget = false;

            GameObject msgObj = CreateChild(name + "_Mensaje", panel.transform);
            RectTransform mRect = msgObj.AddComponent<RectTransform>();
            mRect.anchorMin = new Vector2(0.5f, 0.43f);
            mRect.anchorMax = new Vector2(0.5f, 0.43f);
            mRect.anchoredPosition = Vector2.zero;
            mRect.sizeDelta = new Vector2(860f, 130f);
            Text mText = msgObj.AddComponent<Text>();
            mText.text = message;
            mText.font = defaultFont;
            mText.fontSize = 18;
            mText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            mText.alignment = TextAnchor.MiddleCenter;
            mText.raycastTarget = false;

            GameObject btnObj = CreateChild(name + "_Boton_Accion", panel.transform);
            RectTransform bRect = btnObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0.5f, 0.25f);
            bRect.anchorMax = new Vector2(0.5f, 0.25f);
            bRect.anchoredPosition = Vector2.zero;
            bRect.sizeDelta = new Vector2(400f, 54f);
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = btnColor;
            bImg.raycastTarget = true;
            actionButton = btnObj.AddComponent<Button>();

            GameObject btObj = CreateChild(name + "_Texto_Boton", btnObj.transform);
            RectTransform btRect = btObj.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;
            btRect.anchoredPosition = Vector2.zero;
            btRect.sizeDelta = Vector2.zero;
            Text btText = btObj.AddComponent<Text>();
            btText.text = buttonLabel;
            btText.font = defaultFont;
            btText.fontSize = 18;
            btText.fontStyle = FontStyle.Bold;
            btText.color = Color.white;
            btText.alignment = TextAnchor.MiddleCenter;
            btText.raycastTarget = false;

            panel.SetActive(false);
            return panel;
        }

        private static void CreateHUDCanvas(Transform optionalRoot)
        {
            EnsureEventSystemExists(optionalRoot);

            GameObject canvasObj = CreateChild("Canvas_HUD", optionalRoot);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Marcador de versión V2 del HUD estandarizado de Nivel 3
            CreateChild("LineaDeVida_V2_OK", canvasObj.transform);

            // 1. Línea de Vida con Corazones y Barra
            GameObject barTitleObj = CreateChild("Etiqueta_Linea_Vida", canvasObj.transform);
            RectTransform btRect = barTitleObj.AddComponent<RectTransform>();
            btRect.anchorMin = new Vector2(0f, 1f);
            btRect.anchorMax = new Vector2(0f, 1f);
            btRect.pivot = new Vector2(0f, 1f);
            btRect.anchoredPosition = new Vector2(28f, -6f);
            btRect.sizeDelta = new Vector2(280f, 20f);
            Text btText = barTitleObj.AddComponent<Text>();
            btText.text = "❤️ LÍNEA DE VIDA";
            btText.font = defaultFont;
            btText.fontSize = 13;
            btText.fontStyle = FontStyle.Bold;
            btText.color = new Color(1f, 0.90f, 0.55f, 1f);

            GameObject barFrameObj = CreateChild("Barra_Vida_Contenedor", canvasObj.transform);
            RectTransform barRect = barFrameObj.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0f, 1f);
            barRect.anchoredPosition = new Vector2(25f, -25f);
            barRect.sizeDelta = new Vector2(400f, 56f);

            Image barFrameImg = barFrameObj.AddComponent<Image>();
            Sprite barFrameSprite = SpriteHelper.LoadSprite("Barra_Vida_Fondo");
            if (barFrameSprite != null) barFrameImg.sprite = barFrameSprite;
            barFrameImg.color = Color.white;

            GameObject fillTrackObj = CreateChild("Relleno_Barra_Vida", barFrameObj.transform);
            RectTransform fillRect = fillTrackObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = new Vector2(-16f, -14f);

            Image fillImg = fillTrackObj.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 1f;
            fillImg.color = new Color(0.22f, 0.85f, 0.42f, 0.88f);

            GameObject heartsContainer = CreateChild("Contenedor_Corazones", barFrameObj.transform);
            RectTransform hcRect = heartsContainer.AddComponent<RectTransform>();
            hcRect.anchorMin = new Vector2(0f, 0.5f);
            hcRect.anchorMax = new Vector2(0f, 0.5f);
            hcRect.pivot = new Vector2(0f, 0.5f);
            hcRect.anchoredPosition = new Vector2(14f, 0f);
            hcRect.sizeDelta = new Vector2(210f, 36f);

            Sprite fullHeartSpr = SpriteHelper.LoadSprite("Corazon_Lleno");
            Sprite emptyHeartSpr = SpriteHelper.LoadSprite("Corazon_Vacio");

            Image[] heartImgs = new Image[6];
            for (int i = 0; i < 6; i++)
            {
                GameObject hObj = CreateChild($"Corazon_{i + 1}", heartsContainer.transform);
                RectTransform hr = hObj.AddComponent<RectTransform>();
                hr.anchorMin = new Vector2(0f, 0.5f);
                hr.anchorMax = new Vector2(0f, 0.5f);
                hr.pivot = new Vector2(0f, 0.5f);
                hr.anchoredPosition = new Vector2(i * 34f, 0f);
                hr.sizeDelta = new Vector2(30f, 30f);

                Image hImg = hObj.AddComponent<Image>();
                hImg.sprite = fullHeartSpr;
                hImg.color = Color.white;
                heartImgs[i] = hImg;

                if (i >= 3) hObj.SetActive(false);
            }

            GameObject txtObj = CreateChild("Texto_Salud", barFrameObj.transform);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(1f, 0.5f);
            txtRect.anchorMax = new Vector2(1f, 0.5f);
            txtRect.pivot = new Vector2(1f, 0.5f);
            txtRect.anchoredPosition = new Vector2(-12f, 0f);
            txtRect.sizeDelta = new Vector2(175f, 30f);

            Text heartText = txtObj.AddComponent<Text>();
            heartText.text = "LÍNEA DE VIDA: 3/3";
            heartText.font = defaultFont;
            heartText.fontSize = 15;
            heartText.fontStyle = FontStyle.Bold;
            heartText.color = new Color(1f, 0.95f, 0.85f, 1f);
            heartText.alignment = TextAnchor.MiddleRight;

            // 2. Barra de Energía para Correr [Shift] + Esquive [Espacio]
            GameObject staminaObj = CreateChild("Barra_Energia_Correr", canvasObj.transform);
            RectTransform stRect = staminaObj.AddComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0f, 1f);
            stRect.anchorMax = new Vector2(0f, 1f);
            stRect.pivot = new Vector2(0f, 1f);
            stRect.anchoredPosition = new Vector2(25f, -92f);
            stRect.sizeDelta = new Vector2(320f, 26f);

            Image stBg = staminaObj.AddComponent<Image>();
            stBg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);

            Slider staminaSlider = staminaObj.AddComponent<Slider>();
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
            staminaSlider.value = 1f;
            staminaSlider.interactable = false;

            GameObject stFillArea = CreateChild("Fill Area", staminaObj.transform);
            RectTransform stFaRect = stFillArea.AddComponent<RectTransform>();
            stFaRect.anchorMin = Vector2.zero;
            stFaRect.anchorMax = Vector2.one;
            stFaRect.sizeDelta = new Vector2(-6f, -6f);

            GameObject stFill = CreateChild("Fill", stFillArea.transform);
            RectTransform stFillRect = stFill.AddComponent<RectTransform>();
            stFillRect.anchorMin = Vector2.zero;
            stFillRect.anchorMax = Vector2.one;
            stFillRect.sizeDelta = Vector2.zero;
            Image stFillImg = stFill.AddComponent<Image>();
            stFillImg.color = new Color(0.18f, 0.78f, 0.95f, 0.9f);
            staminaSlider.fillRect = stFillRect;

            GameObject stTxtObj = CreateChild("Texto_Energia", staminaObj.transform);
            RectTransform stTxtRect = stTxtObj.AddComponent<RectTransform>();
            stTxtRect.anchorMin = Vector2.zero;
            stTxtRect.anchorMax = Vector2.one;
            stTxtRect.sizeDelta = Vector2.zero;
            Text sprintStateText = stTxtObj.AddComponent<Text>();
            sprintStateText.text = "⚡ ENERGÍA [Shift]: 100%";
            sprintStateText.font = defaultFont;
            sprintStateText.fontSize = 13;
            sprintStateText.fontStyle = FontStyle.Bold;
            sprintStateText.color = Color.white;
            sprintStateText.alignment = TextAnchor.MiddleCenter;

            // 3. Controles, Objetivo y Contadores
            GameObject statusPanel = CreateChild("Panel_Estado", canvasObj.transform);
            RectTransform sRect = statusPanel.AddComponent<RectTransform>();
            sRect.anchorMin = new Vector2(1f, 1f);
            sRect.anchorMax = new Vector2(1f, 1f);
            sRect.pivot = new Vector2(1f, 1f);
            sRect.anchoredPosition = new Vector2(-25f, -22f);
            sRect.sizeDelta = new Vector2(590f, 54f);

            Text statusText = statusPanel.AddComponent<Text>();
            statusText.text = "WASD Mover · [Shift] Correr · [Espacio] Esquive · [V / Clic / Q] Pala: Matar / Excavar\n[C] Sigilo · [E / V] Desenterrar a Laura · [F] Linterna";
            statusText.font = defaultFont;
            statusText.fontSize = 16;
            statusText.color = new Color(0.95f, 0.92f, 0.82f, 1f);
            statusText.alignment = TextAnchor.UpperRight;

            GameObject objTxtObj = CreateChild("Texto_Objetivo", canvasObj.transform);
            RectTransform oRect = objTxtObj.AddComponent<RectTransform>();
            oRect.anchorMin = new Vector2(0f, 1f);
            oRect.anchorMax = new Vector2(0f, 1f);
            oRect.pivot = new Vector2(0f, 1f);
            oRect.anchoredPosition = new Vector2(25f, -126f);
            oRect.sizeDelta = new Vector2(680f, 36f);
            Text objectiveText = objTxtObj.AddComponent<Text>();
            objectiveText.text = "Objetivo: Sigue las pistas hasta el Árbol Viejo para encontrar a Laura.";
            objectiveText.font = defaultFont;
            objectiveText.fontSize = 15;
            objectiveText.fontStyle = FontStyle.Bold;
            objectiveText.color = new Color(1f, 0.88f, 0.45f, 1f);
            objectiveText.alignment = TextAnchor.UpperLeft;

            GameObject statsObj = CreateChild("Texto_Enemigos_Abatidos", canvasObj.transform);
            RectTransform stCounterRect = statsObj.AddComponent<RectTransform>();
            stCounterRect.anchorMin = new Vector2(1f, 1f);
            stCounterRect.anchorMax = new Vector2(1f, 1f);
            stCounterRect.pivot = new Vector2(1f, 1f);
            stCounterRect.anchoredPosition = new Vector2(-25f, -82f);
            stCounterRect.sizeDelta = new Vector2(360f, 28f);
            Text enemiesText = statsObj.AddComponent<Text>();
            enemiesText.text = "⚔️ Enemigos abatidos: 0";
            enemiesText.font = defaultFont;
            enemiesText.fontSize = 16;
            enemiesText.fontStyle = FontStyle.Bold;
            enemiesText.color = new Color(0.98f, 0.6f, 0.45f, 1f);
            enemiesText.alignment = TextAnchor.UpperRight;

            // 4. Prompt de Interacción
            GameObject promptObj = CreateChild("Prompt_Interaccion", canvasObj.transform);
            RectTransform pRect = promptObj.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.16f);
            pRect.anchorMax = new Vector2(0.5f, 0.16f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(720f, 50f);

            Text promptText = promptObj.AddComponent<Text>();
            promptText.text = "[E] Excavar / Resolver Acertijo / Interactuar";
            promptText.font = defaultFont;
            promptText.fontSize = 22;
            promptText.color = Color.yellow;
            promptText.alignment = TextAnchor.MiddleCenter;
            promptObj.SetActive(false);

            // 5. Banner de Notificación
            GameObject notifObj = CreateChild("Banner_Notificacion", canvasObj.transform);
            RectTransform nRect = notifObj.AddComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0.5f, 0.84f);
            nRect.anchorMax = new Vector2(0.5f, 0.84f);
            nRect.pivot = new Vector2(0.5f, 0.5f);
            nRect.sizeDelta = new Vector2(960f, 60f);

            CanvasGroup nCg = notifObj.AddComponent<CanvasGroup>();
            nCg.alpha = 0f;
            nCg.blocksRaycasts = false;
            nCg.interactable = false;

            Image nBg = notifObj.AddComponent<Image>();
            nBg.color = new Color(0.1f, 0.08f, 0.06f, 0.92f);
            nBg.raycastTarget = false;

            foreach (Graphic g in canvasObj.GetComponentsInChildren<Graphic>(true))
            {
                if (g.GetComponentInParent<Button>(true) == null)
                {
                    g.raycastTarget = false;
                }
            }

            GameObject notifTxtObj = CreateChild("Texto_Notificacion", notifObj.transform);
            RectTransform ntRect = notifTxtObj.AddComponent<RectTransform>();
            ntRect.anchorMin = Vector2.zero;
            ntRect.anchorMax = Vector2.one;
            ntRect.sizeDelta = Vector2.zero;

            Text notifText = notifTxtObj.AddComponent<Text>();
            notifText.font = defaultFont;
            notifText.fontSize = 20;
            notifText.color = new Color(1f, 0.85f, 0.3f, 1f);
            notifText.fontStyle = FontStyle.Bold;
            notifText.alignment = TextAnchor.MiddleCenter;

            // 6. Modal Game Over (cuando la Línea de vida llega a 0)
            GameObject gameOverPanel = CreateFullScreenModal(
                canvasObj.transform,
                "Panel_GameOver",
                new Color(0.03f, 0.04f, 0.07f, 0.92f),
                "💔 LÍNEA DE VIDA AGOTADA",
                new Color(0.95f, 0.30f, 0.30f, 1f),
                "TU LÍNEA DE VIDA HA LLEGADO A CERO",
                "Usa [Shift] para correr rápido, [Espacio] para esquivar y recoge botiquines, manzanas o botellas de agua para restaurar tu Línea de vida.",
                "🔄  REINTENTAR DESDE CHECKPOINT",
                new Color(0.18f, 0.64f, 0.36f, 1f),
                out Button retryBtn
            );

            // 7. Pantalla Final: "¡NIVEL 3 COMPLETADO!" con opción Siguiente Nivel
            GameObject endingPanel = CreateFullScreenModal(
                canvasObj.transform,
                "Pantalla_FIN",
                new Color(0.04f, 0.05f, 0.09f, 0.96f),
                "🏆 ¡NIVEL 3 COMPLETADO! 🏆",
                new Color(1f, 0.88f, 0.25f, 1f),
                "«Las Huellas del Silencio» — Escena Pericial Asegurada",
                "¡Enhorabuena! Has superado con valentía la hacienda abandonada, derrotado a los enemigos en el camino, eludido a las patrullas policiales, documentado los indicios y desenterrado con profundo respeto y dignidad a Laura entre las raíces del Árbol Viejo.\n\nContinúa al Nivel 4 para acudir al Laboratorio Forense del EAAF y consagrar el Mural de la Memoria.",
                "▶  SIGUIENTE NIVEL (NIVEL 4)",
                new Color(0.18f, 0.68f, 0.36f, 1f),
                out Button endingRestartBtn
            );

            // 8. Banner Flotante de Victoria "¡HAS GANADO!"
            GameObject victoryBannerObj = CreateChild("Banner_HasGanado", canvasObj.transform);
            RectTransform vbRect = victoryBannerObj.AddComponent<RectTransform>();
            vbRect.anchorMin = new Vector2(0.5f, 0.72f);
            vbRect.anchorMax = new Vector2(0.5f, 0.72f);
            vbRect.pivot = new Vector2(0.5f, 0.5f);
            vbRect.sizeDelta = new Vector2(750f, 90f);

            Image vbBg = victoryBannerObj.AddComponent<Image>();
            vbBg.color = new Color(0.12f, 0.08f, 0.02f, 0.94f);
            vbBg.raycastTarget = false;

            GameObject vbTxtObj = CreateChild("Texto_Victoria", victoryBannerObj.transform);
            RectTransform vbtRect = vbTxtObj.AddComponent<RectTransform>();
            vbtRect.anchorMin = Vector2.zero;
            vbtRect.anchorMax = Vector2.one;
            vbtRect.sizeDelta = Vector2.zero;

            Text victoryBannerText = vbTxtObj.AddComponent<Text>();
            victoryBannerText.font = defaultFont;
            victoryBannerText.fontSize = 24;
            victoryBannerText.color = new Color(0.98f, 0.85f, 0.25f, 1f);
            victoryBannerText.fontStyle = FontStyle.Bold;
            victoryBannerText.alignment = TextAnchor.MiddleCenter;
            victoryBannerText.text = "🏆 ¡HAS GANADO! 🏆\n¡Todos los enemigos de la hacienda han sido derrotados!";
            victoryBannerObj.SetActive(false);

            // 8B. Modal de Cédula de Búsqueda Forense de Persona Desaparecida
            GameObject searchCardModal = CreateChild("Modal_Cedula_Busqueda", canvasObj.transform);
            RectTransform scmRect = searchCardModal.AddComponent<RectTransform>();
            scmRect.anchorMin = Vector2.zero;
            scmRect.anchorMax = Vector2.one;
            scmRect.sizeDelta = Vector2.zero;

            Image scmOverlay = searchCardModal.AddComponent<Image>();
            scmOverlay.color = new Color(0.02f, 0.02f, 0.03f, 0.88f);

            GameObject scCardPanel = CreateChild("Panel_Cedula", searchCardModal.transform);
            RectTransform sccpRect = scCardPanel.AddComponent<RectTransform>();
            sccpRect.anchorMin = new Vector2(0.5f, 0.5f);
            sccpRect.anchorMax = new Vector2(0.5f, 0.5f);
            sccpRect.pivot = new Vector2(0.5f, 0.5f);
            sccpRect.sizeDelta = new Vector2(760f, 480f);

            Image sccpBg = scCardPanel.AddComponent<Image>();
            sccpBg.color = new Color(0.12f, 0.11f, 0.10f, 0.98f);

            GameObject borderObj = CreateChild("Borde", scCardPanel.transform);
            RectTransform bRect = borderObj.AddComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.sizeDelta = new Vector2(-12f, -12f);
            Image bImg = borderObj.AddComponent<Image>();
            bImg.color = new Color(0.65f, 0.5f, 0.25f, 0.85f);
            bImg.raycastTarget = false;

            GameObject innerCard = CreateChild("Fondo_Interior", borderObj.transform);
            RectTransform icRect = innerCard.AddComponent<RectTransform>();
            icRect.anchorMin = Vector2.zero;
            icRect.anchorMax = Vector2.one;
            icRect.sizeDelta = new Vector2(-6f, -6f);
            Image icImg = innerCard.AddComponent<Image>();
            icImg.color = new Color(0.14f, 0.13f, 0.12f, 1f);
            icImg.raycastTarget = false;

            // Encabezado
            GameObject scHeader = CreateChild("Encabezado", innerCard.transform);
            RectTransform schRect = scHeader.AddComponent<RectTransform>();
            schRect.anchorMin = new Vector2(0f, 1f);
            schRect.anchorMax = new Vector2(1f, 1f);
            schRect.pivot = new Vector2(0.5f, 1f);
            schRect.anchoredPosition = new Vector2(0f, -8f);
            schRect.sizeDelta = new Vector2(-20f, 45f);
            Image schBg = scHeader.AddComponent<Image>();
            schBg.color = new Color(0.55f, 0.12f, 0.12f, 0.95f);
            schBg.raycastTarget = false;

            GameObject scTitleObj = CreateChild("Titulo_Cedula", scHeader.transform);
            RectTransform sctRect = scTitleObj.AddComponent<RectTransform>();
            sctRect.anchorMin = Vector2.zero;
            sctRect.anchorMax = Vector2.one;
            sctRect.sizeDelta = Vector2.zero;
            Text scTitleText = scTitleObj.AddComponent<Text>();
            scTitleText.font = defaultFont;
            scTitleText.fontSize = 18;
            scTitleText.fontStyle = FontStyle.Bold;
            scTitleText.color = new Color(1f, 0.95f, 0.8f, 1f);
            scTitleText.alignment = TextAnchor.MiddleCenter;
            scTitleText.text = "🕊️ CÉDULA DE IDENTIFICACIÓN Y REGISTRO FORENSE";
            scTitleText.raycastTarget = false;

            // Cuadro de Fotografía
            GameObject photoFrame = CreateChild("Marco_Foto", innerCard.transform);
            RectTransform pfRect = photoFrame.AddComponent<RectTransform>();
            pfRect.anchorMin = new Vector2(0f, 0.5f);
            pfRect.anchorMax = new Vector2(0f, 0.5f);
            pfRect.pivot = new Vector2(0f, 0.5f);
            pfRect.anchoredPosition = new Vector2(25f, 10f);
            pfRect.sizeDelta = new Vector2(190f, 250f);
            Image pfBg = photoFrame.AddComponent<Image>();
            pfBg.color = new Color(0.92f, 0.90f, 0.85f, 1f);

            GameObject photoObj = CreateChild("Foto_Persona", photoFrame.transform);
            RectTransform photoRect = photoObj.AddComponent<RectTransform>();
            photoRect.anchorMin = Vector2.zero;
            photoRect.anchorMax = Vector2.one;
            photoRect.sizeDelta = new Vector2(-8f, -8f);
            Image photoImg = photoObj.AddComponent<Image>();
            photoImg.color = Color.white;
            photoImg.preserveAspect = true;

            // Columna de Datos de la Persona
            GameObject dataCol = CreateChild("Columna_Datos", innerCard.transform);
            RectTransform dcRect = dataCol.AddComponent<RectTransform>();
            dcRect.anchorMin = new Vector2(0f, 0.5f);
            dcRect.anchorMax = new Vector2(1f, 0.5f);
            dcRect.pivot = new Vector2(0f, 0.5f);
            dcRect.anchoredPosition = new Vector2(235f, 10f);
            dcRect.sizeDelta = new Vector2(-260f, 260f);

            GameObject nameObj = CreateChild("Texto_Nombre", dataCol.transform);
            RectTransform nmRect = nameObj.AddComponent<RectTransform>();
            nmRect.anchorMin = new Vector2(0f, 1f);
            nmRect.anchorMax = new Vector2(1f, 1f);
            nmRect.pivot = new Vector2(0f, 1f);
            nmRect.anchoredPosition = new Vector2(0f, 0f);
            nmRect.sizeDelta = new Vector2(0f, 32f);
            Text scNameText = nameObj.AddComponent<Text>();
            scNameText.font = defaultFont;
            scNameText.fontSize = 20;
            scNameText.fontStyle = FontStyle.Bold;
            scNameText.color = new Color(1f, 0.85f, 0.3f, 1f);
            scNameText.alignment = TextAnchor.MiddleLeft;

            GameObject ageObj = CreateChild("Texto_Edad", dataCol.transform);
            RectTransform ageRect = ageObj.AddComponent<RectTransform>();
            ageRect.anchorMin = new Vector2(0f, 1f);
            ageRect.anchorMax = new Vector2(1f, 1f);
            ageRect.pivot = new Vector2(0f, 1f);
            ageRect.anchoredPosition = new Vector2(0f, -34f);
            ageRect.sizeDelta = new Vector2(0f, 26f);
            Text scAgeText = ageObj.AddComponent<Text>();
            scAgeText.font = defaultFont;
            scAgeText.fontSize = 17;
            scAgeText.fontStyle = FontStyle.Bold;
            scAgeText.color = Color.white;
            scAgeText.alignment = TextAnchor.MiddleLeft;

            GameObject dateObj = CreateChild("Texto_Fecha", dataCol.transform);
            RectTransform dtRect = dateObj.AddComponent<RectTransform>();
            dtRect.anchorMin = new Vector2(0f, 1f);
            dtRect.anchorMax = new Vector2(1f, 1f);
            dtRect.pivot = new Vector2(0f, 1f);
            dtRect.anchoredPosition = new Vector2(0f, -62f);
            dtRect.sizeDelta = new Vector2(0f, 26f);
            Text scDateText = dateObj.AddComponent<Text>();
            scDateText.font = defaultFont;
            scDateText.fontSize = 16;
            scDateText.fontStyle = FontStyle.Bold;
            scDateText.color = new Color(0.95f, 0.65f, 0.4f, 1f);
            scDateText.alignment = TextAnchor.MiddleLeft;

            GameObject detObj = CreateChild("Texto_Detalles", dataCol.transform);
            RectTransform detRect = detObj.AddComponent<RectTransform>();
            detRect.anchorMin = new Vector2(0f, 1f);
            detRect.anchorMax = new Vector2(1f, 1f);
            detRect.pivot = new Vector2(0f, 1f);
            detRect.anchoredPosition = new Vector2(0f, -94f);
            detRect.sizeDelta = new Vector2(0f, 160f);
            Text scDetText = detObj.AddComponent<Text>();
            scDetText.font = defaultFont;
            scDetText.fontSize = 15;
            scDetText.color = new Color(0.92f, 0.92f, 0.90f, 1f);
            scDetText.alignment = TextAnchor.UpperLeft;

            GameObject scBtnObj = CreateChild("Boton_Continuar_Busqueda", innerCard.transform);
            RectTransform scbRect = scBtnObj.AddComponent<RectTransform>();
            scbRect.anchorMin = new Vector2(0.5f, 0f);
            scbRect.anchorMax = new Vector2(0.5f, 0f);
            scbRect.pivot = new Vector2(0.5f, 0f);
            scbRect.anchoredPosition = new Vector2(0f, 15f);
            scbRect.sizeDelta = new Vector2(400f, 44f);

            Image scbImg = scBtnObj.AddComponent<Image>();
            scbImg.color = new Color(0.2f, 0.65f, 0.35f, 1f);
            Button scBtn = scBtnObj.AddComponent<Button>();

            GameObject scbtObj = CreateChild("Texto", scBtnObj.transform);
            RectTransform scbtRect = scbtObj.AddComponent<RectTransform>();
            scbtRect.anchorMin = Vector2.zero;
            scbtRect.anchorMax = Vector2.one;
            scbtRect.sizeDelta = Vector2.zero;
            Text scbtText = scbtObj.AddComponent<Text>();
            scbtText.font = defaultFont;
            scbtText.fontSize = 16;
            scbtText.fontStyle = FontStyle.Bold;
            scbtText.color = Color.white;
            scbtText.alignment = TextAnchor.MiddleCenter;
            scbtText.text = "CONTINUAR BÚSQUEDA [Espacio / Enter / Clic]";
            scbtText.raycastTarget = false;

            searchCardModal.SetActive(false);

            HUDManager hud = canvasObj.AddComponent<HUDManager>();
            SetPrivateField(hud, "interactionPromptPanel", promptObj);
            SetPrivateField(hud, "victoryBannerObj", victoryBannerObj);
            SetPrivateField(hud, "victoryBannerText", victoryBannerText);
            SetPrivateField(hud, "interactionPromptText", promptText);
            SetPrivateField(hud, "objectiveText", objectiveText);
            SetPrivateField(hud, "enemiesText", enemiesText);
            SetPrivateField(hud, "heartText", heartText);
            SetPrivateField(hud, "heartImages", heartImgs);
            SetPrivateField(hud, "fullHeartSprite", fullHeartSpr);
            SetPrivateField(hud, "emptyHeartSprite", emptyHeartSpr);
            SetPrivateField(hud, "healthFillImage", fillImg);
            SetPrivateField(hud, "staminaSlider", staminaSlider);
            SetPrivateField(hud, "sprintStateText", sprintStateText);
            SetPrivateField(hud, "notificationText", notifText);
            SetPrivateField(hud, "notificationCanvasGroup", nCg);
            SetPrivateField(hud, "gameOverPanel", gameOverPanel);
            SetPrivateField(hud, "retryButton", retryBtn);

            SetPrivateField(hud, "searchCardModal", searchCardModal);
            SetPrivateField(hud, "searchCardPhoto", photoImg);
            SetPrivateField(hud, "searchCardTitleText", scTitleText);
            SetPrivateField(hud, "searchCardNameText", scNameText);
            SetPrivateField(hud, "searchCardAgeText", scAgeText);
            SetPrivateField(hud, "searchCardDateText", scDateText);
            SetPrivateField(hud, "searchCardDetailsText", scDetText);
            SetPrivateField(hud, "searchCardCloseButton", scBtn);

            GameEndingUI endingUI = canvasObj.AddComponent<GameEndingUI>();
            SetPrivateField(endingUI, "endingPanel", endingPanel);
            SetPrivateField(endingUI, "titleText", endingPanel.transform.Find("Pantalla_FIN_Titulo")?.GetComponent<Text>());
            SetPrivateField(endingUI, "subtitleText", endingPanel.transform.Find("Pantalla_FIN_Subtitulo")?.GetComponent<Text>());
            SetPrivateField(endingUI, "messageText", endingPanel.transform.Find("Pantalla_FIN_Mensaje")?.GetComponent<Text>());
            SetPrivateField(endingUI, "restartButton", endingRestartBtn);
        }
    }
}

