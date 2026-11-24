using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    /// <summary>
    /// Obstáculo que aparece de repente desde el suelo en distintas partes del mundo.
    /// Muestra un breve temblor de advertencia en el piso y emerge rápidamente quitando vida si no se esquiva.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class TrampaEmergenteSuelo2D : MonoBehaviour
    {
        public float tiempoOculta = 1.8f;
        public float tiempoAdvertencia = 0.65f;
        public float tiempoArriba = 1.45f;
        public float danoVida = 20f;
        public float alturaOcultaY = -2.35f;
        public float alturaActivaY = -1.28f;

        private enum FaseTrampa { Oculta, Advertencia, Activa }
        private FaseTrampa fase = FaseTrampa.Oculta;
        private float cronometro = 0f;
        private float enfriamientoGolpe = 0f;
        private float posXOriginal;
        private SpriteRenderer sr;
        private BoxCollider2D col;

        private void Awake()
        {
            col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.95f, 1.05f);
            sr = GetComponent<SpriteRenderer>();
            posXOriginal = transform.position.x;
            cronometro = Random.Range(0f, tiempoOculta);
            transform.position = new Vector3(posXOriginal, alturaOcultaY, 0f);
        }

        private void Update()
        {
            if (Time.timeScale <= 0.01f) return;

            if (enfriamientoGolpe > 0f)
                enfriamientoGolpe -= Time.deltaTime;

            cronometro += Time.deltaTime;

            switch (fase)
            {
                case FaseTrampa.Oculta:
                    transform.position = Vector3.Lerp(transform.position, new Vector3(posXOriginal, alturaOcultaY, 0f), Time.deltaTime * 10f);
                    if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.25f);
                    if (cronometro >= tiempoOculta)
                    {
                        cronometro = 0f;
                        fase = FaseTrampa.Advertencia;
                    }
                    break;

                case FaseTrampa.Advertencia:
                    // Temblor en el suelo antes de salir de repente
                    float vibracion = Mathf.Sin(Time.time * 45f) * 0.08f;
                    transform.position = new Vector3(posXOriginal + vibracion, alturaOcultaY + 0.32f, 0f);
                    if (sr != null) sr.color = new Color(1f, 0.75f, 0.2f, 0.85f);
                    if (cronometro >= tiempoAdvertencia)
                    {
                        cronometro = 0f;
                        fase = FaseTrampa.Activa;
                    }
                    break;

                case FaseTrampa.Activa:
                    // Emerge de golpe del suelo
                    transform.position = Vector3.Lerp(transform.position, new Vector3(posXOriginal, alturaActivaY, 0f), Time.deltaTime * 18f);
                    if (sr != null) sr.color = Color.white;
                    if (cronometro >= tiempoArriba)
                    {
                        cronometro = 0f;
                        fase = FaseTrampa.Oculta;
                    }
                    break;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (fase != FaseTrampa.Activa || enfriamientoGolpe > 0f) return;

            PlayerControllerNivel4 jugador = other.GetComponent<PlayerControllerNivel4>();
            if (jugador != null && jugador.controlesActivos)
            {
                enfriamientoGolpe = 1.2f;
                Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    float dirEmpuje = (jugador.transform.position.x >= transform.position.x) ? 3.5f : -3.5f;
                    rb.linearVelocity = new Vector2(dirEmpuje, 6.2f);
                }

                if (GestorNivel4.Instancia != null)
                {
                    GestorNivel4.Instancia.AplicarDesgasteAnimo(danoVida, "¡Cuidado! Trampa emergente en el suelo (-20% Línea de vida). Recoge Botellas de Agua para curarte.");
                }
            }
        }
    }
}

