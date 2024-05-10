using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{

    //arrow indicator object
    public GameObject arrow;

    //3D Model object
    public GameObject target;

    // Update is called once per frame
    void Update()
    {
        if(target == GameObject.Find("Lego Dump Truck"))
            //arrow pointing to 3D Model
            arrow.transform.LookAt(target.transform.position);
        else
            arrow.transform.LookAt(GameObject.Find("Lego Truck Cabin").transform.position);
    }
}
