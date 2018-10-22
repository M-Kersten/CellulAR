using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

    public static ObjectPooler instance;
    public GameObject pooledObject;
    public int pooledAmount;
    public bool willGrow;

    List<GameObject> pooledObjects;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start () {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
	}

    public GameObject GetPooledObject(Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                GameObject currentPooledObject = (GameObject)pooledObjects[i];
                currentPooledObject.transform.position = position;
                currentPooledObject.transform.rotation = rotation;
                currentPooledObject.SetActive(true);
                return currentPooledObject;
            }
        }
        if (willGrow)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            pooledObjects.Add(obj);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

}
