using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Systems
{
    /// <summary>
    /// Punto de control (1: Patio completado, 2: Salida almacén, 3: Entrada a túneles).
    /// Guarda la posición para el reinicio respetuoso en caso de perder los tres corazones.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Identificación de Checkpoint")]
        [SerializeField] private int checkpointIndex = 1;
        [SerializeField] private string checkpointName = "Punto de Control 1 - Patio Superado";
        [SerializeField] private Transform spawnPoint;

        private bool isActivated = false;

        public int CheckpointIndex => checkpointIndex;
        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            if (other.TryGetComponent<PlayerMovement2D>(out var player))
            {
                isActivated = true;
                if (Level3GameManager.Instance != null)
                {
                    Level3GameManager.Instance.SetCheckpoint(checkpointIndex, SpawnPosition, checkpointName);
                }
            }
        }
    }
}

