using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnSpheres : MonoBehaviour {

    public int sphereAmount;
    public GameObject sphere;

    private Collider boxCollider;
    private Vector3 boxMin, boxMax;
    [SerializeField]
    private List<Vector3> spawnPositions;

    private bool intersecting;

    private Vector3 spawnPos;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxMin = boxCollider.bounds.min;
        boxMax = boxCollider.bounds.max;
        for (int i = 0; i < sphereAmount; i++)
        {
            SetSpherePosition();
            GameObject spawnSphere = Instantiate(sphere, spawnPos, Quaternion.identity, transform);
            GameSphere gameSphere = spawnSphere.GetComponent<GameSphere>();
            EventManager.instance.spheres.Add(gameSphere);
            if (i == 0)
            {
                gameSphere.sphereStats.PowerUpUI.PowerUp = PowerUp.speedUp;
            }
            if (i == 1)
            {
                gameSphere.sphereStats.PowerUpUI.PowerUp = PowerUp.shield;
            }
            if (i == 2)
            {
                gameSphere.sphereStats.PowerUpUI.PowerUp = PowerUp.reinforcements;
            }
        }
        
        boxCollider.enabled = false;
    }

    private void SetSpherePosition()
    {
        spawnPos = new Vector3(Random.Range(boxMin.x, boxMax.x), Random.Range(boxMin.y, boxMax.y), Random.Range(boxMin.z, boxMax.z));
        for (int j = 0; j < EventManager.instance.spheres.Count; j++)
        {
            if (Vector3.Distance(EventManager.instance.spheres[j].gameObject.transform.position, spawnPos) < 1)
            {
                SetSpherePosition();
            }
        }
    }

}
