using UnityEngine;
using LasHuellasDelSilencio.Player;
using LasHuellasDelSilencio.Enemies;
using LasHuellasDelSilencio.UI;

namespace LasHuellasDelSilencio.Systems
{
    /// <summary>
    /// Zona privada de la hacienda que activa persecución policial y alerta en pantalla:
    /// "¡DEBES HUIR DEL POLICÍA O TE ARRESTARÁ!"
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PrivateZoneTrigger : MonoBehaviour
    {
        [Header("Configuración de Zona Privada")]
        [SerializeField] private PoliceOfficerAI policeOfficer;
        [SerializeField] private string warningMessage = "🚨 ¡ZONA PRIVADA! ¡DEBES HUIR DEL POLICÍA! SI TE ARRESTA, SE REINICIA EL NIVEL 🚨";

        private float alertCooldown = 0f;
        private bool officerWasAssigned = false;

        private void Start()
        {
            officerWasAssigned = policeOfficer != null;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (officerWasAssigned && (policeOfficer == null || policeOfficer.IsDead || !policeOfficer.gameObject.activeInHierarchy))
            {
                return;
            }

            if (policeOfficer != null)
            {
                officerWasAssigned = true;
            }

            if (other.CompareTag("Player") || other.GetComponent<PlayerMovement2D>() != null)
            {
                PlayerMovement2D player = other.GetComponent<PlayerMovement2D>();
                if (player != null && !player.IsHidden)
                {
                    alertCooldown -= Time.deltaTime;
                    if (alertCooldown <= 0f)
                    {
                        alertCooldown = 4.0f;

                        // Notificación en pantalla
                        HUDManager hud = UnityEngine.Object.FindAnyObjectByType<HUDManager>();
                        if (hud != null)
                        {
                            hud.ShowTemporaryNotification(warningMessage);
                        }

                        // Alertar a la policía
                        if (policeOfficer == null)
                        {
                            PoliceOfficerAI[] allOfficers = UnityEngine.Object.FindObjectsByType<PoliceOfficerAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                            float minDist = float.MaxValue;
                            foreach (var officer in allOfficers)
                            {
                                if (officer == null) continue;
                                float d = Vector2.Distance(transform.position, officer.transform.position);
                                if (d < minDist)
                                {
                                    minDist = d;
                                    policeOfficer = officer;
                                }
                            }
                        }

                        if (policeOfficer != null)
                        {
                            policeOfficer.AlertPoliceToPlayer(player.transform.position);
                        }
                    }
                }
            }
        }
    }
}


