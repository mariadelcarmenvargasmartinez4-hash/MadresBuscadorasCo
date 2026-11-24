using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LasHuellasDelSilencio.Systems;

namespace LasHuellasDelSilencio.EditorScripts
{
    /// <summary>
    /// Herramienta de Editor de Unity para generar y mantener actualizada la escena SampleScene.unity
    /// reutilizando la construcción canónica de RuntimeLevel3Bootstrapper.
    /// </summary>
    [InitializeOnLoad]
    public static class Level3SceneBuilder
    {
        private const string TargetScenePath = "Assets/Nivel_3.unity";

        static Level3SceneBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfEmpty;
        }

        private static void AutoBuildIfEmpty()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != TargetScenePath) return;

            bool needsUpgrade = GameObject.Find("Hacienda_Vieja_Entorno") == null
                || GameObject.Find("Etiqueta_Linea_Vida") == null
                || GameObject.Find("Objetos_Vida_Salud") == null
                || GameObject.Find("Barra_Vida_Contenedor") == null
                || GameObject.Find("Barra_Energia_Correr") == null
                || GameObject.Find("Panel_GameOver") == null
                || GameObject.Find("Puerta_Con_Acertijo") == null
                || GameObject.Find("Tumba_De_Laura") == null
                || GameObject.Find("Escena_Fosa_Clandestina") == null
                || GameObject.Find("Fosas_Restos_Enterrados") == null
                || GameObject.Find("Vigilante_Patio") == null
                || GameObject.Find("Guardian_Tuneles") == null
                || GameObject.Find("MadreBuscadora_Player") == null
                || GameObject.Find("MadreBuscadora_Player").GetComponent<LasHuellasDelSilencio.Player.PlayerCombat2D>() == null
                || GameObject.Find("Modal_Cedula_Busqueda") == null
                || GameObject.Find("Meta_Final_Nivel3") == null
                || GameObject.Find("Vigilante_Patio_Sur") == null
                || GameObject.Find("Vigilante_Caseta_Norte") == null;

            if (needsUpgrade)
            {
                BuildLevel3Hierarchy();
            }
        }

        [MenuItem("Las Huellas del Silencio/Construir Escena Completa de Nivel 3")]
        public static void BuildLevel3Hierarchy()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != TargetScenePath && System.IO.File.Exists(TargetScenePath))
            {
                EditorSceneManager.OpenScene(TargetScenePath);
                activeScene = EditorSceneManager.GetActiveScene();
            }

            Debug.Log("<color=cyan><b>[Las Huellas del Silencio]</b> Construyendo SampleScene profesional con Correr, Vida y Enemigos...</color>");

            GameObject levelRoot = new GameObject("--- NIVEL 3: LAS HUELLAS DEL SILENCIO ---");
            RuntimeLevel3Bootstrapper.BuildCanonicalLevel(levelRoot.transform);

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("<color=green><b>[Las Huellas del Silencio]</b> SampleScene.unity generada y guardada correctamente.</color>");
        }
    }
}
