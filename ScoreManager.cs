using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace CellulAR
{
    /// <summary>
    /// Handles scores and colors
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {

        // invisible in editor
        public static ScoreManager Instance;
        private int playerScore;
        private int opponentScore;
        [HideInInspector]
        public List<GameSphere> selectedSpheres;
        [HideInInspector]
        public bool playerSet;

        //scriptable object
        public GameData gameData;

        public static Dictionary<SphereOwner, Color> sphereColors = new Dictionary<SphereOwner, Color>();

        [Header("references in scene")]
        public Text playerPoints;
        public Text OpponentPoints;
        public RectTransform pointSlider;
        public List<PowerUpPreset> powerUpPresets;
        public float powerUpGainSpeed;
        public List<GameSphere> spheres;

        void Awake()
        {
            Instance = this;
            sphereColors.Add(SphereOwner.none, gameData.noneColor);
            sphereColors.Add(SphereOwner.player, gameData.player1Color);
            sphereColors.Add(SphereOwner.opponent, gameData.player2Color);
        }

        /// <summary>
        /// Give a sphere to opponent
        /// </summary>
        public void SetOpponent()
        {
            int randomSphere = Random.Range(0, spheres.Count);
            // check if spawnsphere is not already owned by player
            if (spheres[randomSphere].sphereStats.SphereOwner == SphereOwner.player)
            {
                SetOpponent();
            }
            else
            {
                spheres[randomSphere].sphereStats.SphereOwner = SphereOwner.opponent;
                spheres[randomSphere].sphereStats.Points = gameData.startingPoints;
            }
        }

        public void UnSelectAllSpheres()
        {
            Debug.Log("unselecting all spheres");
            spheres.ForEach(item => item.sphereStats.Selected = false);
            selectedSpheres.Clear();
        }

        public void DeleteSelectedSphere(GameSphere gameSphere)
        {
            var tempSphere = selectedSpheres.Where(item => item == gameSphere).Select(item => selectedSpheres.Remove(item));
        }

        /// <summary>
        /// send particles from sphere to sphere
        /// </summary>
        /// <param name="fromSphere">can be multiple spheres</param>
        /// <param name="toSphere">can be only one sphere</param>
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
                Vector3 spawnLocation = fromSphere.transform.position + new Vector3(Random.Range(-gameData.spawnRadius, gameData.spawnRadius), Random.Range(-gameData.spawnRadius, gameData.spawnRadius), Random.Range(-gameData.spawnRadius, gameData.spawnRadius));
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
            powerUpPresets.ForEach(item => item.buttonFill.fillAmount = 0);
        }
    }
}