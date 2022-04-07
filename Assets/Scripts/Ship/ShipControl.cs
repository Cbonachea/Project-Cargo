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
    //animator is used to animate the ships thrust
    public Animator animator;
    //cargoDropLocation points to the location from where the cargo gets instantiated and dropped
    public Transform cargoDropLocation;
    //setDeliveryTarget is a reference to the setDeliveryTarget script which randomly chooses a residence as the destination for the delivery
    public SetDeliveryTarget setDeliveryTarget;
    //setNewTarget is used to check if the player can pick up more cargo
    public bool setNewTarget;
    //altitude text field is used to display current altitude on the UI altimeter
    public Text altitude;
    //this references which special is currently active
    public int currentSpecial;
    //this refers to the speed at which you can crash before exploding
    public float crashTolerance = 4;

    //<PRIVATE VARIABLES>
    //rocketBlast area effector creates the force that blasts debris simulating engine blast
    private AreaEffector2D rocketBlast;
    //ship rigidbody is used to apply physics to the ship
    private Rigidbody2D ship;
    //shipTransform is used to determine the altitude of the ship
    private Transform shipTransform;
    //hasCargo gets set within the 'LandedScript' when you pick up or drop off a delivery
    private bool hasCargo;
    //reference to the FuelSystem script
    private FuelSystem fuelSystem;
    //reference to the PlayerInput script
    private PlayerInput playerInput;
    //explosionprefabs are particle systems
    [SerializeField] private GameObject explosionprefab;
    [SerializeField] private GameObject explosionprefab2;
    private enum EngineStatus
    {
        on, off
    }
    private EngineStatus engineStatus;


    private void Awake()
    {
        fuelSystem = GetComponent<FuelSystem>();
        playerInput = GetComponent<PlayerInput>();
        ship = GetComponent<Rigidbody2D>();
        shipTransform = GetComponent<Transform>();
        cargoDropLocation = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        rocketBlast = GetComponentInChildren<AreaEffector2D>();
        isControlling = true;
        hasCargo = false;
        setNewTarget = true;
        currentSpecial = 1;
    }


    //<///////////////////////////////////////////////////////////////////////////>
    //TOGGLING ENGINE/SPECIAL/CARGO STATUS
    void Update()
    {
        altitude.text = Mathf.Round(shipTransform.position.y) + "m";
        if (!isControlling) return;
        if (playerInput.thrustInput) EngineOn();
        if (playerInput.useSpecial) UseSpecial();
        if (!playerInput.thrustInput) EngineOff();
        if (hasCargo && playerInput.cargoDrop) DropCargo();
    }

    void EngineOn()
    {
        if (engineStatus == EngineStatus.on) return;
        animator.SetBool("isThrusting", true);
        rocketBlast.enabled = true;
        FindObjectOfType<AudioManager>().Play("EngineSound");
        engineStatus = EngineStatus.on;
    }
    void EngineOff()
    {
        if (engineStatus == EngineStatus.off) return;
        animator.SetBool("isThrusting", false);
        FindObjectOfType<AudioManager>().Stop("EngineSound");
        rocketBlast.enabled = false;
        engineStatus = EngineStatus.off;
    }
    void DropCargo()
    {
        Instantiate(Cargo, cargoDropLocation.position, cargoDropLocation.rotation);
        hasCargo = false;
        setNewTarget = true;
    }
    public void UseSpecial()
    {
        //    Debug.Log("special");
    }


    //<///////////////////////////////////////////////////////////////////////////>
    //ADDING FORCES AND MOVING THE SHIP
    void FixedUpdate()
    {
        if (!isControlling) return;
        if (playerInput.torqueInputR) TorqueR();
        if (playerInput.torqueInputL) TorqueL();
        if (fuelSystem.currentFuel > 0 && playerInput.thrustInput) Thrust();
    }

    void Thrust() { ship.AddForce(transform.up * thrust); }
    void TorqueL() { ship.AddTorque(torque * Mathf.Deg2Rad, ForceMode2D.Impulse); }
    void TorqueR() { ship.AddTorque(-torque * Mathf.Deg2Rad, ForceMode2D.Impulse); }


    //<///////////////////////////////////////////////////////////////////////////>
    //COLLIDING WITH OBJECTS
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bounce") Bounce();
        else if (collision.relativeVelocity.magnitude >= crashTolerance) Crash();
        else if (collision.gameObject.tag == "Restaurant" && setNewTarget) LoadPackage();
    }

    void Crash()
    {
        isControlling = false;
        animator.SetBool("isThrusting", false);
        FindObjectOfType<AudioManager>().Stop("EngineSound");
        rocketBlast.enabled = false;
        ship.angularDrag = 0.3f;
        Destroy(gameObject);
        FindObjectOfType<AudioManager>().Play("Explosion");
        Instantiate(explosionprefab, gameObject.transform.position, Quaternion.identity);
        Instantiate(explosionprefab2, gameObject.transform.position, Quaternion.identity);
    }
    void Bounce()
    {
        Debug.Log("Bounce Bby");
    }
    void LoadPackage()
    {
        hasCargo = true;
        setDeliveryTarget.SetTarget();
        setNewTarget = false;
    }
}
