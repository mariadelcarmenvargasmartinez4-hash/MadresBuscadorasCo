using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HastaEncontrarles.Nivel4
{
    /// <summary>
    /// Genera una lluvia festiva de confeti multicolor en el Canvas y anima el letrero de "¡GANASTE!"
    /// usando Time.unscaledDeltaTime para que luzca fluido durante la pantalla de Fin de Nivel (Time.timeScale = 0).
    /// </summary>
    public class EfectoConfetiUI : MonoBehaviour
    {
        private class PiezaConfeti
        {
            public RectTransform rt;
            public float velY;
            public float velX;
            public float velRotacion;
            public float faseOscilacion;
        }

        public RectTransform letreroGanaste;
        private List<PiezaConfeti> piezas = new List<PiezaConfeti>();
        private bool inicializado = false;

        private static readonly Color[] ColoresConfeti = new Color[]
        {
            new Color(0.20f, 0.95f, 0.48f, 1f), // Verde esperanza
            new Color(1.00f, 0.84f, 0.15f, 1f), // Dorado cempasúchil
            new Color(1.00f, 0.45f, 0.15f, 1f), // Naranja vivo
            new Color(0.25f, 0.82f, 1.00f, 1f), // Azul cielo
            new Color(0.98f, 0.32f, 0.68f, 1f), // Rosa festivo
            new Color(0.75f, 0.42f, 1.00f, 1f), // Violeta
            new Color(1.00f, 1.00f, 1.00f, 0.95f)
        };

        private void OnEnable()
        {
            if (!inicializado)
            {
                CrearPiezasConfeti(95);
                inicializado = true;
            }
            ReiniciarPosicionesExplosion();
        }

        private void CrearPiezasConfeti(int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                GameObject go = new GameObject("Confeti_" + i);
                go.transform.SetParent(transform, false);
                RectTransform rt = go.AddComponent<RectTransform>();
                float ancho = Random.Range(12f, 24f);
                float alto = Random.Range(8f, 18f);
                rt.sizeDelta = new Vector2(ancho, alto);

                Image img = go.AddComponent<Image>();
                img.color = ColoresConfeti[i % ColoresConfeti.Length];
                img.raycastTarget = false;

                PiezaConfeti p = new PiezaConfeti
                {
                    rt = rt,
                    velY = Random.Range(180f, 440f),
                    velX = Random.Range(-90f, 90f),
                    velRotacion = Random.Range(-260f, 260f),
                    faseOscilacion = Random.Range(0f, 6.28f)
                };
                piezas.Add(p);
            }
        }

        private void ReiniciarPosicionesExplosion()
        {
            for (int i = 0; i < piezas.Count; i++)
            {
                var p = piezas[i];
                p.rt.anchoredPosition = new Vector2(Random.Range(-940f, 940f), Random.Range(150f, 980f));
                p.rt.localEulerAngles = new Vector3(0f, 0f, Random.Range(0f, 360f));
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0.1f) dt = 0.016f;

            // Animar el gran letrero de "¡GANASTE!"
            if (letreroGanaste != null)
            {
                float pulso = 1.0f + Mathf.Sin(Time.unscaledTime * 5.2f) * 0.06f;
                letreroGanaste.localScale = new Vector3(pulso, pulso, 1f);
            }

            // Hacer caer y girar cada pieza de confeti
            for (int i = 0; i < piezas.Count; i++)
            {
                var p = piezas[i];
                if (p.rt == null) continue;

                Vector2 pos = p.rt.anchoredPosition;
                pos.y -= p.velY * dt;
                pos.x += (p.velX + Mathf.Sin(Time.unscaledTime * 3.5f + p.faseOscilacion) * 75f) * dt;

                if (pos.y < -600f)
                {
                    pos.y = Random.Range(560f, 760f);
                    pos.x = Random.Range(-940f, 940f);
                }

                p.rt.anchoredPosition = pos;
                p.rt.Rotate(0f, 0f, p.velRotacion * dt);
            }
        }
    }
}
