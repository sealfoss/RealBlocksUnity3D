using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public delegate void OnGripButtonDown();
    public static event OnGripButtonDown onGripButtonDown;
    public void GrabOnGripButtonDown()
    {
        if(onGripButtonDown != null)
        {
            onGripButtonDown();
        }
    }
	
    
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
