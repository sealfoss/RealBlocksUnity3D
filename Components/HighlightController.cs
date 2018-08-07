using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightController : MonoBehaviour {
    public GameObject highlightMesh;
    private Renderer rend { get {  return highlightMesh.GetComponent<Renderer>(); } }


    // Use this for initialization
    void Start () {
        highlightMesh.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TurnHighlightOn(Color highlightColor)
    {
        highlightMesh.SetActive(true);
        rend.material.color = highlightColor;
    }

    public void TurnHighlightOff()
    {
        highlightMesh.SetActive(false);
    }

    public Color GetHighlightColor()
    {
        return rend.material.color;
    }

    public bool GetHighlightOn()
    {
        return highlightMesh.activeSelf;
    }
}
