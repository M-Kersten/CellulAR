using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using GoogleARCore;
using System.Collections.Generic;

public class ARController : MonoBehaviour
{
    /// <summary>
    /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
    /// </summary>
    public GameObject SpawnPoint;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    //public GameObject DetectedPlanePrefab;

    /// <summary>
    /// A model to place.
    /// </summary>
    public GameObject[] levels;

    public GameObject spawnObject;

    /// <summary>
    /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
    /// </summary>
    //public GameObject SearchingForPlaneUI;

    /// <summary>
    /// The rotation in degrees need to apply to model when the Andy model is placed.
    /// </summary>
    private const float k_ModelRotation = 180.0f;

    /// <summary>
    /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    //private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();

    private List<GameObject> activeObjects = new List<GameObject>();

    private List<GameObject> currentAnchors = new List<GameObject>();

    public void SpawnLevel(int number)
    {
        Vector3 objectPos = new Vector3(SpawnPoint.transform.position.x, SpawnPoint.transform.position.y, SpawnPoint.transform.position.z);
        Pose cameraPose;
        cameraPose.position = objectPos;
        cameraPose.rotation = SpawnPoint.transform.localRotation;
        var newObject = Instantiate(levels[number], objectPos, SpawnPoint.transform.localRotation);
        var objectAnchor = Session.CreateAnchor(cameraPose);        
        newObject.transform.parent = objectAnchor.transform;
        activeObjects.Add(newObject);
    }

    public void SpawnObjectAtPoint()
    {
        Vector3 objectPos = new Vector3(SpawnPoint.transform.position.x, SpawnPoint.transform.position.y, SpawnPoint.transform.position.z);
        Pose cameraPose;
        cameraPose.position = objectPos;
        cameraPose.rotation = SpawnPoint.transform.localRotation;
        GameObject newObject = Instantiate(spawnObject, objectPos, SpawnPoint.transform.localRotation);
        Anchor objectAnchor = Session.CreateAnchor(cameraPose);
        newObject.transform.parent = objectAnchor.transform;
        activeObjects.Add(newObject);
        currentAnchors.Add(objectAnchor.gameObject);
    }

    public void ResetActiveObjects()
    {
        foreach (GameObject item in activeObjects)
        {
            Destroy(item);
        }
        foreach (GameObject anchor in currentAnchors)
        {
            Destroy(anchor);
        }
        EventManager.instance.playerSet = false;
        EventManager.instance.spheres.Clear();
        activeObjects.Clear();
        EventManager.instance.ResetButtonFills();
    }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
        
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }
    }