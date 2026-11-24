#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Herramienta de publicación directa para Itch.io y Netlify Drop (Web HTML5 / WebGL).
/// </summary>
public static class ItchIoExporter
{
    [MenuItem("Itch.io/1. Abrir Itch.io y Mostrar ZIP Web Listo para Subir", false, 10)]
    public static void OpenItchIoAndShowZip()
    {
        string rootDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
        string zipPath = Path.Combine(rootDir, "Hasta_Encontrarles_WebGL_Itchio_Netlify.zip");

        if (File.Exists(zipPath))
        {
            EditorUtility.RevealInFinder(zipPath);
        }

        Application.OpenURL("https://itch.io/game/new");
        Debug.Log("[Itch.io] Abriendo https://itch.io/game/new y mostrando Hasta_Encontrarles_WebGL_Itchio_Netlify.zip");
    }

    [MenuItem("Itch.io/2. Compilar Unity WebGL para Itch.io y Netlify Drop", false, 20)]
    public static void BuildUnityWebGLForItchIo()
    {
        bool ok = ExecuteWebGLBuild(interactive: true);
        if (ok)
        {
            Application.OpenURL("https://itch.io/game/new");
        }
    }

    /// <summary>
    /// Punto de entrada para compilación silenciosa desde terminal (-executeMethod ItchIoExporter.BuildWebGLBatchMode).
    /// </summary>
    public static void BuildWebGLBatchMode()
    {
        try
        {
            bool ok = ExecuteWebGLBuild(interactive: false);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(ok ? 0 : 1);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[ItchIoExporter] Error crítico en BuildWebGLBatchMode: " + ex);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }

    private static bool ExecuteWebGLBuild(bool interactive)
    {
        string rootDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
        string buildDir = Path.Combine(rootDir, "WebGL_Build");
        string zipPath = Path.Combine(rootDir, "Hasta_Encontrarles_WebGL_Itchio_Netlify.zip");

        PlayerSettings.productName = "Hasta Encontrarles - El Camino de la Esperanza";
        PlayerSettings.companyName = "UTNG - Maria y Valeria";
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.template = "APPLICATION:Default";

        string[] scenes = new string[]
        {
            "Assets/Scenes/MenuInicio.unity",
            "Assets/Nivel_1.unity",
            "Assets/Nivel_2.unity",
            "Assets/Nivel_3.unity",
            "Assets/Nivel_4.unity"
        };

        if (Directory.Exists(buildDir))
        {
            try { Directory.Delete(buildDir, true); } catch { }
        }
        Directory.CreateDirectory(buildDir);

        Debug.Log("[ItchIoExporter] Iniciando compilación WebGL de las 5 escenas en: " + buildDir);

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildDir,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        Debug.Log($"[ItchIoExporter] Resultado WebGL: {report.summary.result} | Errores: {report.summary.totalErrors} | Tiempo: {report.summary.totalTime}");

        if (report.summary.result == BuildResult.Succeeded)
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(buildDir, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);
            Debug.Log("[ItchIoExporter] ZIP creado exitosamente en: " + zipPath);

            if (interactive)
            {
                EditorUtility.RevealInFinder(zipPath);
            }
            return true;
        }

        return false;
    }
}
#endif
