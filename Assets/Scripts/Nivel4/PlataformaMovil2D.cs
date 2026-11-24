using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlataformaMovil2D : MonoBehaviour
    {
        public Vector2 desplazamiento = new Vector2(2.5f, 0f);
        public float velocidad = 1.6f;

        private Vector3 posOrigen;
        private Vector3 posAnterior;
        private float desfase;

        private void Awake()
        {
            posOrigen = transform.position;
            posAnterior = posOrigen;
            desfase = Random.Range(0f, 3.14f);
        }

        private void Update()
        {
            if (Time.timeScale <= 0.01f) return;
            float factor = Mathf.Sin(Time.time * velocidad + desfase);
            Vector3 nuevaPos = posOrigen + new Vector3(desplazamiento.x * factor, desplazamiento.y * factor, 0f);
            transform.position = nuevaPos;
            posAnterior = nuevaPos;
        }
    }
}
