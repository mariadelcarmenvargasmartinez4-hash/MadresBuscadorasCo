using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    /// <summary>
    /// Objeto móvil en el suelo (Rocas espinosas rodantes y remolinos de tierra) que recorren el camino
    /// y quitan vida al contacto. El jugador debe esquivarlos saltando con [Espacio / W].
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DesafioIndiferencia2D : MonoBehaviour
    {
        public float puntoIzquierdo = -5.5f;
        public float puntoDerecho = 3.5f;
        public float alturaBaseY = -1.35f;
        public float amplitudVertical = 0.0f;
        public float velocidadPatrulla = 3.2f;
        public float desgasteAlContacto = 20f;
        public bool esRodante = true;
        public bool activo = true;

        private int direccion = 1;
        private float enfriamientoContacto = 0f;
        private float tiempoAturdido = 0f;
        private SpriteRenderer sr;
        private Color colorOriginal = Color.white;

        private void Awake()
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.88f, 0.88f);
            sr = GetComponent<SpriteRenderer>();
            if (sr != null) colorOriginal = sr.color;
        }

        private void Update()
        {
            if (!activo || Time.timeScale <= 0.01f) return;

            if (enfriamientoContacto > 0f)
                enfriamientoContacto -= Time.deltaTime;

            if (tiempoAturdido > 0f)
            {
                tiempoAturdido -= Time.deltaTime;
                if (sr != null) sr.color = new Color(0.4f, 0.9f, 0.6f, 0.35f);
                return;
            }
            else if (sr != null)
            {
                sr.color = colorOriginal;
            }

            // Movimiento rápido por el suelo
            Vector3 pos = transform.position;
            pos.x += direccion * velocidadPatrulla * Time.deltaTime;
            pos.y = alturaBaseY + Mathf.Sin(Time.time * 4.0f + puntoIzquierdo) * amplitudVertical;

            if (pos.x >= puntoDerecho)
            {
                pos.x = puntoDerecho;
                direccion = -1;
                if (sr != null && !esRodante) sr.flipX = true;
            }
            else if (pos.x <= puntoIzquierdo)
            {
                pos.x = puntoIzquierdo;
                direccion = 1;
                if (sr != null && !esRodante) sr.flipX = false;
            }

            transform.position = pos;

            // Si es una roca/rueda espinosa en el suelo, gira al rodar
            if (esRodante)
            {
                transform.Rotate(0f, 0f, -direccion * velocidadPatrulla * 115f * Time.deltaTime);
            }
        }

        public void NeutralizarTemporalmente(float segundos = 4f)
        {
            tiempoAturdido = segundos;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!activo || enfriamientoContacto > 0f || tiempoAturdido > 0f) return;

            PlayerControllerNivel4 jugador = other.GetComponent<PlayerControllerNivel4>();
            if (jugador != null && jugador.controlesActivos)
            {
                enfriamientoContacto = 1.15f;

                // Pequeño empujón físico al ser golpeada por el obstáculo móvil
                Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(direccion * 3.8f, 5.5f);
                }

                if (GestorNivel4.Instancia != null)
                {
                    GestorNivel4.Instancia.AplicarDesgasteAnimo(desgasteAlContacto, "¡Obstáculo en el suelo! (-20% Línea de vida). Recoge Botellas de Agua para recuperar tu Línea de vida.");
                }
            }
        }
    }
}

