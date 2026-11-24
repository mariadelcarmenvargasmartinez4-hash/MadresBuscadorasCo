using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    public enum TipoColeccionable
    {
        AguaSolidaridad,
        EvidenciaFolio
    }

    /// <summary>
    /// Botella de Agua en el mundo: ÚNICA forma de recuperar vida (+25% Vida).
    /// Reaparece automáticamente después de unos segundos para que el jugador pueda sobrevivir a los obstáculos.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ObjetoColeccionable2D : MonoBehaviour
    {
        public TipoColeccionable tipo = TipoColeccionable.AguaSolidaridad;
        public int puntos = 80;
        public float recuperaVida = 25f;

        private Vector3 posInicial;
        private float desfaseTiempo;
        private bool enEsperaReaparicion = false;
        private float cronometroReaparicion = 0f;
        private SpriteRenderer sr;

        private void Awake()
        {
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.65f;
            sr = GetComponent<SpriteRenderer>();
            posInicial = transform.position;
            desfaseTiempo = Random.Range(0f, 6.28f);
        }

        private void Update()
        {
            if (enEsperaReaparicion)
            {
                cronometroReaparicion -= Time.deltaTime;
                if (cronometroReaparicion <= 0f)
                {
                    enEsperaReaparicion = false;
                    if (sr != null) sr.enabled = true;
                }
                return;
            }

            float nuevoY = posInicial.y + Mathf.Sin(Time.time * 3.4f + desfaseTiempo) * 0.14f;
            transform.position = new Vector3(posInicial.x, nuevoY, posInicial.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (enEsperaReaparicion) return;
            PlayerControllerNivel4 jugador = other.GetComponent<PlayerControllerNivel4>();
            if (jugador != null && jugador.controlesActivos)
            {
                CrearEfectoParticulasAgua();
                if (GestorNivel4.Instancia != null)
                {
                    // La vida SOLO se recupera con botellas de agua
                    float curacion = (tipo == TipoColeccionable.AguaSolidaridad) ? recuperaVida : 0f;
                    GestorNivel4.Instancia.RegistrarColeccionableRecogido(tipo, puntos, curacion);
                }

                enEsperaReaparicion = true;
                cronometroReaparicion = 10f; // Reaparece tras 10s
                if (sr != null) sr.enabled = false;
            }
        }

        private void CrearEfectoParticulasAgua()
        {
            GameObject go = new GameObject("ParticulasBotellaAgua");
            go.transform.position = transform.position;
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.2f, 0.85f, 1f, 1f), new Color(0.3f, 1f, 0.65f, 1f));
            var emission = ps.emission;
            emission.enabled = false;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 35;
            ps.Emit(18);
            Destroy(go, 0.7f);
        }
    }
}

