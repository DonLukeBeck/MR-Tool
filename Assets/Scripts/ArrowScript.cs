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
        //arrow pointing to 3D Model
        arrow.transform.LookAt(target.transform.position);
    }
}
