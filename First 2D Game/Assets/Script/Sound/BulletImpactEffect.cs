using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpactEffect : MonoBehaviour
{
    void Start()
    {
        FindObjectOfType<AudioManager>().Play("BulletImpactEffect");
    }
}
