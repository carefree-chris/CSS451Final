using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour {

    public Material NoTexture = null;
    public Material BrickTexture = null;
    public Material EmeraldTexture = null;
    public Material ScalesTexture = null;
    public Material CanvasTexture = null;


    public CustomMesh mTheMesh;
    public CustomCylinder mTheCylinder;
    public CustomCube mTheCube;

    private CustomMesh mSelected;

    private bool objectAvailable = false;

    // Use this for initialization
    void Start()
    {
        SetSelectMesh();

        SetSelectNone();
    }

    //

    public void SetSelectNone()
    {
        //mSelected = null;
        objectAvailable = false;
        mTheMesh.gameObject.SetActive(false);
        mTheCylinder.gameObject.SetActive(false);
    }

    public void SetSelectMesh()
    {
        objectAvailable = true;
        mSelected = mTheMesh;
        mTheMesh.gameObject.SetActive(true);
        mTheCylinder.gameObject.SetActive(false);
    }

    public void SetSelectCylinder()
    {
        objectAvailable = true;
        mSelected = mTheCylinder;
        mTheMesh.gameObject.SetActive(false);
        mTheCylinder.gameObject.SetActive(true);
    }

    //TODO BOX

    // operate on selected
    public void SetSelectionMode(bool on) {
        if (mSelected == null)
            return; //We may have to fix this.
        mSelected.SetShowMarkers(on);
    }
    public Transform SelectAMarker(Ray r) {
        return mSelected.SelectAMarker(r); }

    // Mesh specific
    public void SetMeshResolution(int n) { mTheMesh.SetResolution(n); }
    //public Transform GetMeshTextureXform() { return mTheMesh.GetTextureTransform(); }

    // cylinder specific
    public void SetCylinderRotationAngle(float n)
    {
        mTheCylinder.SetShowMarkers(true);
        mTheCylinder.SetRotationAngle(n);
    }
    public void SetCylinderResolution(int n) { mTheCylinder.SetResolution(n); }

    public GameObject GetSelected()
    {
        return mSelected.gameObject;
    }

    public bool GetObjAvailable()
    {
        return objectAvailable;
    }

    public void SetTextureNoTexture()
    {
        if(mSelected != null)
        {
            mSelected.GetComponent<Renderer>().material = NoTexture;
        }
    }

    public void SetTextureBrickTexture()
    {
        if (mSelected != null)
        {
            mSelected.GetComponent<Renderer>().material = BrickTexture;
        }
    }

    public void SetTextureEmeraldTexture()
    {
        if (mSelected != null)
        {
            mSelected.GetComponent<Renderer>().material = EmeraldTexture;
        }
    }

    public void SetTextureScalesTexture()
    {
        if (mSelected != null)
        {
            mSelected.GetComponent<Renderer>().material = ScalesTexture;
        }
    }

    public void SetTextureCanvasTexture()
    {
        if (mSelected != null)
        {
            mSelected.GetComponent<Renderer>().material = CanvasTexture;
        }
    }

    public void ResetMesh()
    {
        if (mSelected != null)
            mSelected.ResetSelf();

        SetTextureNoTexture();
    }


}
