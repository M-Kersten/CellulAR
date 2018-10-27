using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CellulAR;

public class PowerUpButton : MonoBehaviour
{
    public PowerUp powerUp;

    private Button button;
    public Image buttonClock;
 
    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => ActivatePowerUp());
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(() => ActivatePowerUp());
    }

    private void ActivatePowerUp()
    {
        PowerUpPreset POPreset = ScoreManager.Instance.powerUpPresets.Where(item => item.button == button)
            .FirstOrDefault();
        ResetButton(POPreset);

        GameSphere SphereReset = ScoreManager.Instance.spheres.Where(item => item.sphereStats.PowerUpUI.PowerUp == powerUp)
            .FirstOrDefault();
        ResetSphere(SphereReset);
    }

    private void ResetButton(PowerUpPreset powerUpPreset)
    {
        powerUpPreset.timer = 0;
        buttonClock.fillAmount = 0;
        button.interactable = false;
    }

    private void ResetSphere(GameSphere resetSphere)
    {
        resetSphere.sphereStats.PowerUpUI.sphereClock.fillAmount = 0;
        resetSphere.sphereStats.PowerUpUI.PowerActive = false;
    }

}
