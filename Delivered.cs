using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivered : MonoBehaviour
{

    private GameObject deliveryTarget;

    void Update()
    {
        deliveryTarget = GameObject.FindWithTag("deliveryTarget");
    }

    void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "deliveryTarget")
        {
            Destroy(gameObject);
            deliveryTarget.tag = "Untagged";
        }
    }
}
