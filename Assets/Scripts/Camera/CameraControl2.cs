using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl2 : MonoBehaviour
{

    private CinemachineVirtualCamera vcam;
    public BoxCollider2D LandingZoom;
    public Rigidbody2D ship;
    private float landingZoom;
    private float takeoffZoom;
    private float currentZoom;
    static float t;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        landingZoom = 5f;
        takeoffZoom = 12f;
        t = 0.0f;
    }

    void Update()
    {
        currentZoom = Mathf.Lerp(takeoffZoom, landingZoom, t);
        t += 0.5f * Time.deltaTime;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (ship.velocity.magnitude <= 4f)
        {
            vcam.m_Lens.OrthographicSize = currentZoom;
        }
    }
    //this method is called when a collision is initiated


}