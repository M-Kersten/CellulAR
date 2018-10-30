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
        SphereManager.Instance.OnActivated += ResetAfterUse;
        button = GetComponent<Button>();
        button.onClick.AddListener(() => ActivatePowerUp());
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(() => ActivatePowerUp());
    }

    private void ActivatePowerUp()
    {
        buttonClock.color = Color.green;
        SphereManager.Instance.UnSelectAllSpheres();
        switch (powerUp)
        {
            case PowerUp.none:
                break;
            case PowerUp.speedUp:
                // speedup powerup hier toevoegen
                SphereManager.Instance.SpeedUp(5);
                SphereManager.Instance.ActivePowerUp = PowerUp.speedUp;
                break;
            case PowerUp.shield:
                SphereManager.Instance.ActivePowerUp = PowerUp.shield;
                break;
            case PowerUp.blind:                
                SphereManager.Instance.ActivePowerUp = PowerUp.blind;
                SphereManager.Instance.BlindOpponent();
                break;
        }
    }

    public void ResetPowerUp()
    {
        buttonClock.color = Color.white;
        PowerUpPreset POPreset = SphereManager.Instance.powerUpPresets.Where(item => item.button == button)
                    .FirstOrDefault();
        ResetButton(POPreset);

        GameSphere SphereReset = SphereManager.Instance.spheres.Where(item => item.sphereStats.PowerUpUI.PowerUp == powerUp)
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

    private void ResetAfterUse(PowerUp localPowerUp)
    {
        Debug.Log("checking if powerup is activepowerup");
        if (localPowerUp == powerUp)
        {
            Debug.Log("resetting powerup");
            ResetPowerUp();
        }
    }

}
