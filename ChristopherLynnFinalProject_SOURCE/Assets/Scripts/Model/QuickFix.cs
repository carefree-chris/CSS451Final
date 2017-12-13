using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickFix : MonoBehaviour {

   private RectTransform s1;
    private float direction = 1f;

   // Use this for initialization
	void Start () {
        FixPosition();
	}

    public void FixPosition()
    {
        s1 = GetComponent<RectTransform>();
        Debug.Log("Setting Position of " + gameObject.name + " to " + s1.anchoredPosition);
        s1.anchoredPosition = new Vector3(s1.anchoredPosition.x + (1f * direction), s1.anchoredPosition.y);
        direction *= -1;

    }
	
}
