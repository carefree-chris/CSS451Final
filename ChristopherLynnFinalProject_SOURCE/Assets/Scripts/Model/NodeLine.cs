using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeLine : MonoBehaviour {

    [SerializeField] private Transform endPt1;
    [SerializeField] private Transform endPt2;

    private float width = 1f;

    public void SetLineAttributes(Transform pt1, Transform pt2, float lineWidth)
    {
        if (lineWidth < 0f)
        {
            return;
        }

        width = lineWidth;
        endPt1 = pt1;
        endPt2 = pt2;
    }

    void UpdateLine()
    {
        float length = (endPt2.position - endPt1.position).magnitude / 2f;

        Vector3 direction = (endPt2.position - endPt1.position);

        transform.localScale = new Vector3(width, length, width);
        transform.position = endPt1.position + (direction.normalized * length);
        transform.up = (direction.normalized);
    }
	
	// Update is called once per frame
	void Update () {
        UpdateLine();
	}
}
