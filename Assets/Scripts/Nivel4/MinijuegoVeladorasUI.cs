using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HastaEncontrarles.Nivel4
{
    /// <summary>
    /// Minijuego interactivo de precisión y ritmo para encender las 22 veladoras del Altar Final:
    /// El jugador debe presionar [Espacio] o el botón cuando la chispa del cerillo pase por la Zona Dorada
    /// antes de que las ráfagas de viento apaguen la flama.
    /// </summary>
    public class MinijuegoVeladorasUI : MonoBehaviour
    {
        public RectTransform barraFondo;
        public RectTransform zonaDorada;
        public RectTransform cursorCerillo;
        public Text txtInstruccionEstado;
        public Text txtProgresoVelas;
        public List<Image> iconosFlamasUI = new List<Image>();

        private int rondaActual = 0;
        private const int TOTAL_RONDAS = 5; // 5 bloques que encienden las 22 veladoras
        private float posCursorNorm = 0.5f;
        private int dirCursor = 1;
        private float velCursor = 0.95f;
        private float centroZonaNorm = 0.5f;
        private float anchoZonaNorm = 0.26f;
        private float enfriamientoIntento = 0f;

        private void OnEnable()
        {
            rondaActual = 0;
            posCursorNorm = 0.1f;
            dirCursor = 1;
            ConfigurarRonda(0);
        }

        private void ConfigurarRonda(int ronda)
        {
            rondaActual = ronda;
            velCursor = 0.95f + ronda * 0.25f;
            anchoZonaNorm = Mathf.Max(0.15f, 0.28f - ronda * 0.025f);
            centroZonaNorm = Random.Range(0.24f, 0.76f);

            if (barraFondo != null && zonaDorada != null)
            {
                float anchoTotal = barraFondo.sizeDelta.x;
                zonaDorada.sizeDelta = new Vector2(anchoTotal * anchoZonaNorm, zonaDorada.sizeDelta.y);
                float posX = (centroZonaNorm - 0.5f) * anchoTotal;
                zonaDorada.anchoredPosition = new Vector2(posX, 0f);
            }

            ActualizarVisualVelas();

            int velasEncendidas = Mathf.RoundToInt((rondaActual / (float)TOTAL_RONDAS) * 22f);
            if (txtProgresoVelas != null)
            {
                txtProgresoVelas.text = $"🕯 Veladoras Encendidas: {velasEncendidas} / 22   (Ronda {Mathf.Min(rondaActual + 1, TOTAL_RONDAS)} de {TOTAL_RONDAS})";
            }
        }

        private void ActualizarVisualVelas()
        {
            for (int i = 0; i < iconosFlamasUI.Count; i++)
            {
                if (iconosFlamasUI[i] != null)
                {
                    iconosFlamasUI[i].color = (i < rondaActual)
                        ? new Color(1f, 0.85f, 0.18f, 1f)
                        : new Color(0.28f, 0.32f, 0.40f, 0.65f);
                }
            }
        }

        private void Update()
        {
            if (enfriamientoIntento > 0f)
                enfriamientoIntento -= Time.unscaledDeltaTime;

            float dt = Time.unscaledDeltaTime;
            if (dt > 0.1f) dt = 0.016f;

            posCursorNorm += dirCursor * velCursor * dt;
            if (posCursorNorm >= 1f)
            {
                posCursorNorm = 1f;
                dirCursor = -1;
            }
            else if (posCursorNorm <= 0f)
            {
                posCursorNorm = 0f;
                dirCursor = 1;
            }

            if (barraFondo != null && cursorCerillo != null)
            {
                float anchoTotal = barraFondo.sizeDelta.x;
                cursorCerillo.anchoredPosition = new Vector2((posCursorNorm - 0.5f) * anchoTotal, 0f);
            }

            // Permitir presionar Espacio para encender el cerillo
            if (enfriamientoIntento <= 0f && LeerTeclaEspacio())
            {
                IntentarEncenderVela();
            }
        }

        private bool LeerTeclaEspacio()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.Space)) return true; } catch { }
            return false;
        }

        public void IntentarEncenderVela()
        {
            if (enfriamientoIntento > 0f) return;
            enfriamientoIntento = 0.22f;

            float mitadAncho = anchoZonaNorm * 0.5f + 0.03f;
            bool acierto = Mathf.Abs(posCursorNorm - centroZonaNorm) <= mitadAncho;

            if (acierto)
            {
                rondaActual++;
                if (GestorAudioNivel4.Instancia != null)
                    GestorAudioNivel4.Instancia.ReproducirVeladora();

                if (GestorNivel4.Instancia != null)
                    GestorNivel4.Instancia.EncenderParcialmenteVeladorasMundo(rondaActual, TOTAL_RONDAS);

                if (rondaActual >= TOTAL_RONDAS)
                {
                    ActualizarVisualVelas();
                    if (txtInstruccionEstado != null)
                        txtInstruccionEstado.text = "<color=#68F596><b>🔥 ¡LAS 22 VELADORAS ESTÁN ENCENDIDAS! ¡LO LOGRASTE! 🔥</b></color>";

                    if (GestorNivel4.Instancia != null)
                    {
                        gameObject.SetActive(false);
                        GestorNivel4.Instancia.CompletarRetoVeladorasExitoso();
                    }
                    return;
                }

                if (txtInstruccionEstado != null)
                    txtInstruccionEstado.text = "<color=#68F596><b>✨ ¡Chispa perfecta! Encendiste un bloque de veladoras (+150 pts). ¡Sigue así!</b></color>";

                ConfigurarRonda(rondaActual);
            }
            else
            {
                if (txtInstruccionEstado != null)
                    txtInstruccionEstado.text = "<color=#FF9080><b>💨 ¡La ráfaga de viento apagó el cerillo! (-10% Línea de vida) Apunta a la Zona Dorada.</b></color>";

                if (GestorNivel4.Instancia != null)
                {
                    GestorNivel4.Instancia.AplicarDesgasteAnimo(10f, "El viento apagó el cerillo (-10% Línea de vida).");
                }
            }
        }
    }
}
