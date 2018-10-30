using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using CellulAR;

[Serializable]
public enum SphereOwner
{
    none,
    player,
    opponent
}

[Serializable]
public class SphereStats
{
    public delegate void OwnerChanged(SphereOwner owner);
    public OwnerChanged OnOwnerChanged;

    [SerializeField]
    private float secondPerPointgain;
    public float SecondPerPointgain
    {
        get
        {
            return secondPerPointgain;
        }
        set
        {
            secondPerPointgain = value;
        }
    }

    [SerializeField]
    private SphereOwner sphereOwner;
    public SphereOwner SphereOwner {
        get {
            return sphereOwner;
        }
        set {
            sphereOwner = value;
            OnOwnerChanged(value);            
        }
    }

    public delegate void PointsChanged(int i);
    public PointsChanged OnPointsChanged;
    
    [SerializeField]
    private int _points;
    public int Points {
        get {
            return _points;
        }
        set {
            _points = value;
            OnPointsChanged(value);            
        }
    }

    public delegate void Selecting(bool select);
    public Selecting OnSelected;

    [SerializeField]
    private bool selected;
    public bool Selected {
        get {
            return selected;
        }
        set {
            selected = value;
            OnSelected(value);            
        }
    }
        
    [SerializeField]
    private PowerUpUI powerUpUI;
    public PowerUpUI PowerUpUI
    {
        get { return powerUpUI;  }
        set { powerUpUI = value; }
    }
}
/// <summary>
/// Class handling a sphere in the playfield
/// </summary>
public class GameSphere : MonoBehaviour {
        
    public SphereStats sphereStats;
    public Text pointText;
    public GameObject shield;
    private float timer;
    private float AITimer;

    private bool firstWinCheck = true;

    public bool AIEnabled;
    public float AIMoveMinSeconds, AIMoveMaxSeconds;

    private Material mat;

    private static GameSphere copyingFrom;
    private static FieldInfo[] fields;

    [ContextMenu("Copy values")]
    public void CopyWithReflection()
    {
        copyingFrom = this;
        Type sphereType = typeof(GameSphere);
        FieldInfo[] sphereFields = sphereType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        fields = sphereFields;
    }

    [ContextMenu("Paste values")]
    public void PasteWithReflection()
    {
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(copyingFrom);
            field.SetValue(this, value);
        }
    }

    void Awake()
    {
        SphereManager.Instance.spheres.Add(this);
        mat = GetComponent<Renderer>().material;
        ChangePointDisplay(sphereStats.Points);
        firstWinCheck = true;

        sphereStats.OnOwnerChanged += ChangeColor;
        sphereStats.OnOwnerChanged += ResetTimer;        
        sphereStats.OnOwnerChanged += CheckWinner;

        sphereStats.OnPointsChanged += ChangeGainRate;
        sphereStats.OnPointsChanged += ChangeScaleToPoints;
        sphereStats.OnPointsChanged += ChangePointDisplay;
        sphereStats.OnPointsChanged += ChangePointCounter;

        sphereStats.OnSelected += SetToSelected;

        sphereStats.PowerUpUI.OnPowerUpSet += SetPowerUpIcon;
        sphereStats.PowerUpUI.OnPowerActivated += SetPowerActive;
        SetPowerUpIcon();

        DisablePowerUps();
        mat.color = SphereManager.sphereColors[sphereStats.SphereOwner];
        ChangeColor(sphereStats.SphereOwner);
        ChangeScaleToPoints(sphereStats.Points);
        sphereStats.SecondPerPointgain = SphereManager.Instance.gameData.secondPerPoint;
        AITimer = 5;
    }

    private void OnMouseDown()
    {
        SelectSphere();
    }

    void FixedUpdate()
    {
        mat.mainTextureOffset = new Vector2(Time.time * .1f, 0);

        if (sphereStats.SphereOwner == SphereOwner.player || sphereStats.SphereOwner == SphereOwner.opponent)
        {
            if (timer > 0 && sphereStats.SecondPerPointgain < 4)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                sphereStats.Points++;                
                timer = sphereStats.SecondPerPointgain;                               
            }
            if (sphereStats.PowerUpUI.PowerUp != PowerUp.none)
            {
                CheckForPowerUp();
            }
        }
        if (sphereStats.SphereOwner == SphereOwner.opponent && AIEnabled)
        {
            if (AITimer > 0)
            {
                AITimer -= Time.deltaTime;
            }
            else
            {
                SphereManager.Instance.TransferPoints(this, SphereManager.Instance.spheres[Random.Range(0, SphereManager.Instance.spheres.Count)]);
                AITimer = Random.Range(AIMoveMinSeconds, AIMoveMaxSeconds);
            }
        }
    }

    private void CheckForPowerUp()
    {
        if (sphereStats.PowerUpUI.sphereClock.fillAmount < 1)
        {
            sphereStats.PowerUpUI.sphereClock.fillAmount += Time.deltaTime * SphereManager.Instance.powerUpGainSpeed;
            if (sphereStats.SphereOwner == SphereOwner.player)
            {
                PowerUpPreset preset = SphereManager.Instance.powerUpPresets.Where(item => item.powerUp == sphereStats.PowerUpUI.PowerUp)
                    .FirstOrDefault();
                preset.buttonFill.fillAmount += Time.deltaTime * SphereManager.Instance.powerUpGainSpeed;
            }
        }
        else if (!sphereStats.PowerUpUI.PowerActive && sphereStats.SphereOwner == SphereOwner.player)
        {
            sphereStats.PowerUpUI.PowerActive = true;
            PowerUpPreset preset = SphereManager.Instance.powerUpPresets.Where(item => item.powerUp == sphereStats.PowerUpUI.PowerUp)
                    .FirstOrDefault();
            preset.buttonFill.color = Color.blue;
            Debug.Log("preset button is: " + preset.buttonFill.gameObject.name);
        }
    }

    private void ResetTimer(SphereOwner owner)
    {
        timer = sphereStats.SecondPerPointgain;
        if (owner == SphereOwner.opponent)
        {
            AITimer = Random.Range(AIMoveMinSeconds, AIMoveMaxSeconds);
        }
        sphereStats.PowerUpUI.sphereClock.fillAmount = 0;
    }

    public void ChangeColor(SphereOwner owner)
    {
        Color tempColor = SphereManager.sphereColors[owner];
        mat.color = tempColor;
    }

    public void ChangePointDisplay(int points)
    {
        pointText.text = points.ToString();        
    }

    public void ChangeGainRate(int points)
    {
        sphereStats.SecondPerPointgain = .3f + (points * .02f);
    }

    public void ChangeScaleToPoints(int points)
    {
        float scale = 1.35f + (points * .02f);
        if (scale > 2.5f)
        {
            scale = 2.5f;
        }
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetToSelected(bool selected)
    {
        Color tempColor;
        tempColor = selected ? Color.gray : Color.black;
        mat.SetColor("_EmissionColor", tempColor);
    }

    public void CheckWinner(SphereOwner owner)
    {
        bool won = true;
        // check if opponent has any spheres left
        won = !SphereManager.Instance.spheres.Any(item => item.sphereStats.SphereOwner == SphereOwner.opponent);
        if (firstWinCheck)
        {
            firstWinCheck = false;
            won = false;
        }
        if (won)
        {
            SphereManager.Instance.GameWon();
        }
    }

    public void SelectSphere()
    {
        if (sphereStats.SphereOwner == SphereOwner.player)
        {
            string msg = sphereStats.Selected ? "Unselecting: " + gameObject.name : "Selecting: " + gameObject.name;
            Debug.Log(msg);

            if (SphereManager.Instance.ActivePowerUp == PowerUp.shield)
            {
                ActivatePowerUp();
            }
            else
            {
                if (sphereStats.Selected == true && SphereManager.Instance.selectedSpheres.Count > 0)
                {
                    SphereManager.Instance.selectedSpheres.ForEach(item => SphereManager.Instance.TransferPoints(item, this));
                    SphereManager.Instance.UnSelectAllSpheres();
                }
                else
                {
                    sphereStats.Selected = !sphereStats.Selected;
                    if (sphereStats.Selected)
                    {
                        SphereManager.Instance.selectedSpheres.Add(this);
                    }
                    else
                    {
                        SphereManager.Instance.DeleteSelectedSphere(this);
                    }
                }
            }
        }
        else
        {
            if (!SphereManager.Instance.playerSet)
            {
                SphereManager.Instance.playerSet = true;
                sphereStats.SphereOwner = SphereOwner.player;
                sphereStats.Points = SphereManager.Instance.gameData.startingPoints;
                SphereManager.Instance.SetOpponent();
            }
            else
            {      
                foreach (GameSphere sphere in SphereManager.Instance.selectedSpheres)
                {
                    SphereManager.Instance.TransferPoints(sphere, this);
                }
                SphereManager.Instance.UnSelectAllSpheres();
            }            
        }
    }

    public void ChangePointCounter(int points)
    {
        SphereManager.Instance.DisplayScores(points);
    }

    public void SetPowerUpIcon()
    {
        if (sphereStats.PowerUpUI.PowerUp != PowerUp.none)
        {
            SphereManager.Instance.powerUpPresets.Where(item => item.powerUp == sphereStats.PowerUpUI.PowerUp)
                .Where(item => sphereStats.PowerUpUI.icon.sprite = item.icon)
                .FirstOrDefault();
            sphereStats.PowerUpUI.powerUpObject.SetActive(true);
        }
    }

    private void DisablePowerUps()
    {
        for (int i = 0; i < SphereManager.Instance.powerUpPresets.Count; i++)
        {
            SphereManager.Instance.powerUpPresets[i].button.interactable = false;            
        }
    }

    private void SetPowerActive(bool active)
    {
        PowerUpPreset powerUpPreset = SphereManager.Instance.powerUpPresets.Where(item => item.powerUp == sphereStats.PowerUpUI.PowerUp)
            .FirstOrDefault();
        powerUpPreset.button.interactable = active;        
    }

    private void ActivatePowerUp()
    {
        switch (SphereManager.Instance.ActivePowerUp)
        {
            case PowerUp.none:
                break;
            case PowerUp.speedUp:
                break;
            case PowerUp.shield:
                shield.SetActive(true);
                break;
            case PowerUp.reinforcements:
                break;
            case PowerUp.blind:
                break;
            default:
                break;
        }
        SphereManager.Instance.UnSelectAllSpheres();
        if (SphereManager.Instance.ActivePowerUp == PowerUp.shield)
        {
            SphereManager.Instance.ActivePowerUp = PowerUp.none;
        }
        
    }
}
