using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
//these fields create the components in parenthesis if they are not found

public class ShipControl : MonoBehaviour
{
    //<PUBLIC VARIABLES>
    public bool isControlling;
    //isControlling is used to check if the player can control the ship
    public float thrust;
    //thrust refers to the engine power and is the force applied when thrusting the engine
    public float torque;
    //torque refers to the amount of angular momentum added to the ship when rotating clockwise and counterclockwise
    public Rigidbody2D cargo;
    //cargo is on the cargo which you drop to make deliveries
    public Transform cargoDropLocation;
    //cargoDropLocation points to the location from where the cargo gets instantiated and dropped
    public SetDeliveryTarget setDeliveryTarget;
    //setDeliveryTarget is a reference to the setDeliveryTarget script which randomly chooses a residence as the destination for the delivery
    public bool setNewTarget;
    //setNewTarget is used to check if the player can pick up more cargo
    public float currentFuel;
    //currentFuel refers to the amount of fuel in your tank
    public float fuelConsumption;
    //fuelConsumption is the multiplier for how fast your engine consumes fuel
    public float fuelRefill;
    //fuelRefill is the multiplier for how fast your fuel pumps can fill your fuel tank
    public Slider slider;
    //slider is used to display current fuel level in the UI
    public Text altitude;
    //altitude text field is used to display current altitude on the UI altimeter
    public GameObject explosionprefab;
    public GameObject explosionprefab2;


    //<PRIVATE VARIABLES>
    private AreaEffector2D rocketBlast;
    //rocketBlast area effector creates the force that blasts debris simulating engine blast
    private ParticleSystem crashExplosion;
    //crashExplosion creates the explosions particles upon crashing
    private Animator animator;
    //animator is used to animate the ships thrust
    private Rigidbody2D ship;
    //ship rigidbody is used to apply physics to the ship
    private Transform shipTransform;
    //shipTransform is used to determine the altitude of the ship
    private int shipOffset;
    //shipOffset is used to determine the alitimeter correction but it is currently not in use
    private bool isFuelConsumptionStarted = false;
    //isFuelConsumptionStarted is used to make sure that the fuel consumption only starts one time
    private bool isFuelRefillStarted = false;
    //isFuelRefillStarted is used to make sure that the fuel refill only starts one time
    private bool hasCargo;
    //hasCargo gets set within the 'LandedScript' when you pick up or drop off a delivery
    private PlayerInput playerInput;
    //reference to the playerInput script



    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ship = GetComponent<Rigidbody2D>();
        shipTransform = GetComponent<Transform>();

        cargoDropLocation = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        rocketBlast = GetComponentInChildren<AreaEffector2D>();
        crashExplosion = GetComponentInChildren<ParticleSystem>();
        isControlling = true;
        hasCargo = false;
        setNewTarget = true;
        currentFuel = 100f;
        fuelConsumption = .2f;
        fuelRefill = .7f;
    }
    //this method initializes conditions



    void Update()
    {
        SetFuel(currentFuel);
        altitude.text = Mathf.Round(shipTransform.position.y) + "m";

        if (playerInput.useSpecial)
        {
        thrust = 65;
        fuelConsumption = .4f;
        }

        else
        {
        thrust = 35;
        fuelConsumption = .2f;
        }


        if (playerInput.thrustInput && currentFuel > 0 && isControlling)
        {
        animator.SetBool("isThrusting", true);
        rocketBlast.enabled = true;
        FindObjectOfType<AudioManager>().Play("EngineSound");
            if (!isFuelConsumptionStarted)
            {
            StartCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
            }
        }

    }
    //this method is called every time a frame is drawn

    void FixedUpdate()
    {
        if (playerInput.thrustInput && currentFuel > 0 && isControlling)
        {
            ship.AddForce(transform.up * thrust);
        }

        if (!playerInput.thrustInput)
        {
            animator.SetBool("isThrusting", false);
            FindObjectOfType<AudioManager>().Stop("EngineSound");
            rocketBlast.enabled = false;
            StopCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
            isFuelConsumptionStarted = false;
        }

        if (playerInput.torqueInputR == true && isControlling)
            {ship.AddTorque(-torque * Mathf.Deg2Rad, ForceMode2D.Impulse);}

        if (playerInput.torqueInputL == true && isControlling)
            {ship.AddTorque(torque * Mathf.Deg2Rad, ForceMode2D.Impulse);}

        if (playerInput.cargoDrop == true && hasCargo == true)
            {var newCargo = Instantiate(cargo, cargoDropLocation.position , cargoDropLocation.rotation);
            hasCargo = false;
            setNewTarget = true;}

        if(currentFuel <= 0)
        {
            animator.SetBool("isThrusting", false);
            FindObjectOfType<AudioManager>().Stop("EngineSound");
        }
    }
    //this method is called based on a fixed time step not based on frame rate



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bounce")
        {
            Debug.Log("Bounce Bby");
        }

        else if (collision.relativeVelocity.magnitude >= 4f)
        {
                isControlling = false;
                animator.SetBool("isThrusting", false);
                FindObjectOfType<AudioManager>().Stop("EngineSound");
                rocketBlast.enabled = false;
                StopCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
                isFuelConsumptionStarted = false;
                ship.angularDrag = 0.3f;
                Destroy(gameObject);
                FindObjectOfType<AudioManager>().Play("Explosion");
                Instantiate(explosionprefab, gameObject.transform.position, Quaternion.identity);
                Instantiate(explosionprefab2, gameObject.transform.position, Quaternion.identity);
        }

        else
        { 
            if (collision.gameObject.tag == "Restaurant" && setNewTarget)
            {
            hasCargo = true;
            setDeliveryTarget.SetTarget();
            setNewTarget = false;
            }
        }
    }
    //this method is called when a collision is initiated

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "FuelStation" && !isFuelRefillStarted)
        {
            StartCoroutine(FuelFillOverTimeCoroutine(fuelRefill));
            isFuelRefillStarted = true;
        }
    }
    //this method is called as long as the collided objects remain collided

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "FuelStation")
        {
            StopCoroutine(FuelFillOverTimeCoroutine(fuelRefill));
            isFuelRefillStarted = false;
        }
    }
    //this method is called when the collided objects seperate



    public void SetFuel(float currentFuel)
    {
        slider.value = currentFuel;
    }
    //this method sets the slider value to reflect the fuel amount

    public void FuelDrainOverTime(float fuelConsumption)
    {
        StartCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
    }
    //this method drains fuel over time relative to a fuel consumption rate

    IEnumerator FuelDrainOverTimeCoroutine(float fuelConsumption)
    {
        while (currentFuel > 0 && playerInput.thrustInput == true)
        {
            currentFuel -= fuelConsumption;
            isFuelConsumptionStarted = true;
            yield return new WaitForSeconds(.1f);
        }
    } 
    //this coroutine initiates fuel drain
    
    public void FuelFillOverTime(float fuelRefill)
    {
        StartCoroutine(FuelFillOverTimeCoroutine(fuelRefill));
    }
    //this coroutine initiates fuel refill

    IEnumerator FuelFillOverTimeCoroutine(float fuelRefill)
    {
        while (currentFuel < 100 && playerInput.thrustInput == false)
        {
            currentFuel += fuelRefill;
            isFuelRefillStarted = true;
            yield return new WaitForSeconds(.1f);
        }
    }
    //this coroutine stops fuel filling once tank is full
}
