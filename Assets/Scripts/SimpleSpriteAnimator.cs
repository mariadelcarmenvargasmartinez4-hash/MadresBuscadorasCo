using UnityEngine;

// Anima con los sprites cortados: idle + 4 frames de caminar + estados de salto y excavación opcionales.
public class SimpleSpriteAnimator : MonoBehaviour
{
    [Header("Sprites Básicos")]
    public Sprite idle;
    public Sprite[] walk;
    public float fps = 8f;

    [Header("Sprites de Acciones Especiales (Opcional)")]
    public Sprite jumpSprite;
    public Sprite[] digSprites;
    public float digFps = 6f;

    [Header("Referencia al Jugador")]
    public PlayerController2D player;   // opcional: si es NPC déjalo vacío
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void ApplySpriteSafe(Sprite candidate)
    {
        if (sr != null && candidate != null && candidate.texture != null)
        {
            sr.sprite = candidate;
            if (!sr.enabled) sr.enabled = true;
        }
    }

    void Update()
    {
        if (sr == null) return;

        // Estado: Excavando
        if (player != null && player.IsDigging)
        {
            if (digSprites != null && digSprites.Length > 0)
            {
                Sprite digFrame = digSprites[(int)(Time.time * digFps) % digSprites.Length];
                if (digFrame != null && digFrame.texture != null)
                {
                    ApplySpriteSafe(digFrame);
                    return;
                }
            }
        }

        // Estado: En el aire (Salto / Caída)
        if (player != null && !player.IsGrounded)
        {
            if (jumpSprite != null && jumpSprite.texture != null)
            {
                ApplySpriteSafe(jumpSprite);
                return;
            }
        }

        // Estado: Caminando
        bool moving = player != null && Mathf.Abs(player.Horizontal) > 0.01f;
        if (!moving || walk == null || walk.Length == 0)
        {
            ApplySpriteSafe(idle);
            return;
        }

        // Animación de caminata (protegida contra sprites descargados)
        Sprite walkFrame = walk[(int)(Time.time * fps) % walk.Length];
        if (walkFrame != null && walkFrame.texture != null)
        {
            ApplySpriteSafe(walkFrame);
        }
        else
        {
            ApplySpriteSafe(idle);
        }
    }
}
