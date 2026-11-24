using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;

/// <summary>
/// Herramienta del Editor para acomodar el camino inferior de la cueva/bóveda
/// de modo que sea un camino 100% recto, limpio y continuo, exactamente igual
/// al primer y segundo camino de la ciudad.
/// </summary>
public static class ConfigurarCaminoInferiorRecto
{
    public const string RUTA_ESCENA = "Assets/Nivel_2.unity";

    [MenuItem("Juego/📐 6. Acomodar Camino Inferior Recto", false, 6)]
    public static void AcomodarCaminoRecto()
    {
        Debug.Log("[CaminoInferior] Acomodando camino inferior para que sea 100% recto...");

        var escenaActiva = EditorSceneManager.GetActiveScene();
        if (escenaActiva.path != RUTA_ESCENA)
        {
            EditorSceneManager.OpenScene(RUTA_ESCENA);
        }

        GameObject goGround = GameObject.Find("Ground");
        if (goGround == null)
        {
            Debug.LogError("[CaminoInferior] No se encontró el GameObject 'Ground' en la escena.");
            return;
        }

        Tilemap tilemap = goGround.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("[CaminoInferior] No se encontró el componente Tilemap en 'Ground'.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(tilemap, "Acomodar Camino Inferior Recto");

        TileBase tilePastoIzquierdo = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_108.asset");
        TileBase tilePastoSuperior   = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_109.asset");
        TileBase tilePastoDerecho   = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_110.asset");
        TileBase tileTierra         = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Cainos/Pixel Art Platformer - Village Props/Tileset Palette/TP Ground/TX Tileset Ground_101.asset");

        if (tilePastoSuperior == null)
        {
            Debug.LogError("[CaminoInferior] No se pudo cargar el asset de pasto superior.");
            return;
        }

        // 1. Limpiar pozos y casillas irregulares antiguas (Y <= -7, X de -85 a -35)
        for (int x = -85; x <= -35; x++)
        {
            for (int y = -25; y <= -7; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), null);
            }
        }

        // 2. Construir el nuevo camino recto continuo en Y = -16
        tilemap.SetTile(new Vector3Int(-80, -16, 0), tilePastoIzquierdo != null ? tilePastoIzquierdo : tilePastoSuperior);
        for (int x = -79; x <= -45; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -16, 0), tilePastoSuperior);
        }
        tilemap.SetTile(new Vector3Int(-44, -16, 0), tilePastoDerecho != null ? tilePastoDerecho : tilePastoSuperior);

        // Capa sólida de tierra debajo (Y = -17 e Y = -18)
        if (tileTierra != null)
        {
            for (int x = -80; x <= -44; x++)
            {
                tilemap.SetTile(new Vector3Int(x, -17, 0), tileTierra);
                tilemap.SetTile(new Vector3Int(x, -18, 0), tileTierra);
            }
        }

        tilemap.CompressBounds();
        tilemap.RefreshAllTiles();

        // 3. Desactivar plataformas flotantes quebradas
        string[] nombresDesactivar = new string[]
        {
            "Plataforma_Camino_Inferior_1",
            "Plataforma_Camino_Inferior_2",
            "Plataforma_Camino_Inferior_3",
            "Plataforma_Camino_Inferior_4",
            "Plataforma_Camino_Inferior_5",
            "Plataforma_Cueva_Bajada_1",
            "Plataforma_Cueva_Bajada_2",
            "Plataforma_Cueva_Bajada_3",
            "Plataforma_Cueva_Puente_Centro",
            "Plataforma_Entrada_Cueva_1",
            "Plataforma_Entrada_Cueva_2",
            "Plataforma_Cueva_Hacia_Boveda_1",
            "Plataforma_Cueva_Hacia_Boveda_2",
            "Plataforma_Boveda_Final_Base",
            "Plataforma_Hoja_9_Base",
            "PF Village Props - Wooden Bridge X6",
            "PF Village Props - Platform 01 R X2",
            "PF Village Props - Platform 01 L X2",
            "PF Village Props - Platform 02 X3",
            "PF Village Props - Platform 01 L X1",
            "PF Village Props - Platform 01 R X1",
            "PF Village Props - Platform 01 L X2 (1)"
        };

        foreach (string nombre in nombresDesactivar)
        {
            GameObject obj = GameObject.Find(nombre);
            if (obj != null)
            {
                Undo.RegisterCompleteObjectUndo(obj, "Desactivar plataforma irregular");
                obj.SetActive(false);
            }
        }

        // 4. Alinear elementos sobre el camino recto
        AlinearObjeto("Arco_Boveda_Final", new Vector3(-74.0f, -16.0f, 2.0f));
        AlinearObjeto("Rejas_Boveda_Final", new Vector3(-74.0f, -16.0f, 1.8f));
        AlinearObjeto("Pista_Hoja_10_Boveda_Final", new Vector3(-74.0f, -14.8f, 0.0f));

        AlinearObjeto("Arco_Entrada_Cueva", new Vector3(-67.0f, -16.0f, 2.0f));
        AlinearObjeto("Arco_Pasaje_Cueva", new Vector3(-58.0f, -16.0f, 2.0f));
        AlinearObjeto("Arco_Puente_Cueva", new Vector3(-50.0f, -16.0f, 2.0f));

        AlinearObjeto("Botella_Agua_3_Cueva", new Vector3(-62.5f, -15.2f, 0.0f));
        AlinearObjeto("Pista_Hoja_9_Cadenas", new Vector3(-54.0f, -14.8f, 0.0f));
        AlinearObjeto("Pista_Hoja_8_Andamio", new Vector3(-46.0f, -14.8f, 0.0f));
        AlinearObjeto("Obstaculo_Movil_6_Boveda", new Vector3(-70.0f, -15.0f, 0.0f));

        // 5. Ajustar escalera desde camino 3 hasta camino 4
        GameObject escalera = GameObject.Find("Escalera_Entrada_Cueva");
        if (escalera != null)
        {
            Undo.RegisterCompleteObjectUndo(escalera.transform, "Alinear Escalera");
            escalera.transform.position = new Vector3(-65.5f, -10.5f, 0.0f);
            escalera.transform.localScale = new Vector3(1.6f, 11.0f, 1.0f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[CaminoInferior] ¡Camino inferior acomodado con éxito! Ahora es 100% recto, limpio y continuo.");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Camino Inferior Recto",
                "¡El camino inferior se ha acomodado con éxito!\n\n" +
                "• Se eliminaron los pozos y plataformas quebradas.\n" +
                "• Se creó un camino recto continuo con pasto y tierra sólida (Y = -16).\n" +
                "• La Bóveda final, arcos, pistas y botella quedaron perfectamente alineados sobre el camino.\n" +
                "• La escalera conecta de forma directa el camino 3 con este camino recto.",
                "¡Excelente!");
        }
    }

    private static void AlinearObjeto(string nombre, Vector3 nuevaPosicion)
    {
        GameObject obj = GameObject.Find(nombre);
        if (obj != null)
        {
            Undo.RegisterCompleteObjectUndo(obj.transform, "Alinear " + nombre);
            obj.transform.position = nuevaPosicion;
        }
    }
}

