#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HastaEncontrarles.Nivel4;

namespace HastaEncontrarles.EditorTools
{
    [InitializeOnLoad]
    public static class ConfiguradorEscenaNivel4
    {
        static ConfiguradorEscenaNivel4()
        {
            EditorApplication.delayCall += AsegurarGestorEnEscenaActiva;
        }

        [MenuItem("Hasta Encontrarles/1. Preparar Escena Nivel 4 (Auto-Configurar)")]
        public static void AsegurarGestorEnEscenaActiva()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.GetActiveScene().name.Contains("Nivel_4")) return;

            GestorNivel4 existente = Object.FindAnyObjectByType<GestorNivel4>();
            if (existente == null)
            {
                GameObject obj = new GameObject("GestorNivel4_LaMemoriaViva");
                obj.AddComponent<GestorNivel4>();
                obj.AddComponent<GestorAudioNivel4>();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
                Debug.Log("[Hasta Encontrarles - Nivel 4] GestorNivel4 agregado y guardado automáticamente en la escena.");
            }
        }

        [MenuItem("Hasta Encontrarles/2. Borrar Datos de Guardado (Reiniciar Progreso 0%)")]
        public static void ReiniciarDatosGuardado()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            string rutaJson = System.IO.Path.Combine(Application.persistentDataPath, "progreso_hasta_encontrarles.json");
            if (System.IO.File.Exists(rutaJson))
            {
                System.IO.File.Delete(rutaJson);
            }
            Debug.Log("[Hasta Encontrarles - Nivel 4] Progreso de PlayerPrefs y JSON reiniciado correctamente.");
        }
    }
}
#endif

