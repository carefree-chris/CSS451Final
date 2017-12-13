using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCylinder : CustomMesh {

    private float kInitHeight = 10;
    private float kInitRadius = 5;

    private float mRotAngle = Mathf.PI * 1.5f; // by default
    protected new void Awake()
    {
        mResolution = 10;
        base.Awake();

        base.initRes = mResolution;
    }

    // Use this for initialization
    protected new void Start()
    {
        base.Start();
        transform.localScale = new Vector3(kInitRadius, kInitHeight, kInitRadius);
    }

    // Update is called once per frame
    protected new void Update()
    {
        UpdateCylinder();
        base.Update();
    }

    public void SetRotationAngle(float a)
    {
        mRotAngle = a * Mathf.Deg2Rad;
        Update();
    }

    void UpdateCylinder()
    {
        float deltaTheta = mRotAngle / (float)(mResolution - 1);
        for (int y = 0; y < mResolution; y++)
        {
            int index = PosToVertexIndex(0, y);
            Vector3 p = mMarkers[index].transform.localPosition;
            float r = Mathf.Sqrt(p.x * p.x + p.z * p.z);
            for (int x = 1; x < mResolution; x++)
            {
                float theta = x * deltaTheta;
                float xVal = r * Mathf.Cos(theta);
                float zVal = r * Mathf.Sin(theta);
                index = PosToVertexIndex(x, y);
                mMarkers[index].transform.localPosition = new Vector3(xVal, p.y, zVal);
            }
        }
    }

    override protected void ComputeVertex(Vector3[] v, Vector3[] n)
    {
        float delta = 2f / (float)(mResolution - 1);
        float deltaTheta = mRotAngle / (float)(mResolution - 1);
        for (int y = 0; y < mResolution; y++)
        {
            float yVal = -1f + y * delta;
            for (int x = 0; x < mResolution; x++)
            {
                float theta = x * deltaTheta;
                float xVal = Mathf.Cos(theta);
                float zVal = Mathf.Sin(theta);
                int index = PosToVertexIndex(x, y);
                v[index] = new Vector3(xVal, yVal, zVal);
                n[index] = new Vector3(xVal, 0, zVal);

                mMarkers[index].transform.localPosition = v[index];
                if (x != 0)
                {
                    mMarkers[index].layer = 0; // set to default layer
                    mMarkers[index].GetComponent<Renderer>().material.color = Color.black;
                    mMarkers[index].GetComponent<SphereCollider>().enabled = false;
                }
            }
        }
    }

    
}
