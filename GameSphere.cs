using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
    private float timer;
    private float AITimer;

    private bool firstWinCheck = true;

    public bool AIEnabled;
    public float AIMoveMinSeconds, AIMoveMaxSeconds;

    private Material mat;
    
    void Awake()
    {
        ScoreManager.Instance.spheres.Add(this);
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

        DisablePowerUps();
        mat.color = ScoreManager.sphereColors[sphereStats.SphereOwner];
        ChangeColor(sphereStats.SphereOwner);
        ChangeScaleToPoints(sphereStats.Points);
        sphereStats.SecondPerPointgain = ScoreManager.Instance.gameData.secondPerPoint;
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
                ScoreManager.Instance.TransferPoints(this, ScoreManager.Instance.spheres[Random.Range(0, ScoreManager.Instance.spheres.Count)]);
                AITimer = Random.Range(AIMoveMinSeconds, AIMoveMaxSeconds);
            }
        }
    }

    private void CheckForPowerUp()
    {
        if (sphereStats.PowerUpUI.sphereClock.fillAmount < 1)
        {
            sphereStats.PowerUpUI.sphereClock.fillAmount += Time.deltaTime * ScoreManager.Instance.powerUpGainSpeed;
            if (sphereStats.SphereOwner == SphereOwner.player)
            {
                for (int i = 0; i < ScoreManager.Instance.powerUpPresets.Count; i++)
                {
                    if (ScoreManager.Instance.powerUpPresets[i].powerUp == sphereStats.PowerUpUI.PowerUp)
                    {
                        ScoreManager.Instance.powerUpPresets[i].buttonFill.fillAmount += Time.deltaTime * ScoreManager.Instance.powerUpGainSpeed;
                    }
                }
            }
        }
        else if (!sphereStats.PowerUpUI.PowerActive && sphereStats.SphereOwner == SphereOwner.player)
        {
            Debug.Log("setting powerup button active");
            sphereStats.PowerUpUI.PowerActive = true;
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
        Color tempColor = ScoreManager.sphereColors[owner];
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
        won = !ScoreManager.Instance.spheres.Any(item => item.sphereStats.SphereOwner == SphereOwner.opponent);
        if (firstWinCheck)
        {
            firstWinCheck = false;
            won = false;
        }
        if (won)
        {
            ScoreManager.Instance.GameWon();
        }
    }

    public void SelectSphere()
    {
        if (sphereStats.SphereOwner == SphereOwner.player)
        {
            string msg = sphereStats.Selected ? "Unselecting: " + gameObject.name : "Selecting: " + gameObject.name;
            Debug.Log(msg);

            if (sphereStats.Selected == true && ScoreManager.Instance.selectedSpheres.Count > 0)
            {
                foreach (GameSphere sphere in ScoreManager.Instance.selectedSpheres)
                {
                    ScoreManager.Instance.TransferPoints(sphere, this);
                }                 
                ScoreManager.Instance.UnSelectAllSpheres();
            }
            else
            {
                sphereStats.Selected = !sphereStats.Selected;
                if (sphereStats.Selected)
                {
                    ScoreManager.Instance.selectedSpheres.Add(this);
                }
                else
                {
                    ScoreManager.Instance.DeleteSelectedSphere(this);
                }
            }
        }
        else
        {
            if (!ScoreManager.Instance.playerSet)
            {
                ScoreManager.Instance.playerSet = true;
                sphereStats.SphereOwner = SphereOwner.player;
                sphereStats.Points = ScoreManager.Instance.gameData.startingPoints;
                ScoreManager.Instance.SetOpponent();
            }
            else
            {      
                foreach (GameSphere sphere in ScoreManager.Instance.selectedSpheres)
                {
                    ScoreManager.Instance.TransferPoints(sphere, this);
                }
                ScoreManager.Instance.UnSelectAllSpheres();
            }            
        }
    }

    public void ChangePointCounter(int points)
    {
        ScoreManager.Instance.DisplayScores(points);
    }

    public void SetPowerUpIcon()
    {
        if (sphereStats.PowerUpUI.PowerUp != PowerUp.none)
        {
            ScoreManager.Instance.powerUpPresets.Where(item => item.powerUp == sphereStats.PowerUpUI.PowerUp)
                .Where(item => sphereStats.PowerUpUI.icon.sprite = item.icon)
                .FirstOrDefault();
            sphereStats.PowerUpUI.powerUpObject.SetActive(true);
        }
    }

    private void DisablePowerUps()
    {
        for (int i = 0; i < ScoreManager.Instance.powerUpPresets.Count; i++)
        {
            ScoreManager.Instance.powerUpPresets[i].button.interactable = false;            
        }
    }

    private void SetPowerActive(bool active)
    {
        for (int i = 0; i < ScoreManager.Instance.powerUpPresets.Count; i++)
        {
            if (ScoreManager.Instance.powerUpPresets[i].powerUp == sphereStats.PowerUpUI.PowerUp)
            {
                ScoreManager.Instance.powerUpPresets[i].button.interactable = active;
            }
        }
    }
}
