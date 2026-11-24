using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HastaEncontrarles.Nivel4
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerControllerNivel4 : MonoBehaviour
    {
        public enum EstadoAnimacion
        {
            Idle,
            Caminar,
            Saltar,
            Accion
        }

        [Header("Parámetros de Movimiento 2D")]
        public float velocidadMovimiento = 6.2f;
        public float fuerzaSalto = 10.6f;
        public int saltosMaximos = 2; // Permite Doble Salto para superar obstáculos altos
        public float limiteIzquierdo = -16f;
        public float limiteDerecho = 24f;

        [Header("Detección de Piso (Evita saltos infinitos)")]
        public Vector2 offsetDetectorPiso = new Vector2(0f, -1.42f);
        public Vector2 tamanoDetectorPiso = new Vector2(0.85f, 0.28f);
        public bool estaEnPiso = true;
        private int saltosRestantes = 2;

        [Header("Estado y Animación 2D")]
        public EstadoAnimacion estadoActual = EstadoAnimacion.Idle;
        public bool controlesActivos = true;
        public bool mirandoDerecha = true;

        private Rigidbody2D rb;
        private BoxCollider2D col;
        private SpriteRenderer sr;
        private ParticleSystem particulasPolvo;

        private List<Sprite> framesIdle = new List<Sprite>();
        private List<Sprite> framesWalk = new List<Sprite>();
        private List<Sprite> framesJump = new List<Sprite>();

        private float cronometroAnim = 0f;
        private int indiceFrame = 0;
        private float cronometroPasos = 0f;
        private float cronometroAccion = 0f;

        // Entradas táctiles / UI para móvil y WebGL
        [HideInInspector] public float inputHorizontalTactil = 0f;
        [HideInInspector] public bool saltoTactilSolicitado = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<BoxCollider2D>();
            sr = GetComponent<SpriteRenderer>();

            rb.gravityScale = 2.65f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            col.size = new Vector2(0.95f, 2.65f);
            col.offset = new Vector2(0f, 0f);

            CargarSpritesAnimacion();
            CrearParticulasPolvoPies();
        }

        private void CargarSpritesAnimacion()
        {
            for (int i = 0; i < 4; i++)
            {
                Sprite s = CargarSpriteDesdeRecurso("Sprites/Personajes/esperanza_idle_" + i, 128f);
                if (s != null) framesIdle.Add(s);
            }
            for (int i = 0; i < 6; i++)
            {
                Sprite s = CargarSpriteDesdeRecurso("Sprites/Personajes/esperanza_walk_" + i, 128f);
                if (s != null) framesWalk.Add(s);
            }
            for (int i = 0; i < 2; i++)
            {
                Sprite s = CargarSpriteDesdeRecurso("Sprites/Personajes/esperanza_jump_" + i, 128f);
                if (s != null) framesJump.Add(s);
            }

            if (framesIdle.Count > 0)
            {
                sr.sprite = framesIdle[0];
            }
            sr.sortingOrder = 25;
        }

        public static Sprite CargarSpriteDesdeRecurso(string rutaResources, float ppu = 100f)
        {
            Sprite sp = Resources.Load<Sprite>(rutaResources);
            if (sp != null) return sp;

            Texture2D tex = Resources.Load<Texture2D>(rutaResources);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Bilinear;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
            }
            return null;
        }

        private void CrearParticulasPolvoPies()
        {
            GameObject objPolvo = new GameObject("ParticulasPasosTierra");
            objPolvo.transform.SetParent(transform);
            objPolvo.transform.localPosition = new Vector3(0f, -1.35f, 0f);

            particulasPolvo = objPolvo.AddComponent<ParticleSystem>();
            var main = particulasPolvo.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.45f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 2.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
            main.startColor = new Color(0.95f, 0.82f, 0.45f, 0.75f);
            main.maxParticles = 40;

            var emission = particulasPolvo.emission;
            emission.enabled = false;

            var renderer = objPolvo.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 24;
        }

        private void Update()
        {
            VerificarPiso();

            if (!controlesActivos)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                ActualizarAnimacion(0f);
                return;
            }

            float horizontal = LeerInputHorizontal();
            bool presionoSalto = LeerInputSalto();

            // Desplazamiento horizontal con límites del escenario
            float nuevaVelX = horizontal * velocidadMovimiento;
            if ((transform.position.x <= limiteIzquierdo && nuevaVelX < 0f) ||
                (transform.position.x >= limiteDerecho && nuevaVelX > 0f))
            {
                nuevaVelX = 0f;
            }

            rb.linearVelocity = new Vector2(nuevaVelX, rb.linearVelocity.y);

            // Orientación del personaje (voltearse según dirección)
            if (horizontal > 0.05f && !mirandoDerecha)
            {
                mirandoDerecha = true;
                sr.flipX = false;
            }
            else if (horizontal < -0.05f && mirandoDerecha)
            {
                mirandoDerecha = false;
                sr.flipX = true;
            }

            // Salto y Doble Salto con detección de piso (evita saltos infinitos)
            if (presionoSalto && (estaEnPiso || saltosRestantes > 0))
            {
                float impulso = estaEnPiso ? fuerzaSalto : (fuerzaSalto * 0.88f);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, impulso);
                estaEnPiso = false;
                saltosRestantes--;
                if (particulasPolvo != null) particulasPolvo.Emit(12);
                if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirPaso();
            }

            // Efecto de sonido y polvo al caminar sobre la plaza/terracería
            if (estaEnPiso && Mathf.Abs(nuevaVelX) > 0.2f)
            {
                cronometroPasos += Time.deltaTime;
                if (cronometroPasos >= 0.32f)
                {
                    cronometroPasos = 0f;
                    if (GestorAudioNivel4.Instancia != null) GestorAudioNivel4.Instancia.ReproducirPaso();
                    if (particulasPolvo != null) particulasPolvo.Emit(3);
                }
            }
            else
            {
                cronometroPasos = 0.25f;
            }

            ActualizarAnimacion(nuevaVelX);
        }

        private void VerificarPiso()
        {
            Vector2 centroCheck = (Vector2)transform.position + offsetDetectorPiso;
            Collider2D[] colisiones = Physics2D.OverlapBoxAll(centroCheck, tamanoDetectorPiso, 0f);
            bool detectado = false;
            for (int i = 0; i < colisiones.Length; i++)
            {
                if (colisiones[i] != null && colisiones[i] != col && !colisiones[i].isTrigger)
                {
                    detectado = true;
                    break;
                }
            }
            estaEnPiso = detectado;
            if (estaEnPiso && rb.linearVelocity.y <= 0.1f)
            {
                saltosRestantes = saltosMaximos;
            }
        }

        private void ActualizarAnimacion(float velX)
        {
            if (cronometroAccion > 0f)
            {
                cronometroAccion -= Time.deltaTime;
                estadoActual = EstadoAnimacion.Accion;
            }
            else if (!estaEnPiso)
            {
                estadoActual = EstadoAnimacion.Saltar;
            }
            else if (Mathf.Abs(velX) > 0.15f)
            {
                estadoActual = EstadoAnimacion.Caminar;
            }
            else
            {
                estadoActual = EstadoAnimacion.Idle;
            }

            cronometroAnim += Time.deltaTime;

            if (estadoActual == EstadoAnimacion.Saltar && framesJump.Count > 0)
            {
                int idx = rb.linearVelocity.y > 0.2f ? 0 : (framesJump.Count - 1);
                sr.sprite = framesJump[idx];
            }
            else if (estadoActual == EstadoAnimacion.Caminar && framesWalk.Count > 0)
            {
                if (cronometroAnim >= 0.10f)
                {
                    cronometroAnim = 0f;
                    indiceFrame = (indiceFrame + 1) % framesWalk.Count;
                }
                sr.sprite = framesWalk[indiceFrame % framesWalk.Count];
            }
            else if (framesIdle.Count > 0)
            {
                if (cronometroAnim >= 0.26f)
                {
                    cronometroAnim = 0f;
                    indiceFrame = (indiceFrame + 1) % framesIdle.Count;
                }
                sr.sprite = framesIdle[indiceFrame % framesIdle.Count];
            }
        }

        public void EjecutarAnimacionAccion(float duracion = 0.5f)
        {
            cronometroAccion = duracion;
            if (particulasPolvo != null) particulasPolvo.Emit(14);
        }

        private float LeerInputHorizontal()
        {
            float valor = inputHorizontalTactil;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) valor -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) valor += 1f;
            }
            if (Gamepad.current != null)
            {
                float stick = Gamepad.current.leftStick.ReadValue().x;
                if (Mathf.Abs(stick) > 0.15f) valor += stick;
            }
#endif
            try
            {
                float ejeLegacy = Input.GetAxisRaw("Horizontal");
                if (Mathf.Abs(ejeLegacy) > 0.05f && Mathf.Abs(valor) < 0.05f)
                    valor = ejeLegacy;
            }
            catch { }

            return Mathf.Clamp(valor, -1f, 1f);
        }

        private bool LeerInputSalto()
        {
            if (saltoTactilSolicitado)
            {
                saltoTactilSolicitado = false;
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.wasPressedThisFrame ||
                    Keyboard.current.upArrowKey.wasPressedThisFrame ||
                    Keyboard.current.spaceKey.wasPressedThisFrame)
                    return true;
            }
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                return true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
                    return true;
            }
            catch { }

            return false;
        }
    }
}

