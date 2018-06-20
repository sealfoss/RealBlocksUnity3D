using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour {

    private InteractiveObjectController owningObject;
    private Renderer rend;
    public Color blockColor;
    public bool randomColor;

    // Use this for initialization
    void Start ()
    {
        owningObject = this.GetComponent<InteractiveObjectController>();
        rend = GetComponent<Renderer>();
        
        if(randomColor)
        {
            blockColor = GetRandomColor();
        }

        rend.material.color = blockColor;
    }
	
	// Update is called once per frame
	void Update () {
	}

    // helpers

    private Color GetRandomColor()
    {

        Color rando = Random.ColorHSV(0, 1, 0.5f, 1, 0.75f, 1);
        return rando;

        //return new Color(r, g, b);
    }
    
       
}
