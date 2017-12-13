using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrimitiveControl : MonoBehaviour {

    public Dropdown TheMenu = null;
    public XFormControl XformControl = null;

    AdvancedSceneNode mSelectedNode = null;
    AdvancedNodePrimitive mSelectedPrimitive = null;

    List<Dropdown.OptionData> mSelectMenuOptions = new List<Dropdown.OptionData>();
    List<Transform> mSelectedTransform = new List<Transform>();

    

    public void InitializeNodeCtrl(AdvancedSceneNode root)
    {
        GenerateList(root);
        TheMenu.onValueChanged.AddListener(SelectionChange);
        SelectionChange(0);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void SelectionChange(int index)
    {

        if (mSelectedNode == null || mSelectedNode.PrimitiveList.Count <= 0)
            return;

        
        mSelectedPrimitive = mSelectedTransform[index].GetComponent<AdvancedNodePrimitive>();

        XformControl.SetSelectedObject(mSelectedTransform[index]);

        //mTranslator.SetPositionToSelected(mCurrentSelected.transform);
    }

    public void GenerateList(AdvancedSceneNode currentNode)
    {

        mSelectedNode = currentNode;

        mSelectMenuOptions.Clear();
        mSelectedTransform.Clear();
        TheMenu.ClearOptions();

        if (currentNode.PrimitiveList.Count <= 0)
        {
            mSelectMenuOptions.Add(new Dropdown.OptionData("No Primitives Available"));
        } else
        {

            for (int i = 0; i < currentNode.PrimitiveList.Count; i++)
            {
                mSelectMenuOptions.Add(new Dropdown.OptionData(currentNode.PrimitiveList[i].transform.name));
                mSelectedTransform.Add(currentNode.PrimitiveList[i].transform);
            }

        }

        SelectionChange(0);
        TheMenu.AddOptions(mSelectMenuOptions);
    }
}
