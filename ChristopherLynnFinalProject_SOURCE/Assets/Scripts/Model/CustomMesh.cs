using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMesh : MonoBehaviour {
    private float kInitSize = 10f;

    protected void Awake()
    {
        //InitializeTextureTransform();
    }

    // Use this for initialization
    protected void Start()
    {
        mShowMeshPts = new GameObject("ShowMesh");
        mShowNormals = new GameObject("ShowNormals");
        mShowMeshPts.transform.SetParent(transform);
        mShowNormals.transform.SetParent(transform);

        

        transform.localScale = new Vector3(kInitSize, kInitSize, kInitSize);

        BuildNewMesh();

        SetShowMarkers(false);
        SetShowNormals(false);
    }

    // Update is called once per frame
    protected void Update()
    {
        Mesh m = GetComponent<MeshFilter>().mesh;
        Vector3[] v = m.vertices;
        Vector3[] n = m.normals;
        Vector2[] uv = m.uv;
        int[] t = m.triangles;

        // OK, this is stupid and very inefficient, but ... for now
        ComputeMeshNormal(v, n);
        UpdateDrawElements(v, n);
        //UpdateTextureTransform(uv);

        m.Clear();
        m.vertices = v;
        m.normals = n;
        m.uv = uv;
        m.triangles = t;
    }

    #region Selection Support

    private Transform mCurrentSelected = null;
    Color kSelectedColor = Color.red;

    private int kSelectLayerMask = 0x1 << kSelectionLayer;

    void SetSelected(Transform t)
    {
        if (mCurrentSelected != null)
            mCurrentSelected.gameObject.GetComponent<Renderer>().material.color = kMarkerColor;

        mCurrentSelected = t;
        if (mCurrentSelected != null)
        {
            mCurrentSelected.gameObject.GetComponent<Renderer>().material.color = kSelectedColor;
        }
    }

    public void SetShowMarkers(bool t)
    {
        bool show = t || (mCurrentSelected != null);
        mShowMeshPts.SetActive(show);
        SetShowNormals(show); // for now
    }
    public void SetShowNormals(bool t) { mShowNormals.SetActive(t); }

    public Transform SelectAMarker(Ray aRay)
    {
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(aRay, out hitInfo, Mathf.Infinity, kSelectLayerMask);
        if (hit)
        {
            string name = hitInfo.transform.gameObject.name;
            SetSelected(hitInfo.transform);
            Debug.Log("RayHit: " + name);
        }
        else
        {
            SetSelected(null);
        }
        return mCurrentSelected;
    }
    #endregion

    #region Normal Support
    TriangleIndices ti = new TriangleIndices(); // this is to conserve number of times we need to new

    // (x,y): column/row lower-left position, ul: either upper(1) or lower(0)
    //      computes the normal vector for this triangle
    Vector3 TriangleNormal(Vector3[] v, int x, int y, int ul)
    {
        GetTriangleIndices(x, y, ref ti);
        Vector3 v1 = v[ti.v[ul, 2]] - v[ti.v[ul, 0]];
        Vector3 v2 = v[ti.v[ul, 1]] - v[ti.v[ul, 0]];
        return Vector3.Cross(v2, v1);
    }

    void ComputeMeshNormal(Vector3[] v, Vector3[] n)
    {
        int x, y, index;
        for (y = 0; y < (mResolution - 1); y++)
            for (x = 0; x < (mResolution - 1); x++)
            {
                int count = 0;
                index = PosToVertexIndex(x, y);

                // 1. get the ul of (x, y)
                n[index] = TriangleNormal(v, x, y, kLowerTriangleFlag);
                n[index] += TriangleNormal(v, x, y, kUpperTriangleFlag);
                count += 2;

                if (x > 0)
                {
                    n[index] += TriangleNormal(v, x - 1, y, kLowerTriangleFlag);
                    count++;
                }
                if (y > 0)
                {
                    n[index] += TriangleNormal(v, x, y - 1, kUpperTriangleFlag);
                    count++;
                }
                if (count == 4) // this means both x and y > 0
                {
                    n[index] += TriangleNormal(v, x - 1, y - 1, kLowerTriangleFlag);
                    n[index] += TriangleNormal(v, x - 1, y - 1, kUpperTriangleFlag);
                    count += 2;
                }
                n[index].Normalize();
            }

        // now the edge cases:
        //   1. Bottom-Right (only one triangle) (Resolution - 1, 0)
        //   2. Top-Left (only one triangles)
        index = mResolution - 1;  // Bottom Right
        n[index] = TriangleNormal(v, mResolution - 2, 0, kLowerTriangleFlag);
        n[index].Normalize();

        index = (mResolution - 1) * mResolution; // Top-Left
        n[index] = TriangleNormal(v, 0, mResolution - 2, kUpperTriangleFlag);
        n[index].Normalize();

        // now the vertices at right edges (does not incldue the bottom-right vertex)
        //    for this last column: (Resolution-1, y)
        //    Triangles are the two from (Resolution-2, y-1) AND lower from the y above
        x = mResolution - 1;  // x is always Resoution - 1
        for (y = 1; y < mResolution; y++)
        {
            // we are always interested in triangles from (x-1, y-1)
            index = PosToVertexIndex(x, y);
            n[index] = TriangleNormal(v, x - 1, y - 1, kLowerTriangleFlag);
            n[index] += TriangleNormal(v, x - 1, y - 1, kUpperTriangleFlag);
            if (y < (mResolution - 1))
                n[index] += TriangleNormal(v, x - 1, y, kLowerTriangleFlag);
            n[index].Normalize();
        }

        // now the top row
        y = mResolution - 1;
        for (x = 1; x < mResolution - 1; x++) // does not include the two corners (done with those)
        {
            index = PosToVertexIndex(x, y);
            n[index] = TriangleNormal(v, x - 1, y - 1, kLowerTriangleFlag);
            n[index] += TriangleNormal(v, x - 1, y - 1, kUpperTriangleFlag);
            n[index] += TriangleNormal(v, x, y - 1, kUpperTriangleFlag);
            n[index].Normalize();
        }
    }
    #endregion

    #region Draw Support
    Vector3 kMarkerSize = new Vector3(0.1f, 0.1f, 0.1f);
    Color kMarkerColor = Color.white;

    const float kNormalHeight = 0.5f;
    const float kNormalRadius = 0.05f;
    Vector3 kNormalSize = new Vector3(kNormalRadius, kNormalHeight, kNormalRadius);
    Color kNormalColor = Color.white;


    private GameObject mShowMeshPts;  // for showing the sphere positions
    private GameObject mShowNormals;  // for showing the normals

    protected GameObject[] mMarkers = null;
    protected LineSegment[] mNormals = null;
    private const string kSelectionSphereName = "SelectionSphere";
    private const int kSelectionLayer = 20;

    void CreateDrawSupport()
    {
        if (mMarkers != null)
        {
            // assume the same for normal
            for (int i = 0; i < mMarkers.Length; i++)
            {
                Destroy(mNormals[i].gameObject);
                Destroy(mMarkers[i]);
            }
        }
        mMarkers = new GameObject[mNumVertices];
        mNormals = new LineSegment[mNumVertices];
        // Crete the vertex markers and normal vectors
        for (int i = 0; i < mNumVertices; i++)
        {
            mMarkers[i] = CreateMarker();
            mNormals[i] = CreateNormal();
        }
    }

    // Update functions
    void UpdateDrawElements(Vector3[] v, Vector3[] n)
    {
        // check to make sure we need to do the work
        if (mShowMeshPts.activeSelf)
        {
            for (int i = 0; i < mNumVertices; i++)
                v[i] = mMarkers[i].transform.localPosition;
        }

        if (mShowNormals.activeSelf)
        {
            for (int i = 0; i < mNumVertices; i++)
                mNormals[i].SetEndPoints(v[i], v[i] + kNormalHeight * n[i]);
        }
    }

    // Creation functions ...
    GameObject CreateMarker()
    {
        GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        o.name = kSelectionSphereName;
        o.layer = kSelectionLayer;
        o.transform.SetParent(mShowMeshPts.transform);
        o.transform.localScale = kMarkerSize;
        o.GetComponent<Renderer>().material.color = kMarkerColor;
        return o;
    }

    LineSegment CreateNormal()
    {
        GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        LineSegment l = o.AddComponent<LineSegment>();
        l.SetWidth(kNormalRadius);
        o.transform.SetParent(mShowNormals.transform);
        o.GetComponent<Renderer>().material.color = kNormalColor;
        o.GetComponent<CapsuleCollider>().enabled = false;
        return l;
    }
    #endregion

    #region Create Mesh
    protected int mResolution = 5;
    // Resolution x Resolution number of vertices
    // R-1 x R-1 x 2 number of triangles

    protected int mNumVertices;
    protected int mNumTriangles;

    private const int kLowerTriangleFlag = 0;
    private const int kUpperTriangleFlag = 1;

    class TriangleIndices
    {
        public int[,] v;  // vertices, 0 is for lower, 1 is for upper
        public TriangleIndices()
        {
            v = new int[2, 3] { { 0, 0, 0 }, { 0, 0, 0 } };
        }
    };

    public void SetResolution(int n)
    {
        if ((n < 2) || (n == mResolution))
            return; // ignore

        mResolution = n;
        BuildNewMesh();

    }
    // Assume Resolution is a good number (>= 2)
    bool BuildNewMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        int nv = mResolution * mResolution;
        if (nv == mesh.vertices.Length)  // Assume we are good.
            return false;

        // other wise ...        
        mNumTriangles = (mResolution - 1) * (mResolution - 1) * 2 * 3;  // these are the vertices of the triangles
        mNumVertices = nv;
        Debug.Log("Mesh init:" + mNumTriangles + " " + mNumVertices);

        // set the vertices (x, 0, z)
        Vector3[] v = new Vector3[mNumVertices];
        Vector3[] n = new Vector3[mNumVertices];
        Vector2[] uv = new Vector2[mNumVertices];
        CreateDrawSupport();
        //ComputeUV(uv);
        ComputeVertex(v, n);


        // set the triangles:
        //    r/c = row/column , 0 th n-1
        //    each location, has u/l upper/lower triangle
        int[] t = new int[mNumTriangles];
        TriangleIndices ti = new TriangleIndices();
        for (int y = 0; y < (mResolution - 1); y++)
            for (int x = 0; x < (mResolution - 1); x++)
            {

                GetTriangleIndices(x, y, ref ti);
                int index = PosToTriangleIndex(x, y, kLowerTriangleFlag);

                for (int i = 0; i < 3; i++)
                {
                    t[index + i] = ti.v[0, i]; // lower triangle

                    t[index + 3 + i] = ti.v[1, i]; // upper triangle
                }
            }

        // create a new mesh component
        mesh.Clear();

        mesh.vertices = v;
        mesh.triangles = t;
        mesh.normals = n;
        mesh.uv = uv;

        UpdateDrawElements(v, n);
        return true;

        /*
            NEVER do this:
                mesh.colors = new Color[3];
                mesh.colors[0] = new Color(0, 1, 0);
            
            The above DOES NOT WORK!! YOU MUST FIRST allocate and set the arrays BEFORE
            assigning to mesh.WHATEVER

            I believe, during the assignment, mesh initialize stuff!
          */
    }

    // in all of the following:
    // row      --> y-value (i-values)
    // column   --> x-value (j-values)

    // (x, y): column/row index of a vertex
    // returns the 1D index of Vertex[]
    protected int PosToVertexIndex(int x, int y)
    {
        return x + (y * mResolution);
    }

    // (x, y): column/row of the lower-left corner
    // ul - 0 for lower and 1 for upper
    // returns the index of the triangle-array, 
    //      the design is such that:
    //      Triangle-Low: index   to index+2
    //      Triangle-hi : index+3 to index+5
    // 
    protected int PosToTriangleIndex(int x, int y, int ul)
    {
        return ((2 * (x + (y * (mResolution - 1)))) + ul) * 3;
    }

    // (x,y): column/row of lower left corner
    // returns the triangle[] indices for both lower [0] and upper [1]
    void GetTriangleIndices(int x, int y, ref TriangleIndices ti)
    {
        int yn = y * mResolution;
        int y1n = (y + 1) * mResolution;

        ti.v[0, 0] = yn + x;      // Lower triangle
        ti.v[0, 1] = y1n + x + 1;
        ti.v[0, 2] = yn + x + 1;

        ti.v[1, 0] = yn + x;      // upper triangle
        ti.v[1, 1] = y1n + x;
        ti.v[1, 2] = y1n + x + 1;
    }

    virtual protected void ComputeVertex(Vector3[] v, Vector3[] n)
    {
        float delta = 2f / (float)(mResolution - 1);
        for (int y = 0; y < mResolution; y++)
        {
            float yVal = -1f + y * delta;
            for (int x = 0; x < mResolution; x++)
            {
                float xVal = -1f + (x * delta);
                int index = PosToVertexIndex(x, y);
                v[index] = new Vector3(xVal, 0, yVal);
                n[index] = new Vector3(0, 1f, 0);

                mMarkers[index].transform.localPosition = v[index];
            }
        }
    }
    #endregion
}
