using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//these fields create the components in parenthesis if they are not found
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class ShipControl : MonoBehaviour
{
    //<PUBLIC VARIABLES>
    
    //isControlling is used to check if the player can control the ship
    public bool isControlling;
    //thrust refers to the engine power and is the force applied when thrusting the engine
    public float thrust;
    //torque refers to the amount of angular momentum added to the ship when rotating clockwise and counterclockwise
    public float torque;    
    //cargo is on the cargo which you drop to make deliveries
    public Rigidbody2D cargo;
    //cargoDropLocation points to the location from where the cargo gets instantiated and dropped
    public Transform cargoDropLocation;
    //setDeliveryTarget is a reference to the setDeliveryTarget script which randomly chooses a residence as the destination for the delivery
    public SetDeliveryTarget setDeliveryTarget;
    //setNewTarget is used to check if the player can pick up more cargo
    public bool setNewTarget;
    //currentFuel refers to the amount of fuel in your tank
    public float currentFuel;
    //fuelConsumption is the multiplier for how fast your engine consumes fuel
    public float fuelConsumption;
    //fuelRefill is the multiplier for how fast your fuel pumps can fill your fuel tank
    public float fuelRefill;
    //slider is used to display current fuel level in the UI
    public Slider slider;
    //altitude text field is used to display current altitude on the UI altimeter
    public Text altitude;

    //<PRIVATE VARIABLES>

    //rocketBlast area effector creates the force that blasts debris simulating engine blast
    private AreaEffector2D rocketBlast;
    //animator is used to animate the ships thrust
    private Animator animator;
    //ship rigidbody is used to apply physics to the ship
    private Rigidbody2D ship;
    //shipTransform is used to determine the altitude of the ship
    private Transform shipTransform;
    //isFuelConsumptionStarted is used to make sure that the fuel consumption only starts one time
    private bool isFuelConsumptionStarted = false;
    //isFuelRefillStarted is used to make sure that the fuel refill only starts one time
    private bool isFuelRefillStarted = false;
    //hasCargo gets set within the 'LandedScript' when you pick up or drop off a delivery
    private bool hasCargo;
    //reference to the playerInput script
    private PlayerInput playerInput;
    //explosionprefab is a particle system
    [SerializeField] private GameObject explosionprefab;
    [SerializeField] private GameObject explosionprefab2;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        ship = GetComponent<Rigidbody2D>();
        shipTransform = GetComponent<Transform>();
                cargoDropLocation = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        rocketBlast = GetComponentInChildren<AreaEffector2D>();
        isControlling = true;
        hasCargo = false;
        setNewTarget = true;
        currentFuel = 100f;
        fuelConsumption = .2f;
        fuelRefill = .7f;
    }

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

        if (isControlling && playerInput.thrustInput && currentFuel > 0)
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

    void FixedUpdate()
    {
        if (isControlling && playerInput.thrustInput && currentFuel > 0)
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

        if (playerInput.torqueInputR && isControlling)
        {
            ship.AddTorque(-torque * Mathf.Deg2Rad, ForceMode2D.Impulse);
        }

        if (playerInput.torqueInputL && isControlling) 
        {
            ship.AddTorque(torque * Mathf.Deg2Rad, ForceMode2D.Impulse);
        }

        if (playerInput.cargoDrop && hasCargo)
        {
            Instantiate(cargo, cargoDropLocation.position , cargoDropLocation.rotation);
            hasCargo = false;
            setNewTarget = true;
        }

        if(currentFuel <= 0)
        {
            animator.SetBool("isThrusting", false);
            FindObjectOfType<AudioManager>().Stop("EngineSound");
        }
    }

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

        else if (collision.gameObject.tag == "Restaurant" && setNewTarget)
        { 
            hasCargo = true;
            setDeliveryTarget.SetTarget();
            setNewTarget = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "FuelStation" && !isFuelRefillStarted)
        {
            isFuelRefillStarted = true;
            StartCoroutine(FuelFillOverTimeCoroutine(fuelRefill));
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "FuelStation")
        {
            StopCoroutine(FuelFillOverTimeCoroutine(fuelRefill));
            isFuelRefillStarted = false;
        }
    }

    //this method sets the slider value to reflect the fuel amount
    public void SetFuel(float currentFuel)
    {
        slider.value = currentFuel;
    }

    //this method drains fuel over time relative to a fuel consumption rate
    public void FuelDrainOverTime(float fuelConsumption)
    {
        StartCoroutine(FuelDrainOverTimeCoroutine(fuelConsumption));
    }

    //this coroutine initiates fuel drain
    IEnumerator FuelDrainOverTimeCoroutine(float fuelConsumption)
    {
        while (playerInput.thrustInput && currentFuel > 0)
        {
            currentFuel -= fuelConsumption;
            isFuelConsumptionStarted = true;
            yield return new WaitForSeconds(.1f);
        }
    }

    //this coroutine initiates fuel refill
    public void FuelFillOverTime(float fuelRefill)
    {
        StartCoroutine(FuelFillOverTimeCoroutine(fuelRefill));
    }

    //this coroutine stops fuel filling once tank is full
    IEnumerator FuelFillOverTimeCoroutine(float fuelRefill)
    {
        while (!playerInput.thrustInput && currentFuel < 100)
        {
            currentFuel += fuelRefill;
            isFuelRefillStarted = true;
            yield return new WaitForSeconds(.1f);
        }
    }
}
