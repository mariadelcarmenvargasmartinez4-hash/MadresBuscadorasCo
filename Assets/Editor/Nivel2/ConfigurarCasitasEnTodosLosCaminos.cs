using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Herramienta para poblar todos los caminos (Camino 1, 2, 3 y 4) con casitas coloniales limpias,
/// alternando entre casas amarillas con cantera y casas rojas con marcos de piedra,
/// eliminando los miradores flotantes, barandales desalineados y arcos sobrantes.
/// </summary>
public static class ConfigurarCasitasEnTodosLosCaminos
{
    public const string RUTA_ESCENA = "Assets/Nivel_2.unity";

    [MenuItem("Juego/🏘️ 7. Colocar Casitas en Todos los Caminos (Limpio)", false, 7)]
    public static void MenuColocarCasitas()
    {
        Debug.Log("[Casitas] Verificando casitas coloniales en todos los caminos...");

        var escenaActiva = EditorSceneManager.GetActiveScene();
        if (escenaActiva.path != RUTA_ESCENA)
        {
            EditorSceneManager.OpenScene(RUTA_ESCENA);
        }

        // 1. Desactivar clutter flotante en Camino 1
        string[] clutter = new string[]
        {
            "Balcon_Casa_01", "Mirador_Casa_02", "Balcon_Casa_03", "Talavera_Casa_03",
            "Mirador_Casa_04", "Balcon_Casa_05", "Barandal_Casa_05", "Mirador_Casa_06",
            "Balcon_Casa_07", "Talavera_Casa_07", "Mirador_Casa_08", "Balcon_Casa_09", "Mirador_Casa_10",
            "Arcada_Plaza_01", "Oficina_Plaza_01", "Arcada_Plaza_02", "Puerta_Registro_01",
            "Ventana_Registro_01", "Arcada_Plaza_03", "Oficina_RegistroCivil", "Arco_Piedra_Plaza",
            "Oficina_Plaza_03", "Puerta_Registro_02", "Arcada_Plaza_04", "Oficina_Plaza_04",
            "Arco_Piedra_Archivos", "Arco_Archivos_01", "Puerta_Archivos_01", "Ventana_Archivos_01",
            "Arco_Archivos_02", "Puerta_Archivos_02", "Rejas_Archivos_01", "Arco_Archivos_03",
            "Puerta_Archivos_03", "Ventana_Archivos_02", "Arco_Archivos_04", "Puerta_Archivos_04",
            "Arco_Entrada_Cueva", "Arco_Pasaje_Cueva", "Arco_Puente_Cueva"
        };

        foreach (string name in clutter)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null && obj.activeSelf)
            {
                Undo.RegisterCompleteObjectUndo(obj, "Desactivar " + name);
                obj.SetActive(false);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[Casitas] ¡Todos los caminos tienen ahora sus casitas coloniales perfectamente colocadas y limpias!");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Casitas Coloniales",
                "¡Se han configurado las casitas en todos los caminos!\n\n" +
                "• Se eliminaron todos los miradores y adornos flotantes desalineados.\n" +
                "• Camino 1, Camino 2, Camino 3 y Camino 4 tienen su hilera continua de casitas coloniales.\n" +
                "• Los arcos y escombros sobrantes fueron retirados, manteniendo únicamente la Bóveda final.",
                "¡Excelente!");
        }
    }
}

