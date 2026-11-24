using System.Collections.Generic;
using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    public enum TipoEstacionNivel4
    {
        LaboratorioEAAF,
        DonaTereSolidaridad,
        SondeoVarillaDemostracion,
        MuralMemoriaKiosco,
        AltarVeladorasActoFinal
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class ZonaInteractiva2D : MonoBehaviour
    {
        public TipoEstacionNivel4 tipoEstacion;
        public string tituloEstacion = "Interactuar";
        public string instruccionTecla = "[E] Interactuar";
        public float radioInteraccion = 2.4f;

        [Header("Animación de Personaje / Objeto")]
        public SpriteRenderer renderizadorPrincipal;
        public List<Sprite> framesAnimacion = new List<Sprite>();
        public SpriteRenderer haloResplandor;

        private BoxCollider2D boxTrigger;
        private bool jugadorCerca = false;
        private float tAnim = 0f;
        private int idxFrame = 0;
        private Transform transformJugador;

        public bool JugadorEnRango => jugadorCerca;

        private void Awake()
        {
            boxTrigger = GetComponent<BoxCollider2D>();
            boxTrigger.isTrigger = true;
            // Según Matriz de Pruebas (Tester 1 Mateo): radio amplio de 2.4 unidades
            boxTrigger.size = new Vector2(2.6f, 3.2f);
        }

        private void Update()
        {
            // Animación de cuadros del NPC u objeto
            if (renderizadorPrincipal != null && framesAnimacion.Count > 1)
            {
                tAnim += Time.deltaTime;
                if (tAnim >= 0.38f)
                {
                    tAnim = 0f;
                    idxFrame = (idxFrame + 1) % framesAnimacion.Count;
                    renderizadorPrincipal.sprite = framesAnimacion[idxFrame];
                }
            }

            // Resplandor intermitente suave para guiar la mirada del jugador (Matriz UX)
            if (haloResplandor != null)
            {
                float pulso = 0.25f + 0.22f * Mathf.Sin(Time.unscaledTime * 3.2f);
                Color c = haloResplandor.color;
                c.a = jugadorCerca ? Mathf.Min(0.75f, pulso * 1.8f) : pulso;
                haloResplandor.color = c;
            }

            // Verificación de proximidad adicional por distancia
            if (transformJugador == null && GestorNivel4.Instancia != null && GestorNivel4.Instancia.Jugador != null)
            {
                transformJugador = GestorNivel4.Instancia.Jugador.transform;
            }

            if (transformJugador != null)
            {
                float dist = Mathf.Abs(transformJugador.position.x - transform.position.x);
                jugadorCerca = dist <= radioInteraccion;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerControllerNivel4>() != null)
            {
                jugadorCerca = true;
                transformJugador = other.transform;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerControllerNivel4>() != null)
            {
                jugadorCerca = false;
            }
        }
    }
}

