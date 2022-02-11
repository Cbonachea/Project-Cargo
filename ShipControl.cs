﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
//these fields create the components in parenthesis if they are not found

public class ShipControl : MonoBehaviour
{

    public float thrust;
    public float torque;
    public Rigidbody2D cargo;
    public Transform cargoDropLocation;
    public SetDeliveryTarget setDeliveryTarget;
    public bool setNewTarget;
    public float currentFuel;
    public float fuelConsumption;
    public Slider slider;

    private AreaEffector2D rocketBlast;
    private Animator animator;
    private Rigidbody2D ship;
    private bool isCoroutineStarted = false;


    private PlayerInput playerInput;
    //reference to the playerInput script
    private bool hasCargo;
    //bool that gets set within the 'LandedScript' when you pick up or drop off a delivery
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ship = GetComponent<Rigidbody2D>();

        cargoDropLocation = GetComponent<Transform>();
        //where the cargo is spawned when the 'drop' button is pressed
        animator = GetComponent<Animator>();
        rocketBlast = GetComponentInChildren<AreaEffector2D>();
        hasCargo = false;
        setNewTarget = true;
        currentFuel = 35;
        fuelConsumption = .2f;
    }

    void Update()
    {
        SetFuel(currentFuel);
    }


    void FixedUpdate()
    {
        if (playerInput.thrustInput == true && currentFuel > 0)
        {
            animator.SetBool("isThrusting", true);
            ship.AddForce(transform.up * thrust);
            rocketBlast.enabled = true;
            FindObjectOfType<AudioManager>().Play("EngineSound");
            if (!isCoroutineStarted)
            {
                StartCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
            }
        }

        if (playerInput.thrustInput == false)
        {
            animator.SetBool("isThrusting", false);
            FindObjectOfType<AudioManager>().Stop("EngineSound");
            rocketBlast.enabled = false;
            StopCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
            isCoroutineStarted = false;
        }

        if (playerInput.torqueInputR == true)
            {ship.AddTorque(-torque * Mathf.Deg2Rad, ForceMode2D.Impulse);}

        if (playerInput.torqueInputL == true)
            {ship.AddTorque(torque * Mathf.Deg2Rad, ForceMode2D.Impulse);}

        if (playerInput.cargoDrop == true && hasCargo == true)
            {Instantiate(cargo, cargoDropLocation.position , cargoDropLocation.rotation );
            hasCargo = false;
            setNewTarget = true;}

        if(currentFuel <= 0)
        {
            animator.SetBool("isThrusting", false);
            FindObjectOfType<AudioManager>().Stop("EngineSound");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Restaurant" && setNewTarget == true)
        {
            hasCargo = true;
            setDeliveryTarget.SetTarget();
            setNewTarget = false;
        }
    }

    public void SetFuel(float currentFuel)
    {
        slider.value = currentFuel;
    }

    public void FuelDrainOverTime(float fuelConsumption)
    {
        StartCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
    }

    IEnumerator FuelDrainOverTimeCoroutine(float fuelConsumption)
    {
        while (currentFuel > 0 && playerInput.thrustInput == true)
        {
            currentFuel -= fuelConsumption;
            isCoroutineStarted = true;
            yield return new WaitForSeconds(.1f);
        }
    }
}
