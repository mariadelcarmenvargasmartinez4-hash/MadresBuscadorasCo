using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.IO.Compression;

/// <summary>
/// Herramienta de compilación profesional para Itch.io (WebGL y Windows Standalone).
/// Configura los parámetros de PlayerSettings para máximo rendimiento y compatibilidad,
/// compila la escena SampleScene y empaqueta el resultado en archivos ZIP listos para publicar.
/// </summary>
public static class CompiladorWebGL
{
    public const string RUTA_SALIDA_WEBGL = "Builds/WebGL/HastaEncontrarles_Nivel2";
    public const string RUTA_ZIP_ITCHIO = "Builds/WebGL/HastaEncontrarles_Nivel2_ItchIO.zip";

    public const string RUTA_SALIDA_WINDOWS = "Builds/Windows/HastaEncontrarles_Nivel2";
    public const string RUTA_ZIP_WINDOWS = "Builds/Windows/HastaEncontrarles_Nivel2_Windows.zip";

    [MenuItem("Juego/🚀 4. Compilar Juego a WebGL para Itch.io", false, 4)]
    public static void MenuCompilarWebGL()
    {
        Debug.Log("[CompiladorWebGL] Iniciando preparación y compilación para WebGL (Itch.io)...");
        ConfigurarParametrosProfesionales();
        EjecutarCompilacionWebGL();
    }

    [MenuItem("Juego/💻 5. Compilar Juego para Windows (PC / Itch.io)", false, 5)]
    public static void MenuCompilarWindows()
    {
        Debug.Log("[CompiladorWebGL] Iniciando compilación para Windows Standalone (Itch.io)...");
        ConfigurarParametrosProfesionales();
        EjecutarCompilacionWindows();
    }

    public static void CompilarParaWebGLCLI()
    {
        Debug.Log("[CompiladorWebGL-CLI] Compilación WebGL por línea de comandos iniciada.");
        ConfigurarParametrosProfesionales();
        EjecutarCompilacionWebGL();
    }

    public static void ConfigurarParametrosProfesionales()
    {
        // 1. Identidad del Juego y Metadatos Profesionales
        PlayerSettings.companyName = "Madres Buscadoras";
        PlayerSettings.productName = "Hasta Encontrarles: El Camino de la Esperanza";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.runInBackground = true;

        // 2. Resolución WebGL estándar HD 16:9 (Ideal para el reproductor embebido de Itch.io)
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;

        // 3. Compatibilidad WebGL en Itch.io (Sin compresión gzip forzada en servidor para evitar errores 404/Content-Encoding)
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.memorySize = 256;

        AssetDatabase.SaveAssets();
        Debug.Log("[CompiladorWebGL] Parámetros de PlayerSettings configurados con éxito.");
    }

    public static bool EjecutarCompilacionWebGL()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            Debug.LogWarning("[CompiladorWebGL] El módulo de soporte WebGL no está instalado en este Editor de Unity.");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Módulo WebGL Requerido",
                    "Para compilar el juego a WebGL necesitas tener instalado el módulo 'Web Build Support' en Unity 6000.3.25f1.\n\n" +
                    "Pasos rápidos para instalarlo (1 minuto):\n" +
                    "1. Abre 'Unity Hub'.\n" +
                    "2. En el menú izquierdo, haz clic en 'Installs' (Instalaciones).\n" +
                    "3. En tu versión de Unity (6000.3.25f1), haz clic en el engranaje ⚙️ y selecciona 'Add modules' (Agregar módulos).\n" +
                    "4. Marca la casilla 'Web Build Support' e instala.\n" +
                    "5. Regresa a Unity y presiona 'Juego -> 🚀 4. Compilar Juego a WebGL para Itch.io'.",
                    "Entendido");
            }
            return false;
        }

        // Cambiar la plataforma activa a WebGL si está en Windows Standalone
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            Debug.Log("[CompiladorWebGL] Cambiando plataforma activa del Editor a WebGL...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }

        string carpetaDestino = Path.GetFullPath(RUTA_SALIDA_WEBGL);
        string carpetaParent = Path.GetDirectoryName(carpetaDestino);

        if (!Directory.Exists(carpetaParent)) Directory.CreateDirectory(carpetaParent);
        if (Directory.Exists(carpetaDestino))
        {
            try { Directory.Delete(carpetaDestino, true); } catch { }
        }
        Directory.CreateDirectory(carpetaDestino);

        BuildPlayerOptions opciones = new BuildPlayerOptions();
        opciones.scenes = new string[] { "Assets/Nivel_2.unity" };
        opciones.locationPathName = carpetaDestino;
        opciones.target = BuildTarget.WebGL;
        opciones.options = BuildOptions.None;

        Debug.Log($"[CompiladorWebGL] Compilando WebGL hacia: {carpetaDestino}");
        BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
        BuildSummary resumen = reporte.summary;

        string archivoIndex = Path.Combine(carpetaDestino, "index.html");

        if (resumen.result == BuildResult.Succeeded && File.Exists(archivoIndex))
        {
            Debug.Log($"[CompiladorWebGL] ¡Compilación WebGL EXITOSA! Tamaño: {resumen.totalSize / (1024 * 1024):F2} MB. Tiempo: {resumen.totalTime.TotalSeconds:F1}s.");

            string rutaZip = Path.GetFullPath(RUTA_ZIP_ITCHIO);
            if (File.Exists(rutaZip)) File.Delete(rutaZip);

            try
            {
                ZipFile.CreateFromDirectory(carpetaDestino, rutaZip, System.IO.Compression.CompressionLevel.Optimal, false);
                Debug.Log($"[CompiladorWebGL] Paquete ZIP generado exitosamente: {rutaZip}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[CompiladorWebGL] Error al empaquetar ZIP: " + ex.Message);
            }

            GenerarGuiaItchIO(carpetaParent);

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Compilación WebGL Completada",
                    "¡El juego se ha compilado exitosamente para WebGL!\n\n" +
                    $"• Carpeta WebGL: {carpetaDestino}\n" +
                    $"• Archivo ZIP para Itch.io: {rutaZip}\n\n" +
                    "Ya puedes subir el archivo .zip directamente a tu panel de itch.io.", "¡Excelente!");
            }

            return true;
        }
        else
        {
            Debug.LogError($"[CompiladorWebGL] La compilación falló con estado: {resumen.result}. Total de errores: {resumen.totalErrors}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Error de Compilación WebGL",
                    $"La compilación no pudo completarse.\nErrores reportados: {resumen.totalErrors}\nRevisa la consola de Unity para más detalles.", "Entendido");
            }
            return false;
        }
    }

    public static bool EjecutarCompilacionWindows()
    {
        string carpetaDestino = Path.GetFullPath(RUTA_SALIDA_WINDOWS);
        string carpetaParent = Path.GetDirectoryName(carpetaDestino);

        if (!Directory.Exists(carpetaParent)) Directory.CreateDirectory(carpetaParent);
        if (Directory.Exists(carpetaDestino))
        {
            try { Directory.Delete(carpetaDestino, true); } catch { }
        }
        Directory.CreateDirectory(carpetaDestino);

        string exePath = Path.Combine(carpetaDestino, "HastaEncontrarles_Nivel2.exe");

        BuildPlayerOptions opciones = new BuildPlayerOptions();
        opciones.scenes = new string[] { "Assets/Nivel_2.unity" };
        opciones.locationPathName = exePath;
        opciones.target = BuildTarget.StandaloneWindows64;
        opciones.options = BuildOptions.None;

        Debug.Log($"[CompiladorWebGL] Compilando Windows Standalone hacia: {exePath}");
        BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
        BuildSummary resumen = reporte.summary;

        if (resumen.result == BuildResult.Succeeded)
        {
            Debug.Log($"[CompiladorWebGL] ¡Compilación Windows EXITOSA! Tamaño: {resumen.totalSize / (1024 * 1024):F2} MB. Tiempo: {resumen.totalTime.TotalSeconds:F1}s.");

            string rutaZip = Path.GetFullPath(RUTA_ZIP_WINDOWS);
            if (File.Exists(rutaZip)) File.Delete(rutaZip);

            try
            {
                ZipFile.CreateFromDirectory(carpetaDestino, rutaZip, System.IO.Compression.CompressionLevel.Optimal, false);
                Debug.Log($"[CompiladorWebGL] Paquete ZIP Windows generado exitosamente: {rutaZip}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[CompiladorWebGL] Error al empaquetar ZIP: " + ex.Message);
            }

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Compilación Windows Completada",
                    "¡El juego se ha compilado exitosamente para Windows!\n\n" +
                    $"• Ejecutable: {exePath}\n" +
                    $"• Archivo ZIP para Itch.io: {rutaZip}\n\n" +
                    "Ya puedes subir el archivo .zip a itch.io como versión descargable para PC.", "¡Excelente!");
            }

            return true;
        }
        else
        {
            Debug.LogError($"[CompiladorWebGL] La compilación falló con estado: {resumen.result}. Total de errores: {resumen.totalErrors}");
            return false;
        }
    }

    private static void GenerarGuiaItchIO(string carpetaDestino)
    {
        string rutaGuia = Path.Combine(carpetaDestino, "INSTRUCCIONES_PUBLICAR_ITCH_IO.txt");
        string contenido = 
@"================================================================================
GUÍA RÁPIDA: PUBLICAR 'HASTA ENCONTRARLES: EL CAMINO DE LA ESPERANZA' EN ITCH.IO
================================================================================

1. ACCEDER A ITCH.IO
   • Inicia sesión en https://itch.io/
   • En tu perfil (arriba a la derecha), haz clic en 'Create new project' o ve a https://itch.io/game/new

2. CONFIGURACIÓN BÁSICA DEL PROYECTO
   • Title: Hasta Encontrarles: El Camino de la Esperanza - Nivel 2
   • Short description / Tagline: Juego de plataformas y deducción histórica en memoria de los 22 migrantes de San Luis de la Paz, Guanajuato.
   • Classification: Games
   • Kind of project: HTML (You have a ZIP or HTML file that will play in the browser)

3. SUBIR EL ARCHIVO DEL JUEGO
   • En la sección 'Uploads', haz clic en 'Upload files'
   • Selecciona el archivo:
     Builds/WebGL/HastaEncontrarles_Nivel2_ItchIO.zip
   • Marca la casilla:
     [X] This file will be played in the browser

4. CONFIGURACIÓN DEL REPRODUCTOR (EMBED OPTIONS)
   • Viewport dimensions: 1280 x 720 (o 960 x 540)
   • Orientation: Auto detect / Landscape
   • Fullscreen button: [X] Enable fullscreen button
   • Automatically start on page load: [X] Enable

5. DETALLES Y VISIBILIDAD
   • Genre: Platformer / Adventure / Serious Game
   • Tags: 2D, Pixel Art, Story Rich, Platformer, Historical, Mexico, Serious Game
   • Community: Comments enabled
   • Visibility: Public (o Restricted / Draft para probarlo primero)

6. GUARDAR Y PROBAR
   • Haz clic en 'Save & View page'. ¡Tu juego estará funcionando en el navegador!
================================================================================";

        File.WriteAllText(rutaGuia, contenido, System.Text.Encoding.UTF8);
    }
}

