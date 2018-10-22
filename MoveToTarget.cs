using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTarget : MonoBehaviour
{

    public float speed;
    public GameObject origin;
    public GameSphere originSphere;
    private SphereOwner originOwner;
    public GameObject target;
    private float timer;
    private TrailRenderer trail;

    private void OnEnable()
    {
        if (trail == null)
        {
            trail = transform.GetChild(0).GetComponent<TrailRenderer>();
        }
        timer = 0.3f;        
    }

    private void OnDisable()
    {
        trail.enabled = false;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
        }
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            trail.enabled = true;
        }
    }

    public void SetOrigin(GameSphere _origin)
    {
        origin = _origin.gameObject;
        originSphere = _origin;
        originOwner = originSphere.sphereStats.SphereOwner;
        if (originOwner == SphereOwner.opponent)
        {
            trail.colorGradient = EventManager.instance.player2Points;
        }
        else
        {
            trail.colorGradient = EventManager.instance.player1Points;
        }
    }

    public void SetTarget(GameObject _target)
    {
        target = _target;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (origin != null)
        {
            if (collision.gameObject == target)
            {
                GameSphere sphere = collision.GetComponent<GameSphere>();
                if (originOwner == SphereOwner.player)
                {
                    if (sphere.sphereStats.SphereOwner == SphereOwner.player)
                    {
                        sphere.sphereStats.Points++;
                    }
                    else
                    {
                        sphere.sphereStats.Points--;
                        if (sphere.sphereStats.Points < 0)
                        {
                            sphere.sphereStats.SphereOwner = SphereOwner.player;
                            sphere.sphereStats.Points++;
                        }
                    }
                }
                
                else
                {
                    if (originOwner == SphereOwner.opponent)
                    {
                        if (sphere.sphereStats.SphereOwner == SphereOwner.opponent)
                        {
                            sphere.sphereStats.Points++;
                        }
                        else
                        {
                            sphere.sphereStats.Points--;
                            if (sphere.sphereStats.Points < 0)
                            {
                                sphere.sphereStats.SphereOwner = SphereOwner.opponent;
                                sphere.sphereStats.Points++;
                            }
                        }
                    }                    
                }
                gameObject.SetActive(false);
            }
        }
    }
}
