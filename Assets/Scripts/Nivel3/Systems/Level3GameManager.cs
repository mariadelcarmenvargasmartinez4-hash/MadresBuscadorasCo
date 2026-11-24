using System;
using System.Collections.Generic;
using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Enemies;

namespace LasHuellasDelSilencio.Systems
{
    public enum LevelZone
    {
        Entrada = 1,
        PatioPrincipal = 2,
        CasaAbandonada = 3,
        Caballerizas = 4,
        Almacen = 5,
        CampoDeBusqueda = 6,
        SotanoYTuneles = 7,
        ZonaDeHallazgo = 8
    }

    /// <summary>
    /// Administrador maestro del Nivel 3 "Las huellas del silencio".
    /// Controla la progresión de las 8 zonas, objetivos, inventario,
    /// registro de indicios/documentos, puntos de control y escena final narrativa.
    /// </summary>
    public class Level3GameManager : MonoBehaviour
    {
        public static Level3GameManager Instance { get; private set; }

        [Header("Zona Actual")]
        [SerializeField] private LevelZone currentZone = LevelZone.Entrada;

        [Header("Puntos de Control")]
        [SerializeField] private Vector3 initialSpawnPoint = new Vector3(-8f, -0.8f, 0f);
        private Vector3 currentCheckpointPos;
        private int lastCheckpointIndex = 0;

        [Header("Contadores de Misión")]
        [SerializeField] private int totalEvidenceRequired = 10;
        [SerializeField] private int totalDocumentsRequired = 5;
        private HashSet<string> registeredEvidence = new HashSet<string>();
        private HashSet<string> collectedDocuments = new HashSet<string>();
        private HashSet<string> inventoryItems = new HashSet<string>();
        private int enemiesDefeated = 0;
        private bool lauraFound = false;

        [Header("Referencias a Jugador")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerMovement2D playerMovement;

        public LevelZone CurrentZone => currentZone;
        public int CluesCount => registeredEvidence.Count;
        public int TotalClues => totalEvidenceRequired;
        public int DocumentsCount => collectedDocuments.Count;
        public int TotalDocuments => totalDocumentsRequired;
        public int EnemiesDefeated => enemiesDefeated;
        public bool LauraFound => lauraFound;

        public event Action<string> OnObjectiveChanged;
        public event Action<int, int> OnCluesUpdated;
        public event Action<int, int> OnDocumentsUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentCheckpointPos = initialSpawnPoint;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (playerHealth == null) playerHealth = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
            if (playerMovement == null) playerMovement = UnityEngine.Object.FindAnyObjectByType<PlayerMovement2D>();

            SetZone(LevelZone.Entrada);
            OnCluesUpdated?.Invoke(CluesCount, totalEvidenceRequired);
            OnDocumentsUpdated?.Invoke(DocumentsCount, totalDocumentsRequired);
        }

        public void SetZone(LevelZone zone)
        {
            currentZone = zone;
            string objective = GetObjectiveForZone(zone);
            OnObjectiveChanged?.Invoke(objective);

            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.UpdateObjective(objective);
                UI.HUDManager.Instance.ShowTemporaryNotification($"Zona {((int)zone)}: {zone}");
            }
        }

        public string GetObjectiveForZone(LevelZone zone)
        {
            return zone switch
            {
                LevelZone.Entrada => "Inspecciona la caseta de vigilancia, recoge la llave y abre la reja principal.",
                LevelZone.PatioPrincipal => "Cruza el patio principal con sigilo, esquiva la patrulla del vigilante y busca notas.",
                LevelZone.CasaAbandonada => "Registra las habitaciones de la casa y reúne los 3 fragmentos de la nota.",
                LevelZone.Caballerizas => "Localiza la herramienta para destrabar la puerta y elude al perseguidor veloz.",
                LevelZone.Almacen => "Recupera la linterna y los documentos del almacén; huye por la ventana lateral.",
                LevelZone.CampoDeBusqueda => "Explora el terreno exterior, documenta los indicios y coloca los marcadores sin perturbar el suelo.",
                LevelZone.SotanoYTuneles => "Avanza en silencio absoluto con sigilo acústico para no alertar al Guardián y resuelve el mecanismo de símbolos.",
                LevelZone.ZonaDeHallazgo => lauraFound
                    ? "Has encontrado a Laura bajo el Árbol Viejo. Acordona la escena pericial [E] para los peritos."
                    : "Has llegado a la Zona 8. Busca el Árbol Viejo, desentierra a Laura [E] y asegura la escena.",
                _ => "Explora la hacienda y documenta los indicios."
            };
        }

        public void RegisterEvidence(string evidenceId, bool isFinalDiscovery)
        {
            if (!registeredEvidence.Contains(evidenceId))
            {
                registeredEvidence.Add(evidenceId);
                OnCluesUpdated?.Invoke(CluesCount, totalEvidenceRequired);

                if (UI.HUDManager.Instance != null)
                {
                    UI.HUDManager.Instance.UpdateClues(CluesCount, totalEvidenceRequired);
                    UI.HUDManager.Instance.ShowTemporaryNotification($"Indicio documentado ({CluesCount}/{totalEvidenceRequired})");
                }

                if (isFinalDiscovery)
                {
                    TriggerFinalNarrativeScene();
                }
            }
        }

        public void RegisterDocument(string documentId)
        {
            if (!collectedDocuments.Contains(documentId))
            {
                collectedDocuments.Add(documentId);
                OnDocumentsUpdated?.Invoke(DocumentsCount, totalDocumentsRequired);

                if (UI.HUDManager.Instance != null)
                {
                    UI.HUDManager.Instance.UpdateDocuments(DocumentsCount, totalDocumentsRequired);
                    UI.HUDManager.Instance.ShowTemporaryNotification($"Documento archivado ({DocumentsCount}/{totalDocumentsRequired})");
                }
            }
        }

        public void AddInventoryItem(string itemName)
        {
            inventoryItems.Add(itemName);
        }

        public bool HasItemOrRequirement(string itemName)
        {
            return inventoryItems.Contains(itemName);
        }

        public void RegisterEnemyDefeated(string enemyName)
        {
            enemiesDefeated++;
            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.UpdateEnemiesDefeated(enemiesDefeated);
                UI.HUDManager.Instance.ShowTemporaryNotification($"⚔️ ¡{enemyName} derrotado! (Enemigos abatidos: {enemiesDefeated})");
            }

            // Comprobar si todos los enemigos y patrullas de la hacienda han sido derrotados
            BaseEnemyAI[] remainingEnemies = UnityEngine.Object.FindObjectsByType<BaseEnemyAI>(FindObjectsSortMode.None);
            int aliveCount = 0;
            foreach (var e in remainingEnemies)
            {
                if (e != null && !e.IsDead) aliveCount++;
            }

            if (aliveCount <= 0)
            {
                if (UI.HUDManager.Instance != null)
                {
                    UI.HUDManager.Instance.ShowVictoryAnnouncement("🏆 ¡HAS GANADO! 🏆\n¡Todos los enemigos de la hacienda han sido derrotados!");
                }
            }
        }

        public void MarkLauraFound()
        {
            lauraFound = true;
            string nextObjective = "Has encontrado a Laura bajo el Árbol Viejo. Acordona la escena pericial [E].";
            OnObjectiveChanged?.Invoke(nextObjective);
            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.UpdateObjective(nextObjective);
            }
        }

        public void SetCheckpoint(int index, Vector3 position, string name)
        {
            if (index > lastCheckpointIndex)
            {
                lastCheckpointIndex = index;
                currentCheckpointPos = position;

                if (UI.HUDManager.Instance != null)
                {
                    UI.HUDManager.Instance.ShowTemporaryNotification($"Punto de control guardado: {name}");
                }
            }
        }

        public void OnPlayerDefeated()
        {
            if (UI.HUDManager.Instance != null)
            {
                UI.HUDManager.Instance.ShowGameOverModal(() => RespawnPlayer());
            }
            else
            {
                RespawnPlayer();
            }
        }

        public void RespawnPlayer()
        {
            if (playerHealth != null)
            {
                playerHealth.RespawnAt(currentCheckpointPos);
            }
        }

        private void TriggerFinalNarrativeScene()
        {
            if (playerMovement != null)
            {
                playerMovement.SetMovementLocked(true);
            }

            if (UI.NarrativeEndingUI.Instance != null)
            {
                UI.NarrativeEndingUI.Instance.StartEndingSequence();
            }
        }
    }
}


