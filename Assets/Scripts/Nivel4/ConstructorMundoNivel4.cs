using System.Collections.Generic;
using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    /// <summary>
    /// Construye el escenario 2D limpio y dinámico del Nivel 4:
    /// Sin rectángulos cafés flotantes estáticos; en su lugar, coloca obstáculos móviles rodantes en el suelo,
    /// trampas que emergen de repente del suelo en todo el mundo y Botellas de Agua (única fuente de vida).
    /// </summary>
    public static class ConstructorMundoNivel4
    {
        public static Sprite CrearSpriteColor(Color color, int ancho = 32, int alto = 32, float ppu = 32f)
        {
            Texture2D tex = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
            Color[] cols = new Color[ancho * alto];
            for (int i = 0; i < cols.Length; i++) cols[i] = color;
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, ancho, alto), new Vector2(0.5f, 0.5f), ppu);
        }

        public static Sprite CrearSpriteCirculoGradiente(Color colorCentro, int diametro = 64)
        {
            Texture2D tex = new Texture2D(diametro, diametro, TextureFormat.RGBA32, false);
            float radio = diametro * 0.5f;
            Vector2 centro = new Vector2(radio, radio);
            for (int y = 0; y < diametro; y++)
            {
                for (int x = 0; x < diametro; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), centro) / radio;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a;
                    tex.SetPixel(x, y, new Color(colorCentro.r, colorCentro.g, colorCentro.b, colorCentro.a * a));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, diametro, diametro), new Vector2(0.5f, 0.5f), 32f);
        }

        public static void ConstruirEscenarioPlazaYLab(
            Transform contenedorMundo,
            out PlayerControllerNivel4 jugadorCreado,
            out List<ZonaInteractiva2D> estacionesCreadas,
            out List<SpriteRenderer> panuelosMundo,
            out List<ParticleSystem> flamasVeladorasMundo,
            out List<DesafioIndiferencia2D> listaDesafiosPatrulla)
        {
            estacionesCreadas = new List<ZonaInteractiva2D>();
            panuelosMundo = new List<SpriteRenderer>();
            flamasVeladorasMundo = new List<ParticleSystem>();
            listaDesafiosPatrulla = new List<DesafioIndiferencia2D>();

            // 1. FONDO PRINCIPAL (bg_mural_memorial)
            Sprite spBg = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/bg_mural_memorial", 32f);
            for (int i = -1; i <= 1; i++)
            {
                GameObject objBg = new GameObject("Fondo_Plaza_" + i);
                objBg.transform.SetParent(contenedorMundo);
                objBg.transform.position = new Vector3(i * 28f, 2.2f, 10f);
                objBg.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
                SpriteRenderer srBg = objBg.AddComponent<SpriteRenderer>();
                srBg.sprite = spBg != null ? spBg : CrearSpriteColor(new Color(0.16f, 0.13f, 0.24f), 256, 128, 16f);
                srBg.sortingOrder = -30;
            }

            // 2. PISO DE CANTERA DE LA PLAZA CENTRAL Y COLISIONES 2D
            GameObject objPiso = new GameObject("Piso_Plaza_SanLuisDeLaPaz");
            objPiso.transform.SetParent(contenedorMundo);
            objPiso.transform.position = new Vector3(4f, -3.55f, 0f);
            SpriteRenderer srPiso = objPiso.AddComponent<SpriteRenderer>();
            srPiso.sprite = CrearSpriteColor(new Color(0.24f, 0.20f, 0.22f), 64, 64, 32f);
            objPiso.transform.localScale = new Vector3(38f, 1.8f, 1f);
            srPiso.sortingOrder = -5;
            BoxCollider2D colPiso = objPiso.AddComponent<BoxCollider2D>();
            colPiso.size = new Vector2(2f, 2f);

            // 3. TRAMPAS QUE SALEN DE REPENTE DEL SUELO EN TODO EL MUNDO
            ConstruirTrampasEmergentesSuelo(contenedorMundo);

            // 4. OBJETOS MÓVILES RODANTES EN EL SUELO EN TODO EL MUNDO
            ConstruirObstaculosMovilesSuelo(contenedorMundo, listaDesafiosPatrulla);

            // 5. BOTELLAS DE AGUA EN TODO EL MUNDO (ÚNICA FORMA DE RECUPERAR VIDA)
            ConstruirBotellasDeAguaMundo(contenedorMundo);

            // 6. EDIFICIO IZQUIERDO: LABORATORIO FORENSE DEL EAAF (X = -11.5)
            ConstruirEdificioLaboratorioEAAF(contenedorMundo, new Vector3(-11.5f, 0.3f, 0f), estacionesCreadas);

            // 7. ESTACIÓN DE DEMOSTRACIÓN DE LA VARILLA "T" (X = -5.8)
            ConstruirEstacionVarillaT(contenedorMundo, new Vector3(-5.8f, -1.0f, 0f), estacionesCreadas);

            // 8. ESTACIÓN DE DOÑA TERE Y SOLIDARIDAD COLECTIVA (X = 0.8)
            ConstruirEstacionDonaTere(contenedorMundo, new Vector3(0.8f, -0.45f, 0f), estacionesCreadas);

            // 9. KIOSCO CENTRAL Y MURAL DE BORDADO DE LOS 22 AUSENTES (X = 9.2)
            ConstruirKioscoYMural22(contenedorMundo, new Vector3(9.2f, 0.5f, 0f), estacionesCreadas, panuelosMundo);

            // 10. ALTAR DE LAS 22 VELADORAS PARA EL RETO DE ENCENDIDO Y ACTO FINAL (X = 17.5)
            ConstruirAltarVeladoras(contenedorMundo, new Vector3(17.5f, -0.8f, 0f), estacionesCreadas, flamasVeladorasMundo);

            // 11. PROTAGONISTA: ESPERANZA (MADRE BUSCADORA) EN X = -14.2
            GameObject objJugador = new GameObject("Esperanza_Player2D");
            objJugador.transform.SetParent(contenedorMundo);
            objJugador.transform.position = new Vector3(-14.2f, -0.35f, 0f);
            jugadorCreado = objJugador.AddComponent<PlayerControllerNivel4>();
            CrearEtiquetaMundo(objJugador.transform, "Esperanza", new Vector3(0f, 1.65f, 0f), new Color(1f, 0.92f, 0.55f), 26);
        }

        private static void ConstruirTrampasEmergentesSuelo(Transform padre)
        {
            Sprite spTrampa = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Objetos/trampa_suelo", 64f);
            float[] posicionesX = new float[] { -8.6f, -6.8f, -3.5f, -1.2f, 2.8f, 5.2f, 12.6f, 14.8f };

            for (int i = 0; i < posicionesX.Length; i++)
            {
                GameObject go = new GameObject("TrampaSuelo_" + i);
                go.transform.SetParent(padre);
                go.transform.position = new Vector3(posicionesX[i], -1.4f, 0f);

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = spTrampa != null ? spTrampa : CrearSpriteColor(new Color(0.9f, 0.25f, 0.25f), 48, 48, 48f);
                sr.sortingOrder = 14;

                TrampaEmergenteSuelo2D trampa = go.AddComponent<TrampaEmergenteSuelo2D>();
                trampa.tiempoOculta = 1.4f + (i % 3) * 0.45f;
                trampa.tiempoAdvertencia = 0.6f;
                trampa.tiempoArriba = 1.35f;
                trampa.danoVida = 18f;
            }
        }

        private static void ConstruirObstaculosMovilesSuelo(Transform padre, List<DesafioIndiferencia2D> lista)
        {
            Sprite spRoca = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Objetos/obstaculo_rodante", 64f);
            Sprite spRemolino = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Objetos/remolino_suelo", 64f);

            // Obstáculos que ruedan y se deslizan rápido en el suelo por todo el mundo
            lista.Add(CrearObstaculoMovil(padre, "RocaRodante_1", -9.5f, -6.2f, -1.32f, 3.2f, true, spRoca));
            lista.Add(CrearObstaculoMovil(padre, "RemolinoSuelo_2", -5.2f, -1.8f, -1.30f, 3.6f, false, spRemolino));
            lista.Add(CrearObstaculoMovil(padre, "RocaRodante_3", -1.4f, 2.4f, -1.32f, 3.5f, true, spRoca));
            lista.Add(CrearObstaculoMovil(padre, "RemolinoSuelo_4", 2.2f, 6.2f, -1.30f, 3.9f, false, spRemolino));
            lista.Add(CrearObstaculoMovil(padre, "RocaRodante_5", 5.8f, 11.2f, -1.32f, 3.4f, true, spRoca));
            lista.Add(CrearObstaculoMovil(padre, "RocaRodante_6", 11.8f, 15.8f, -1.32f, 4.1f, true, spRoca));
        }

        private static DesafioIndiferencia2D CrearObstaculoMovil(
            Transform padre,
            string nombre,
            float xIzq,
            float xDer,
            float yBase,
            float vel,
            bool rodante,
            Sprite sp)
        {
            GameObject obj = new GameObject(nombre);
            obj.transform.SetParent(padre);
            obj.transform.position = new Vector3((xIzq + xDer) * 0.5f, yBase, 0f);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sp != null ? sp : CrearSpriteCirculoGradiente(new Color(0.85f, 0.25f, 0.25f, 1f), 48);
            sr.sortingOrder = 19;

            DesafioIndiferencia2D des = obj.AddComponent<DesafioIndiferencia2D>();
            des.puntoIzquierdo = xIzq;
            des.puntoDerecho = xDer;
            des.alturaBaseY = yBase;
            des.amplitudVertical = rodante ? 0.04f : 0.12f;
            des.velocidadPatrulla = vel;
            des.desgasteAlContacto = 20f;
            des.esRodante = rodante;

            return des;
        }

        private static void ConstruirBotellasDeAguaMundo(Transform padre)
        {
            Sprite spBotella = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Objetos/botella_agua", 64f);

            // Botellas de agua repartidas a lo largo de todo el mundo (en el suelo y a media altura de salto)
            Vector2[] posicionesBotellas = new Vector2[]
            {
                new Vector2(-9.2f, -0.8f),
                new Vector2(-7.0f, 0.2f),
                new Vector2(-4.2f, -0.8f),
                new Vector2(-1.6f, 0.3f),
                new Vector2(0.8f, -0.7f),
                new Vector2(3.4f, 0.2f),
                new Vector2(6.0f, -0.8f),
                new Vector2(9.2f, 0.1f),
                new Vector2(12.2f, -0.8f),
                new Vector2(15.0f, 0.2f)
            };

            for (int i = 0; i < posicionesBotellas.Length; i++)
            {
                GameObject go = new GameObject("BotellaAgua_" + (i + 1));
                go.transform.SetParent(padre);
                go.transform.position = new Vector3(posicionesBotellas[i].x, posicionesBotellas[i].y, 0f);

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = spBotella != null ? spBotella : CrearSpriteCirculoGradiente(new Color(0.2f, 0.85f, 1f, 1f), 48);
                sr.sortingOrder = 20;

                ObjetoColeccionable2D comp = go.AddComponent<ObjetoColeccionable2D>();
                comp.tipo = TipoColeccionable.AguaSolidaridad;
                comp.puntos = 80;
                comp.recuperaVida = 25f;

                CrearEtiquetaMundo(go.transform, "💧 +25% Línea de vida", new Vector3(0f, 0.65f, 0f), new Color(0.45f, 0.92f, 1f), 20);
            }
        }

        private static void ConstruirEdificioLaboratorioEAAF(Transform padre, Vector3 pos, List<ZonaInteractiva2D> estaciones)
        {
            GameObject grupo = new GameObject("Estacion_1_LaboratorioEAAF");
            grupo.transform.SetParent(padre);
            grupo.transform.position = pos;

            CrearBloqueVisual(grupo.transform, "Fachada_Lab", new Vector3(0f, 0.3f, 0f), new Vector3(4.6f, 4.2f, 1f), new Color(0.78f, 0.84f, 0.90f), -10);
            CrearBloqueVisual(grupo.transform, "Techo_Lab", new Vector3(0f, 2.5f, 0f), new Vector3(5.0f, 0.45f, 1f), new Color(0.18f, 0.36f, 0.58f), -9);
            CrearBloqueVisual(grupo.transform, "Puerta_Lab", new Vector3(-0.9f, -0.65f, 0f), new Vector3(1.3f, 2.2f, 1f), new Color(0.22f, 0.32f, 0.44f), -8);

            CrearEtiquetaMundo(grupo.transform, "LABORATORIO FORENSE (EAAF)", new Vector3(0f, 2.95f, 0f), new Color(0.55f, 0.88f, 1f), 30);

            GameObject npcPerito = new GameObject("NPC_PeritoEAAF");
            npcPerito.transform.SetParent(grupo.transform);
            npcPerito.transform.localPosition = new Vector3(0.95f, -0.65f, 0f);
            SpriteRenderer srPerito = npcPerito.AddComponent<SpriteRenderer>();
            Sprite sp0 = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Personajes/perito_eaaf_0", 128f);
            Sprite sp1 = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Personajes/perito_eaaf_1", 128f);
            srPerito.sprite = sp0 != null ? sp0 : CrearSpriteColor(Color.cyan, 64, 128, 32f);
            srPerito.flipX = true;
            srPerito.sortingOrder = 15;

            SpriteRenderer halo = CrearHaloInteractivo(grupo.transform, new Vector3(0.95f, -0.65f, 0f), new Color(0.3f, 0.85f, 1f, 0.4f));

            ZonaInteractiva2D zona = grupo.AddComponent<ZonaInteractiva2D>();
            zona.tipoEstacion = TipoEstacionNivel4.LaboratorioEAAF;
            zona.tituloEstacion = "Dra. Elena Vázquez — Perito Forense del EAAF";
            zona.instruccionTecla = "Presiona [E] para recibir el Dictamen Forense Oficial";
            zona.renderizadorPrincipal = srPerito;
            if (sp0 != null) zona.framesAnimacion.Add(sp0);
            if (sp1 != null) zona.framesAnimacion.Add(sp1);
            zona.haloResplandor = halo;

            CrearEtiquetaMundo(npcPerito.transform, "[E] Perito EAAF", new Vector3(0f, 1.65f, 0f), Color.white, 24);
            estaciones.Add(zona);
        }

        private static void ConstruirEstacionVarillaT(Transform padre, Vector3 pos, List<ZonaInteractiva2D> estaciones)
        {
            GameObject grupo = new GameObject("Estacion_VarillaT_JardinMemoria");
            grupo.transform.SetParent(padre);
            grupo.transform.position = pos;

            CrearBloqueVisual(grupo.transform, "Banderin_Asta", new Vector3(0.5f, 0.1f, 0f), new Vector3(0.08f, 1.4f, 1f), new Color(0.9f, 0.9f, 0.9f), 5);
            CrearBloqueVisual(grupo.transform, "Banderin_TelaRoja", new Vector3(0.8f, 0.55f, 0f), new Vector3(0.55f, 0.35f, 1f), new Color(0.92f, 0.2f, 0.2f), 6);

            SpriteRenderer halo = CrearHaloInteractivo(grupo.transform, Vector3.zero, new Color(1f, 0.78f, 0.25f, 0.35f));

            ZonaInteractiva2D zona = grupo.AddComponent<ZonaInteractiva2D>();
            zona.tipoEstacion = TipoEstacionNivel4.SondeoVarillaDemostracion;
            zona.tituloEstacion = "Sitio de Rastreo con Varilla 'T'";
            zona.instruccionTecla = "Presiona [F] para aturdir obstáculos cercanos con la Varilla T";
            zona.haloResplandor = halo;

            CrearEtiquetaMundo(grupo.transform, "[F] Varilla 'T'", new Vector3(0.5f, 1.15f, 0f), new Color(1f, 0.85f, 0.45f), 22);
            estaciones.Add(zona);
        }

        private static void ConstruirEstacionDonaTere(Transform padre, Vector3 pos, List<ZonaInteractiva2D> estaciones)
        {
            GameObject grupo = new GameObject("Estacion_2_DonaTere_Solidaridad");
            grupo.transform.SetParent(padre);
            grupo.transform.position = pos;

            CrearBloqueVisual(grupo.transform, "Mesa_Solidaridad", new Vector3(-1.1f, -0.8f, 0f), new Vector3(1.8f, 0.9f, 1f), new Color(0.52f, 0.32f, 0.20f), 4);
            CrearBloqueVisual(grupo.transform, "Mantel_Verde", new Vector3(-1.1f, -0.4f, 0f), new Vector3(1.9f, 0.22f, 1f), new Color(0.18f, 0.62f, 0.36f), 5);

            GameObject npcTere = new GameObject("NPC_DonaTere");
            npcTere.transform.SetParent(grupo.transform);
            npcTere.transform.localPosition = new Vector3(0.3f, 0.1f, 0f);
            SpriteRenderer srTere = npcTere.AddComponent<SpriteRenderer>();
            Sprite sp0 = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Personajes/dona_tere_0", 128f);
            Sprite sp1 = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/Personajes/dona_tere_1", 128f);
            srTere.sprite = sp0 != null ? sp0 : CrearSpriteColor(Color.green, 64, 128, 32f);
            srTere.flipX = true;
            srTere.sortingOrder = 15;

            SpriteRenderer halo = CrearHaloInteractivo(grupo.transform, new Vector3(0f, 0f, 0f), new Color(0.25f, 0.95f, 0.55f, 0.4f));

            ZonaInteractiva2D zona = grupo.AddComponent<ZonaInteractiva2D>();
            zona.tipoEstacion = TipoEstacionNivel4.DonaTereSolidaridad;
            zona.tituloEstacion = "Doña Tere — Puesto de Botellas de Agua y Solidaridad";
            zona.instruccionTecla = "Presiona [E] para conversar con Doña Tere y recibir orientación";
            zona.renderizadorPrincipal = srTere;
            if (sp0 != null) zona.framesAnimacion.Add(sp0);
            if (sp1 != null) zona.framesAnimacion.Add(sp1);
            zona.haloResplandor = halo;

            CrearEtiquetaMundo(grupo.transform, "[E] Doña Tere", new Vector3(0.3f, 1.75f, 0f), new Color(0.45f, 1f, 0.65f), 24);
            estaciones.Add(zona);
        }

        private static void ConstruirKioscoYMural22(
            Transform padre,
            Vector3 pos,
            List<ZonaInteractiva2D> estaciones,
            List<SpriteRenderer> panuelosMundo)
        {
            GameObject grupo = new GameObject("Estacion_3_KioscoMuralMemoria");
            grupo.transform.SetParent(padre);
            grupo.transform.position = pos;

            CrearBloqueVisual(grupo.transform, "Base_Kiosco_Cantera", new Vector3(0f, -1.35f, 0f), new Vector3(7.4f, 1.4f, 1f), new Color(0.65f, 0.48f, 0.38f), -8);
            CrearBloqueVisual(grupo.transform, "Muro_Memoria_Fondo", new Vector3(0f, 0.65f, 0f), new Vector3(6.8f, 3.1f, 1f), new Color(0.20f, 0.18f, 0.24f), -7);
            CrearBloqueVisual(grupo.transform, "Marco_Dorado_Mural", new Vector3(0f, 0.65f, 0f), new Vector3(7.0f, 3.3f, 1f), new Color(0.85f, 0.62f, 0.18f), -8);
            CrearBloqueVisual(grupo.transform, "Cupula_Kiosco", new Vector3(0f, 2.6f, 0f), new Vector3(7.6f, 0.65f, 1f), new Color(0.58f, 0.22f, 0.18f), -6);

            CrearEtiquetaMundo(grupo.transform, "KIOSCO Y MURAL DE LOS 22 DESAPARECIDOS", new Vector3(0f, 3.25f, 0f), new Color(0.45f, 1f, 0.65f), 30);

            int idx = 1;
            for (int fila = 0; fila < 4; fila++)
            {
                int cols = (fila < 2) ? 6 : 5;
                float espaciadoX = 1.02f;
                float inicioX = -(cols - 1) * espaciadoX * 0.5f;
                float posY = 1.65f - fila * 0.68f;

                for (int c = 0; c < cols; c++)
                {
                    GameObject objPan = new GameObject("PanueloMural_" + idx.ToString("00"));
                    objPan.transform.SetParent(grupo.transform);
                    objPan.transform.localPosition = new Vector3(inicioX + c * espaciadoX, posY, 0f);
                    objPan.transform.localScale = new Vector3(0.36f, 0.36f, 1f);

                    SpriteRenderer srPan = objPan.AddComponent<SpriteRenderer>();
                    string rutaPan = "Sprites/Migrantes/panuelo_" + idx.ToString("00");
                    Sprite spPan = PlayerControllerNivel4.CargarSpriteDesdeRecurso(rutaPan, 100f);
                    srPan.sprite = spPan != null ? spPan : CrearSpriteColor(new Color(0.15f, 0.75f, 0.38f), 64, 64, 100f);
                    srPan.sortingOrder = -4;
                    srPan.color = new Color(0.3f, 0.45f, 0.35f, 0.28f);

                    panuelosMundo.Add(srPan);
                    idx++;
                }
            }

            SpriteRenderer halo = CrearHaloInteractivo(grupo.transform, new Vector3(0f, -0.5f, 0f), new Color(0.2f, 1f, 0.5f, 0.45f));

            ZonaInteractiva2D zona = grupo.AddComponent<ZonaInteractiva2D>();
            zona.tipoEstacion = TipoEstacionNivel4.MuralMemoriaKiosco;
            zona.tituloEstacion = "Mural de Bordado y Memoria (Kiosco Central)";
            zona.instruccionTecla = "Presiona [E] para bordar y colocar los 22 pañuelos con hilo verde";
            zona.radioInteraccion = 3.6f;
            zona.haloResplandor = halo;

            CrearEtiquetaMundo(grupo.transform, "[E] Bordar 22 Pañuelos", new Vector3(0f, -0.95f, 0f), Color.white, 24);
            estaciones.Add(zona);
        }

        private static void ConstruirAltarVeladoras(
            Transform padre,
            Vector3 pos,
            List<ZonaInteractiva2D> estaciones,
            List<ParticleSystem> flamasVeladoras)
        {
            GameObject grupo = new GameObject("Estacion_4_AltarVeladorasActoFinal");
            grupo.transform.SetParent(padre);
            grupo.transform.position = pos;

            CrearBloqueVisual(grupo.transform, "Escalon_Altar_1", new Vector3(0f, -0.8f, 0f), new Vector3(4.8f, 0.45f, 1f), new Color(0.45f, 0.32f, 0.25f), -4);
            CrearBloqueVisual(grupo.transform, "Escalon_Altar_2", new Vector3(0f, -0.45f, 0f), new Vector3(3.8f, 0.35f, 1f), new Color(0.58f, 0.38f, 0.24f), -3);
            CrearBloqueVisual(grupo.transform, "Tapete_Cempasuchil", new Vector3(0f, -0.22f, 0f), new Vector3(3.6f, 0.14f, 1f), new Color(0.98f, 0.62f, 0.08f), -2);

            Sprite spPoster = PlayerControllerNivel4.CargarSpriteDesdeRecurso("Sprites/poster_22_desaparecidos", 260f);
            if (spPoster != null)
            {
                GameObject objPoster = new GameObject("Estela_Poster_22");
                objPoster.transform.SetParent(grupo.transform);
                objPoster.transform.localPosition = new Vector3(0f, 1.45f, 0f);
                SpriteRenderer srPoster = objPoster.AddComponent<SpriteRenderer>();
                srPoster.sprite = spPoster;
                srPoster.sortingOrder = -2;
            }

            for (int i = 0; i < 22; i++)
            {
                int fila = i / 11;
                int col = i % 11;
                float vx = -2.0f + col * 0.40f;
                float vy = (fila == 0) ? -0.48f : -0.12f;

                GameObject objVela = new GameObject("Veladora_" + (i + 1));
                objVela.transform.SetParent(grupo.transform);
                objVela.transform.localPosition = new Vector3(vx, vy, 0f);

                CrearBloqueVisual(objVela.transform, "Vaso_Cera", Vector3.zero, new Vector3(0.14f, 0.28f, 1f), new Color(0.98f, 0.95f, 0.85f), 8);

                GameObject objFlama = new GameObject("ParticulasFlama");
                objFlama.transform.SetParent(objVela.transform);
                objFlama.transform.localPosition = new Vector3(0f, 0.2f, 0f);

                ParticleSystem ps = objFlama.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.loop = true;
                main.playOnAwake = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.88f, 0.25f, 0.95f), new Color(1f, 0.45f, 0.08f, 0.7f));
                main.maxParticles = 25;

                var emission = ps.emission;
                emission.rateOverTime = 12f;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.05f;

                var psRenderer = objFlama.GetComponent<ParticleSystemRenderer>();
                psRenderer.sortingOrder = 12;

                ps.Stop();
                flamasVeladoras.Add(ps);
            }

            SpriteRenderer halo = CrearHaloInteractivo(grupo.transform, new Vector3(0f, 0.2f, 0f), new Color(1f, 0.78f, 0.2f, 0.45f));

            ZonaInteractiva2D zona = grupo.AddComponent<ZonaInteractiva2D>();
            zona.tipoEstacion = TipoEstacionNivel4.AltarVeladorasActoFinal;
            zona.tituloEstacion = "Altar de las 22 Veladoras — Reto del Fuego de la Memoria";
            zona.instruccionTecla = "Presiona [E] para iniciar el Reto de Encender las Veladoras";
            zona.radioInteraccion = 3.0f;
            zona.haloResplandor = halo;

            CrearEtiquetaMundo(grupo.transform, "[E] Reto de Veladoras", new Vector3(0f, 3.4f, 0f), new Color(1f, 0.85f, 0.35f), 26);
            estaciones.Add(zona);
        }

        private static GameObject CrearBloqueVisual(Transform padre, string nombre, Vector3 localPos, Vector3 escala, Color color, int sortingOrder)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre);
            go.transform.localPosition = localPos;
            go.transform.localScale = escala;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CrearSpriteColor(color, 32, 32, 32f);
            sr.sortingOrder = sortingOrder;
            return go;
        }

        private static SpriteRenderer CrearHaloInteractivo(Transform padre, Vector3 localPos, Color colorHalo)
        {
            GameObject go = new GameObject("HaloResplandor");
            go.transform.SetParent(padre);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(2.2f, 2.2f, 1f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CrearSpriteCirculoGradiente(colorHalo, 64);
            sr.sortingOrder = -1;
            return sr;
        }

        private static void CrearEtiquetaMundo(Transform padre, string texto, Vector3 localPos, Color colorTexto, int fontSize)
        {
            GameObject go = new GameObject("Etiqueta_" + texto.Substring(0, Mathf.Min(10, texto.Length)));
            go.transform.SetParent(padre);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(0.045f, 0.045f, 1f);

            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = texto;
            tm.fontSize = fontSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = colorTexto;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 30;
        }
    }
}

