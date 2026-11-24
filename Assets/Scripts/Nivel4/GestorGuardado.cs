using System;
using System.IO;
using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    [Serializable]
    public class DatosProgresoJuego
    {
        public bool juegoCompletado100 = false;
        public bool creditosDesbloqueados = false;
        public int nivelDesbloqueadoMaximo = 4;
        public int puntajeMaximoHistorico = 0;
        public float mejorTiempoNivel4 = 0f;
        public int panuelosColocadosHistorico = 0;
        public bool dictamenForenseObtenido = false;
        public float volumenMusica = 0.8f;
        public float volumenSFX = 0.9f;
        public int calidadGrafica = 2;
        public bool modoAutodidacta = true;
        public string fechaUltimoGuardado = "";
    }

    public static class GestorGuardado
    {
        private const string KEY_COMPLETADO_100 = "HE_JuegoCompletado100";
        private const string KEY_CREDITOS = "HE_CreditosDesbloqueados";
        private const string KEY_NIVEL_MAX = "HE_NivelDesbloqueadoMaximo";
        private const string KEY_PUNTAJE_MAX = "HE_PuntajeMaximoHistorico";
        private const string KEY_MEJOR_TIEMPO = "HE_MejorTiempoNivel4";
        private const string KEY_PANUELOS_HIST = "HE_PanuelosHistorico";
        private const string KEY_DICTAMEN = "HE_DictamenForense";
        private const string KEY_VOL_BGM = "HE_VolumenBGM";
        private const string KEY_VOL_SFX = "HE_VolumenSFX";
        private const string KEY_CALIDAD = "HE_CalidadGrafica";
        private const string KEY_AUTODIDACTA = "HE_ModoAutodidacta";

        private static string RutaArchivoJSON => Path.Combine(Application.persistentDataPath, "progreso_hasta_encontrarles.json");

        public static DatosProgresoJuego CargarProgreso()
        {
            DatosProgresoJuego datos = new DatosProgresoJuego();

            try
            {
                if (File.Exists(RutaArchivoJSON))
                {
                    string json = File.ReadAllText(RutaArchivoJSON);
                    DatosProgresoJuego desdeJson = JsonUtility.FromJson<DatosProgresoJuego>(json);
                    if (desdeJson != null)
                    {
                        datos = desdeJson;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Aviso al leer JSON de guardado: " + e.Message);
            }

            // Sincronizar con PlayerPrefs para doble respaldo
            if (PlayerPrefs.HasKey(KEY_COMPLETADO_100))
                datos.juegoCompletado100 = PlayerPrefs.GetInt(KEY_COMPLETADO_100, 0) == 1;
            if (PlayerPrefs.HasKey(KEY_CREDITOS))
                datos.creditosDesbloqueados = PlayerPrefs.GetInt(KEY_CREDITOS, 0) == 1;
            if (PlayerPrefs.HasKey(KEY_NIVEL_MAX))
                datos.nivelDesbloqueadoMaximo = Mathf.Max(datos.nivelDesbloqueadoMaximo, PlayerPrefs.GetInt(KEY_NIVEL_MAX, 4));
            if (PlayerPrefs.HasKey(KEY_PUNTAJE_MAX))
                datos.puntajeMaximoHistorico = Mathf.Max(datos.puntajeMaximoHistorico, PlayerPrefs.GetInt(KEY_PUNTAJE_MAX, 0));
            if (PlayerPrefs.HasKey(KEY_MEJOR_TIEMPO))
                datos.mejorTiempoNivel4 = PlayerPrefs.GetFloat(KEY_MEJOR_TIEMPO, 0f);
            if (PlayerPrefs.HasKey(KEY_PANUELOS_HIST))
                datos.panuelosColocadosHistorico = Mathf.Max(datos.panuelosColocadosHistorico, PlayerPrefs.GetInt(KEY_PANUELOS_HIST, 0));
            if (PlayerPrefs.HasKey(KEY_DICTAMEN))
                datos.dictamenForenseObtenido = PlayerPrefs.GetInt(KEY_DICTAMEN, 0) == 1;
            if (PlayerPrefs.HasKey(KEY_VOL_BGM))
                datos.volumenMusica = PlayerPrefs.GetFloat(KEY_VOL_BGM, 0.8f);
            if (PlayerPrefs.HasKey(KEY_VOL_SFX))
                datos.volumenSFX = PlayerPrefs.GetFloat(KEY_VOL_SFX, 0.9f);
            if (PlayerPrefs.HasKey(KEY_CALIDAD))
                datos.calidadGrafica = PlayerPrefs.GetInt(KEY_CALIDAD, 2);
            if (PlayerPrefs.HasKey(KEY_AUTODIDACTA))
                datos.modoAutodidacta = PlayerPrefs.GetInt(KEY_AUTODIDACTA, 1) == 1;

            return datos;
        }

        public static void GuardarProgreso(DatosProgresoJuego datos)
        {
            if (datos == null) return;
            datos.fechaUltimoGuardado = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            PlayerPrefs.SetInt(KEY_COMPLETADO_100, datos.juegoCompletado100 ? 1 : 0);
            PlayerPrefs.SetInt(KEY_CREDITOS, datos.creditosDesbloqueados ? 1 : 0);
            PlayerPrefs.SetInt(KEY_NIVEL_MAX, datos.nivelDesbloqueadoMaximo);
            PlayerPrefs.SetInt(KEY_PUNTAJE_MAX, datos.puntajeMaximoHistorico);
            PlayerPrefs.SetFloat(KEY_MEJOR_TIEMPO, datos.mejorTiempoNivel4);
            PlayerPrefs.SetInt(KEY_PANUELOS_HIST, datos.panuelosColocadosHistorico);
            PlayerPrefs.SetInt(KEY_DICTAMEN, datos.dictamenForenseObtenido ? 1 : 0);
            PlayerPrefs.SetFloat(KEY_VOL_BGM, datos.volumenMusica);
            PlayerPrefs.SetFloat(KEY_VOL_SFX, datos.volumenSFX);
            PlayerPrefs.SetInt(KEY_CALIDAD, datos.calidadGrafica);
            PlayerPrefs.SetInt(KEY_AUTODIDACTA, datos.modoAutodidacta ? 1 : 0);
            PlayerPrefs.Save();

            try
            {
                string json = JsonUtility.ToJson(datos, true);
                File.WriteAllText(RutaArchivoJSON, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Aviso al escribir JSON de guardado: " + e.Message);
            }
        }

        public static void RegistrarVictoriaNivel4(int puntajeObtenido, float tiempoSegundos, int panuelosColocados)
        {
            DatosProgresoJuego datos = CargarProgreso();
            datos.juegoCompletado100 = true;
            datos.creditosDesbloqueados = true;
            datos.dictamenForenseObtenido = true;
            datos.nivelDesbloqueadoMaximo = 4;
            if (puntajeObtenido > datos.puntajeMaximoHistorico)
                datos.puntajeMaximoHistorico = puntajeObtenido;
            if (datos.mejorTiempoNivel4 <= 0.01f || tiempoSegundos < datos.mejorTiempoNivel4)
                datos.mejorTiempoNivel4 = tiempoSegundos;
            if (panuelosColocados > datos.panuelosColocadosHistorico)
                datos.panuelosColocadosHistorico = panuelosColocados;

            GuardarProgreso(datos);
        }

        public static void GuardarOpciones(float volMusica, float volSfx, int calidad, bool autodidacta)
        {
            DatosProgresoJuego datos = CargarProgreso();
            datos.volumenMusica = Mathf.Clamp01(volMusica);
            datos.volumenSFX = Mathf.Clamp01(volSfx);
            datos.calidadGrafica = Mathf.Clamp(calidad, 0, 2);
            datos.modoAutodidacta = autodidacta;
            GuardarProgreso(datos);
        }
    }
}
