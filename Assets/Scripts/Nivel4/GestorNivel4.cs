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

namespace HastaEncontrarles.Nivel4
{
    public enum EstadoNivel4
    {
        Splash,
        MenuPrincipal,
        Jugar,
        Pausado,
        FinDeNivel,
        GameOver,
        Creditos
    }

    public class GestorNivel4 : MonoBehaviour
    {
        public static GestorNivel4 Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInicializarEscena()
        {
            if (!SceneManager.GetActiveScene().name.Contains("Nivel_4")) return;
            if (FindAnyObjectByType<GestorNivel4>() == null)
            {
                GameObject go = new GameObject("GestorNivel4_Master");
                go.AddComponent<GestorNivel4>();
            }
        }

        [Header("Estado Actual (4 Estados Obligatorios + Menús)")]
        public EstadoNivel4 estadoActual = EstadoNivel4.Jugar;

        [Header("Progresión Narrativa Lineal del Nivel 4")]
        public bool dictamenForenseRecibido = false;
        public bool preguntaAutodidactaRespondida = false;
        public int panuelosColocados = 0;
        public const int TOTAL_PANUELOS = 22;
        public bool veladorasEncendidas = false;
        public int botellasAguaRecogidas = 0;

        [Header("Línea de vida (Se recupera con Botellas de Agua)")]
        public float animoColectivo = 100f;
        public float desgastePasivoPorSegundo = 0f;
        public int puntajeInvestigacion = 0;
        public float tiempoNivelSegundos = 0f;
        public bool modoAutodidactaActivo = true;

        // Referencias del mundo 2D
        public PlayerControllerNivel4 Jugador { get; private set; }
        private Transform contenedorMundo;
        private List<ZonaInteractiva2D> estaciones = new List<ZonaInteractiva2D>();
        private List<SpriteRenderer> panuelosEnMuralMundo = new List<SpriteRenderer>();
        private List<ParticleSystem> flamasVeladorasMundo = new List<ParticleSystem>();
        private List<DesafioIndiferencia2D> listaObstaculosPatrulla = new List<DesafioIndiferencia2D>();

        // Datos de los 22 migrantes y diálogos
        private List<DatosMigrante> lista22Migrantes;
        private List<LineaDialogo> dialogoActual;
        private int indiceDialogo = 0;
        private System.Action accionAlTerminarDialogo;
        private Coroutine corrutinaMaquinaEscribir;
        private bool escribiendoTexto = false;
        private string textoCompletoLineaActual = "";

        // Referencias de UI / Canvas
        private Canvas canvasPrincipal;
        private Font fuenteUI;

        // Paneles de Estados y Pantallas
        private GameObject panelSplash;
        private GameObject panelMenuPrincipal;
        private GameObject panelSeleccionNiveles;
        private GameObject panelOpciones;
        private GameObject panelHUD;
        private GameObject panelPausa;
        private GameObject panelVictoria;
        private GameObject panelGameOver;
        private GameObject panelCreditos;

        // Subpaneles de Mecánicas del Nivel 4
        private GameObject panelDialogo;
        private GameObject panelPreguntaAutodidacta;
        private GameObject panelCuadernoBusqueda;
        private GameObject panelMuralBordado;
        private GameObject panelRetoVeladoras;
        private GameObject panelActoFinalDocumental;
        private GameObject panelGuiaAutodidacta;

        // Elementos dinámicos del HUD
        private Image barraAnimoFill;
        private Text textoAnimoPorcentaje;
        private Text textoContadorPanuelos;
        private Text textoPuntajeHUD;
        private Text textoTiempoHUD;
        private Text textoObjetivoMinimapa;
        private RectTransform marcadorJugadorMinimapa;
        private GameObject avisoInteraccionFlotante;
        private Text textoAvisoInteraccion;

        // Elementos del Panel de Diálogo (ui_caja_dialogo)
        private Text textoHablanteDialogo;
        private Text textoCargoDialogo;
        private Text textoCuerpoDialogo;
        private Text textoNotaAutodidactaDialogo;

        // Elementos del Mural de Bordado y Rompecabezas (22 ausentes)
        private RompecabezasMuralUI controladorRompecabezasMural;
        private bool rompecabezasInicializado = false;
        private Image imgRetratoDetalleMural;
        private Text txtNombreDetalleMural;
        private Text txtExpedienteDetalleMural;
        private Text txtHistoriaDetalleMural;
        private Text txtBordadoDetalleMural;
        private Text txtEstadoBotonBordar;
        private List<Image> fondosBotonesGrid22 = new List<Image>();

        // Elementos de Victoria y Cuaderno
        private Text txtResumenVictoria;
        private Text txtContenidoCuaderno;

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }
            Instancia = this;

            fuenteUI = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (fuenteUI == null) fuenteUI = Font.CreateDynamicFontFromOSFont("Arial", 16);

            lista22Migrantes = DatosMemoriaNivel4.ObtenerLos22Ausentes();

            if (FindAnyObjectByType<GestorAudioNivel4>() == null)
            {
                GameObject goAudio = new GameObject("GestorAudioNivel4");
                goAudio.AddComponent<GestorAudioNivel4>();
            }

            DatosProgresoJuego progreso = GestorGuardado.CargarProgreso();
            modoAutodidactaActivo = progreso.modoAutodidacta;

            ConfigurarCamara2D();
            AsegurarEventSystem();
            ConstruirMundo2D();
            ConstruirInterfazCompleta();

            // Iniciar directamente en el Nivel 4 para mantener continuidad desde el Nivel 3
            IniciarEstadoJugar();
        }

        private void ConfigurarCamara2D()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject goCam = new GameObject("Main Camera");
                goCam.tag = "MainCamera";
                cam = goCam.AddComponent<Camera>();
                goCam.AddComponent<AudioListener>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 4.6f;
            cam.backgroundColor = new Color(0.11f, 0.09f, 0.18f, 1f);
            cam.transform.position = new Vector3(-10f, 0.35f, -10f);
        }

        private void AsegurarEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                es.AddComponent<InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        private void ConstruirMundo2D()
        {
            if (contenedorMundo != null) Destroy(contenedorMundo.gameObject);
            GameObject goMundo = new GameObject("Mundo2D_Nivel4_SanLuisDeLaPaz");
            contenedorMundo = goMundo.transform;

            ConstructorMundoNivel4.ConstruirEscenarioPlazaYLab(
                contenedorMundo,
                out PlayerControllerNivel4 jug,
                out estaciones,
                out panuelosEnMuralMundo,
                out flamasVeladorasMundo,
                out listaObstaculosPatrulla);

            Jugador = jug;
        }

        private IEnumerator SecuenciaSplashAutomatica()
        {
            yield return new WaitForSecondsRealtime(2.2f);
            if (estadoActual == EstadoNivel4.Splash)
            {
                CambiarEstado(EstadoNivel4.MenuPrincipal);
            }
        }

        private void Update()
        {
            if (Jugador != null && Camera.main != null)
            {
                float targetX = Mathf.Clamp(Jugador.transform.position.x + 1.2f, -11.8f, 15.8f);
                Vector3 posCam = Camera.main.transform.position;
                posCam.x = Mathf.Lerp(posCam.x, targetX, Time.unscaledDeltaTime * 5.2f);
                posCam.y = Mathf.Lerp(posCam.y, 0.35f, Time.unscaledDeltaTime * 4.2f);
                Camera.main.transform.position = posCam;
            }

            if (estadoActual == EstadoNivel4.Splash && LeerTeclaConfirmacion())
            {
                CambiarEstado(EstadoNivel4.MenuPrincipal);
                return;
            }

            if (estadoActual == EstadoNivel4.Jugar)
            {
                ActualizarLogicaGameplay();
            }
            else if (estadoActual == EstadoNivel4.Pausado)
            {
                if (LeerTeclaPausa())
                {
                    ReanudarJuego();
                }
            }
        }

        private void ActualizarLogicaGameplay()
        {
            if (LeerTeclaPausa())
            {
                PausarJuego();
                return;
            }

            bool ventanaAbierta = (panelDialogo != null && panelDialogo.activeSelf) ||
                                  (panelPreguntaAutodidacta != null && panelPreguntaAutodidacta.activeSelf) ||
                                  (panelMuralBordado != null && panelMuralBordado.activeSelf) ||
                                  (panelRetoVeladoras != null && panelRetoVeladoras.activeSelf) ||
                                  (panelCuadernoBusqueda != null && panelCuadernoBusqueda.activeSelf) ||
                                  (panelActoFinalDocumental != null && panelActoFinalDocumental.activeSelf) ||
                                  (panelGuiaAutodidacta != null && panelGuiaAutodidacta.activeSelf);

            if (Jugador != null)
            {
                Jugador.controlesActivos = !ventanaAbierta;
            }

            if (panelDialogo != null && panelDialogo.activeSelf)
            {
                if (LeerTeclaAvanzarDialogo())
                {
                    AvanzarLineaDialogo();
                }
                return;
            }

            if (panelMuralBordado != null && panelMuralBordado.activeSelf)
            {
                if (LeerTeclaAvanzarDialogo())
                {
                    BordarPanueloSeleccionado();
                }
                return;
            }

            if (ventanaAbierta) return;

            tiempoNivelSegundos += Time.deltaTime;
            animoColectivo = Mathf.Max(0f, animoColectivo - desgastePasivoPorSegundo * Time.deltaTime);

            if (animoColectivo <= 0.01f)
            {
                ActivarGameOver();
                return;
            }

            if (LeerTeclaCuaderno())
            {
                AlternarCuadernoBusqueda();
                return;
            }

            if (LeerTeclaVarilla())
            {
                EjecutarSondeoVarillaT();
            }

            ZonaInteractiva2D estacionCercana = ObtenerEstacionMasCercana();
            if (estacionCercana != null)
            {
                avisoInteraccionFlotante.SetActive(true);
                textoAvisoInteraccion.text = estacionCercana.tituloEstacion + "\n<color=#FFE875>" + estacionCercana.instruccionTecla + "</color>";

                if (LeerTeclaInteractuar())
                {
                    InteractuarConEstacion(estacionCercana);
                }
            }
            else
            {
                avisoInteraccionFlotante.SetActive(false);
            }

            ActualizarHUD();
        }

        private ZonaInteractiva2D ObtenerEstacionMasCercana()
        {
            if (Jugador == null) return null;
            ZonaInteractiva2D mejor = null;
            float menorDist = 999f;
            foreach (var est in estaciones)
            {
                if (est != null && est.JugadorEnRango)
                {
                    float d = Mathf.Abs(Jugador.transform.position.x - est.transform.position.x);
                    if (d < menorDist)
                    {
                        menorDist = d;
                        mejor = est;
                    }
                }
            }
            return mejor;
        }

        public void InteractuarEstacionActualDesdeBotonUI()
        {
            if (panelDialogo != null && panelDialogo.activeSelf)
            {
                AvanzarLineaDialogo();
                return;
            }
            ZonaInteractiva2D est = ObtenerEstacionMasCercana();
            if (est != null) InteractuarConEstacion(est);
            else EjecutarSondeoVarillaT();
        }

        private void InteractuarConEstacion(ZonaInteractiva2D est)
        {
            if (Jugador != null) Jugador.EjecutarAnimacionAccion(0.5f);

            switch (est.tipoEstacion)
            {
                case TipoEstacionNivel4.LaboratorioEAAF:
                    IniciarDialogo(DatosMemoriaNivel4.ObtenerDialogoPeritoEAAF(), () =>
                    {
                        if (!preguntaAutodidactaRespondida)
                        {
                            panelPreguntaAutodidacta.SetActive(true);
                        }
                        else if (!dictamenForenseRecibido)
                        {
                            OtorgarDictamenForense();
                        }
                    });
                    break;

                case TipoEstacionNivel4.SondeoVarillaDemostracion:
                    EjecutarSondeoVarillaT();
                    break;

                case TipoEstacionNivel4.DonaTereSolidaridad:
                    // Nota: la vida SOLO se recupera con las Botellas de Agua en el mundo
                    puntajeInvestigacion += 50;
                    IniciarDialogo(DatosMemoriaNivel4.ObtenerDialogoDonaTere(), null);
                    break;

                case TipoEstacionNivel4.MuralMemoriaKiosco:
                    if (!dictamenForenseRecibido)
                    {
                        IniciarDialogo(new List<LineaDialogo>
                        {
                            new LineaDialogo
                            {
                                hablante = "Esperanza (Madre Buscadora)",
                                cargo = "Colectivo Justicia y Esperanza",
                                texto = "Antes de consagrar el Mural de los 22 Pañuelos Verdes, debemos acudir a la izquierda al Laboratorio Forense del EAAF para recibir el Dictamen Científico Oficial.",
                                notaEducativa = "GUÍA DEL NIVEL 4: Camina hacia la izquierda (tecla [A]) esquivando los obstáculos del suelo hasta la Dra. Elena Vázquez."
                            }
                        }, null);
                    }
                    else
                    {
                        AbrirMuralBordado();
                    }
                    break;

                case TipoEstacionNivel4.AltarVeladorasActoFinal:
                    if (!dictamenForenseRecibido || panuelosColocados < TOTAL_PANUELOS)
                    {
                        IniciarDialogo(new List<LineaDialogo>
                        {
                            new LineaDialogo
                            {
                                hablante = "Doña Tere y Colectivo",
                                cargo = "Altar de las 22 Veladoras",
                                texto = $"Esperanza, aún nos falta colocar todos los pañuelos en el Kiosco Central (Llevamos {panuelosColocados}/{TOTAL_PANUELOS}). Ninguno de los 22 puede quedarse sin su nombre bordado antes del Reto de Encender las Veladoras.",
                                notaEducativa = "OBJETIVO REQUERIDO: Interactúa con el Kiosco Central para completar los 22/22 pañuelos bordados con hilo verde."
                            }
                        }, null);
                    }
                    else
                    {
                        AbrirRetoEncenderVeladoras();
                    }
                    break;
            }
        }

        public void EjecutarSondeoVarillaT()
        {
            if (Jugador != null)
            {
                Jugador.EjecutarAnimacionAccion(0.45f);
                CrearOndaPulsoVarilla(Jugador.transform.position);

                // Aturdir obstáculos móviles cercanos por unos segundos (sin recuperar vida, ya que solo las botellas de agua curan)
                foreach (var sombra in listaObstaculosPatrulla)
                {
                    if (sombra != null && Vector2.Distance(Jugador.transform.position, sombra.transform.position) <= 4.0f)
                    {
                        sombra.NeutralizarTemporalmente(3.5f);
                        puntajeInvestigacion += 35;
                    }
                }
            }

            if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirVarilla();
            ActualizarHUD();
        }

        private void CrearOndaPulsoVarilla(Vector3 posCentro)
        {
            GameObject go = new GameObject("OndaPulsoVarillaT");
            go.transform.position = posCentro;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.28f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.25f, 1f, 0.58f, 0.9f), new Color(1f, 0.88f, 0.25f, 0.9f));
            var emission = ps.emission;
            emission.enabled = false;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 30;
            ps.Emit(24);
            Destroy(go, 0.7f);
        }

        /// <summary>
        /// La vida (animoColectivo) ÚNICAMENTE se recupera al recoger Botellas de Agua (TipoColeccionable.AguaSolidaridad).
        /// </summary>
        public void RegistrarColeccionableRecogido(TipoColeccionable tipo, int pts, float vidaRecuperada)
        {
            puntajeInvestigacion += pts;
            if (tipo == TipoColeccionable.AguaSolidaridad)
            {
                botellasAguaRecogidas++;
                animoColectivo = Mathf.Min(100f, animoColectivo + vidaRecuperada);
            }
            if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirSolidaridad();
            ActualizarHUD();
        }

        private void OtorgarDictamenForense()
        {
            dictamenForenseRecibido = true;
            puntajeInvestigacion += 500;
            if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirPapel();
            ActualizarHUD();
        }

        // ==================== SISTEMA DE DIÁLOGO SECUENCIAL ACCESIBLE ====================
        private void IniciarDialogo(List<LineaDialogo> lineas, System.Action alTerminar)
        {
            if (lineas == null || lineas.Count == 0) return;
            dialogoActual = lineas;
            indiceDialogo = 0;
            accionAlTerminarDialogo = alTerminar;
            panelDialogo.SetActive(true);
            avisoInteraccionFlotante.SetActive(false);
            MostrarLineaDialogoActual();
        }

        private void MostrarLineaDialogoActual()
        {
            LineaDialogo linea = dialogoActual[indiceDialogo];
            textoHablanteDialogo.text = linea.hablante;
            textoCargoDialogo.text = linea.cargo;
            textoNotaAutodidactaDialogo.text = linea.notaEducativa;
            textoCompletoLineaActual = linea.texto;

            if (corrutinaMaquinaEscribir != null) StopCoroutine(corrutinaMaquinaEscribir);
            corrutinaMaquinaEscribir = StartCoroutine(EfectoMaquinaEscribir(textoCompletoLineaActual));
        }

        private IEnumerator EfectoMaquinaEscribir(string texto)
        {
            escribiendoTexto = true;
            textoCuerpoDialogo.text = "";
            for (int i = 0; i < texto.Length; i++)
            {
                textoCuerpoDialogo.text += texto[i];
                if (i % 3 == 0 && GestorAudioNivel4.Instancia != null)
                {
                    GestorAudioNivel4.Instancia.ReproducirBlipDialogo();
                }
                yield return new WaitForSecondsRealtime(0.014f);
            }
            escribiendoTexto = false;
        }

        public void AvanzarLineaDialogo()
        {
            if (escribiendoTexto)
            {
                if (corrutinaMaquinaEscribir != null) StopCoroutine(corrutinaMaquinaEscribir);
                textoCuerpoDialogo.text = textoCompletoLineaActual;
                escribiendoTexto = false;
                return;
            }

            indiceDialogo++;
            if (dialogoActual != null && indiceDialogo < dialogoActual.Count)
            {
                MostrarLineaDialogoActual();
            }
            else
            {
                panelDialogo.SetActive(false);
                var callback = accionAlTerminarDialogo;
                accionAlTerminarDialogo = null;
                callback?.Invoke();
            }
        }

        // ==================== ROMPECABEZAS DEL MURAL DE LOS 22 DESAPARECIDOS ====================
        private void AbrirMuralBordado()
        {
            panelMuralBordado.SetActive(true);
            avisoInteraccionFlotante.SetActive(false);

            if (controladorRompecabezasMural != null && !rompecabezasInicializado)
            {
                controladorRompecabezasMural.InicializarRompecabezas(lista22Migrantes);
                rompecabezasInicializado = true;
            }
        }

        public void SincronizarProgresoRompecabezasMural(int colocadosCorrectamente)
        {
            if (colocadosCorrectamente > panuelosColocados)
            {
                int nuevos = colocadosCorrectamente - panuelosColocados;
                puntajeInvestigacion += nuevos * 120;
            }
            panuelosColocados = colocadosCorrectamente;

            for (int i = 0; i < panuelosEnMuralMundo.Count && i < lista22Migrantes.Count; i++)
            {
                if (panuelosEnMuralMundo[i] != null)
                {
                    panuelosEnMuralMundo[i].color = lista22Migrantes[i].panueloColocado
                        ? Color.white
                        : new Color(0.3f, 0.45f, 0.35f, 0.28f);
                }
            }
            ActualizarHUD();
        }

        public void SeleccionarMigranteEnMural(int idx)
        {
            if (controladorRompecabezasMural != null)
            {
                controladorRompecabezasMural.AlHacerClicEnCasilla22(idx);
            }
        }

        public void BordarPanueloSeleccionado()
        {
            if (controladorRompecabezasMural != null)
            {
                controladorRompecabezasMural.ResolverPiezaActual();
            }
        }

        public void BordarTodosEnColectivo()
        {
            if (controladorRompecabezasMural != null)
            {
                controladorRompecabezasMural.ResolverTodoElRompecabezas22();
            }
            else
            {
                SincronizarProgresoRompecabezasMural(TOTAL_PANUELOS);
            }
        }

        // ==================== RETO INTERACTIVO DE ENCENDER VELADORAS Y ACTO FINAL ====================
        private void AbrirRetoEncenderVeladoras()
        {
            avisoInteraccionFlotante.SetActive(false);
            panelRetoVeladoras.SetActive(true);
        }

        public void EncenderParcialmenteVeladorasMundo(int rondaCompletada, int totalRondas)
        {
            puntajeInvestigacion += 150;
            int cantidadEncender = Mathf.RoundToInt((rondaCompletada / (float)totalRondas) * flamasVeladorasMundo.Count);
            for (int i = 0; i < cantidadEncender && i < flamasVeladorasMundo.Count; i++)
            {
                if (flamasVeladorasMundo[i] != null && !flamasVeladorasMundo[i].isPlaying)
                {
                    flamasVeladorasMundo[i].Play();
                }
            }
            ActualizarHUD();
        }

        public void CompletarRetoVeladorasExitoso()
        {
            veladorasEncendidas = true;
            puntajeInvestigacion += 500;

            foreach (var sombra in listaObstaculosPatrulla)
            {
                if (sombra != null) sombra.activo = false;
            }

            foreach (var ps in flamasVeladorasMundo)
            {
                if (ps != null && !ps.isPlaying) ps.Play();
            }

            LanzarExplosionConfetiMundo2D();

            if (GestorAudioNivel4.Instancia != null)
            {
                GestorAudioNivel4.Instancia.ReproducirVeladora();
                GestorAudioNivel4.Instancia.IniciarAudioNivel4();
            }

            panelActoFinalDocumental.SetActive(true);
            ActualizarHUD();
        }

        private void LanzarExplosionConfetiMundo2D()
        {
            if (Jugador == null) return;
            GameObject go = new GameObject("ExplosionConfetiMundo2D");
            go.transform.position = Jugador.transform.position + new Vector3(0f, 3.5f, 0f);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 2.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 6.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.32f);
            main.gravityModifier = 0.45f;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.2f, 1f, 0.5f, 1f), new Color(1f, 0.85f, 0.15f, 1f));
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(12f, 1f, 1f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 40;
            ps.Play();
        }

        public void ConcluirActoFinalYMostrarVictoria()
        {
            panelActoFinalDocumental.SetActive(false);

            if (GestorAudioNivel4.Instancia != null)
            {
                GestorAudioNivel4.Instancia.ReproducirVeladora();
                GestorAudioNivel4.Instancia.ReproducirSolidaridad();
            }

            GestorGuardado.RegistrarVictoriaNivel4(puntajeInvestigacion, tiempoNivelSegundos, panuelosColocados);
            DatosProgresoJuego datosGuardados = GestorGuardado.CargarProgreso();

            int min = Mathf.FloorToInt(tiempoNivelSegundos / 60f);
            int seg = Mathf.FloorToInt(tiempoNivelSegundos % 60f);

            txtResumenVictoria.text =
                $"<size=28><color=#FFE866><b>★ ¡MISIÓN CUMPLIDA AL 100%: LA MEMORIA VIVA TRIUNFA! ★</b></color></size>\n\n" +
                $"• Dictamen Científico del EAAF: <color=#68F596>INTEGRADO EN CUADERNO</color>\n" +
                $"• Pañuelos Bordados en el Kiosco: <color=#68F596>{panuelosColocados} / {TOTAL_PANUELOS}</color>\n" +
                $"• Reto de las 22 Veladoras: <color=#FFE268>SUPERADO Y ENCENDIDO</color>\n" +
                $"• Botellas de Agua Recolectadas: <color=#80E8FF>{botellasAguaRecogidas}</color>\n\n" +
                $"• Tiempo Total del Nivel 4: <b>{min:00}:{seg:00}</b>\n" +
                $"• Puntaje Final Obtenido: <color=#68F596><b>{puntajeInvestigacion} pts</b></color>\n" +
                $"• Puntaje Máximo Histórico Registrado: <color=#FFE268><b>{datosGuardados.puntajeMaximoHistorico} pts</b></color>";

            CambiarEstado(EstadoNivel4.FinDeNivel);
        }

        // ==================== CONTROL DE LOS 4 ESTADOS OBLIGATORIOS ====================
        public void CambiarEstado(EstadoNivel4 nuevoEstado)
        {
            estadoActual = nuevoEstado;

            if (panelSplash != null) panelSplash.SetActive(nuevoEstado == EstadoNivel4.Splash);
            if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(nuevoEstado == EstadoNivel4.MenuPrincipal);
            if (panelHUD != null) panelHUD.SetActive(nuevoEstado == EstadoNivel4.Jugar || nuevoEstado == EstadoNivel4.Pausado);
            if (panelPausa != null) panelPausa.SetActive(nuevoEstado == EstadoNivel4.Pausado);
            if (panelVictoria != null) panelVictoria.SetActive(nuevoEstado == EstadoNivel4.FinDeNivel);
            if (panelGameOver != null) panelGameOver.SetActive(nuevoEstado == EstadoNivel4.GameOver);
            if (panelCreditos != null) panelCreditos.SetActive(nuevoEstado == EstadoNivel4.Creditos);

            if (panelSeleccionNiveles != null) panelSeleccionNiveles.SetActive(false);
            if (panelOpciones != null) panelOpciones.SetActive(false);

            switch (nuevoEstado)
            {
                case EstadoNivel4.Splash:
                case EstadoNivel4.MenuPrincipal:
                    Time.timeScale = 1f;
                    if (Jugador != null) Jugador.controlesActivos = false;
                    if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBGMMenu();
                    break;

                case EstadoNivel4.Jugar:
                    Time.timeScale = 1f;
                    if (Jugador != null) Jugador.controlesActivos = true;
                    if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.IniciarAudioNivel4();
                    ActualizarHUD();
                    break;

                case EstadoNivel4.Pausado:
                    Time.timeScale = 0f;
                    if (Jugador != null) Jugador.controlesActivos = false;
                    break;

                case EstadoNivel4.FinDeNivel:
                    Time.timeScale = 0f;
                    if (Jugador != null) Jugador.controlesActivos = false;
                    break;

                case EstadoNivel4.GameOver:
                    Time.timeScale = 0f;
                    if (Jugador != null) Jugador.controlesActivos = false;
                    if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirAlerta();
                    break;

                case EstadoNivel4.Creditos:
                    Time.timeScale = 1f;
                    if (Jugador != null) Jugador.controlesActivos = false;
                    if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.IniciarAudioNivel4();
                    break;
            }
        }

        public void IniciarEstadoJugar()
        {
            CambiarEstado(EstadoNivel4.Jugar);
            if (modoAutodidactaActivo && !dictamenForenseRecibido && tiempoNivelSegundos < 1f)
            {
                panelGuiaAutodidacta.SetActive(true);
            }
        }

        public void PausarJuego() => CambiarEstado(EstadoNivel4.Pausado);
        public void ReanudarJuego() => CambiarEstado(EstadoNivel4.Jugar);

        public void ReiniciarNivelActual()
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

        public void SalirAlMenuPrincipal()
        {
            Time.timeScale = 1f;
            CerrarSubpanelesGameplay();
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
            try
            {
                SceneManager.LoadScene("MenuInicio");
            }
            catch
            {
                CambiarEstado(EstadoNivel4.MenuPrincipal);
            }
        }

        public void ActivarGameOver()
        {
            CerrarSubpanelesGameplay();
            CambiarEstado(EstadoNivel4.GameOver);
        }

        public void SalirAplicacion()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CerrarSubpanelesGameplay()
        {
            if (panelDialogo != null) panelDialogo.SetActive(false);
            if (panelPreguntaAutodidacta != null) panelPreguntaAutodidacta.SetActive(false);
            if (panelCuadernoBusqueda != null) panelCuadernoBusqueda.SetActive(false);
            if (panelMuralBordado != null) panelMuralBordado.SetActive(false);
            if (panelRetoVeladoras != null) panelRetoVeladoras.SetActive(false);
            if (panelActoFinalDocumental != null) panelActoFinalDocumental.SetActive(false);
            if (panelGuiaAutodidacta != null) panelGuiaAutodidacta.SetActive(false);
        }

        public void AplicarDesgasteAnimo(float cantidad, string motivo)
        {
            animoColectivo = Mathf.Max(0f, animoColectivo - cantidad);
            if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirAlerta();
            ActualizarHUD();
            if (animoColectivo <= 0.01f)
            {
                ActivarGameOver();
            }
        }

        public void AlternarCuadernoBusqueda()
        {
            if (panelCuadernoBusqueda == null) return;
            bool abrir = !panelCuadernoBusqueda.activeSelf;
            panelCuadernoBusqueda.SetActive(abrir);
            if (abrir)
            {
                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirPapel();
                RefrescarTextoCuaderno();
            }
        }

        private void RefrescarTextoCuaderno()
        {
            string estadoDictamen = dictamenForenseRecibido
                ? "<color=#68F596><b>✓ DICTAMEN FORENSE OFICIAL (EAAF) — CONFIRMADO</b></color>\n" +
                  "• Expediente: AP/PGR/SIEDO/UEITMIO/067/2012\n" +
                  "• Metodología: Antropología forense, odontología y genética (ADN nuclear y mitocondrial).\n" +
                  "• Resultado: Identificación científica confirmada de Mariano Luna Jiménez en San Fernando, Tamaulipas, y continuación del protocolo de búsqueda en vida y en campo para sus 21 compañeros de San Luis de la Paz."
                : "<color=#FFCC66><b>⏳ DICTAMEN FORENSE PENDIENTE DE RECEPCIÓN</b></color>\n" +
                  "• Acude al Laboratorio del EAAF (extremo izquierdo de la escena) y habla con la Dra. Elena Vázquez.";

            txtContenidoCuaderno.text =
                "<b>=== CUADERNO DE BÚSQUEDA Y MEMORIA COLECTIVA ===</b>\n\n" +
                "<b>1. RUTA RECONSTRUIDA (NIVELES 1 AL 3):</b>\n" +
                "• <b>Nivel 1 (Carretera):</b> Salida del autobús el 21 de marzo de 2011 desde San Luis de la Paz rumbo a Dolores Hidalgo. Fotografía grupal y talón de boleto recuperados.\n" +
                "• <b>Nivel 2 (Oficinas):</b> Cotejo de señas particulares y obtención de copias selladas ante la burocracia.\n" +
                "• <b>Nivel 3 (Semidesierto):</b> Sondeo respetuoso con la Varilla 'T', marcaje con banderines rojos y entrega de coordenadas al EAAF sin contaminar la escena.\n\n" +
                "<b>2. DICTAMEN CIENTÍFICO DEL NIVEL 4:</b>\n" +
                estadoDictamen + "\n\n" +
                $"<b>3. REGISTRO DE LOS 22 MIGRANTES EN EL MURAL ({panuelosColocados}/22 BORDADOS):</b>\n" +
                "Fernando Guzmán Ramírez, José Antonio Guerrero López, Miguel Ángel Ramírez Araiza, José Luis Duarte Cruz, Rafael Paz Guerrero, Juan Manuel Rojas Pérez, Santos Eloy Peralta Hernández, Antonio Coronilla Luna, José Humberto Morín López, Alejandro Castillo Ramírez, Raúl Trejo Medina, Valentín Alamilla Camacho, Ricardo Salazar Sánchez, Juan Manuel Duarte Cruz, Héctor Castillo Salazar, Gregorio Coronilla Luna, Ángel Padrón Sandoval, José Manuel Pérez Guerrero, Isidro González Coronilla, Miguel Jaramillo Palacios, Samuel Guzmán Castañeda y Mariano Luna Jiménez.";
        }

        private void ActualizarHUD()
        {
            if (barraAnimoFill != null)
            {
                barraAnimoFill.fillAmount = Mathf.Clamp01(animoColectivo / 100f);
                barraAnimoFill.color = animoColectivo > 55f
                    ? new Color(0.22f, 0.85f, 0.45f)
                    : (animoColectivo > 25f ? new Color(0.95f, 0.72f, 0.18f) : new Color(0.92f, 0.25f, 0.25f));
            }
            if (textoAnimoPorcentaje != null)
                textoAnimoPorcentaje.text = $"❤️ LÍNEA DE VIDA: {Mathf.RoundToInt(animoColectivo)}%";

            if (textoContadorPanuelos != null)
                textoContadorPanuelos.text = $"Pañuelos colocados: {panuelosColocados} / {TOTAL_PANUELOS}";

            if (textoPuntajeHUD != null)
                textoPuntajeHUD.text = $"Puntaje: {puntajeInvestigacion} pts";

            if (textoTiempoHUD != null)
            {
                int min = Mathf.FloorToInt(tiempoNivelSegundos / 60f);
                int seg = Mathf.FloorToInt(tiempoNivelSegundos % 60f);
                textoTiempoHUD.text = $"Tiempo: {min:00}:{seg:00}   |   💧 Botellas de Agua: {botellasAguaRecogidas}";
            }

            if (textoObjetivoMinimapa != null && Jugador != null)
            {
                float px = Jugador.transform.position.x;
                if (!dictamenForenseRecibido)
                {
                    string flecha = px > -9.8f ? "◄ Camina a la izquierda" : "● Interactúa con [E]";
                    textoObjetivoMinimapa.text = $"OBJETIVO 1/3: Recibir Dictamen Forense del EAAF ({flecha})";
                }
                else if (panuelosColocados < TOTAL_PANUELOS)
                {
                    string flecha = px < 7.5f ? "Esquiva obstáculos del suelo hacia la derecha ►" : "● Presiona [E] en el Kiosco";
                    textoObjetivoMinimapa.text = $"OBJETIVO 2/3: Bordar Pañuelos en el Kiosco ({panuelosColocados}/22) — {flecha}";
                }
                else if (!veladorasEncendidas)
                {
                    string flecha = px < 15.8f ? "Avanza al Altar a la derecha ►" : "● Presiona [E] para el Reto de Veladoras";
                    textoObjetivoMinimapa.text = $"OBJETIVO 3/3: Superar el Reto de Encender las 22 Veladoras — {flecha}";
                }
                else
                {
                    textoObjetivoMinimapa.text = "🎉 ¡¡GANASTE!! Memoria Viva Consagrada al 100%.";
                }

                if (marcadorJugadorMinimapa != null)
                {
                    float norm = Mathf.InverseLerp(-16f, 20f, px);
                    marcadorJugadorMinimapa.anchoredPosition = new Vector2(Mathf.Lerp(-150f, 150f, norm), 0f);
                }
            }
        }

        // ==================== LECTURA DE CONTROLES MULTIPLATAFORMA ====================
        private bool LeerTeclaPausa()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) return true; } catch { }
            return false;
        }

        private bool LeerTeclaInteractuar()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)) return true; } catch { }
            return false;
        }

        private bool LeerTeclaAvanzarDialogo()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                return true;
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame))
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return)) return true; } catch { }
            return false;
        }

        private bool LeerTeclaCuaderno()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame))
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Tab)) return true; } catch { }
            return false;
        }

        private bool LeerTeclaVarilla()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.F)) return true; } catch { }
            return false;
        }

        private bool LeerTeclaConfirmacion()
        {
            return LeerTeclaAvanzarDialogo() || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        }

        // ==================== CONSTRUCCIÓN DE LA INTERFAZ (HUD, MENÚS Y LOS 4 ESTADOS) ====================
        private void ConstruirInterfazCompleta()
        {
            GameObject goCanvas = new GameObject("Canvas_UI_Nivel4");
            canvasPrincipal = goCanvas.AddComponent<Canvas>();
            canvasPrincipal.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasPrincipal.sortingOrder = 100;

            CanvasScaler scaler = goCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            goCanvas.AddComponent<GraphicRaycaster>();

            ConstruirPanelSplash(goCanvas.transform);
            ConstruirPanelMenuPrincipal(goCanvas.transform);
            ConstruirPanelHUD(goCanvas.transform);
            ConstruirPanelDialogo(goCanvas.transform);
            ConstruirPanelPreguntaAutodidacta(goCanvas.transform);
            ConstruirPanelMuralBordado(goCanvas.transform);
            ConstruirPanelRetoVeladoras(goCanvas.transform);
            ConstruirPanelCuadernoBusqueda(goCanvas.transform);
            ConstruirPanelActoFinalDocumental(goCanvas.transform);
            ConstruirPanelGuiaAutodidacta(goCanvas.transform);
            ConstruirPanelPausa(goCanvas.transform);
            ConstruirPanelVictoria(goCanvas.transform);
            ConstruirPanelGameOver(goCanvas.transform);
            ConstruirPanelCreditos(goCanvas.transform);
            ConstruirPanelSeleccionNiveles(goCanvas.transform);
            ConstruirPanelOpciones(goCanvas.transform);
        }

        private void ConstruirPanelRetoVeladoras(Transform raiz)
        {
            panelRetoVeladoras = CrearPanelCompleto(raiz, "Panel_RetoEncenderVeladoras", new Color(0.04f, 0.03f, 0.08f, 0.94f));
            GameObject caja = CrearCajaUI(panelRetoVeladoras.transform, "CajaRetoVelas", Vector2.zero, new Vector2(1380f, 820f), new Color(0.11f, 0.09f, 0.17f, 0.98f));

            CrearTextoUI(caja.transform, "TituloRetoVelas",
                "<b>🔥 RETO DEL ALTAR: ¡ENCIENDE LAS 22 VELADORAS CONTRA EL VIENTO! 🔥</b>\n" +
                "<size=21><color=#FFD970>Presiona [ESPACIO] o el botón exactamente cuando el cerillo 🔥 pase por la Zona Dorada</color></size>",
                new Vector2(0f, 320f), new Vector2(1280f, 90f), 30, TextAnchor.MiddleCenter, Color.white);

            CrearBotonUI(caja.transform, "✖ Salir [Esc]", new Vector2(560f, 330f), new Vector2(190f, 50f), new Color(0.55f, 0.22f, 0.24f), () => panelRetoVeladoras.SetActive(false), 18);

            Text txtProgreso = CrearTextoUI(caja.transform, "TxtProgresoVelas",
                "🕯 Veladoras Encendidas: 0 / 22   (Ronda 1 de 5)",
                new Vector2(0f, 225f), new Vector2(900f, 45f), 25, TextAnchor.MiddleCenter, new Color(0.55f, 1f, 0.72f));

            // 5 Grandes Veladoras simbólicas en la UI que se iluminan conforme avanzas
            MinijuegoVeladorasUI compReto = panelRetoVeladoras.AddComponent<MinijuegoVeladorasUI>();
            compReto.txtProgresoVelas = txtProgreso;

            for (int i = 0; i < 5; i++)
            {
                float vx = -400f + i * 200f;
                GameObject vaso = CrearCajaUI(caja.transform, "VelaUI_" + i, new Vector2(vx, 85f), new Vector2(90f, 130f), new Color(0.95f, 0.92f, 0.82f, 1f));
                CrearTextoUI(vaso.transform, "LblVela_" + i, $"Grupo {i + 1}", new Vector2(0f, -40f), new Vector2(86f, 28f), 15, TextAnchor.MiddleCenter, new Color(0.2f, 0.15f, 0.1f));

                GameObject flama = CrearCajaUI(vaso.transform, "FlamaUI_" + i, new Vector2(0f, 90f), new Vector2(54f, 74f), new Color(0.28f, 0.32f, 0.40f, 0.65f));
                CrearTextoUI(flama.transform, "IconoFuego", "🔥", Vector2.zero, new Vector2(54f, 70f), 34, TextAnchor.MiddleCenter, Color.white);
                compReto.iconosFlamasUI.Add(flama.GetComponent<Image>());
            }

            // Barra de Precisión (Viento Azul vs Zona Dorada de Calma)
            GameObject barraViento = CrearCajaUI(caja.transform, "BarraVientoFondo", new Vector2(0f, -75f), new Vector2(920f, 64f), new Color(0.14f, 0.26f, 0.44f, 1f));
            CrearTextoUI(barraViento.transform, "TxtVientoIzq", "💨 VIENTO", new Vector2(-380f, 0f), new Vector2(140f, 50f), 18, TextAnchor.MiddleCenter, new Color(0.65f, 0.85f, 1f));
            CrearTextoUI(barraViento.transform, "TxtVientoDer", "VIENTO 💨", new Vector2(380f, 0f), new Vector2(140f, 50f), 18, TextAnchor.MiddleCenter, new Color(0.65f, 0.85f, 1f));

            GameObject zonaOro = CrearCajaUI(barraViento.transform, "ZonaDoradaCalma", Vector2.zero, new Vector2(220f, 64f), new Color(0.96f, 0.74f, 0.12f, 0.95f));
            CrearTextoUI(zonaOro.transform, "LblZonaOro", "✨ ZONA SIN VIENTO ✨", Vector2.zero, new Vector2(210f, 50f), 16, TextAnchor.MiddleCenter, new Color(0.15f, 0.10f, 0.02f));

            GameObject cursor = CrearCajaUI(barraViento.transform, "CursorCerillo", Vector2.zero, new Vector2(28f, 88f), new Color(1f, 0.32f, 0.12f, 1f));
            CrearTextoUI(cursor.transform, "LblCerillo", "🔥", new Vector2(0f, 48f), new Vector2(60f, 50f), 32, TextAnchor.MiddleCenter, Color.white);

            Text txtEstado = CrearTextoUI(caja.transform, "TxtEstadoReto",
                "¡Espera a que el cerillo 🔥 entre en la franja dorada y presiona [ESPACIO]!",
                new Vector2(0f, -175f), new Vector2(1100f, 55f), 22, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.65f));

            compReto.barraFondo = barraViento.GetComponent<RectTransform>();
            compReto.zonaDorada = zonaOro.GetComponent<RectTransform>();
            compReto.cursorCerillo = cursor.GetComponent<RectTransform>();
            compReto.txtInstruccionEstado = txtEstado;

            CrearBotonUI(caja.transform, "🔥 ¡ENCENDER VELADORA AHORA! [ESPACIO]",
                new Vector2(0f, -280f), new Vector2(660f, 76f), new Color(0.88f, 0.42f, 0.12f), () => compReto.IntentarEncenderVela(), 24);

            panelRetoVeladoras.SetActive(false);
        }

        private void ConstruirPanelSplash(Transform raiz)
        {
            panelSplash = CrearPanelCompleto(raiz, "Panel_Splash", new Color(0.07f, 0.06f, 0.11f, 1f));

            Sprite spLogo = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/logo_colectivo", 100f);
            if (spLogo != null)
            {
                GameObject goLogo = new GameObject("LogoColectivo");
                goLogo.transform.SetParent(panelSplash.transform, false);
                RectTransform rt = goLogo.AddComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 150f);
                rt.sizeDelta = new Vector2(210f, 210f);
                Image img = goLogo.AddComponent<Image>();
                img.sprite = spLogo;
                img.preserveAspect = true;
            }

            CrearTextoUI(panelSplash.transform, "TituloSplash",
                "<b>HASTA ENCONTRARLES: EL CAMINO DE LA ESPERANZA</b>\n" +
                "<size=28><color=#78F0A0>Nivel 4: La Memoria Viva y el Reclamo de Justicia</color></size>\n\n" +
                "<size=22>Universidad Tecnológica del Norte de Guanajuato (UTNG)\n" +
                "Equipo: Valeria Guadalupe Calvillo Mendoza y María del Carmen Vargas Martínez\n" +
                "En memoria de los 22 migrantes desaparecidos de San Luis de la Paz (21 de marzo de 2011)</size>\n\n" +
                "<size=20><color=#FFE270>[ Presiona clic o ESPACIO para continuar ]</color></size>",
                new Vector2(0f, -110f), new Vector2(1400f, 340f), 36, TextAnchor.MiddleCenter, Color.white);
        }

        private void ConstruirPanelMenuPrincipal(Transform raiz)
        {
            panelMenuPrincipal = CrearPanelCompleto(raiz, "Panel_MenuPrincipal", new Color(0.09f, 0.08f, 0.14f, 0.94f));

            Sprite spPoster = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/poster_22_desaparecidos", 100f);
            if (spPoster != null)
            {
                GameObject goPoster = new GameObject("PosterMenu");
                goPoster.transform.SetParent(panelMenuPrincipal.transform, false);
                RectTransform rtP = goPoster.AddComponent<RectTransform>();
                rtP.anchoredPosition = new Vector2(-470f, -10f);
                rtP.sizeDelta = new Vector2(560f, 700f);
                Image imgP = goPoster.AddComponent<Image>();
                imgP.sprite = spPoster;
                imgP.preserveAspect = true;
            }

            Sprite spEsp = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Personajes/esperanza_idle_0", 100f);
            if (spEsp != null)
            {
                GameObject goEsp = new GameObject("EsperanzaMenu");
                goEsp.transform.SetParent(panelMenuPrincipal.transform, false);
                RectTransform rtE = goEsp.AddComponent<RectTransform>();
                rtE.anchoredPosition = new Vector2(520f, -40f);
                rtE.sizeDelta = new Vector2(360f, 560f);
                Image imgE = goEsp.AddComponent<Image>();
                imgE.sprite = spEsp;
                imgE.preserveAspect = true;
            }

            CrearTextoUI(panelMenuPrincipal.transform, "EncabezadoMenu",
                "<b>HASTA ENCONTRARLES: EL CAMINO DE LA ESPERANZA</b>\n" +
                "<size=26><color=#75F0A0>Nivel 4: La Memoria Viva y el Reclamo de Justicia — San Luis de la Paz, Gto.</color></size>",
                new Vector2(0f, 410f), new Vector2(1650f, 130f), 40, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.78f));

            DatosProgresoJuego prog = GestorGuardado.CargarProgreso();
            string infoGuardado = prog.juegoCompletado100
                ? $"<color=#68F596>★ PROGRESO GUARDADO: 100% COMPLETADO | Récord: {prog.puntajeMaximoHistorico} pts</color>"
                : $"Progreso Guardado: Nivel 4 Desbloqueado | Récord actual: {prog.puntajeMaximoHistorico} pts";

            CrearTextoUI(panelMenuPrincipal.transform, "SubtituloGuardado", infoGuardado,
                new Vector2(25f, 295f), new Vector2(760f, 50f), 22, TextAnchor.MiddleCenter, Color.white);

            CrearBotonUI(panelMenuPrincipal.transform, "▶ JUGAR NIVEL 4 (LA MEMORIA VIVA)", new Vector2(25f, 200f), new Vector2(540f, 68f), new Color(0.16f, 0.62f, 0.36f), IniciarEstadoJugar);
            CrearBotonUI(panelMenuPrincipal.transform, "🗺 SELECCIÓN DE NIVELES (NIVELES 1 AL 4)", new Vector2(25f, 115f), new Vector2(540f, 62f), new Color(0.22f, 0.44f, 0.68f), () => panelSeleccionNiveles.SetActive(true));
            CrearBotonUI(panelMenuPrincipal.transform, "⚙ OPCIONES (VOLUMEN, CONTROLES Y CALIDAD)", new Vector2(25f, 30f), new Vector2(540f, 62f), new Color(0.32f, 0.36f, 0.48f), () => panelOpciones.SetActive(true));
            CrearBotonUI(panelMenuPrincipal.transform, "📖 GUÍA AUTODIDACTA E HISTORIA REAL", new Vector2(25f, -55f), new Vector2(540f, 62f), new Color(0.55f, 0.40f, 0.16f), () => { IniciarEstadoJugar(); panelGuiaAutodidacta.SetActive(true); });
            CrearBotonUI(panelMenuPrincipal.transform, "📜 PANTALLA DE CRÉDITOS Y LICENCIAS", new Vector2(25f, -140f), new Vector2(540f, 62f), new Color(0.38f, 0.28f, 0.52f), () => CambiarEstado(EstadoNivel4.Creditos));
            CrearBotonUI(panelMenuPrincipal.transform, "✖ SALIR DEL JUEGO", new Vector2(25f, -225f), new Vector2(540f, 58f), new Color(0.62f, 0.20f, 0.22f), SalirAplicacion);

            CrearTextoUI(panelMenuPrincipal.transform, "PieAutores",
                "Autoras: Valeria Guadalupe Calvillo Mendoza y María del Carmen Vargas Martínez — UTNG (GIDS06001)",
                new Vector2(0f, -470f), new Vector2(1500f, 50f), 20, TextAnchor.MiddleCenter, new Color(0.75f, 0.78f, 0.85f));
        }

        private void ConstruirPanelHUD(Transform raiz)
        {
            panelHUD = new GameObject("Panel_HUD_Gameplay");
            panelHUD.transform.SetParent(raiz, false);
            RectTransform rtHud = panelHUD.AddComponent<RectTransform>();
            rtHud.anchorMin = Vector2.zero;
            rtHud.anchorMax = Vector2.one;
            rtHud.offsetMin = Vector2.zero;
            rtHud.offsetMax = Vector2.zero;

            // 1. ESQUINA SUPERIOR IZQUIERDA (Línea de vida)
            GameObject cajaIzq = CrearCajaAnclada(panelHUD.transform, "HUD_EsquinaSupIzquierda",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -20f), new Vector2(520f, 158f), new Color(0.08f, 0.09f, 0.14f, 0.90f));

            textoAnimoPorcentaje = CrearTextoUI(cajaIzq.transform, "LineaDeVida_Texto", "❤️ LÍNEA DE VIDA: 100%",
                new Vector2(0f, 52f), new Vector2(480f, 32f), 20, TextAnchor.MiddleLeft, new Color(1f, 0.90f, 0.55f));

            GameObject barraFondo = CrearCajaUI(cajaIzq.transform, "LineaDeVida_BarraFondo", new Vector2(0f, 22f), new Vector2(480f, 22f), new Color(0.20f, 0.20f, 0.25f, 0.95f));
            GameObject barraRelleno = CrearCajaUI(barraFondo.transform, "LineaDeVida_BarraFill", Vector2.zero, new Vector2(480f, 22f), new Color(0.22f, 0.85f, 0.42f, 1f));
            barraAnimoFill = barraRelleno.GetComponent<Image>();
            barraAnimoFill.type = Image.Type.Filled;
            barraAnimoFill.fillMethod = Image.FillMethod.Horizontal;
            barraAnimoFill.fillAmount = 1f;

            textoContadorPanuelos = CrearTextoUI(cajaIzq.transform, "TxtPanuelos", "Pañuelos colocados: 0 / 22",
                new Vector2(-95f, -18f), new Vector2(280f, 34f), 22, TextAnchor.MiddleLeft, new Color(0.48f, 1f, 0.68f));

            textoPuntajeHUD = CrearTextoUI(cajaIzq.transform, "TxtPuntaje", "Puntaje: 0 pts",
                new Vector2(145f, -18f), new Vector2(200f, 34f), 20, TextAnchor.MiddleRight, new Color(1f, 0.88f, 0.45f));

            textoTiempoHUD = CrearTextoUI(cajaIzq.transform, "TxtTiempo", "Tiempo: 00:00   |   💧 Botellas de Agua: 0",
                new Vector2(0f, -52f), new Vector2(480f, 28f), 19, TextAnchor.MiddleLeft, new Color(0.82f, 0.86f, 0.92f));

            // 2. CENTRO SUPERIOR: Minimapa e Indicador de Objetivos
            GameObject cajaCentro = CrearCajaAnclada(panelHUD.transform, "HUD_CentroMinimapaObjetivos",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(760f, 112f), new Color(0.08f, 0.10f, 0.15f, 0.86f));

            textoObjetivoMinimapa = CrearTextoUI(cajaCentro.transform, "TxtObjetivo",
                "OBJETIVO 1/3: Recibir Dictamen Forense del EAAF (◄ Camina a la izquierda)",
                new Vector2(0f, 24f), new Vector2(730f, 44f), 21, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.55f));

            GameObject barraMapa = CrearCajaUI(cajaCentro.transform, "MinimapaBarra", new Vector2(0f, -24f), new Vector2(340f, 24f), new Color(0.18f, 0.22f, 0.30f, 1f));
            CrearTextoUI(barraMapa.transform, "PinLab", "🔬Lab EAAF", new Vector2(-120f, 0f), new Vector2(95f, 22f), 14, TextAnchor.MiddleCenter, new Color(0.6f, 0.9f, 1f));
            CrearTextoUI(barraMapa.transform, "PinTere", "💧Doña Tere", new Vector2(-15f, 0f), new Vector2(95f, 22f), 14, TextAnchor.MiddleCenter, new Color(0.6f, 1f, 0.7f));
            CrearTextoUI(barraMapa.transform, "PinKiosco", "🧵Kiosco", new Vector2(65f, 0f), new Vector2(80f, 22f), 14, TextAnchor.MiddleCenter, new Color(0.45f, 1f, 0.65f));
            CrearTextoUI(barraMapa.transform, "PinAltar", "🕯Altar", new Vector2(130f, 0f), new Vector2(70f, 22f), 14, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));

            GameObject goPinJug = CrearCajaUI(barraMapa.transform, "PinEsperanza", new Vector2(-110f, 0f), new Vector2(14f, 28f), new Color(1f, 0.45f, 0.15f, 1f));
            marcadorJugadorMinimapa = goPinJug.GetComponent<RectTransform>();

            // 3. ESQUINA SUPERIOR DERECHA
            GameObject cajaDer = CrearCajaAnclada(panelHUD.transform, "HUD_EsquinaSupDerecha",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -20f), new Vector2(470f, 112f), new Color(0.08f, 0.10f, 0.15f, 0.86f));

            CrearBotonUI(cajaDer.transform, "📓 Cuaderno [C]", new Vector2(-145f, 16f), new Vector2(150f, 44f), new Color(0.45f, 0.32f, 0.18f), AlternarCuadernoBusqueda, 17);
            CrearBotonUI(cajaDer.transform, "🧵 Mural (22)", new Vector2(8f, 16f), new Vector2(142f, 44f), new Color(0.16f, 0.55f, 0.34f), () =>
            {
                if (!dictamenForenseRecibido) OtorgarDictamenForense();
                AbrirMuralBordado();
            }, 17);
            CrearBotonUI(cajaDer.transform, "⏸ PAUSA [Esc]", new Vector2(155f, 16f), new Vector2(140f, 44f), new Color(0.28f, 0.34f, 0.48f), PausarJuego, 17);

            CrearBotonUI(cajaDer.transform, "💡 Guía Autodidacta", new Vector2(-70f, -32f), new Vector2(280f, 34f), new Color(0.24f, 0.42f, 0.58f), () => panelGuiaAutodidacta.SetActive(true), 16);
            CrearBotonUI(cajaDer.transform, "⚙ Audio", new Vector2(140f, -32f), new Vector2(115f, 34f), new Color(0.32f, 0.32f, 0.40f), () => panelOpciones.SetActive(true), 16);

            // 4. AVISO FLOTANTE DE INTERACCIÓN
            avisoInteraccionFlotante = CrearCajaAnclada(panelHUD.transform, "UI_IndicadorTecla_E",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 115f), new Vector2(680f, 76f), new Color(0.08f, 0.16f, 0.12f, 0.94f));
            textoAvisoInteraccion = CrearTextoUI(avisoInteraccionFlotante.transform, "TxtAvisoE",
                "Estación Cercana\n[E] Interactuar", Vector2.zero, new Vector2(650f, 68f), 22, TextAnchor.MiddleCenter, Color.white);
            avisoInteraccionFlotante.SetActive(false);

            // 5. BARRA INFERIOR DE CONTROLES TÁCTILES / ACCESIBILIDAD
            GameObject barraControles = CrearCajaAnclada(panelHUD.transform, "HUD_ControlesAdaptados",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 18f), new Vector2(1080f, 68f), new Color(0.07f, 0.08f, 0.12f, 0.80f));

            CrearBotonMantenerMovimiento(barraControles.transform, "◄ Caminar Izq [A]", new Vector2(-395f, 0f), new Vector2(210f, 50f), -1f);
            CrearBotonMantenerMovimiento(barraControles.transform, "Caminar Der [D] ►", new Vector2(-170f, 0f), new Vector2(210f, 50f), 1f);
            CrearBotonUI(barraControles.transform, "▲ Doble Salto [Esp/W]", new Vector2(45f, 0f), new Vector2(195f, 50f), new Color(0.28f, 0.45f, 0.62f), () =>
            {
                if (Jugador != null) Jugador.saltoTactilSolicitado = true;
            }, 17);
            CrearBotonUI(barraControles.transform, "★ Interactuar [E]", new Vector2(250f, 0f), new Vector2(190f, 50f), new Color(0.18f, 0.62f, 0.36f), InteractuarEstacionActualDesdeBotonUI, 18);
            CrearBotonUI(barraControles.transform, "⛏ Varilla T [F]", new Vector2(440f, 0f), new Vector2(165f, 50f), new Color(0.62f, 0.42f, 0.18f), EjecutarSondeoVarillaT, 18);
        }

        private void CrearBotonMantenerMovimiento(Transform padre, string texto, Vector2 pos, Vector2 tam, float dir)
        {
            Button btn = CrearBotonUI(padre, texto, pos, tam, new Color(0.22f, 0.28f, 0.38f), null, 18);
            EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((_) => { if (Jugador != null) Jugador.inputHorizontalTactil = dir; });
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((_) => { if (Jugador != null) Jugador.inputHorizontalTactil = 0f; });
            trigger.triggers.Add(up);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((_) => { if (Jugador != null) Jugador.inputHorizontalTactil = 0f; });
            trigger.triggers.Add(exit);
        }

        private void ConstruirPanelDialogo(Transform raiz)
        {
            panelDialogo = CrearPanelCompleto(raiz, "Panel_UI_CajaDialogo", new Color(0f, 0f, 0f, 0.55f));
            GameObject caja = CrearCajaAnclada(panelDialogo.transform, "ui_caja_dialogo",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 95f), new Vector2(1480f, 340f), new Color(0.08f, 0.10f, 0.15f, 0.96f));

            textoHablanteDialogo = CrearTextoUI(caja.transform, "TxtHablante", "Hablante",
                new Vector2(-380f, 125f), new Vector2(650f, 40f), 28, TextAnchor.MiddleLeft, new Color(0.48f, 1f, 0.68f));
            textoCargoDialogo = CrearTextoUI(caja.transform, "TxtCargo", "Cargo",
                new Vector2(-280f, 88f), new Vector2(850f, 30f), 19, TextAnchor.MiddleLeft, new Color(0.75f, 0.85f, 0.95f));

            textoCuerpoDialogo = CrearTextoUI(caja.transform, "TxtCuerpo", "...",
                new Vector2(0f, 15f), new Vector2(1400f, 110f), 25, TextAnchor.UpperLeft, Color.white);

            GameObject cajaEdu = CrearCajaUI(caja.transform, "CajaNotaAutodidacta", new Vector2(-140f, -95f), new Vector2(1100f, 75f), new Color(0.14f, 0.24f, 0.20f, 0.95f));
            textoNotaAutodidactaDialogo = CrearTextoUI(cajaEdu.transform, "TxtNotaEdu", "",
                Vector2.zero, new Vector2(1070f, 68f), 18, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.72f));

            CrearBotonUI(caja.transform, "SIGUIENTE [Espacio] ▶", new Vector2(560f, -95f), new Vector2(270f, 68f), new Color(0.18f, 0.62f, 0.36f), AvanzarLineaDialogo, 21);

            panelDialogo.SetActive(false);
        }

        private void ConstruirPanelPreguntaAutodidacta(Transform raiz)
        {
            panelPreguntaAutodidacta = CrearPanelCompleto(raiz, "Panel_PreguntaAutodidacta", new Color(0.04f, 0.06f, 0.10f, 0.90f));
            GameObject caja = CrearCajaUI(panelPreguntaAutodidacta.transform, "CajaPregunta", Vector2.zero, new Vector2(1320f, 720f), new Color(0.10f, 0.14f, 0.20f, 0.98f));

            PreguntaAutodidacta preg = DatosMemoriaNivel4.ObtenerPreguntaForense();

            CrearTextoUI(caja.transform, "TituloPreg", $"<b>🔬 {preg.tituloFase.ToUpper()}</b>",
                new Vector2(0f, 290f), new Vector2(1220f, 55f), 29, TextAnchor.MiddleCenter, new Color(0.55f, 0.90f, 1f));

            CrearTextoUI(caja.transform, "CuerpoPreg", preg.pregunta,
                new Vector2(0f, 195f), new Vector2(1200f, 95f), 24, TextAnchor.MiddleCenter, Color.white);

            Text txtRetro = CrearTextoUI(caja.transform, "TxtRetroalimentacion", "Selecciona la respuesta correcta para certificar e integrar el Dictamen Forense al Cuaderno de Búsqueda:",
                new Vector2(0f, -195f), new Vector2(1180f, 95f), 21, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.55f));

            GameObject btnContinuar = null;

            for (int i = 0; i < preg.opciones.Length; i++)
            {
                int idxOpcion = i;
                Button b = CrearBotonUI(caja.transform, $"{(char)('A' + i)}) {preg.opciones[i]}",
                    new Vector2(0f, 85f - i * 85f), new Vector2(1180f, 70f), new Color(0.18f, 0.26f, 0.38f), () =>
                    {
                        if (idxOpcion == preg.indiceCorrecto)
                        {
                            preguntaAutodidactaRespondida = true;
                            puntajeInvestigacion += 250;
                            txtRetro.text = $"<color=#68F596><b>{preg.explicacionCorrecta}</b></color>";
                            OtorgarDictamenForense();
                            if (btnContinuar != null) btnContinuar.SetActive(true);
                        }
                        else
                        {
                            txtRetro.text = $"<color=#FF9980>{preg.explicacionIncorrecta} Intenta con la opción A.</color>";
                        }
                    }, 20);
            }

            Button btnRecibir = CrearBotonUI(caja.transform, "✓ GUARDAR DICTAMEN EN CUADERNO Y CONTINUAR A LA PLAZA",
                new Vector2(0f, -295f), new Vector2(740f, 62f), new Color(0.16f, 0.65f, 0.36f), () =>
                {
                    panelPreguntaAutodidacta.SetActive(false);
                }, 21);
            btnContinuar = btnRecibir.gameObject;
            btnContinuar.SetActive(false);

            panelPreguntaAutodidacta.SetActive(false);
        }

        private void ConstruirPanelMuralBordado(Transform raiz)
        {
            panelMuralBordado = CrearPanelCompleto(raiz, "Panel_MuralBordado22", new Color(0.05f, 0.07f, 0.10f, 0.95f));
            GameObject caja = CrearCajaUI(panelMuralBordado.transform, "CajaMural", Vector2.zero, new Vector2(1760f, 950f), new Color(0.10f, 0.13f, 0.18f, 0.98f));

            controladorRompecabezasMural = panelMuralBordado.AddComponent<RompecabezasMuralUI>();

            CrearTextoUI(caja.transform, "TituloMural",
                "<b>🧩 DESAFÍO DE ROMPECABEZAS: EL MURAL DE LOS 22 DESAPARECIDOS DE SAN LUIS DE LA PAZ</b>\n" +
                "<size=19><color=#FFE270>Haz clic en dos retratos del cuadro izquierdo para intercambiar su posición (1 al 22) o arma el retrato 3x3 de la derecha</color></size>",
                new Vector2(-130f, 415f), new Vector2(1420f, 80f), 26, TextAnchor.MiddleLeft, Color.white);

            CrearBotonUI(caja.transform, "✖ Cerrar [Esc]", new Vector2(740f, 415f), new Vector2(200f, 54f), new Color(0.55f, 0.22f, 0.24f), () => panelMuralBordado.SetActive(false), 20);

            // Cuadro Izquierdo: Tablero de Rompecabezas de 22 Piezas
            GameObject panelGrid = CrearCajaUI(caja.transform, "Grid22Ausentes", new Vector2(-345f, -35f), new Vector2(990f, 770f), new Color(0.07f, 0.09f, 0.12f, 0.92f));
            fondosBotonesGrid22.Clear();

            for (int i = 0; i < lista22Migrantes.Count; i++)
            {
                int idxCasilla = i;
                int fila = i / 5;
                int col = i % 5;
                float gx = -380f + col * 190f;
                float gy = 290f - fila * 142f;
                if (fila == 4) gx = -95f + col * 190f;

                GameObject celda = CrearCajaUI(panelGrid.transform, "Celda_" + (i + 1), new Vector2(gx, gy), new Vector2(180f, 134f), new Color(0.24f, 0.28f, 0.38f, 0.95f));
                Image imgMarco = celda.GetComponent<Image>();
                fondosBotonesGrid22.Add(imgMarco);
                controladorRompecabezasMural.marcosFotomural22.Add(imgMarco);

                Button btnCelda = celda.AddComponent<Button>();
                btnCelda.onClick.AddListener(() => SeleccionarMigranteEnMural(idxCasilla));

                // Foto de la pieza de rompecabezas actual en esta casilla
                GameObject goFoto = new GameObject("Foto_" + (i + 1));
                goFoto.transform.SetParent(celda.transform, false);
                RectTransform rtF = goFoto.AddComponent<RectTransform>();
                rtF.anchoredPosition = new Vector2(0f, 14f);
                rtF.sizeDelta = new Vector2(158f, 92f);
                Image imgF = goFoto.AddComponent<Image>();
                imgF.preserveAspect = true;
                controladorRompecabezasMural.imagenesFotomural22.Add(imgF);

                // Insignia del número de Casilla Fija en la esquina superior izquierda
                GameObject badgeCasilla = CrearCajaUI(celda.transform, "BadgeCasilla", new Vector2(-60f, 46f), new Vector2(54f, 24f), new Color(0.06f, 0.08f, 0.12f, 0.88f));
                CrearTextoUI(badgeCasilla.transform, "TxtNumCasilla", $"<b>#{i + 1}</b>", Vector2.zero, new Vector2(52f, 22f), 13, TextAnchor.MiddleCenter, new Color(0.65f, 0.9f, 1f));

                Text lblPieza = CrearTextoUI(celda.transform, "Lbl_" + (i + 1), $"Pieza #{i + 1}",
                    new Vector2(0f, -48f), new Vector2(174f, 28f), 13, TextAnchor.MiddleCenter, Color.white);
                controladorRompecabezasMural.etiquetasFotomural22.Add(lblPieza);
            }

            // Cuadro Derecho: Rompecabezas 3x3 del Retrato en Pañuelo Verde
            GameObject panelDetalle = CrearCajaUI(caja.transform, "DetallePanueloVerde", new Vector2(495f, -35f), new Vector2(650f, 770f), new Color(0.09f, 0.24f, 0.15f, 0.96f));

            CrearTextoUI(panelDetalle.transform, "SubtituloPuzzle3x3",
                "<color=#FFE875><b>🧩 RECONSTRUYE EL RETRATO (3x3) O ENCAJA SU CASILLA:</b></color>",
                new Vector2(0f, 355f), new Vector2(610f, 28f), 16, TextAnchor.MiddleCenter, Color.white);

            // Tablero 3x3 de 9 piezas del retrato (252x252)
            GameObject marco3x3 = CrearCajaUI(panelDetalle.transform, "TableroRetrato3x3", new Vector2(0f, 205f), new Vector2(264f, 264f), new Color(0.16f, 0.55f, 0.32f, 1f));
            for (int fila3 = 0; fila3 < 3; fila3++)
            {
                for (int col3 = 0; col3 < 3; col3++)
                {
                    int idx3 = fila3 * 3 + col3;
                    float px = -84f + col3 * 84f;
                    float py = 84f - fila3 * 84f;

                    GameObject celda3 = CrearCajaUI(marco3x3.transform, "Pieza3x3_" + idx3, new Vector2(px, py), new Vector2(82f, 82f), new Color(0.1f, 0.15f, 0.12f, 1f));
                    controladorRompecabezasMural.bordesPiezas3x3.Add(celda3.GetComponent<Image>());

                    Button btn3 = celda3.AddComponent<Button>();
                    btn3.onClick.AddListener(() => controladorRompecabezasMural.AlHacerClicEnPieza3x3(idx3));

                    GameObject goImg3 = new GameObject("ImgFragmento_" + idx3);
                    goImg3.transform.SetParent(celda3.transform, false);
                    RectTransform rt3 = goImg3.AddComponent<RectTransform>();
                    rt3.anchorMin = Vector2.zero;
                    rt3.anchorMax = Vector2.one;
                    rt3.offsetMin = new Vector2(2f, 2f);
                    rt3.offsetMax = new Vector2(-2f, -2f);
                    Image img3 = goImg3.AddComponent<Image>();
                    controladorRompecabezasMural.piezasRetrato3x3.Add(img3);
                }
            }

            txtNombreDetalleMural = CrearTextoUI(panelDetalle.transform, "TxtNombreMigrante", "Nombre",
                new Vector2(0f, 45f), new Vector2(600f, 38f), 23, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.55f));

            txtExpedienteDetalleMural = CrearTextoUI(panelDetalle.transform, "TxtExpedienteMigrante", "Expediente",
                new Vector2(0f, 4f), new Vector2(600f, 40f), 16, TextAnchor.MiddleCenter, new Color(0.78f, 0.95f, 0.85f));

            txtHistoriaDetalleMural = CrearTextoUI(panelDetalle.transform, "TxtHistoriaMigrante", "Historia",
                new Vector2(0f, -68f), new Vector2(590f, 92f), 18, TextAnchor.MiddleCenter, Color.white);

            txtBordadoDetalleMural = CrearTextoUI(panelDetalle.transform, "TxtBordadoMigrante", "Mensaje bordado",
                new Vector2(0f, -145f), new Vector2(590f, 54f), 18, TextAnchor.MiddleCenter, new Color(0.55f, 1f, 0.72f));

            Text txtEstadoPuzzle = CrearTextoUI(panelDetalle.transform, "TxtEstadoRompecabezas",
                "Haz clic en dos fichas de la izquierda para ordenarlas del #1 al #22, o intercambia las 9 piezas del retrato.",
                new Vector2(0f, -202f), new Vector2(610f, 48f), 16, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.65f));

            controladorRompecabezasMural.txtNombreDetalle = txtNombreDetalleMural;
            controladorRompecabezasMural.txtExpedienteDetalle = txtExpedienteDetalleMural;
            controladorRompecabezasMural.txtHistoriaDetalle = txtHistoriaDetalleMural;
            controladorRompecabezasMural.txtBordadoDetalle = txtBordadoDetalleMural;
            controladorRompecabezasMural.txtEstadoRompecabezas = txtEstadoPuzzle;

            Button btnBordarUno = CrearBotonUI(panelDetalle.transform, "🧩 ENCAJAR Y BORDAR PIEZA ACTUAL [Espacio / Clic]",
                new Vector2(0f, -268f), new Vector2(590f, 58f), new Color(0.18f, 0.68f, 0.36f), BordarPanueloSeleccionado, 19);
            txtEstadoBotonBordar = btnBordarUno.GetComponentInChildren<Text>();

            CrearBotonUI(panelDetalle.transform, "🤝 RESOLVER ROMPECABEZAS EN BRIGADA (Completar 22/22)",
                new Vector2(0f, -338f), new Vector2(590f, 50f), new Color(0.72f, 0.52f, 0.14f), BordarTodosEnColectivo, 18);

            panelMuralBordado.SetActive(false);
        }

        private void ConstruirPanelCuadernoBusqueda(Transform raiz)
        {
            panelCuadernoBusqueda = CrearPanelCompleto(raiz, "Panel_UI_CuadernoFondo", new Color(0.04f, 0.05f, 0.08f, 0.88f));
            GameObject caja = CrearCajaUI(panelCuadernoBusqueda.transform, "ui_cuaderno_fondo", Vector2.zero, new Vector2(1420f, 820f), new Color(0.94f, 0.90f, 0.80f, 0.98f));

            txtContenidoCuaderno = CrearTextoUI(caja.transform, "TxtCuaderno", "",
                new Vector2(0f, 35f), new Vector2(1320f, 640f), 22, TextAnchor.UpperLeft, new Color(0.16f, 0.12f, 0.10f));

            CrearBotonUI(caja.transform, "✖ Cerrar Cuaderno de Búsqueda [C]", new Vector2(0f, -340f), new Vector2(460f, 60f), new Color(0.45f, 0.25f, 0.16f), () => panelCuadernoBusqueda.SetActive(false), 21);

            panelCuadernoBusqueda.SetActive(false);
        }

        private void ConstruirPanelActoFinalDocumental(Transform raiz)
        {
            panelActoFinalDocumental = CrearPanelCompleto(raiz, "Panel_ActoFinalDocumental", new Color(0.05f, 0.04f, 0.08f, 0.94f));
            GameObject caja = CrearCajaUI(panelActoFinalDocumental.transform, "CajaActoFinal", Vector2.zero, new Vector2(1540f, 860f), new Color(0.11f, 0.09f, 0.16f, 0.98f));

            Sprite spPoster = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/poster_22_desaparecidos", 100f);
            if (spPoster != null)
            {
                GameObject goP = new GameObject("PosterActoFinal");
                goP.transform.SetParent(caja.transform, false);
                RectTransform rtP = goP.AddComponent<RectTransform>();
                rtP.anchoredPosition = new Vector2(-430f, 20f);
                rtP.sizeDelta = new Vector2(540f, 680f);
                Image imgP = goP.AddComponent<Image>();
                imgP.sprite = spPoster;
                imgP.preserveAspect = true;
            }

            CrearTextoUI(caja.transform, "TituloActoFinal",
                "<b>🕯 ACTO FINAL: LA MEMORIA VIVA Y EL RECLAMO DE JUSTICIA</b>\n" +
                "<size=22><color=#FFD86B>22 Veladoras Encendidas — Plaza Principal de San Luis de la Paz, Guanajuato</color></size>",
                new Vector2(250f, 320f), new Vector2(880f, 85f), 28, TextAnchor.MiddleCenter, Color.white);

            CrearTextoUI(caja.transform, "CuerpoDocumental",
                DatosMemoriaNivel4.ObtenerTextoActoFinalDocumental(),
                new Vector2(250f, 20f), new Vector2(860f, 480f), 23, TextAnchor.MiddleLeft, new Color(0.96f, 0.94f, 0.88f));

            CrearBotonUI(caja.transform, "🎉 ¡CONCLUIR NIVEL 4 Y CELEBRAR VICTORIA CON CONFETI!",
                new Vector2(250f, -315f), new Vector2(720f, 68f), new Color(0.16f, 0.65f, 0.36f), ConcluirActoFinalYMostrarVictoria, 22);

            panelActoFinalDocumental.SetActive(false);
        }

        private void ConstruirPanelGuiaAutodidacta(Transform raiz)
        {
            panelGuiaAutodidacta = CrearPanelCompleto(raiz, "Panel_GuiaAutodidacta", new Color(0.04f, 0.06f, 0.10f, 0.90f));
            GameObject caja = CrearCajaUI(panelGuiaAutodidacta.transform, "CajaGuia", Vector2.zero, new Vector2(1420f, 800f), new Color(0.10f, 0.14f, 0.20f, 0.98f));

            CrearTextoUI(caja.transform, "TxtGuiaCompleta",
                "<b>📖 GUÍA DEL JUEGO: OBSTÁCULOS EN EL SUELO, BOTELLAS DE AGUA Y RETO DE VELADORAS</b>\n\n" +
                "<b>• PELIGROS Y OBSTÁCULOS EN TODO EL MUNDO:</b>\n" +
                "  - A lo largo del suelo hay <b>Rocas Espinosas Rodantes</b>, <b>Remolinos</b> y <b>Trampas que salen de repente del suelo</b>.\n" +
                "  - Esquívalas con el <b>Doble Salto [Espacio / W / Flecha Arriba]</b> o atúrdelas con la <b>Varilla 'T' [F]</b>.\n\n" +
                "<b>• LÍNEA DE VIDA (💧 BOTELLAS DE AGUA):</b>\n" +
                "  - Tu <b>Línea de vida</b> se recupera recogiendo las <b>Botellas de Agua (💧 +25% Línea de vida)</b> repartidas por el mundo.\n\n" +
                "<b>• LOS 3 PASOS PARA GANAR EL NIVEL 4:</b>\n" +
                "  1. Habla con la <b>Dra. Elena Vázquez (EAAF)</b> a la izquierda para recibir el Dictamen Forense Oficial.\n" +
                "  2. Esquiva las trampas hacia el <b>Kiosco Central</b> y coloca los <b>22 pañuelos bordados con hilo verde</b>.\n" +
                "  3. Llega al <b>Altar de Veladoras</b> a la derecha y supera el <b>Reto de Encender las 22 Veladoras contra el Viento</b> para ganar con lluvia de confeti.",
                new Vector2(0f, 45f), new Vector2(1300f, 600f), 21, TextAnchor.UpperLeft, Color.white);

            CrearBotonUI(caja.transform, "▶ ¡ENTENDIDO, INICIAR AVENTURA EN LA PLAZA!",
                new Vector2(0f, -315f), new Vector2(640f, 64f), new Color(0.16f, 0.64f, 0.36f), () => panelGuiaAutodidacta.SetActive(false), 22);

            panelGuiaAutodidacta.SetActive(false);
        }

        // ==================== ESTADO PAUSADO (PAUSE MENU ESTANDARIZADO) ====================
        private void ConstruirPanelPausa(Transform raiz)
        {
            panelPausa = CrearPanelCompleto(raiz, "Panel_EstadoPausado", new Color(0.03f, 0.04f, 0.07f, 0.85f));
            GameObject caja = CrearCajaUI(panelPausa.transform, "CajaMenuPausa", Vector2.zero, new Vector2(760f, 700f), new Color(0.10f, 0.12f, 0.18f, 0.98f));

            CrearTextoUI(caja.transform, "TituloPausa",
                "<b><color=#FFE059>⏸ MENÚ DE PAUSA</color></b>\n<size=20><color=#C8DCF0>Nivel 4: La Memoria Viva y el Reclamo de Justicia</color></size>",
                new Vector2(0f, 260f), new Vector2(680f, 80f), 34, TextAnchor.MiddleCenter, Color.white);

            CrearBotonUI(caja.transform, "▶  REANUDAR JUEGO", new Vector2(0f, 145f), new Vector2(560f, 64f), new Color(0.18f, 0.64f, 0.36f), ReanudarJuego, 22);
            CrearBotonUI(caja.transform, "🔄  REINICIAR NIVEL", new Vector2(0f, 60f), new Vector2(560f, 60f), new Color(0.22f, 0.48f, 0.72f), ReiniciarNivelActual, 21);
            CrearBotonUI(caja.transform, "🔊  OPCIONES DE AUDIO", new Vector2(0f, -25f), new Vector2(560f, 60f), new Color(0.55f, 0.44f, 0.18f), () => panelOpciones.SetActive(true), 21);
            CrearBotonUI(caja.transform, "🏠  MENÚ PRINCIPAL", new Vector2(0f, -110f), new Vector2(560f, 60f), new Color(0.65f, 0.22f, 0.24f), SalirAlMenuPrincipal, 21);

            CrearTextoUI(caja.transform, "LblVerificacionDocente", "Verificación Rápida de Estados:",
                new Vector2(0f, -190f), new Vector2(620f, 30f), 17, TextAnchor.MiddleCenter, new Color(0.8f, 0.85f, 0.9f));

            CrearBotonUI(caja.transform, "⚠ Probar Línea de vida agotada", new Vector2(-145f, -245f), new Vector2(270f, 48f), new Color(0.55f, 0.22f, 0.22f), ActivarGameOver, 16);
            CrearBotonUI(caja.transform, "🎉 Probar ¡GANASTE! + Confeti", new Vector2(145f, -245f), new Vector2(270f, 48f), new Color(0.55f, 0.44f, 0.14f), () =>
            {
                dictamenForenseRecibido = true;
                BordarTodosEnColectivo();
                ConcluirActoFinalYMostrarVictoria();
            }, 16);

            panelPausa.SetActive(false);
        }

        // ==================== ESTADO FIN DE NIVEL (VICTORY SCREEN CON LETRERO ¡GANASTE! Y CONFETI) ====================
        private void ConstruirPanelVictoria(Transform raiz)
        {
            panelVictoria = CrearPanelCompleto(raiz, "Panel_EstadoFinDeNivel_Victoria", new Color(0.03f, 0.04f, 0.07f, 0.94f));
            GameObject caja = CrearCajaUI(panelVictoria.transform, "CajaVictoria", new Vector2(0f, -25f), new Vector2(1320f, 800f), new Color(0.10f, 0.12f, 0.18f, 0.98f));

            GameObject letreroBanner = CrearCajaUI(panelVictoria.transform, "Letrero_GANASTE_Banner",
                new Vector2(0f, 415f), new Vector2(1140f, 130f), new Color(0.95f, 0.72f, 0.12f, 1f));
            GameObject interiorLetrero = CrearCajaUI(letreroBanner.transform, "InteriorLetrero",
                Vector2.zero, new Vector2(1120f, 114f), new Color(0.12f, 0.48f, 0.26f, 1f));

            CrearTextoUI(interiorLetrero.transform, "TxtLetreroGanaste",
                "<b>🏆 ¡¡NIVEL 4 Y JUEGO COMPLETADOS!! 🏆</b>\n" +
                "<size=24><color=#FFE875>HASTA ENCONTRARLES: EL CAMINO DE LA ESPERANZA (100% COMPLETADO)</color></size>",
                Vector2.zero, new Vector2(1090f, 108f), 40, TextAnchor.MiddleCenter, Color.white);

            txtResumenVictoria = CrearTextoUI(caja.transform, "TxtResumenVictoria", "",
                new Vector2(0f, 55f), new Vector2(1160f, 380f), 24, TextAnchor.MiddleCenter, Color.white);

            CrearBotonUI(caja.transform, "📜  VER CRÉDITOS FINALES Y LICENCIAS", new Vector2(-290f, -235f), new Vector2(530f, 68f), new Color(0.18f, 0.68f, 0.36f), () => CambiarEstado(EstadoNivel4.Creditos), 21);
            CrearBotonUI(caja.transform, "🔄  REINICIAR NIVEL 4", new Vector2(290f, -235f), new Vector2(530f, 68f), new Color(0.22f, 0.48f, 0.72f), ReiniciarNivelActual, 21);
            CrearBotonUI(caja.transform, "🏠  MENÚ PRINCIPAL", new Vector2(0f, -325f), new Vector2(460f, 56f), new Color(0.65f, 0.22f, 0.24f), SalirAlMenuPrincipal, 20);

            GameObject capaConfeti = new GameObject("Capa_LluviaConfeti_Victoria");
            capaConfeti.transform.SetParent(panelVictoria.transform, false);
            RectTransform rtConfeti = capaConfeti.AddComponent<RectTransform>();
            rtConfeti.anchorMin = Vector2.zero;
            rtConfeti.anchorMax = Vector2.one;
            rtConfeti.offsetMin = Vector2.zero;
            rtConfeti.offsetMax = Vector2.zero;

            EfectoConfetiUI compConfeti = capaConfeti.AddComponent<EfectoConfetiUI>();
            compConfeti.letreroGanaste = letreroBanner.GetComponent<RectTransform>();

            panelVictoria.SetActive(false);
        }

        // ==================== ESTADO GAME OVER (LÍNEA DE VIDA AGOTADA) ====================
        private void ConstruirPanelGameOver(Transform raiz)
        {
            panelGameOver = CrearPanelCompleto(raiz, "Panel_EstadoGameOver_Derrota", new Color(0.03f, 0.04f, 0.07f, 0.92f));
            GameObject caja = CrearCajaUI(panelGameOver.transform, "CajaGameOver", Vector2.zero, new Vector2(1060f, 640f), new Color(0.10f, 0.12f, 0.18f, 0.98f));

            CrearTextoUI(caja.transform, "TituloGameOver",
                "<b><color=#FF5858>💔 LÍNEA DE VIDA AGOTADA</color></b>\n\n" +
                "<size=23><color=#FFD0C8>Los obstáculos y trampas del suelo redujeron tu Línea de vida a cero.\n" +
                "Recuerda que tu Línea de vida se recupera recogiendo las 💧 Botellas de Agua en el mundo,\n" +
                "y que puedes usar el Doble Salto [Espacio / W] para esquivar las trampas del suelo.</color></size>",
                new Vector2(0f, 110f), new Vector2(960f, 260f), 32, TextAnchor.MiddleCenter, Color.white);

            CrearBotonUI(caja.transform, "🔄  REINTENTAR NIVEL", new Vector2(-220f, -150f), new Vector2(400f, 68f), new Color(0.18f, 0.64f, 0.36f), ReiniciarNivelActual, 22);
            CrearBotonUI(caja.transform, "🏠  MENÚ PRINCIPAL", new Vector2(220f, -150f), new Vector2(400f, 68f), new Color(0.65f, 0.22f, 0.24f), SalirAlMenuPrincipal, 22);

            panelGameOver.SetActive(false);
        }

        // ==================== PANTALLA DE CRÉDITOS Y LICENCIAMIENTO ====================
        private void ConstruirPanelCreditos(Transform raiz)
        {
            panelCreditos = CrearPanelCompleto(raiz, "Panel_CreditosYLicencias", new Color(0.05f, 0.06f, 0.10f, 0.97f));
            GameObject caja = CrearCajaUI(panelCreditos.transform, "CajaCreditos", Vector2.zero, new Vector2(1540f, 900f), new Color(0.10f, 0.12f, 0.18f, 0.98f));

            CrearTextoUI(caja.transform, "TxtCreditos",
                DatosMemoriaNivel4.ObtenerTextoCreditosYLicencias(),
                new Vector2(0f, 45f), new Vector2(1420f, 700f), 20, TextAnchor.UpperLeft, new Color(0.94f, 0.95f, 0.98f));

            CrearBotonUI(caja.transform, "🏠 VOLVER AL MENÚ PRINCIPAL", new Vector2(-240f, -370f), new Vector2(440f, 62f), new Color(0.22f, 0.45f, 0.68f), SalirAlMenuPrincipal, 21);
            CrearBotonUI(caja.transform, "▶ VOLVER A LA PLAZA (NIVEL 4)", new Vector2(240f, -370f), new Vector2(440f, 62f), new Color(0.16f, 0.62f, 0.36f), IniciarEstadoJugar, 21);

            panelCreditos.SetActive(false);
        }

        // ==================== SELECCIÓN DE NIVELES (1 AL 4) ====================
        private void ConstruirPanelSeleccionNiveles(Transform raiz)
        {
            panelSeleccionNiveles = CrearPanelCompleto(raiz, "Panel_SeleccionNiveles", new Color(0.05f, 0.06f, 0.10f, 0.94f));
            GameObject caja = CrearCajaUI(panelSeleccionNiveles.transform, "CajaNiveles", Vector2.zero, new Vector2(1440f, 800f), new Color(0.11f, 0.14f, 0.20f, 0.98f));

            CrearTextoUI(caja.transform, "TituloSelNiveles",
                "<b>🗺 SELECCIÓN DE CAPÍTULOS — HASTA ENCONTRARLES</b>\n" +
                "<size=20><color=#95E5B5>Estructura narrativa de los 4 niveles documentales</color></size>",
                new Vector2(0f, 310f), new Vector2(1300f, 80f), 32, TextAnchor.MiddleCenter, Color.white);

            string[] titulos = {
                "✓ NIVEL 1: La Partida y el Silencio de la Carretera (San Luis de la Paz → Dolores Hidalgo) — COMPLETADO",
                "✓ NIVEL 2: Las Puertas Cerradas de la Burocracia (Cotejo de Expedientes y Oficios) — COMPLETADO",
                "✓ NIVEL 3: El Abrazo a la Tierra (Búsqueda en Campo con Varilla 'T' y Banderines) — COMPLETADO",
                "★ NIVEL 4 (ACTUAL): La Memoria Viva y el Reclamo de Justicia (Laboratorio EAAF + Mural de 22 Pañuelos)"
            };

            string[] escenasNiveles = { "Nivel_1", "Nivel_2", "Nivel_3", "Nivel_4" };
            for (int i = 0; i < 4; i++)
            {
                int idxNivel = i;
                Color colBtn = (i == 3) ? new Color(0.18f, 0.64f, 0.36f) : new Color(0.22f, 0.48f, 0.72f);
                CrearBotonUI(caja.transform, titulos[i], new Vector2(0f, 165f - i * 105f), new Vector2(1240f, 82f), colBtn, () =>
                {
                    if (idxNivel == 3) IniciarEstadoJugar();
                    else
                    {
                        Time.timeScale = 1f;
                        SceneManager.LoadScene(escenasNiveles[idxNivel]);
                    }
                }, 21);
            }

            CrearBotonUI(caja.transform, "✖ Volver al Menú", new Vector2(0f, -305f), new Vector2(380f, 58f), new Color(0.55f, 0.22f, 0.24f), () => panelSeleccionNiveles.SetActive(false), 20);
            panelSeleccionNiveles.SetActive(false);
        }

        // ==================== MENÚ DE OPCIONES ====================
        private void ConstruirPanelOpciones(Transform raiz)
        {
            panelOpciones = CrearPanelCompleto(raiz, "Panel_Opciones", new Color(0.04f, 0.05f, 0.09f, 0.92f));
            GameObject caja = CrearCajaUI(panelOpciones.transform, "CajaOpciones", Vector2.zero, new Vector2(1020f, 680f), new Color(0.12f, 0.15f, 0.22f, 0.98f));

            DatosProgresoJuego datos = GestorGuardado.CargarProgreso();

            CrearTextoUI(caja.transform, "TituloOpciones", "<b>⚙ MENÚ DE OPCIONES GENERALES</b>",
                new Vector2(0f, 265f), new Vector2(900f, 60f), 32, TextAnchor.MiddleCenter, Color.white);

            Text txtBgm = CrearTextoUI(caja.transform, "LblVolBGM", $"Volumen de Música (bgm_memorial_esperanza): {Mathf.RoundToInt(datos.volumenMusica * 100)}%",
                new Vector2(0f, 170f), new Vector2(860f, 40f), 22, TextAnchor.MiddleCenter, new Color(0.8f, 0.95f, 1f));

            CrearBotonUI(caja.transform, "➖ Bajar BGM", new Vector2(-180f, 115f), new Vector2(240f, 50f), new Color(0.25f, 0.35f, 0.48f), () =>
            {
                datos.volumenMusica = Mathf.Clamp01(datos.volumenMusica - 0.1f);
                AplicarYGuardarOpcionesUI(datos, txtBgm, null);
            }, 19);
            CrearBotonUI(caja.transform, "➕ Subir BGM", new Vector2(180f, 115f), new Vector2(240f, 50f), new Color(0.22f, 0.52f, 0.38f), () =>
            {
                datos.volumenMusica = Mathf.Clamp01(datos.volumenMusica + 0.1f);
                AplicarYGuardarOpcionesUI(datos, txtBgm, null);
            }, 19);

            Text txtSfx = CrearTextoUI(caja.transform, "LblVolSFX", $"Volumen de Efectos (SFX y Ambiente Plaza): {Mathf.RoundToInt(datos.volumenSFX * 100)}%",
                new Vector2(0f, 35f), new Vector2(860f, 40f), 22, TextAnchor.MiddleCenter, new Color(0.8f, 1f, 0.88f));

            CrearBotonUI(caja.transform, "➖ Bajar SFX", new Vector2(-180f, -20f), new Vector2(240f, 50f), new Color(0.25f, 0.35f, 0.48f), () =>
            {
                datos.volumenSFX = Mathf.Clamp01(datos.volumenSFX - 0.1f);
                AplicarYGuardarOpcionesUI(datos, null, txtSfx);
                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBlipDialogo();
            }, 19);
            CrearBotonUI(caja.transform, "➕ Subir SFX", new Vector2(180f, -20f), new Vector2(240f, 50f), new Color(0.22f, 0.52f, 0.38f), () =>
            {
                datos.volumenSFX = Mathf.Clamp01(datos.volumenSFX + 0.1f);
                AplicarYGuardarOpcionesUI(datos, null, txtSfx);
                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBlipDialogo();
            }, 19);

            CrearTextoUI(caja.transform, "LblCalidad", "Calidad Gráfica y Controles: Adaptados para PC, WebGL y Móvil (Teclado / Gamepad / Táctil)",
                new Vector2(0f, -115f), new Vector2(900f, 45f), 20, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.65f));

            CrearBotonUI(caja.transform, "✓ GUARDAR Y CERRAR OPCIONES", new Vector2(0f, -245f), new Vector2(460f, 62f), new Color(0.16f, 0.62f, 0.36f), () => panelOpciones.SetActive(false), 21);

            panelOpciones.SetActive(false);
        }

        private void AplicarYGuardarOpcionesUI(DatosProgresoJuego datos, Text txtBgm, Text txtSfx)
        {
            if (txtBgm != null) txtBgm.text = $"Volumen de Música (bgm_memorial_esperanza): {Mathf.RoundToInt(datos.volumenMusica * 100)}%";
            if (txtSfx != null) txtSfx.text = $"Volumen de Efectos (SFX y Ambiente Plaza): {Mathf.RoundToInt(datos.volumenSFX * 100)}%";
            if (GestorAudioNivel4.Instancia != null)
            {
                GestorAudioNivel4.Instancia.ActualizarVolumenes(datos.volumenMusica, datos.volumenSFX);
            }
            GestorGuardado.GuardarOpciones(datos.volumenMusica, datos.volumenSFX, datos.calidadGrafica, datos.modoAutodidacta);
        }

        // ==================== UTILIDADES DE CONSTRUCCIÓN DE UI ====================
        private GameObject CrearPanelCompleto(Transform padre, string nombre, Color colorFondo)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.AddComponent<Image>();
            img.color = colorFondo;
            return go;
        }

        private GameObject CrearCajaUI(Transform padre, string nombre, Vector2 pos, Vector2 tam, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private GameObject CrearCajaAnclada(Transform padre, string nombre, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 tam, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private Text CrearTextoUI(Transform padre, string nombre, string contenido, Vector2 pos, Vector2 tam, int tamFuente, TextAnchor alineacion, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            Text txt = go.AddComponent<Text>();
            txt.font = fuenteUI;
            txt.text = contenido;
            txt.fontSize = tamFuente;
            txt.alignment = alineacion;
            txt.color = color;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private Button CrearBotonUI(Transform padre, string texto, Vector2 pos, Vector2 tam, Color colorBase, UnityEngine.Events.UnityAction accion, int tamFuente = 22)
        {
            GameObject go = CrearCajaUI(padre, "Btn_" + texto.Substring(0, Mathf.Min(12, texto.Length)), pos, tam, colorBase);
            Button btn = go.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            cb.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            btn.colors = cb;

            if (accion != null)
            {
                btn.onClick.AddListener(() =>
                {
                    if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBlipDialogo();
                    accion.Invoke();
                });
            }

            CrearTextoUI(go.transform, "TextoBtn", "<b>" + texto + "</b>", Vector2.zero, tam - new Vector2(16f, 8f), tamFuente, TextAnchor.MiddleCenter, Color.white);
            return btn;
        }
    }
}


