using System;
using UnityEngine;
using LasHuellasDelSilencio.Player;

namespace LasHuellasDelSilencio.Interaction
{
    /// <summary>
    /// Rompecabezas de 3 símbolos de la Zona 7 (Sótano y túneles).
    /// El jugador debe alinear los 3 símbolos correspondientes a las marcas encontradas en las paredes.
    /// </summary>
    public class SymbolPuzzle : MonoBehaviour, IInteractable
    {
        [Header("Configuración del Puzzle")]
        [SerializeField] private string[] solutionSequence = new string[] { "Sol", "Espiga", "Agua" };
        [SerializeField] private string[] availableSymbols = new string[] { "Sol", "Luna", "Espiga", "Cruz", "Agua", "Ojo" };

        private int[] currentIndices = new int[3];
        private bool isSolved = false;

        [Header("Recompensa")]
        [SerializeField] private DoorMechanism tunnelDoorToUnlock;

        public bool IsSolved => isSolved;

        private void Start()
        {
            // Inicializar en estado no resuelto
            currentIndices[0] = 1;
            currentIndices[1] = 0;
            currentIndices[2] = 3;
        }

        public string GetInteractionPrompt()
        {
            if (isSolved) return "Mecanismo de símbolos (Resuelto).";
            return $"Presiona [E] para Manipular Mecanismo [{availableSymbols[currentIndices[0]]} | {availableSymbols[currentIndices[1]]} | {availableSymbols[currentIndices[2]]}]";
        }

        public bool CanInteract()
        {
            return !isSolved;
        }

        public void Interact(PlayerMovement2D player)
        {
            if (isSolved) return;

            // Ciclar símbolos como mecanismo interactivo
            currentIndices[0] = (currentIndices[0] + 1) % availableSymbols.Length;
            if (currentIndices[0] == 0)
            {
                currentIndices[1] = (currentIndices[1] + 1) % availableSymbols.Length;
                if (currentIndices[1] == 0)
                {
                    currentIndices[2] = (currentIndices[2] + 1) % availableSymbols.Length;
                }
            }

            CheckSolution();
        }

        private void CheckSolution()
        {
            bool match = true;
            for (int i = 0; i < 3; i++)
            {
                if (availableSymbols[currentIndices[i]] != solutionSequence[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                isSolved = true;
                if (UI.HUDManager.Instance != null)
                {
                    UI.HUDManager.Instance.ShowTemporaryNotification("¡Los tres símbolos encajan! La compuerta subterránea se destraba.");
                }

                if (tunnelDoorToUnlock != null)
                {
                    tunnelDoorToUnlock.UnlockAndOpen();
                }
            }
            else
            {
                if (UI.HUDManager.Instance != null)
                {
                    UI.HUDManager.Instance.ShowTemporaryNotification($"Símbolos: [{availableSymbols[currentIndices[0]]} - {availableSymbols[currentIndices[1]]} - {availableSymbols[currentIndices[2]]}]");
                }
            }
        }
    }
}

