using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public enum PowerUp
{
    none,
    speedUp,
    shield,
    reinforcements
}

[Serializable]
public class PowerUpUI
{
    public delegate void PowerUpSetting();
    public PowerUpSetting OnPowerUpSet;
    [SerializeField]
    private PowerUp powerUp;
    public PowerUp PowerUp
    {
        get
        {
            return powerUp;
        }
        set
        {
            powerUp = value;
            OnPowerUpSet();
        }
    }
    public GameObject powerUpObject;
    public Image icon;
    public Image sphereClock;

    public delegate void PowerActivated(bool active);
    public PowerActivated OnPowerActivated;
    [SerializeField]
    private bool powerActive;
    public bool PowerActive
    {
        get
        {
            return powerActive;
        }
        set
        {
            powerActive = value;
            OnPowerActivated(value);
        }
    }    
}


[Serializable]
public class PowerUpPreset
{
    public PowerUp powerUp;
    public Sprite icon;
    public float timer;
    public Image buttonFill;
    public Button button;
}

public class PowerUps : MonoBehaviour {
   
}
