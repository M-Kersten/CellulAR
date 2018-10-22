using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EventManager : MonoBehaviour {

    public static Dictionary<SphereOwner, Color> sphereColors = new Dictionary<SphereOwner, Color>();

    public Color noneColor;
    public Color player1Color;
    public Color player2Color;
    public Gradient player1Points;
    public Gradient player2Points;
    public float secondPerPoint;
    
    public GameObject particlePrefab;
    public float spawnRadius;
    [HideInInspector]
    public bool playerSet = false;
    public int startingPoints;
    
    public Text playerPoints;
    public Text OpponentPoints;
    public RectTransform pointSlider;

    private int playerScore;
    private int opponentScore;
    
    public static EventManager instance;

    public List<GameSphere> spheres;

    [HideInInspector]
    public List<GameSphere> selectedSpheres;
    public List<PowerUpPreset> powerUpPresets;
    public float powerUpGainSpeed;

    void Awake()
    {
        instance = this;
        sphereColors.Add(SphereOwner.none, noneColor);
        sphereColors.Add(SphereOwner.player, player1Color);
        sphereColors.Add(SphereOwner.opponent, player2Color);
        Debug.Log(sphereColors);
    }

    public void SetOpponent()
    {
        int randomSphere = Random.Range(0, spheres.Count);
        if (spheres[randomSphere].sphereStats.SphereOwner == SphereOwner.player)
        {
            SetOpponent();
        }
        else
        {
            spheres[randomSphere].sphereStats.SphereOwner = SphereOwner.opponent;
            spheres[randomSphere].sphereStats.Points = startingPoints;
        }        
    }

    public void UnSelectAllSpheres()
    {
        Debug.Log("unselecting all spheres");
        foreach (GameSphere sphere in spheres)
        {
            sphere.sphereStats.Selected = false;
        }
        selectedSpheres.Clear();
    }

    public void DeleteSelectedSphere(GameSphere gameSphere)
    {
        foreach (GameSphere tempSphere in selectedSpheres)
        {
            if (tempSphere == gameSphere)
            {
                selectedSpheres.Remove(tempSphere);
            }
        }
    }

    public void TransferPoints(GameSphere fromSphere, GameSphere toSphere)
    {
        StartCoroutine(TransferPointsRoutine(fromSphere, toSphere));
    }

    private IEnumerator TransferPointsRoutine(GameSphere fromSphere, GameSphere toSphere)
    {
        if (fromSphere == toSphere)
        {
           yield return null;
        }
        int halfPoints = fromSphere.sphereStats.Points / 2;        
        for (int i = 0; i < halfPoints; i++)
        {
            Vector3 spawnLocation = fromSphere.transform.position + new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius), Random.Range(-spawnRadius, spawnRadius));
            GameObject particle = ObjectPooler.instance.GetPooledObject(spawnLocation, Quaternion.identity);
            MoveToTarget move = particle.GetComponent<MoveToTarget>();
            move.SetOrigin(fromSphere);
            move.SetTarget(toSphere.gameObject);
            fromSphere.sphereStats.Points--;
            yield return new WaitForSeconds(.05f);
        }
        yield return null;
    }
     
    public void TransferPoints(SphereStats fromSphere, SphereStats toSphere)
    {
        int halfPoints = fromSphere.Points / 2;
            
        if (toSphere.SphereOwner == SphereOwner.opponent || toSphere.SphereOwner == SphereOwner.none)
        {
            fromSphere.Points -= halfPoints;
            int remainingPoints = halfPoints - toSphere.Points;
            if (remainingPoints > 0)
            {
                toSphere.SphereOwner = SphereOwner.player;
                toSphere.Points = remainingPoints;
            }
            else
            {
                toSphere.Points = -remainingPoints;
            }
        }
        else
        {
           fromSphere.Points -= halfPoints;
           toSphere.Points += halfPoints;
        }
        Debug.Log("Transfering " + halfPoints + " points from: " + fromSphere.ToString() + ", to: " + toSphere.ToString());
              
    }

    public void GameWon()
    {
        UIManager.Instance.OpenPage(3);
    }
    
    public void DisplayScores(int points)
    {
        int currentPlayerPoints = 0;
        int currentOpponentPoints = 0;
        for (int i = 0; i < spheres.Count; i++)
        {
            if (spheres[i].sphereStats.SphereOwner == SphereOwner.player)
            {
                currentPlayerPoints += spheres[i].sphereStats.Points;
            }
            if (spheres[i].sphereStats.SphereOwner == SphereOwner.opponent)
            {
                currentOpponentPoints += spheres[i].sphereStats.Points;
            }
        }
        playerScore = currentPlayerPoints;
        opponentScore = currentOpponentPoints;
        playerPoints.text = playerScore.ToString();
        OpponentPoints.text = opponentScore.ToString();
        float pointPercentage = (float)playerScore / ((float)playerScore + (float)opponentScore) * 100f;
        pointSlider.localPosition = new Vector3((pointPercentage - 50) * 5f, 29f, 0f);
       
    }
    
    public void ResetButtonFills()
    {
        for (int i = 0; i < powerUpPresets.Count; i++)
        {
            powerUpPresets[i].buttonFill.fillAmount = 0;
        }
    }

}
