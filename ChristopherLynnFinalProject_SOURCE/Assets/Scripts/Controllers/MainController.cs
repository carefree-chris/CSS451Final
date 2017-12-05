using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainController : MonoBehaviour {

    //Controllers
    public CameraController camCtrl = null;
    public SceneNodeControl SNCtrl = null;
    public AxisController axis = null;

    public GameObject helpTextPanel = null;


    private Text textMessage;


	void Start () {

        Debug.Assert(camCtrl != null);
        Debug.Assert(SNCtrl != null);
        Debug.Assert(axis != null);
        Debug.Assert(helpTextPanel != null);

        textMessage = helpTextPanel.GetComponentInChildren<Text>();

        Debug.Assert(textMessage != null);

        textMessage.text = "Select a scene node to get started!";
    }
	
    public void AddNode()
    {
        bool canAddNode = SNCtrl.AddNode();

        if (canAddNode)
        {
            textMessage.text = "Node Added!";
        } else
        {
            textMessage.text = "Can add a node (do you have a node selected?)";
        }
    }

    /*public GameObject getSelected()
    {
        //return mSelected;
    }*/
}
