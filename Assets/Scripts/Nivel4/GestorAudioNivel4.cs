using System;
using UnityEngine;

namespace HastaEncontrarles.Nivel4
{
    public class GestorAudioNivel4 : MonoBehaviour
    {
        public static GestorAudioNivel4 Instancia { get; private set; }

        [Header("Fuentes de Audio")]
        public AudioSource fuenteBGM;
        public AudioSource fuenteAmbiente;
        public AudioSource fuenteSFX;

        [Header("Clips Musicales (BGM)")]
        public AudioClip bgmMemorialEsperanza;
        public AudioClip bgmRutaDesierta;

        [Header("Clips de Efectos de Sonido (SFX)")]
        public AudioClip sfxDialogoBlip;
        public AudioClip sfxPasosTierra;
        public AudioClip sfxVarillaTierra;
        public AudioClip sfxPapelHojear;
        public AudioClip sfxVeladoraEncender;
        public AudioClip sfxPanueloBordar;
        public AudioClip sfxVientoPlaza;
        public AudioClip sfxAnimoRecuperar;
        public AudioClip sfxAlertaDesgaste;

        private float volumenBGM = 0.8f;
        private float volumenSFX = 0.9f;

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }
            Instancia = this;

            InicializarFuentes();
            CargarClipsYRespaldoProcedural();

            DatosProgresoJuego datos = GestorGuardado.CargarProgreso();
            ActualizarVolumenes(datos.volumenMusica, datos.volumenSFX);
        }

        private void InicializarFuentes()
        {
            if (fuenteBGM == null)
            {
                GameObject objBgm = new GameObject("AudioSource_BGM");
                objBgm.transform.SetParent(transform);
                fuenteBGM = objBgm.AddComponent<AudioSource>();
                fuenteBGM.loop = true;
                fuenteBGM.playOnAwake = false;
            }

            if (fuenteAmbiente == null)
            {
                GameObject objAmb = new GameObject("AudioSource_Ambiente");
                objAmb.transform.SetParent(transform);
                fuenteAmbiente = objAmb.AddComponent<AudioSource>();
                fuenteAmbiente.loop = true;
                fuenteAmbiente.playOnAwake = false;
            }

            if (fuenteSFX == null)
            {
                GameObject objSfx = new GameObject("AudioSource_SFX");
                objSfx.transform.SetParent(transform);
                fuenteSFX = objSfx.AddComponent<AudioSource>();
                fuenteSFX.loop = false;
                fuenteSFX.playOnAwake = false;
            }
        }

        private void CargarClipsYRespaldoProcedural()
        {
            // Intentar cargar desde Resources/Audio primero
            if (bgmMemorialEsperanza == null) bgmMemorialEsperanza = Resources.Load<AudioClip>("Audio/bgm_memorial_esperanza");
            if (bgmRutaDesierta == null) bgmRutaDesierta = Resources.Load<AudioClip>("Audio/bgm_ruta_desierta");
            if (sfxDialogoBlip == null) sfxDialogoBlip = Resources.Load<AudioClip>("Audio/sfx_dialogo_blip");
            if (sfxPasosTierra == null) sfxPasosTierra = Resources.Load<AudioClip>("Audio/sfx_pasos_tierra");
            if (sfxVarillaTierra == null) sfxVarillaTierra = Resources.Load<AudioClip>("Audio/sfx_varilla_tierra");
            if (sfxPapelHojear == null) sfxPapelHojear = Resources.Load<AudioClip>("Audio/sfx_papel_hojear");
            if (sfxVeladoraEncender == null) sfxVeladoraEncender = Resources.Load<AudioClip>("Audio/sfx_veladora_encender");
            if (sfxPanueloBordar == null) sfxPanueloBordar = Resources.Load<AudioClip>("Audio/sfx_panuelo_bordar");
            if (sfxVientoPlaza == null) sfxVientoPlaza = Resources.Load<AudioClip>("Audio/sfx_viento_plaza");
            if (sfxAnimoRecuperar == null) sfxAnimoRecuperar = Resources.Load<AudioClip>("Audio/sfx_animo_recuperar");
            if (sfxAlertaDesgaste == null) sfxAlertaDesgaste = Resources.Load<AudioClip>("Audio/sfx_alerta_desgaste");

            // Respaldo sintético garantizado si algún clip aún no se ha importado
            if (bgmMemorialEsperanza == null) bgmMemorialEsperanza = SintetizarBGMMemorial();
            if (bgmRutaDesierta == null) bgmRutaDesierta = SintetizarBGMRutaDesierta();
            if (sfxDialogoBlip == null) sfxDialogoBlip = SintetizarBlipDialogo();
            if (sfxPasosTierra == null) sfxPasosTierra = SintetizarPasoTierra();
            if (sfxVarillaTierra == null) sfxVarillaTierra = SintetizarVarillaTierra();
            if (sfxPapelHojear == null) sfxPapelHojear = SintetizarPapelHojear();
            if (sfxVeladoraEncender == null) sfxVeladoraEncender = SintetizarVeladora();
            if (sfxPanueloBordar == null) sfxPanueloBordar = SintetizarBordado();
            if (sfxVientoPlaza == null) sfxVientoPlaza = SintetizarVientoPlaza();
            if (sfxAnimoRecuperar == null) sfxAnimoRecuperar = SintetizarSolidaridad();
            if (sfxAlertaDesgaste == null) sfxAlertaDesgaste = SintetizarAlerta();
        }

        public void ActualizarVolumenes(float nuevoVolBgm, float nuevoVolSfx)
        {
            volumenBGM = Mathf.Clamp01(nuevoVolBgm);
            volumenSFX = Mathf.Clamp01(nuevoVolSfx);

            if (fuenteBGM != null) fuenteBGM.volume = volumenBGM * 0.65f;
            if (fuenteAmbiente != null) fuenteAmbiente.volume = volumenSFX * 0.32f;
            if (fuenteSFX != null) fuenteSFX.volume = volumenSFX;
        }

        public void IniciarAudioNivel4()
        {
            if (fuenteBGM != null && bgmMemorialEsperanza != null)
            {
                if (fuenteBGM.clip != bgmMemorialEsperanza || !fuenteBGM.isPlaying)
                {
                    fuenteBGM.clip = bgmMemorialEsperanza;
                    fuenteBGM.loop = true;
                    fuenteBGM.Play();
                }
            }

            if (fuenteAmbiente != null && sfxVientoPlaza != null && !fuenteAmbiente.isPlaying)
            {
                fuenteAmbiente.clip = sfxVientoPlaza;
                fuenteAmbiente.loop = true;
                fuenteAmbiente.Play();
            }
        }

        public void ReproducirBGMMenu()
        {
            if (fuenteBGM != null && bgmRutaDesierta != null)
            {
                if (fuenteBGM.clip != bgmRutaDesierta || !fuenteBGM.isPlaying)
                {
                    fuenteBGM.clip = bgmRutaDesierta;
                    fuenteBGM.loop = true;
                    fuenteBGM.Play();
                }
            }
        }

        public void ReproducirBlipDialogo() => ReproducirSFX(sfxDialogoBlip, 0.55f);
        public void ReproducirPaso() => ReproducirSFX(sfxPasosTierra, 0.45f);
        public void ReproducirVarilla() => ReproducirSFX(sfxVarillaTierra, 0.85f);
        public void ReproducirPapel() => ReproducirSFX(sfxPapelHojear, 0.75f);
        public void ReproducirVeladora() => ReproducirSFX(sfxVeladoraEncender, 0.9f);
        public void ReproducirBordado() => ReproducirSFX(sfxPanueloBordar, 0.85f);
        public void ReproducirSolidaridad() => ReproducirSFX(sfxAnimoRecuperar, 0.85f);
        public void ReproducirAlerta() => ReproducirSFX(sfxAlertaDesgaste, 0.75f);

        private void ReproducirSFX(AudioClip clip, float escala)
        {
            if (fuenteSFX != null && clip != null)
            {
                fuenteSFX.PlayOneShot(clip, volumenSFX * escala);
            }
        }

        private AudioClip SintetizarBGMMemorial()
        {
            int sr = 22050;
            float duracion = 12f;
            int muestras = Mathf.RoundToInt(sr * duracion);
            float[] datos = new float[muestras];
            // Acordes acústicos emotivos: Am - F - C - G
            float[][] acordes = new float[][]
            {
                new float[] { 220.0f, 261.63f, 329.63f, 440.0f },
                new float[] { 174.61f, 220.0f, 261.63f, 349.23f },
                new float[] { 261.63f, 329.63f, 392.0f, 523.25f },
                new float[] { 196.0f, 246.94f, 293.66f, 392.0f }
            };

            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                int idxAcorde = Mathf.FloorToInt(t / 3f) % 4;
                float tLocal = t % 3f;
                int idxNota = Mathf.FloorToInt((tLocal / 3f) * 8f) % 4;
                float tNota = (tLocal * 8f / 3f) % 1f;

                float freq = acordes[idxAcorde][idxNota];
                float envNota = Mathf.Exp(-tNota * 3.2f);
                float guitarra = (Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.Sin(4f * Mathf.PI * freq * t)) * envNota * 0.25f;

                float padViolonchelo = Mathf.Sin(2f * Mathf.PI * (acordes[idxAcorde][0] * 0.5f) * t) * 0.14f;
                datos[i] = Mathf.Clamp(guitarra + padViolonchelo, -0.9f, 0.9f);
            }

            AudioClip clip = AudioClip.Create("bgm_memorial_esperanza", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarBGMRutaDesierta()
        {
            int sr = 22050;
            int muestras = sr * 8;
            float[] datos = new float[muestras];
            float[] notas = { 164.81f, 196.0f, 246.94f, 220.0f };
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                int idx = Mathf.FloorToInt(t / 2f) % 4;
                float tLocal = t % 2f;
                float env = Mathf.Exp(-tLocal * 2.2f);
                datos[i] = Mathf.Sin(2f * Mathf.PI * notas[idx] * t) * env * 0.28f;
            }
            AudioClip clip = AudioClip.Create("bgm_ruta_desierta", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarBlipDialogo()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.055f);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float env = 1f - ((float)i / muestras);
                datos[i] = Mathf.Sin(2f * Mathf.PI * 540f * t) * env * 0.35f;
            }
            AudioClip clip = AudioClip.Create("sfx_dialogo_blip", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarPasoTierra()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.09f);
            float[] datos = new float[muestras];
            System.Random rnd = new System.Random(42);
            for (int i = 0; i < muestras; i++)
            {
                float env = Mathf.Exp(-((float)i / muestras) * 5f);
                float ruido = ((float)rnd.NextDouble() * 2f - 1f) * 0.35f;
                datos[i] = ruido * env;
            }
            AudioClip clip = AudioClip.Create("sfx_pasos_tierra", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarVarillaTierra()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.28f);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * 12f);
                datos[i] = (Mathf.Sin(2f * Mathf.PI * 310f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 780f * t)) * env * 0.45f;
            }
            AudioClip clip = AudioClip.Create("sfx_varilla_tierra", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarPapelHojear()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.16f);
            float[] datos = new float[muestras];
            System.Random rnd = new System.Random(101);
            for (int i = 0; i < muestras; i++)
            {
                float u = (float)i / muestras;
                float env = Mathf.Sin(u * Mathf.PI);
                datos[i] = ((float)rnd.NextDouble() * 2f - 1f) * env * 0.28f;
            }
            AudioClip clip = AudioClip.Create("sfx_papel_hojear", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarVeladora()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.55f);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * 4.5f);
                datos[i] = (Mathf.Sin(2f * Mathf.PI * 523.25f * t) + 0.6f * Mathf.Sin(2f * Mathf.PI * 659.25f * t) + 0.4f * Mathf.Sin(2f * Mathf.PI * 783.99f * t)) * env * 0.3f;
            }
            AudioClip clip = AudioClip.Create("sfx_veladora_encender", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarBordado()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.32f);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * 7f);
                float freq = t < 0.12f ? 440f : 587.33f;
                datos[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.38f;
            }
            AudioClip clip = AudioClip.Create("sfx_panuelo_bordar", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarVientoPlaza()
        {
            int sr = 22050;
            int muestras = sr * 6;
            float[] datos = new float[muestras];
            System.Random rnd = new System.Random(77);
            float filtro = 0f;
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float blanco = (float)rnd.NextDouble() * 2f - 1f;
                filtro = filtro * 0.96f + blanco * 0.04f;
                float mod = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.33f * t);
                datos[i] = filtro * mod * 0.35f;
            }
            AudioClip clip = AudioClip.Create("sfx_viento_plaza", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarSolidaridad()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.45f);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * 5f);
                float freq = t < 0.15f ? 392f : (t < 0.3f ? 493.88f : 587.33f);
                datos[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.35f;
            }
            AudioClip clip = AudioClip.Create("sfx_animo_recuperar", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }

        private AudioClip SintetizarAlerta()
        {
            int sr = 22050;
            int muestras = Mathf.RoundToInt(sr * 0.35f);
            float[] datos = new float[muestras];
            for (int i = 0; i < muestras; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-t * 6f);
                datos[i] = Mathf.Sin(2f * Mathf.PI * 165f * t) * env * 0.4f;
            }
            AudioClip clip = AudioClip.Create("sfx_alerta_desgaste", muestras, 1, sr, false);
            clip.SetData(datos, 0);
            return clip;
        }
    }
}
