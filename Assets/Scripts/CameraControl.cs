using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{

    private CinemachineVirtualCamera vcam;
    public Transform transform;
    public float height;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        height = transform.position.y;
        if(height >= 25)
        {
            height = 25;
        }
        if(height <= 10)
        {
            height = 10;
        }
    }

    void FixedUpdate()
    {
        vcam.m_Lens.OrthographicSize = height;
    }
}