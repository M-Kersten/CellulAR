using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCooldown : MonoBehaviour {

    public float shieldDuration;
    private float timer;

    private void OnEnable()
    {
        timer = shieldDuration;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
        transform.Rotate(new Vector3(0, 1, 0), .1f);
    }

}
