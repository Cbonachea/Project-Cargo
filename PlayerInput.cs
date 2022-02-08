using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool thrustInput { get; private set; }
    public bool torqueInputL { get; private set; }
    public bool torqueInputR { get; private set; }
    public bool cargoDrop { get; private set; }

    private void Update()
    {
        if (Input.GetKey("w"))
        { thrustInput = true; }

        if (Input.GetKeyUp("w"))
        { thrustInput = false; }

        if (Input.GetKey("d"))
        { torqueInputR = true; }
        
        if (Input.GetKeyUp("d"))
        {torqueInputR = false;}

        if (Input.GetKey("a"))
        { torqueInputL = true; }
        
        if (Input.GetKeyUp("a"))
        {torqueInputL = false;}

        if (Input.GetKey("space"))
        {cargoDrop = true;}

        if (Input.GetKeyUp("space"))
        {cargoDrop = false;}
    }
}
