using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDeliveryTarget : MonoBehaviour
{

    public GameObject[] residences;
    public GameObject deliveryTarget;
    int index;

    public void SetTarget()
    {
        index = Random.Range(0, residences.Length);
        deliveryTarget = residences[index];
        deliveryTarget.tag = "deliveryTarget";
        deliveryTarget.GetComponentInChildren<SpriteRenderer>().enabled = true;
    }
}