using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HastaEncontrarles.Nivel4
{
    /// <summary>
    /// Convierte el Mural de los 22 Desaparecidos en un desafío doble de Rompecabezas (Puzzle):
    /// 1) Cuadro Izquierdo (Mosaico de 22 Piezas): Las 22 fichas inician desordenadas; haz clic en dos fichas
    ///    para intercambiarlas y colocar cada retrato en su casilla correcta (1 al 22).
    /// 2) Cuadro Derecho (Rompecabezas Fotográfico 3x3): Reconstruye las 9 piezas del retrato del migrante
    ///    seleccionado intercambiando fragmentos hasta armar su fotografía completa.
    /// </summary>
    public class RompecabezasMuralUI : MonoBehaviour
    {
        [Header("Referencias del Tablero de 22 Piezas (Izquierda)")]
        public List<Image> imagenesFotomural22 = new List<Image>();
        public List<Text> etiquetasFotomural22 = new List<Text>();
        public List<Image> marcosFotomural22 = new List<Image>();

        [Header("Referencias del Rompecabezas 3x3 del Retrato (Derecha)")]
        public List<Image> piezasRetrato3x3 = new List<Image>();
        public List<Image> bordesPiezas3x3 = new List<Image>();
        public Text txtEstadoRompecabezas;
        public Text txtNombreDetalle;
        public Text txtExpedienteDetalle;
        public Text txtHistoriaDetalle;
        public Text txtBordadoDetalle;

        private List<DatosMigrante> listaMigrantes;
        // ordenActual22[casilla] = índice del migrante (0..21) que está actualmente en esa casilla
        private int[] ordenActual22 = new int[22];
        private int casillaSeleccionada22 = -1;
        private int migranteInspeccionado = 0;

        // Estado del rompecabezas 3x3 de la foto derecha (9 piezas: 0..8)
        private int[] ordenPiezas3x3 = new int[9];
        private Sprite[] spritesCortados3x3 = new Sprite[9];
        private int piezaSeleccionada3x3 = -1;
        private bool retrato3x3Resuelto = false;
        private bool inicializado = false;

        public void InicializarRompecabezas(List<DatosMigrante> migrantes)
        {
            listaMigrantes = migrantes;
            for (int i = 0; i < 22; i++)
            {
                ordenActual22[i] = i;
                listaMigrantes[i].panueloColocado = false;
            }

            // Mezclar las 22 piezas del cuadro izquierdo para que sea un verdadero reto de rompecabezas
            System.Random rnd = new System.Random(20110321);
            for (int i = 21; i > 0; i--)
            {
                int j = rnd.Next(0, i + 1);
                int temp = ordenActual22[i];
                ordenActual22[i] = ordenActual22[j];
                ordenActual22[j] = temp;
            }

            // Garantizar que ninguna empiece ya resuelta por azar
            for (int i = 0; i < 22; i++)
            {
                if (ordenActual22[i] == i)
                {
                    int otro = (i + 1) % 22;
                    int tmp = ordenActual22[i];
                    ordenActual22[i] = ordenActual22[otro];
                    ordenActual22[otro] = tmp;
                }
            }

            casillaSeleccionada22 = -1;
            inicializado = true;
            ActualizarTablero22();
            CargarRetrato3x3DeMigrante(ordenActual22[0]);
        }

        public void AlHacerClicEnCasilla22(int indiceCasilla)
        {
            if (!inicializado || listaMigrantes == null) return;

            int idMigranteEnCasilla = ordenActual22[indiceCasilla];
            CargarRetrato3x3DeMigrante(idMigranteEnCasilla);

            // Si esta casilla ya tiene su pieza correcta encajada, informar al jugador
            if (ordenActual22[indiceCasilla] == indiceCasilla)
            {
                if (txtEstadoRompecabezas != null)
                {
                    txtEstadoRompecabezas.text = $"<color=#68F596>✓ La Casilla #{indiceCasilla + 1} ya tiene su retrato correcto encajado.</color>";
                }
                casillaSeleccionada22 = -1;
                ActualizarTablero22();
                return;
            }

            if (casillaSeleccionada22 == -1)
            {
                casillaSeleccionada22 = indiceCasilla;
                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBlipDialogo();
                if (txtEstadoRompecabezas != null)
                {
                    DatosMigrante m = listaMigrantes[idMigranteEnCasilla];
                    txtEstadoRompecabezas.text = $"<color=#FFE866>Pieza seleccionada: <b>#{m.id} {m.nombreCompleto}</b>. Ahora haz clic en la <b>Casilla #{m.id}</b> para colocarla en su lugar.</color>";
                }
            }
            else if (casillaSeleccionada22 == indiceCasilla)
            {
                casillaSeleccionada22 = -1;
            }
            else
            {
                // Intercambiar las dos piezas del rompecabezas de 22 migrantes
                int temp = ordenActual22[casillaSeleccionada22];
                ordenActual22[casillaSeleccionada22] = ordenActual22[indiceCasilla];
                ordenActual22[indiceCasilla] = temp;

                int nuevaInspeccion = ordenActual22[indiceCasilla];
                casillaSeleccionada22 = -1;

                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBordado();

                VerificarProgreso22();
                CargarRetrato3x3DeMigrante(nuevaInspeccion);
            }

            ActualizarTablero22();
        }

        private void VerificarProgreso22()
        {
            int colocadosCorrectamente = 0;
            for (int i = 0; i < 22; i++)
            {
                bool correcto = (ordenActual22[i] == i);
                listaMigrantes[i].panueloColocado = correcto;
                if (correcto) colocadosCorrectamente++;
            }

            if (GestorNivel4.Instancia != null)
            {
                GestorNivel4.Instancia.SincronizarProgresoRompecabezasMural(colocadosCorrectamente);
            }

            if (txtEstadoRompecabezas != null)
            {
                if (colocadosCorrectamente >= 22)
                {
                    txtEstadoRompecabezas.text = "<color=#68F596><b>🎉 ¡ROMPECABEZAS DEL MURAL COMPLETADO (22/22)! Ahora ve al Altar a encender las veladoras.</b></color>";
                }
                else
                {
                    txtEstadoRompecabezas.text = $"<color=#88F5B0>✓ Piezas encajadas en su lugar correcto: <b>{colocadosCorrectamente} / 22</b>. ¡Sigue ordenando el rompecabezas!</color>";
                }
            }
        }

        private void ActualizarTablero22()
        {
            for (int c = 0; c < 22 && c < imagenesFotomural22.Count; c++)
            {
                int idxMigrante = ordenActual22[c];
                DatosMigrante m = listaMigrantes[idxMigrante];

                Sprite sp = PlayerControllerNivel4.CargarSpriteDesdeRecurso(m.nombreRecursoFoto, 100f);
                if (sp != null && imagenesFotomural22[c] != null)
                {
                    imagenesFotomural22[c].sprite = sp;
                }

                bool encajadaCorrecta = (idxMigrante == c);
                string nombreCorto = m.nombreCompleto.Length > 17 ? m.nombreCompleto.Substring(0, 17) : m.nombreCompleto;

                if (etiquetasFotomural22[c] != null)
                {
                    if (encajadaCorrecta)
                        etiquetasFotomural22[c].text = $"<color=#78FFA8>✓ #{m.id} {nombreCorto}</color>";
                    else
                        etiquetasFotomural22[c].text = $"<color=#FFD866>[Ir a #{m.id}]</color> {nombreCorto}";
                }

                if (marcosFotomural22[c] != null)
                {
                    if (c == casillaSeleccionada22)
                        marcosFotomural22[c].color = new Color(1.0f, 0.82f, 0.12f, 1f); // Amarillo brillante seleccionada
                    else if (encajadaCorrecta)
                        marcosFotomural22[c].color = new Color(0.14f, 0.72f, 0.36f, 0.98f); // Verde bordado encajada
                    else
                        marcosFotomural22[c].color = new Color(0.24f, 0.28f, 0.38f, 0.94f); // Gris azulado desordenada
                }
            }
        }

        // ==================== ROMPECABEZAS 3x3 DEL RETRATO SELECCIONADO (CUADRO DERECHO) ====================
        public void CargarRetrato3x3DeMigrante(int idxMigrante)
        {
            migranteInspeccionado = Mathf.Clamp(idxMigrante, 0, listaMigrantes.Count - 1);
            DatosMigrante m = listaMigrantes[migranteInspeccionado];

            if (txtNombreDetalle != null) txtNombreDetalle.text = $"PIEZA #{m.id}: {m.nombreCompleto.ToUpper()}";
            if (txtExpedienteDetalle != null) txtExpedienteDetalle.text = $"Expediente: {m.expediente}  |  {m.oficioEdad}";
            if (txtHistoriaDetalle != null) txtHistoriaDetalle.text = m.historiaMemoria;
            if (txtBordadoDetalle != null) txtBordadoDetalle.text = m.mensajeBordado;

            CortarTexturaEn9Piezas("Sprites/Migrantes/panuelo_" + m.id.ToString("00"));

            piezaSeleccionada3x3 = -1;
            bool yaEstabaColocado = (ordenActual22[migranteInspeccionado] == migranteInspeccionado);

            for (int i = 0; i < 9; i++) ordenPiezas3x3[i] = i;

            if (!yaEstabaColocado)
            {
                // Mezclar las 9 piezas del retrato 3x3
                System.Random rnd = new System.Random(m.id * 997);
                for (int i = 8; i > 0; i--)
                {
                    int j = rnd.Next(0, i + 1);
                    int tmp = ordenPiezas3x3[i];
                    ordenPiezas3x3[i] = ordenPiezas3x3[j];
                    ordenPiezas3x3[j] = tmp;
                }
                retrato3x3Resuelto = false;
            }
            else
            {
                retrato3x3Resuelto = true;
            }

            RefrescarVista3x3();
        }

        private void CortarTexturaEn9Piezas(string rutaRecurso)
        {
            Texture2D tex = Resources.Load<Texture2D>(rutaRecurso);
            if (tex == null)
            {
                Sprite spFallback = Resources.Load<Sprite>(rutaRecurso);
                if (spFallback != null) tex = spFallback.texture;
            }

            if (tex == null) return;

            int w = tex.width / 3;
            int h = tex.height / 3;

            // Fila 0 es arriba, Fila 2 es abajo (en coordenadas de textura Y=0 es abajo)
            for (int fila = 0; fila < 3; fila++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int idx = fila * 3 + col;
                    int rx = col * w;
                    int ry = (2 - fila) * h;
                    Rect rect = new Rect(rx, ry, w, h);
                    spritesCortados3x3[idx] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
                }
            }
        }

        public void AlHacerClicEnPieza3x3(int idxCelda3x3)
        {
            if (retrato3x3Resuelto) return;

            if (piezaSeleccionada3x3 == -1)
            {
                piezaSeleccionada3x3 = idxCelda3x3;
                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBlipDialogo();
            }
            else if (piezaSeleccionada3x3 == idxCelda3x3)
            {
                piezaSeleccionada3x3 = -1;
            }
            else
            {
                int tmp = ordenPiezas3x3[piezaSeleccionada3x3];
                ordenPiezas3x3[piezaSeleccionada3x3] = ordenPiezas3x3[idxCelda3x3];
                ordenPiezas3x3[idxCelda3x3] = tmp;
                piezaSeleccionada3x3 = -1;

                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBordado();

                // Verificar si las 9 piezas del retrato quedaron armadas
                bool armado = true;
                for (int i = 0; i < 9; i++)
                {
                    if (ordenPiezas3x3[i] != i)
                    {
                        armado = false;
                        break;
                    }
                }

                if (armado)
                {
                    retrato3x3Resuelto = true;
                    EncajarMigranteActualEnTablero22();
                }
            }

            RefrescarVista3x3();
        }

        private void RefrescarVista3x3()
        {
            for (int i = 0; i < 9 && i < piezasRetrato3x3.Count; i++)
            {
                int idFragmento = ordenPiezas3x3[i];
                if (spritesCortados3x3[idFragmento] != null && piezasRetrato3x3[i] != null)
                {
                    piezasRetrato3x3[i].sprite = spritesCortados3x3[idFragmento];
                }

                if (bordesPiezas3x3[i] != null)
                {
                    if (i == piezaSeleccionada3x3)
                        bordesPiezas3x3[i].color = new Color(1f, 0.85f, 0.15f, 1f);
                    else if (retrato3x3Resuelto || idFragmento == i)
                        bordesPiezas3x3[i].color = new Color(0.18f, 0.85f, 0.45f, 0.9f);
                    else
                        bordesPiezas3x3[i].color = new Color(0.1f, 0.15f, 0.12f, 0.8f);
                }
            }
        }

        /// <summary>
        /// Coloca automáticamente la ficha del migrante actual en su casilla correcta del tablero de 22
        /// y arma su retrato 3x3.
        /// </summary>
        public void ResolverPiezaActual()
        {
            for (int i = 0; i < 9; i++) ordenPiezas3x3[i] = i;
            retrato3x3Resuelto = true;
            RefrescarVista3x3();
            EncajarMigranteActualEnTablero22();
        }

        private void EncajarMigranteActualEnTablero22()
        {
            // Buscar en qué casilla está actualmente `migranteInspeccionado` y llevarlo a la casilla `migranteInspeccionado`
            int casillaDondeEsta = -1;
            for (int i = 0; i < 22; i++)
            {
                if (ordenActual22[i] == migranteInspeccionado)
                {
                    casillaDondeEsta = i;
                    break;
                }
            }

            if (casillaDondeEsta != -1 && casillaDondeEsta != migranteInspeccionado)
            {
                int tmp = ordenActual22[migranteInspeccionado];
                ordenActual22[migranteInspeccionado] = ordenActual22[casillaDondeEsta];
                ordenActual22[casillaDondeEsta] = tmp;
            }

            casillaSeleccionada22 = -1;
            if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBordado();

            VerificarProgreso22();
            ActualizarTablero22();

            // Seleccionar el siguiente migrante desordenado si aún quedan
            for (int i = 0; i < 22; i++)
            {
                if (ordenActual22[i] != i)
                {
                    CargarRetrato3x3DeMigrante(ordenActual22[i]);
                    return;
                }
            }
        }

        public void ResolverTodoElRompecabezas22()
        {
            for (int i = 0; i < 22; i++)
            {
                ordenActual22[i] = i;
                listaMigrantes[i].panueloColocado = true;
            }
            for (int i = 0; i < 9; i++) ordenPiezas3x3[i] = i;
            retrato3x3Resuelto = true;
            casillaSeleccionada22 = -1;

            if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirBordado();

            VerificarProgreso22();
            ActualizarTablero22();
            RefrescarVista3x3();
        }
    }
}

