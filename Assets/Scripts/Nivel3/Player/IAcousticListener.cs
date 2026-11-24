using UnityEngine;

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Interfaz para detectar y recibir ondas de sonido en el entorno.
    /// Utilizada especialmente por el Guardián en la Zona 7 (Túneles).
    /// </summary>
    public interface IAcousticListener
    {
        void OnNoiseHeard(Vector3 noiseOrigin, float intensity);
    }
}

