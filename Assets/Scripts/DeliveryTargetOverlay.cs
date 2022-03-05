using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryTargetOverlay : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;


    void Update()
    {
        if(gameObject.tag == "deliveryTarget")
        {
            spriteRenderer.enabled = true;
        }
        if (gameObject.tag == "Untagged")
        {
            spriteRenderer.enabled = false;
        }
    }
}
