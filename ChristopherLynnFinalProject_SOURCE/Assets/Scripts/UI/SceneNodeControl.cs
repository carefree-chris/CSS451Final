using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneNodeControl : MonoBehaviour {


    public GameObject TestPrimitive = null;

    public Color activeColor = Color.blue;
    public Color inactiveColor = Color.red;

    public GameObject node = null;
    public GameObject nodeContainer = null;
    public GameObject nodeLine = null;

    public Dropdown TheMenu = null;
    public AdvancedSceneNode TheRoot = null;
    public XFormControl XformControl = null;
    public Translator mTranslator = null;

    private float nodeInitialOffset = 1f;
    private float offsetDirection = 1f;

    AdvancedSceneNode mCurrentSelected = null;

    const string kChildSpace = " ";
    List<Dropdown.OptionData> mSelectMenuOptions = new List<Dropdown.OptionData>();
    List<Transform> mSelectedTransform = new List<Transform>();    

    // Use this for initialization
    void Start () {
        Debug.Assert(TheMenu != null);
        Debug.Assert(TheRoot != null);
        Debug.Assert(XformControl != null);
        Debug.Assert(node != null);
        Debug.Assert(nodeContainer != null);
        Debug.Assert(nodeLine != null);

        /*mSelectMenuOptions.Add(new Dropdown.OptionData(TheRoot.transform.name));
        mSelectedTransform.Add(TheRoot.transform);
        //GetChildrenNames("", TheRoot.transform);
        GetChildrenNames("", TheRoot);
        TheMenu.AddOptions(mSelectMenuOptions);*/
        GenerateList();
        
        TheMenu.onValueChanged.AddListener(SelectionChange);

        mCurrentSelected = TheRoot;
        SelectionChange(0);
    }

    void GenerateList()
    {
        mSelectMenuOptions.Clear();
        mSelectedTransform.Clear();
        TheMenu.ClearOptions();

        mSelectMenuOptions.Add(new Dropdown.OptionData(TheRoot.transform.name));
        mSelectedTransform.Add(TheRoot.transform);
        //GetChildrenNames("", TheRoot.transform);
        GetChildrenNames("", TheRoot);
        TheMenu.AddOptions(mSelectMenuOptions);
    }

    void Update()
    {
        Vector3 pos, dir;
        Matrix4x4 m = Matrix4x4.identity;
        TheRoot.CompositeXform(ref m, out pos, out dir);
    }

    /*void GetChildrenNames(string blanks, Transform node)
    {
        string space = blanks + kChildSpace;
        for (int i = node.childCount - 1; i >= 0; i--)
        {
            Transform child = node.GetChild(i);
            AdvancedSceneNode cn = child.GetComponent<AdvancedSceneNode>();
            if (cn != null)
            {
                mSelectMenuOptions.Add(new Dropdown.OptionData(space + child.name));
                mSelectedTransform.Add(child);
                GetChildrenNames(blanks + kChildSpace, child);
            }
        }
    }*/

    void GetChildrenNames(string blanks, AdvancedSceneNode node)
    {
        string space = blanks + kChildSpace;
        for (int i = node.SceneNodeList.Count - 1; i >= 0; i--)
        {
            
            AdvancedSceneNode cn = node.SceneNodeList[i];
            if (cn != null)
            {
                mSelectMenuOptions.Add(new Dropdown.OptionData(space + cn.transform.name));
                mSelectedTransform.Add(cn.transform);
                GetChildrenNames(blanks + kChildSpace, cn);
            }
        }
    }

    void SelectionChange(int index)
    {
        mCurrentSelected.gameObject.GetComponent<MeshRenderer>().material.color = inactiveColor;
        mCurrentSelected.UnSelect();

        mCurrentSelected = mSelectedTransform[index].GetComponent<AdvancedSceneNode>();
        mCurrentSelected.SetToSelect();

        mCurrentSelected.gameObject.GetComponent<MeshRenderer>().material.color = activeColor;

        XformControl.SetSelectedObject(mSelectedTransform[index]);

        //mTranslator.SetPositionToSelected(mCurrentSelected.transform);
    }

    public AdvancedSceneNode GetRootNode()
    {
        return TheRoot;
    }

    public bool AddNode()
    {
        if (mCurrentSelected == null)
        {
            return false;
        } else
        {

            AdvancedSceneNode nNode = Instantiate(node, nodeContainer.transform).GetComponent<AdvancedSceneNode>();
            nNode.transform.position = new Vector3(mCurrentSelected.transform.position.x + (nodeInitialOffset * offsetDirection),
                mCurrentSelected.transform.position.y - nodeInitialOffset,
                mCurrentSelected.transform.position.z);

            mCurrentSelected.SceneNodeList.Add(nNode);

            GameObject nLine = Instantiate(nodeLine, nodeContainer.transform);
            nLine.GetComponent<NodeLine>().SetLineAttributes(mCurrentSelected.transform, nNode.transform, 0.25f);

            nNode.line = nLine;
            nNode.transform.name = "Lvl" + (TheMenu.value + 1) + "Node";

            offsetDirection *= -1f;

            /*Dropdown.OptionData nData = new Dropdown.OptionData(nNode.transform.name);
            mSelectMenuOptions.Add(nData);
            mSelectedTransform.Add(nNode.transform);
            TheMenu.AddOptions(mSelectMenuOptions);*/


            
            GenerateList();
            //Now, select our new current node
            nNode.transform.parent = mCurrentSelected.transform;
            SelectionChange(TheMenu.value + 1);
            TheMenu.value += 1;

            

            return true;
        }
    }

    public bool RemoveSelectedNode()
    {
        if (mSelectedTransform.IndexOf(mCurrentSelected.transform) == 0)
        {
            return false;
        } else
        {

            AdvancedSceneNode toDelete = mCurrentSelected;

            
            int nIndex = mSelectedTransform.IndexOf(mCurrentSelected.transform) - 1;
            

            toDelete.DeleteSelf();
            GenerateList();
            SelectionChange(nIndex);
            TheMenu.value = nIndex;

            mCurrentSelected.RemoveNode(toDelete);

            return true;
        }
    }

    //Assumes that the input has the right shader
    public bool AddPrimitive()
    {
        if (mCurrentSelected == null)
        {
            return false;
        }
        else
        {
            AdvancedNodePrimitive primitive = Instantiate(TestPrimitive, nodeContainer.transform).GetComponent<AdvancedNodePrimitive>();

            primitive.transform.name = "Primitive_" + mCurrentSelected.transform.name;

            primitive.gameObject.transform.position = mCurrentSelected.transform.position;

            //Debug.Log();
            
            mCurrentSelected.PrimitiveList.Add(primitive.GetComponent<AdvancedNodePrimitive>());
            primitive.transform.parent = mCurrentSelected.transform;
            return true;
        }
    }

    public bool SwitchSelectedNode(AdvancedSceneNode nNode)
    {
        if (mSelectedTransform.Contains(nNode.transform))
        {
            //Debug.Log("CONTAINS TIS NODE: " + mSelectedTransform.IndexOf(nNode.transform));
            SelectionChange(mSelectedTransform.IndexOf(nNode.transform));
            TheMenu.value = mSelectedTransform.IndexOf(nNode.transform);
            return true;
        } else
        {
            return false;
        }
    }

}
