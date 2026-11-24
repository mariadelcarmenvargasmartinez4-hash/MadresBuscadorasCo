using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LasHuellasDelSilencio.Player
{
    /// <summary>
    /// Linterna con batería limitada según la Zona 5 (Almacén) y Zona 7 (Túneles).
    /// Controla la iluminación 2D, consumo de batería y parpadeo al agotarse.
    /// </summary>
    public class PlayerFlashlight : MonoBehaviour
    {
        [Header("Configuración de Linterna")]
        [SerializeField] private bool hasFlashlight = false;
        [SerializeField] private bool isOn = false;
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float currentBattery = 100f;
        [SerializeField] private float drainRatePerSecond = 2.0f; // Dura 50 segundos continuos

        [Header("Comportamiento con Poca Batería")]
        [SerializeField] private float lowBatteryThreshold = 20f;
        [SerializeField] private float flickerSpeed = 12f;

        [Header("Componentes de Luz")]
        [SerializeField] private GameObject flashlightObject;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleSfx;

        public bool HasFlashlight => hasFlashlight;
        public bool IsOn => isOn;
        public float CurrentBattery => currentBattery;
        public float MaxBattery => maxBattery;
        public float BatteryNormalized => Mathf.Clamp01(currentBattery / maxBattery);

        public event Action<float> OnBatteryChanged;
        public event Action<bool> OnFlashlightToggled;

        private void Start()
        {
            UpdateLightState();
            OnBatteryChanged?.Invoke(BatteryNormalized);
        }

        private void Update()
        {
            if (!hasFlashlight) return;

            // Leer tecla F para encender/apagar
            bool togglePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                togglePressed = true;
#else
            if (Input.GetKeyDown(KeyCode.F))
                togglePressed = true;
#endif

            if (togglePressed)
            {
                Toggle();
            }

            // Consumo de batería
            if (isOn && currentBattery > 0f)
            {
                currentBattery -= drainRatePerSecond * Time.deltaTime;
                if (currentBattery <= 0f)
                {
                    currentBattery = 0f;
                    SetFlashlightState(false);
                }
                OnBatteryChanged?.Invoke(BatteryNormalized);

                // Parpadeo con batería baja
                if (currentBattery <= lowBatteryThreshold && flashlightObject != null)
                {
                    bool flicker = Mathf.PingPong(Time.time * flickerSpeed, 1f) > 0.35f;
                    flashlightObject.SetActive(flicker);
                }
            }
        }

        public void AcquireFlashlight()
        {
            hasFlashlight = true;
            currentBattery = maxBattery;
            SetFlashlightState(true);
        }

        public void AddBattery(float amount)
        {
            currentBattery = Mathf.Min(maxBattery, currentBattery + amount);
            OnBatteryChanged?.Invoke(BatteryNormalized);
        }

        public void Toggle()
        {
            if (currentBattery <= 0f && !isOn) return;
            SetFlashlightState(!isOn);
        }

        public void SetFlashlightState(bool active)
        {
            isOn = active;
            UpdateLightState();
            if (toggleSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(toggleSfx);
            }
            OnFlashlightToggled?.Invoke(isOn);
        }

        private void UpdateLightState()
        {
            if (flashlightObject != null)
            {
                flashlightObject.SetActive(isOn && currentBattery > 0f);
            }
        }
    }
}

