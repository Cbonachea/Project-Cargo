using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 
public class FuelSystem : MonoBehaviour
{
    //PUBLIC VARIABLES
    // maxFuel sets how much fuel a given tank can carry
    public int maxFuel;
    //currentFuel refers to the amount of fuel in your tank
    public int currentFuel;
    //engineConsumption is the multiplier for how fast your engine consumes fuel on paper
    public float engineConsumption;
    //fuelConsumption is the multiplier for how fast your engine actually consumes fuel
    public int fuelConsumption;
    //isFuelConsumptionStarted is used to make sure that the fuel consumption only starts one time
    public bool isFuelConsumptionStarted;
    //fuelRefill is the multiplier for how fast your fuel pumps can fill your fuel tank on paper
    public float fuelPumpRate;
    //fuelRefill is the multiplier for how fast your fuel pumps can actually fill your fuel tank
    public int fuelRefill;
    //slider is used to display current fuel level in the UI
    public Slider slider;


    //PRIVATE VARIABLES
    //reference to the ShipControl script
    private ShipControl shipControl;
    //reference to the PlayerInput script
    private PlayerInput playerInput;
    //isFuelRefillStarted is used to make sure that the fuel refill only starts one time
    private bool isFuelRefillStarted = false;


    void Awake()
    {
        maxFuel = 100;
        currentFuel = maxFuel;
        SetFuel(currentFuel);
        shipControl = GetComponent<ShipControl>();
        playerInput = GetComponent<PlayerInput>();
        fuelConsumption = 1;
        fuelRefill = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFuel <= 0) OutofGas();
        if (currentFuel > 0 && playerInput.thrustInput) DrainFuel();
        if (!playerInput.thrustInput) StopDrainingFuel();
    }

    void OnCollisionStay2D(Collision2D collision) 
    {if (!isFuelRefillStarted && collision.gameObject.tag == "FuelStation") {RefillFuel();}}
    void OnCollisionExit2D(Collision2D collision) 
    {if (collision.gameObject.tag == "FuelStation") {StopRefillingFuel();}}

    public void DrainFuel()
    {
        if (!isFuelConsumptionStarted)
        {
            StartCoroutine(co_DrainFuel());
            isFuelConsumptionStarted = true;
        }
    }
    public void StopDrainingFuel()
    {
        StopCoroutine(co_DrainFuel());
        isFuelConsumptionStarted = false;
    }
    private IEnumerator co_DrainFuel()
    {
        while (currentFuel > 0 && playerInput.thrustInput)
        {
            currentFuel -= fuelConsumption;
            yield return new WaitForSeconds(.1f);
        }
    }
    public void OutofGas()
    {
        shipControl.animator.SetBool("isThrusting", false);
        FindObjectOfType<AudioManager>().Stop("EngineSound");
    }
    public void RefillFuel()
    {
        if (!isFuelRefillStarted)
        {
            isFuelRefillStarted = true;
            StartCoroutine(co_RefillFuel());
        }
    }
    public void StopRefillingFuel()
    {
        StopCoroutine(co_RefillFuel());
        isFuelRefillStarted = false;
    }
    private IEnumerator co_RefillFuel()
    {
        while (currentFuel < 100 && !playerInput.thrustInput)
        {
            isFuelRefillStarted = true;
            currentFuel += fuelRefill;
            yield return new WaitForSeconds(.1f);
        }
    }
}
