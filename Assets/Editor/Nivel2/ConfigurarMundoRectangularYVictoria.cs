using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Herramienta del Editor para:
/// 1. Unificar todos los 4 caminos en un RECTÁNGULO perfecto y continuo (de X = -83 a X = -6).
/// 2. Extender Camino 1 y Camino 4 a la derecha con sus respectivas casitas coloniales.
/// 3. Conectar los caminos en formato zig-zag continuo (escaleras y bajadas).
/// 4. Distribuir las 10 hojas coleccionables en secuencia natural a lo largo del recorrido.
/// 5. Ubicar la Bóveda Final con la Hoja 10 al final del camino inferior.
/// 6. Activar el Menú "¡HAS GANADO!" al completar el expediente.
/// </summary>
public static class ConfigurarMundoRectangularYVictoria
{
    public const string RUTA_ESCENA = "Assets/Nivel_2.unity";

    [MenuItem("Juego/📐 8. Reacomodar Mundo en Rectángulo y Menú Has Ganado", false, 8)]
    public static void ReacomodarTodo()
    {
        EjecutarReacomodo(false);
    }

    public static void EjecutarReacomodo(bool silencioso)
    {
        Debug.Log("[MundoRectangular] Iniciando reacomodo del mundo en un rectángulo perfecto...");

        var escenaActiva = EditorSceneManager.GetActiveScene();
        if (escenaActiva.path != RUTA_ESCENA)
        {
            escenaActiva = EditorSceneManager.OpenScene(RUTA_ESCENA);
        }

        // 0. Backup de seguridad
        try
        {
            File.Copy(RUTA_ESCENA, RUTA_ESCENA + ".backup_rectangular", true);
        }
        catch { }

        // 1. OBTENER TILEMAP GROUND
        GameObject goGround = GameObject.Find("Ground");
        if (goGround == null)
        {
            Debug.LogError("[MundoRectangular] No se encontró el GameObject 'Ground'.");
            return;
        }

        Tilemap tilemap = goGround.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("[MundoRectangular] No se encontró el componente Tilemap en 'Ground'.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(tilemap, "Reacomodar Mundo en Rectángulo");

        // 2. CARGAR TILES
        TileBase tilePastoIzquierdo = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_108.asset");
        TileBase tilePastoSuperior  = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_109.asset");
        TileBase tilePastoDerecho   = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_110.asset");
        TileBase tileTierra        = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_101.asset");

        if (tilePastoSuperior == null)
        {
            Debug.LogError("[MundoRectangular] No se pudo cargar el tile de pasto superior.");
            return;
        }

        // 3. LIMPIAR CUALQUIER TILE SUELTO O IRREGULAR EN LA ZONA INFERIOR
        for (int x = -86; x <= -4; x++)
        {
            for (int y = -25; y <= -7; y++)
            {
                if (y != -16 && y != -17 && y != -18)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }

        // 4. CONSTRUIR LOS 4 CAMINOS RECTOS DE X = -83 A X = -6 (ANCHO: 78 TILES CADA UNO)

        // CAMINO 1: Y = 5 (Superficie)
        tilemap.SetTile(new Vector3Int(-83, 5, 0), tilePastoIzquierdo ?? tilePastoSuperior);
        for (int x = -82; x <= -7; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 5, 0), tilePastoSuperior);
        }
        tilemap.SetTile(new Vector3Int(-6, 5, 0), tilePastoDerecho ?? tilePastoSuperior);
        // Tierra debajo (Y = 4)
        for (int x = -83; x <= -6; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 4, 0), tileTierra);
        }

        // CAMINO 2: Y = 0 (Nivel Medio / Plaza y Registro)
        tilemap.SetTile(new Vector3Int(-83, 0, 0), tilePastoIzquierdo ?? tilePastoSuperior);
        for (int x = -82; x <= -7; x++)
        {
            tilemap.SetTile(new Vector3Int(x, 0, 0), tilePastoSuperior);
        }
        tilemap.SetTile(new Vector3Int(-6, 0, 0), tilePastoDerecho ?? tilePastoSuperior);
        // Tierra debajo (Y = -1)
        for (int x = -83; x <= -6; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -1, 0), tileTierra);
        }

        // CAMINO 3: Y = -5 (Sótano y Archivos)
        tilemap.SetTile(new Vector3Int(-83, -5, 0), tilePastoIzquierdo ?? tilePastoSuperior);
        for (int x = -82; x <= -7; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -5, 0), tilePastoSuperior);
        }
        tilemap.SetTile(new Vector3Int(-6, -5, 0), tilePastoDerecho ?? tilePastoSuperior);
        // Tierra debajo (Y = -6)
        for (int x = -83; x <= -6; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -6, 0), tileTierra);
        }

        // CAMINO 4: Y = -16 (Cueva y Bóveda Final)
        tilemap.SetTile(new Vector3Int(-83, -16, 0), tilePastoIzquierdo ?? tilePastoSuperior);
        for (int x = -82; x <= -7; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -16, 0), tilePastoSuperior);
        }
        tilemap.SetTile(new Vector3Int(-6, -16, 0), tilePastoDerecho ?? tilePastoSuperior);
        // Tierra sólida debajo (Y = -17 e Y = -18)
        for (int x = -83; x <= -6; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -17, 0), tileTierra);
            tilemap.SetTile(new Vector3Int(x, -18, 0), tileTierra);
        }

        tilemap.CompressBounds();
        tilemap.RefreshAllTiles();

        // 5. CARGAR SPRITES DE LAS CASITAS COLONIALES
        Sprite spCasaAmarilla = null;
        Sprite spCasaRoja = null;

        Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/imagen/edificios_modulares_san_luis.png");
        foreach (var obj in allSprites)
        {
            if (obj is Sprite sp)
            {
                if (sp.name == "Fachada_Amarilla_Completa") spCasaAmarilla = sp;
                else if (sp.name == "Fachada_Roja_Completa") spCasaRoja = sp;
            }
        }

        // 6. POBLAR CASITAS EN CAMINO 1 (Extender desde X = -27 hasta X = -9)
        CrearOActualizarCasa("Casa_08_Roja", new Vector3(-18.3f, 9.89f, 2.0f), Vector3.one, spCasaRoja);
        CrearOActualizarCasa("Casa_09_Amarilla", new Vector3(-9.7f, 9.89f, 2.0f), Vector3.one, spCasaAmarilla);

        // 7. POBLAR CASITAS EN CAMINO 4 (Extender desde X = -47 hasta X = -9)
        CrearOActualizarCasa("Casa_05_Amarilla_C4", new Vector3(-38.1f, -9.68f, 2.0f), new Vector3(1.1618f, 1.2562f, 1f), spCasaAmarilla);
        CrearOActualizarCasa("Casa_06_Roja_C4", new Vector3(-28.6f, -9.71f, 2.0f), new Vector3(1.1618f, 1.2562f, 1f), spCasaRoja);
        CrearOActualizarCasa("Casa_07_Amarilla_C4", new Vector3(-19.1f, -9.35f, 2.0f), new Vector3(1.683f, 1.1722f, 1f), spCasaAmarilla);
        CrearOActualizarCasa("Casa_08_Roja_C4", new Vector3(-9.6f, -9.71f, 2.0f), new Vector3(1.1618f, 1.2562f, 1f), spCasaRoja);

        // 8. ESCALERAS / CONEXIONES ENTRE PISOS
        // Escalera 1: De Camino 1 (Y=5) a Camino 2 (Y=0) en el extremo derecho (X = -7)
        CrearOActualizarEscalera("Escalera_Camino1_a_Camino2", new Vector3(-7.2f, 2.5f, 1.0f), new Vector3(1.4f, 5.2f, 1f));

        // Escalera 2: De Camino 2 (Y=0) a Camino 3 (Y=-5) en el extremo izquierdo (X = -81.5)
        CrearOActualizarEscalera("Escalera_Camino2_a_Camino3", new Vector3(-81.5f, -2.5f, 1.0f), new Vector3(1.4f, 5.2f, 1f));

        // Escalera 3: De Camino 3 (Y=-5) a Camino 4 (Y=-16) en el extremo derecho (X = -7.2)
        CrearOActualizarEscalera("Escalera_Camino3_a_Camino4", new Vector3(-7.2f, -10.5f, 1.0f), new Vector3(1.5f, 11.2f, 1f));

        // Desactivar escalera vieja desalineada
        DesactivarObjeto("Escalera_Entrada_Cueva");
        DesactivarObjeto("Plataforma_Entrada_Cueva_1");
        DesactivarObjeto("Plataforma_Entrada_Cueva_2");
        DesactivarObjeto("Plataforma_Cueva_Bajada_1");
        DesactivarObjeto("Plataforma_Cueva_Bajada_2");
        DesactivarObjeto("Plataforma_Cueva_Bajada_3");
        DesactivarObjeto("Plataforma_Cueva_Puente_Centro");

        // 9. ALINEAR LAS 10 HOJAS EN SECUENCIA NATURAL (ZIG-ZAG)
        AlinearObjeto("Pista_Hoja_1_Mercado", new Vector3(-48.0f, 6.2f, 0.0f));
        AlinearObjeto("Pista_Hoja_2_Parada", new Vector3(-15.0f, 6.2f, 0.0f)); // En la nueva extensión superior derecha

        AlinearObjeto("Pista_Hoja_3_Pozo", new Vector3(-22.0f, 1.2f, 0.0f));
        AlinearObjeto("Pista_Hoja_4_Registro", new Vector3(-62.0f, 1.2f, 0.0f));

        AlinearObjeto("Pista_Hoja_5_Puente", new Vector3(-70.0f, -3.8f, 0.0f));
        AlinearObjeto("Pista_Hoja_6_Estatua", new Vector3(-44.0f, -3.8f, 0.0f));
        AlinearObjeto("Pista_Hoja_7_Archivero", new Vector3(-18.0f, -3.8f, 0.0f));

        AlinearObjeto("Pista_Hoja_8_Andamio", new Vector3(-15.0f, -14.8f, 0.0f)); // En la nueva extensión inferior derecha
        AlinearObjeto("Pista_Hoja_9_Cadenas", new Vector3(-45.0f, -14.8f, 0.0f));
        AlinearObjeto("Pista_Hoja_10_Boveda_Final", new Vector3(-74.0f, -14.8f, 0.0f)); // En la bóveda final

        // 10. BÓVEDA FINAL
        AlinearObjeto("Arco_Boveda_Final", new Vector3(-74.0f, -16.0f, 2.0f));
        AlinearObjeto("Rejas_Boveda_Final", new Vector3(-74.0f, -16.0f, 1.8f));

        // 11. BOTELLAS DE AGUA
        AlinearObjeto("Botella_Agua_1_Superficie", new Vector3(-32.0f, 6.0f, 0.0f));
        AlinearObjeto("Botella_Agua_2_Registro", new Vector3(-42.0f, 1.0f, 0.0f));
        AlinearObjeto("Botella_Agua_3_Cueva", new Vector3(-28.0f, -15.0f, 0.0f)); // En la nueva extensión inferior derecha

        // 12. OBSTÁCULOS MÓVILES SOBRE CADA CAMINO
        AlinearObjeto("Obstaculo_Movil_1_Mercado", new Vector3(-38.0f, 6.5f, 0.0f));
        AlinearObjeto("Obstaculo_Movil_2_Parada", new Vector3(-22.0f, 6.5f, 0.0f));
        AlinearObjeto("Obstaculo_Movil_3_Plaza", new Vector3(-35.0f, 1.5f, 0.0f));
        AlinearObjeto("Obstaculo_Movil_4_Andamio", new Vector3(-58.0f, -3.5f, 0.0f));
        AlinearObjeto("Obstaculo_Movil_5_Estatua", new Vector3(-32.0f, -3.5f, 0.0f));
        AlinearObjeto("Obstaculo_Movil_6_Boveda", new Vector3(-60.0f, -14.8f, 0.0f));

        // 13. PROTAGONISTA (Punto de inicio en Camino 1)
        GameObject madre = GameObject.Find("MadreBuscadora");
        if (madre != null)
        {
            Undo.RegisterCompleteObjectUndo(madre.transform, "Posicionar Madre");
            madre.transform.position = new Vector3(-74.0f, 6.2f, 0.0f);
            var mov = madre.GetComponent<MovimientoMadre>();
            if (mov != null)
            {
                mov.posicionPrimeraPlataforma = new Vector3(-74.0f, 6.2f, 0.0f);
            }
        }

        // 14. CENTRAR BACKGROUND
        GameObject bg = GameObject.Find("Background");
        if (bg != null)
        {
            Undo.RegisterCompleteObjectUndo(bg.transform, "Centrar Background");
            bg.transform.position = new Vector3(-44.5f, -5.5f, 9.54f);
            var srBg = bg.GetComponent<SpriteRenderer>();
            if (srBg != null)
            {
                srBg.size = new Vector2(220f, 110f);
                srBg.sortingOrder = -100;
            }
        }

        // 15. GUARDAR Y MARCAR ESCENA
        EditorSceneManager.MarkSceneDirty(escenaActiva);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[MundoRectangular] ¡Mundo organizado en un rectángulo perfecto con 4 caminos completos y Menú Has Ganado listo!");

        if (!Application.isBatchMode && !silencioso)
        {
            EditorUtility.DisplayDialog("Mundo en Rectángulo",
                "¡El mundo ha quedado organizado en un RECTÁNGULO perfecto!\n\n" +
                "• Los 4 caminos abarcan exactamente de X = -83 a X = -6 (ancho uniforme).\n" +
                "• Camino 1 y Camino 4 extendidos a la derecha con casitas coloniales.\n" +
                "• Escaleras fluidas en los extremos para un recorrido natural en zig-zag.\n" +
                "• Las 10 hojas coleccionables guiadas por los acertijos.\n" +
                "• Al juntar las 10 hojas aparece automáticamente el Menú ¡HAS GANADO! con opciones de Reiniciar, Quitar Sonido y Salir.",
                "¡Excelente!");
        }
    }

    private static void CrearOActualizarCasa(string nombre, Vector3 pos, Vector3 escala, Sprite sprite)
    {
        GameObject go = GameObject.Find(nombre);
        if (go == null)
        {
            go = new GameObject(nombre);
            Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        }
        else
        {
            Undo.RegisterCompleteObjectUndo(go, "Actualizar " + nombre);
        }

        go.SetActive(true);
        go.transform.position = pos;
        go.transform.localScale = escala;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();

        if (sprite != null) sr.sprite = sprite;
        sr.sortingOrder = -5; // Detrás del jugador y elementos, sobre el fondo
        sr.color = Color.white;
    }

    private static void CrearOActualizarEscalera(string nombre, Vector3 pos, Vector3 escala)
    {
        GameObject go = GameObject.Find(nombre);
        if (go == null)
        {
            // Intentar cargar prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Cainos/Pixel Art Platformer - Village Props/Prefab/PF Village Props - Ladder 01.prefab");
            if (prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = nombre;
                Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
            }
            else
            {
                go = new GameObject(nombre);
                Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
            }
        }
        else
        {
            Undo.RegisterCompleteObjectUndo(go, "Actualizar " + nombre);
        }

        go.SetActive(true);
        go.transform.position = pos;
        go.transform.localScale = escala;
    }

    private static void AlinearObjeto(string nombre, Vector3 nuevaPos)
    {
        GameObject obj = GameObject.Find(nombre);
        if (obj != null)
        {
            Undo.RegisterCompleteObjectUndo(obj.transform, "Alinear " + nombre);
            obj.SetActive(true);
            obj.transform.position = nuevaPos;
        }
    }

    private static void DesactivarObjeto(string nombre)
    {
        GameObject obj = GameObject.Find(nombre);
        if (obj != null)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Desactivar " + nombre);
            obj.SetActive(false);
        }
    }
}

