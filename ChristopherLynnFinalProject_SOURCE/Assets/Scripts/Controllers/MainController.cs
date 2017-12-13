using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainController : MonoBehaviour {

    public List<GameObject> CustomPrimitives = new List<GameObject>();

    //The two different views of our program
    public GameObject TheRiggingWorld = null;
    public GameObject TheModellingWorld = null;

    public GameObject View1UI = null;
    public GameObject View2UI = null;

    //Controllers
    public CameraController camCtrl = null;
    public SceneNodeControl SNCtrl = null;
    public AxisController axis = null;

    public GameObject helpTextPanel = null;
    public Translator mTranslator = null;

    private Text textMessage;

    //Rigging
    public Dropdown mCustomPrimitiveSelect;

    //3D Modelling
    private MeshController mModellingCtrl;

    public Dropdown mModellingSelect = null;
    public Toggle verticeManip = null;
    public Button mAddToListButton = null;

    public SliderWithEcho mResControl = null;
    public SliderWithEcho mCylinderResControl = null;
    public SliderWithEcho mCylinderRotControl = null;
    public SliderWithEcho mCubeResControl = null;

    void Awake()
    {
        mModellingCtrl = TheModellingWorld.GetComponent<MeshController>();
    }

    void Start () {

        Debug.Assert(camCtrl != null);
        Debug.Assert(SNCtrl != null);
        Debug.Assert(axis != null);
        Debug.Assert(helpTextPanel != null);
        Debug.Assert(mTranslator != null);

        textMessage = helpTextPanel.GetComponentInChildren<Text>();

        Debug.Assert(textMessage != null);

        textMessage.text = "To start, add a node, and Ctrl-Left Click to move it around!";

        Debug.Assert(TheRiggingWorld != null && TheModellingWorld != null);

        /* 3D Modelling Portion */
        mResControl.InitSliderRange(2, 20, 5);
        mResControl.SetSliderListener(ResolutionChange);

        mCylinderResControl.InitSliderRange(4, 20, 10);
        mCylinderResControl.SetSliderListener(CylinderResChange);

        mCylinderRotControl.InitSliderRange(10, 360, 275);
        mCylinderRotControl.SetSliderListener(CylinderRotChange);


        mModellingSelect.onValueChanged.AddListener(ChangeSelection);


        //TODO maybe reconsider
        SwitchView();
        SwitchView();
        ChangeSelection(0);
        
        ToggleVerticeManipulation(false);

        mResControl.gameObject.SetActive(false);
        mCylinderResControl.gameObject.SetActive(false);
        mCylinderRotControl.gameObject.SetActive(false);
        mCubeResControl.gameObject.SetActive(false);


        //Last thing we do
        textMessage.text = "To start, use the \"Starting Object\" dropdown menu to choose your starting primitive shape!";

    }

    void Update()
    {
        SelectionSupport();
    }

    #region Node Stuff (3D Rigging Portion)
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

        if (CustomPrimitives.Count <= 0)
        {
            textMessage.text = "In order to add a primitive, you first have to create one! Click on \"Switch View\" and create a primitive for your game.";
            return;
        }
        

        bool canAddPrimitive = SNCtrl.AddPrimitive(CustomPrimitives[mCustomPrimitiveSelect.value]);

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
    #endregion

    #region Selection
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
                        Debug.Log("Selected Node: " + t.name);
                    } else if (t != null)
                    {
                        Debug.Log("Selected Not Node: " + t.name);
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
    #endregion

    /* Misc. Stuff*/
    public void SwitchView()
    {
        int camIndex = camCtrl.SwitchView();

        switch(camIndex)
        {
            case 0:
                textMessage.text = "Switched to 3D Modelling View!";
                TheRiggingWorld.SetActive(false);
                TheModellingWorld.SetActive(true);

                View1UI.SetActive(true);
                View2UI.SetActive(false);
                break;
            case 1:
                textMessage.text = "Switched to Rigging View!";
                TheRiggingWorld.SetActive(true);
                TheModellingWorld.SetActive(false);
                UpdateCustomPrimitiveDropdown();

                View1UI.SetActive(false);
                View2UI.SetActive(true);

                QuickFix[] toCorrect = View2UI.GetComponentsInChildren<QuickFix>();
                foreach(QuickFix itemToCorrect in toCorrect)
                {
                    itemToCorrect.FixPosition();
                }
                break;
            default:
                break;
        }

    }

    #region Mesh Manipulation (Modelling Portion)
    void ResolutionChange(float n)
    {
        mModellingCtrl.SetMeshResolution((int)n);
        if (mModellingSelect.value == 0)
        {  // currently working with Mesh
            mTranslator.SetTargetTransform(null);
        }
    }

    void CylinderResChange(float n)
    {
        mModellingCtrl.SetCylinderResolution((int)(n));
        if (mModellingSelect.value == 1)
        {
            // currently working with Cylinder
            mTranslator.SetTargetTransform(null);
        }
    }

    void CylinderRotChange(float a)
    {
        mModellingCtrl.SetCylinderRotationAngle(a);
    }

    void ChangeSelection(int c)
    {
        ToggleVerticeManipulation(false);

        if (c == 0)
        {

            

            return;
        }
        else if (c == 1)
        {
            mModellingCtrl.SetSelectMesh();



            mResControl.gameObject.SetActive(true);
            mCylinderResControl.gameObject.SetActive(false);
            mCylinderRotControl.gameObject.SetActive(false);
            mCubeResControl.gameObject.SetActive(false);

            mModellingSelect.value = 0;
        }
        else if (c == 2)
        {
            mModellingCtrl.SetSelectCylinder();

            mResControl.gameObject.SetActive(false);
            mCylinderResControl.gameObject.SetActive(true);
            mCylinderRotControl.gameObject.SetActive(true);
            mCubeResControl.gameObject.SetActive(false);

            mModellingSelect.value = 0;
        } else if (c == 3)
        {
            //TODO set to cube



            mResControl.gameObject.SetActive(false);
            mCylinderResControl.gameObject.SetActive(false);
            mCylinderRotControl.gameObject.SetActive(false);
            mCubeResControl.gameObject.SetActive(true);

            mModellingSelect.value = 0;
        } else
        {
            Debug.Log("ERROR: This statement should not be reached");
        }
    }



    public void PublicToggleVerticeManipulation()
    {
        ToggleVerticeManipulation(verticeManip.isOn);
    }

    private void ToggleVerticeManipulation(bool isOn)
    {
        mModellingCtrl.SetSelectionMode(isOn);
        verticeManip.isOn = isOn;
    }
    #endregion

    //For handling transition from view 1 to 2
    private string toAddName = "";

    public void AddToPrimitivesList()
    {
        GameObject objToAdd = mModellingCtrl.GetSelected();

        if (objToAdd != null && mModellingCtrl.GetObjAvailable())
        {
            GameObject nObj = Instantiate(objToAdd, Vector3.zero, Quaternion.identity);
            Destroy(nObj.GetComponent<CustomMesh>());

            foreach(Transform child in nObj.transform)
            {
                Debug.Log("Destroying: " + child.name);
                Destroy(child.gameObject);
            }

            nObj.transform.position = Vector3.zero;
            nObj.AddComponent<AdvancedNodePrimitive>();

            switch (nObj.tag)
            {
                case "Plane":
                    nObj.transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
                case "Cylinder":
                    nObj.transform.localScale = new Vector3(1f, 2f, 1f);
                    break;
                case "Cube":
                    nObj.transform.localScale = new Vector3(1f, 1f, 1f); //TODO Set this correctly
                    break;
                default:
                    Debug.Log("ERROR: THIS LINE SHOULD NOT BE REACHED");
                    break;
            }


            nObj.transform.localScale = new Vector3(1f, 1f, 1f);

            //We need to update the mesh collider, so we can select it properly with our mouse
            Destroy(nObj.GetComponent<MeshCollider>());
            nObj.AddComponent<MeshCollider>();

            nObj.name = "Primitive No. " + CustomPrimitives.Count;
            CustomPrimitives.Add(nObj);
            nObj.layer = 2; // To no raycast
            nObj.transform.localScale = new Vector3(1f,1f,1f);

            ResetCurrentObj();

        } else
        {
            Debug.Log("Cannot add to list");
        }
    }

    private void ResetCurrentObj()
    {
        //GameObject objToReset = mModellingCtrl.GetSelected();
        mModellingCtrl.ResetMesh();
    }

    private void UpdateCustomPrimitiveDropdown()
    {

        if (CustomPrimitives.Count <= 0)
        {

            Dropdown.OptionData noneAvailable = new Dropdown.OptionData();
            noneAvailable.text = "No Objects Created";

            List<Dropdown.OptionData> empty = new List<Dropdown.OptionData>();
            empty.Add(noneAvailable);

            mCustomPrimitiveSelect.ClearOptions();
            mCustomPrimitiveSelect.AddOptions(empty);

        } else
        {
            mCustomPrimitiveSelect.ClearOptions();
            List<Dropdown.OptionData> nData = new List<Dropdown.OptionData>();
            for (int i = 0; i < CustomPrimitives.Count; i++)
            {
                Dropdown.OptionData toAdd = new Dropdown.OptionData();
                toAdd.text = CustomPrimitives[i].name;

                nData.Add(toAdd);

            }

            mCustomPrimitiveSelect.AddOptions(nData);

        }
    }

}
