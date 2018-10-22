using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour
{

    private Button button;

    public PowerUp powerUp;

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
        for (int i = 0; i < EventManager.instance.powerUpPresets.Count; i++)
        {
            if (EventManager.instance.powerUpPresets[i].button == button)
            {
                button.enabled = false;
            }
        }
    }
}
