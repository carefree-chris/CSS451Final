using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour {
    // we expect this is bounded to a Selector GameObject
    // with empty object as parent and X/Y/Z/Origin children

    enum TranslateMode
    {
        TranslateInX,
        TranslateInY,
        TranslateInZ,
        NoTranslate
    };
    
    private const int kSelectorLayer = 21;
    private const int kSelectorLayerMask = 1 << kSelectorLayer;
    private TranslateMode mCurrentMode = TranslateMode.NoTranslate;
    private Vector3 mInitMousePosition = Vector3.zero;
    private const float kMouseScaleFactor = 0.01f;

    private Transform mTarget = null;
    private Color XColor = Color.red;
    private Color YColor = Color.green;
    private Color ZColor = Color.blue;
    private Color SelectedColor = Color.yellow;

	// Use this for initialization
	void Start () {
        foreach (Transform child in transform)
            child.transform.gameObject.layer = kSelectorLayer;
        SetChildrenColor();
        
        transform.gameObject.SetActive(false);
	}
	
    public void SetTargetTransform(Transform t)
    {
        mTarget = t;
        transform.gameObject.SetActive(t != null);
        if (mTarget != null)
        {
            transform.localPosition = t.position;
        }
    }

    public void ResetSelectMode()
    {
        mCurrentMode = TranslateMode.NoTranslate;
        SetChildrenColor();
    }

    public bool SelectMode(Ray aRay)    // return the status of consuming the mouse
    {
        if (mTarget == null)
            return false;

        SetChildrenColor();
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(aRay, out hitInfo, Mathf.Infinity, kSelectorLayerMask);
        if (hit)
        {
            string hitName = hitInfo.transform.name;
            //Debug.Log("RayHit: " + hitName);
            mInitMousePosition = Input.mousePosition;

            if (hitName == "AxisX")
                mCurrentMode = TranslateMode.TranslateInX;
            else if (hitName == "AxisY")
                mCurrentMode = TranslateMode.TranslateInY;
            else if (hitName == "AxisZ")
                mCurrentMode = TranslateMode.TranslateInZ;
            else
                mCurrentMode = TranslateMode.NoTranslate;

            if (mCurrentMode != TranslateMode.NoTranslate)
                hitInfo.transform.GetComponent<Renderer>().material.color = SelectedColor;
        }
        return (mCurrentMode != TranslateMode.NoTranslate);
    }

    public void DragTargetTranslation()
    {
        if (mTarget == null)
            return;

        if (mCurrentMode != TranslateMode.NoTranslate)
        {   // this means, one of the translation axis is already selected
            Vector3 delta = kMouseScaleFactor * (Input.mousePosition - mInitMousePosition);
            mInitMousePosition = Input.mousePosition;
            switch (mCurrentMode)
            {
                case TranslateMode.TranslateInX:
                    delta.y = delta.z = 0;
                    break;
                case TranslateMode.TranslateInY:
                    delta.x = delta.z = 0;
                    break;
                case TranslateMode.TranslateInZ:
                    delta.z = delta.x; //changed this
                    delta.y = delta.x = 0;
                    break;
            }
            mTarget.position += delta;
            transform.position = mTarget.position;
        }
    }

    void SetChildrenColor()
    {
        transform.FindChild("AxisX").gameObject.GetComponent<Renderer>().material.color = Color.red;
        transform.FindChild("AxisY").gameObject.GetComponent<Renderer>().material.color = Color.green;
        transform.FindChild("AxisZ").gameObject.GetComponent<Renderer>().material.color = Color.blue;
    }

    public void SetPositionToSelected(Transform target)
    {
        transform.position = target.position;
    }
}
