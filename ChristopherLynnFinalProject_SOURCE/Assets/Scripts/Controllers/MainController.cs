using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainController : MonoBehaviour {


    //Controllers
    public CameraController camCtrl = null;
    public SceneNodeControl SNCtrl = null;
    public AxisController axis = null;

    public GameObject helpTextPanel = null;
    public Translator mTranslator = null;

    private Text textMessage;


	void Start () {

        Debug.Assert(camCtrl != null);
        Debug.Assert(SNCtrl != null);
        Debug.Assert(axis != null);
        Debug.Assert(helpTextPanel != null);
        Debug.Assert(mTranslator != null);

        textMessage = helpTextPanel.GetComponentInChildren<Text>();

        Debug.Assert(textMessage != null);

        textMessage.text = "To start, add a node, and Ctrl-Left Click to move it around!";
    }

    void Update()
    {
        SelectionSupport();
    }
	
    /* Node Stuff (3D Rigging Portion) */
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

    public void AddPrimitive()
    {

        

        bool canAddPrimitive = SNCtrl.AddPrimitive();

        if (canAddPrimitive)
        {
            textMessage.text = "Added primitive to your current node!";
        } else
        {
            textMessage.text = "ERROR: Unable to add primitive.";
        }


    }

    public void RemoveNode()
    {
        if (SNCtrl.RemoveSelectedNode())
        {
            textMessage.text = "Node deleted!!";

        } else {
            textMessage.text = "Sorry, you can't delete the root node.";
        }
    }

    // Mouse click selection 
    void SelectionSupport()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
            return; // camera manipulate is on, ignore


        SendSelectionRay();
        /*
        if (Input.GetKey(KeyCode.LeftControl))
        {
            //mModel.SetSelectionMode(true);
            SendSelectionRay();
        }
        else
        {
            //mModel.SetSelectionMode(false);
            mTranslator.ResetSelectMode();
        }*/
    }

    void SendSelectionRay()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // check for GUI
        {
            Ray aRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
            if (Input.GetMouseButtonDown(0))
            {
                if (!mTranslator.SelectMode(aRay))
                {
                    //Transform t = mModel.SelectAMarker(aRay);
                    Transform t = hit.transform;
                    mTranslator.SetTargetTransform(t);

                    if (t != null && t.tag == "Node")
                    {
                        SNCtrl.SwitchSelectedNode(t.gameObject.GetComponent<AdvancedSceneNode>());
                    }
                }
            }
            else if (Input.GetMouseButton(0))
            {
                mTranslator.DragTargetTranslation();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                mTranslator.ResetSelectMode();
            }
        }
    }

    /* Misc. Stuff*/
    public void SwitchView()
    {
        int camIndex = camCtrl.SwitchView();

        switch(camIndex)
        {
            case 0:
                textMessage.text = "Switched to 3D Modelling View!";
                break;
            case 1:
                textMessage.text = "Switched to Rigging View!";
                break;
            default:
                break;
        }

    }


}
