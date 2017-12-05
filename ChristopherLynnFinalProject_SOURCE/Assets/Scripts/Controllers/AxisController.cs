using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisController : MonoBehaviour {

    private bool isEnabled = true;

    public GameObject axisOrigin = null;
    public GameObject axisX = null;
    public GameObject axisY = null;
    public GameObject axisZ = null;

    // Use this for initialization
    void Start () {
        Debug.Assert(axisOrigin != null);
        Debug.Assert(axisX != null);
        Debug.Assert(axisY != null);
        Debug.Assert(axisZ != null);
    }
	
	public void Enable()
    {
        axisOrigin.GetComponent<MeshRenderer>().enabled = true;
        axisX.GetComponent<MeshRenderer>().enabled = true;
        axisY.GetComponent<MeshRenderer>().enabled = true;
        axisZ.GetComponent<MeshRenderer>().enabled = true;

        isEnabled = true;
    }

    public void Disable()
    {
        axisOrigin.GetComponent<MeshRenderer>().enabled = false;
        axisX.GetComponent<MeshRenderer>().enabled = false;
        axisY.GetComponent<MeshRenderer>().enabled = false;
        axisZ.GetComponent<MeshRenderer>().enabled = false;

        isEnabled = false;
    }

    
}
