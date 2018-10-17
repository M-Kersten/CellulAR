﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
}

public class GameSphere : MonoBehaviour {
        
    public SphereStats sphereStats;
    public Text pointText;
    private float timer;
    private float AITimer;

    private bool firstWinCheck = true;

    public bool AIEnabled;
    public float AIMoveMinSeconds, AIMoveMaxSeconds;

    private Material mat;

    private void Update()
    {
        mat.mainTextureOffset = new Vector2(Time.time * .1f, 0);
    }

    void Awake()
    {
        mat = GetComponent<Renderer>().material;
        ChangePointDisplay(sphereStats.Points);
        sphereStats.OnOwnerChanged += ChangeColor;
        sphereStats.OnOwnerChanged += ResetTimer;
        firstWinCheck = true;
        sphereStats.OnOwnerChanged += CheckWinner;
        sphereStats.OnPointsChanged += ChangePointDisplay;
        sphereStats.OnPointsChanged += ChangeGainRate;
        sphereStats.OnPointsChanged += ChangeScaleToPoints;
        sphereStats.OnPointsChanged += EventManager.instance.DisplayScores;
        sphereStats.OnSelected += SetToSelected;
        mat.color = EventManager.sphereColors[sphereStats.SphereOwner];
        ChangeColor(sphereStats.SphereOwner);
        ChangeScaleToPoints(sphereStats.Points);
        sphereStats.SecondPerPointgain = EventManager.instance.secondPerPoint;
        AITimer = 5;
    }

    void FixedUpdate()
    {
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
        }
        if (sphereStats.SphereOwner == SphereOwner.opponent && AIEnabled)
        {
            if (AITimer > 0)
            {
                AITimer -= Time.deltaTime;
            }
            else
            {
                EventManager.instance.TransferPoints(this, EventManager.instance.spheres[Random.Range(0, EventManager.instance.spheres.Count)]);
                AITimer = Random.Range(AIMoveMinSeconds, AIMoveMaxSeconds);
            }
        }
    }

    private void ResetTimer(SphereOwner owner)
    {
        timer = sphereStats.SecondPerPointgain;
        if (owner == SphereOwner.opponent)
        {
            AITimer = Random.Range(AIMoveMinSeconds, AIMoveMaxSeconds);
        }        
    }

    public void ChangeColor(SphereOwner owner)
    {
        Color tempColor = EventManager.sphereColors[owner];
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
        if (firstWinCheck)
        {
            firstWinCheck = false;
            won = false;
        }
        foreach (GameSphere sphere in EventManager.instance.spheres)
        {
            if (sphere.sphereStats.SphereOwner == SphereOwner.opponent)
            {
                won = false;
            }
        }
        if (won)
        {
            EventManager.instance.GameWon();
        }
    }

    public void SelectSphere()
    {
        if (sphereStats.SphereOwner == SphereOwner.player)
        {
            string msg = sphereStats.Selected ? "Unselecting: " + gameObject.name : "Selecting: " + gameObject.name;
            Debug.Log(msg);

            if (sphereStats.Selected == true && EventManager.instance.selectedSpheres.Count > 0)
            {
                if (EventManager.instance.particlePoints)
                {
                    foreach (GameSphere sphere in EventManager.instance.selectedSpheres)
                    {
                        EventManager.instance.TransferPoints(sphere, this);
                    }
                }
                else
                {
                    foreach (GameSphere sphere in EventManager.instance.selectedSpheres)
                    {
                        EventManager.instance.TransferPoints(sphere.sphereStats, sphereStats);
                    }
                }                            
                EventManager.instance.UnSelectAllSpheres();
            }
            else
            {
                sphereStats.Selected = !sphereStats.Selected;
                if (sphereStats.Selected)
                {
                    EventManager.instance.selectedSpheres.Add(this);
                }
                else
                {
                    EventManager.instance.DeleteSelectedSphere(this);
                }
            }
        }
        else
        {
            if (!EventManager.instance.playerSet)
            {
                EventManager.instance.playerSet = true;
                sphereStats.SphereOwner = SphereOwner.player;
                sphereStats.Points = EventManager.instance.startingPoints;
                EventManager.instance.SetOpponent();                
            }
            else
            {
                Debug.Log("selected playerSpheres: " + EventManager.instance.selectedSpheres.Count);
                if (EventManager.instance.particlePoints)
                {
                    foreach (GameSphere sphere in EventManager.instance.selectedSpheres)
                    {
                        EventManager.instance.TransferPoints(sphere, this);
                    }
                }
                else
                {
                    foreach (GameSphere sphere in EventManager.instance.selectedSpheres)
                    {
                        EventManager.instance.TransferPoints(sphere.sphereStats, sphereStats);
                    }
                }
                EventManager.instance.UnSelectAllSpheres();
            }            
        }
    }
    
    private void OnMouseDown()
    {
        SelectSphere();
    }
}