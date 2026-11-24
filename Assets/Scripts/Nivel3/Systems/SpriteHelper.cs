using System.IO;
using UnityEngine;

namespace LasHuellasDelSilencio.Systems
{
    /// <summary>
    /// Cargador universal de Sprites para Unity.
    /// Carga vía Resources o lectura directa de archivo PNG en disco,
    /// y configura el SpriteRenderer con su sprite y capa de ordenamiento correcta.
    /// IMPORTANTE: No sobrescribe sr.material para permitir que el SpriteRenderer
    /// de Unity aplique su material predeterminado con la textura propia de cada sprite.
    /// </summary>
    public static class SpriteHelper
    {
        public static void ConfigureRenderer(SpriteRenderer sr, Sprite sprite, int sortingOrder)
        {
            if (sr == null) return;
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            // Garantizar color blanco opaco para que se vean todos los colores reales del sprite
            sr.color = Color.white;
        }

        public static Sprite LoadSprite(string resourceName)
        {
            // 1. Intentar carga estándar de Resources
            Sprite s = Resources.Load<Sprite>(resourceName);
            if (s != null && s.rect.width > 1f && s.rect.height > 1f)
            {
                return s;
            }

            // Si es un sprite múltiple recortado (como MadreBuscadora_Protagonista)
            Sprite[] all = Resources.LoadAll<Sprite>(resourceName);
            if (all != null && all.Length > 0)
            {
                foreach (var item in all)
                {
                    if (item != null && item.rect.width > 1f && item.rect.height > 1f)
                    {
                        return item;
                    }
                }
            }

            // 1B. Si está en Resources como Texture2D (ej. en compilación WebGL / Windows)
            Texture2D resTex = Resources.Load<Texture2D>(resourceName);
            if (resTex != null && resTex.width > 1 && resTex.height > 1)
            {
                return Sprite.Create(resTex, new Rect(0, 0, resTex.width, resTex.height), new Vector2(0.5f, 0.5f), 64f);
            }

            // 2. Fallback: Carga directa de archivo PNG desde Assets/Resources o Assets/Sprites
            string[] searchPaths = new string[]
            {
                Path.Combine(Application.dataPath, "Resources", resourceName + ".png"),
                Path.Combine(Application.dataPath, "Sprites", resourceName + ".png"),
                Path.Combine(Application.dataPath, "Resources", resourceName + ".jpg"),
                Path.Combine(Application.dataPath, "Sprites", resourceName + ".jpg")
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(path);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        tex.filterMode = FilterMode.Point;
                        if (tex.LoadImage(fileData))
                        {
                            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[SpriteHelper] Error al leer {path}: {ex.Message}");
                    }
                }
            }

            // 3. Fallback procedural si el archivo no existiera
            return CreateProceduralColorSprite(new Color(0.6f, 0.5f, 0.4f, 1f), 32, 32);
        }

        public static Sprite CreateProceduralColorSprite(Color col, int width = 32, int height = 32)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++) colors[i] = col;
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 64f);
        }
    }
}
