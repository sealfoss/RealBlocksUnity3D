using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHighlightController : MonoBehaviour {
    public float overlapSphereRadius = 0.25f;
    public Color selectedColor;
    public Color grabbedColor;
    public Color attachedColor;
    public bool highlightOn;
    public bool highlightGrabbed;
    public bool highlightSelected;
    public bool highlightAttached;

    private InteractiveObjectController owningObject;
    private Renderer rend;

	// Use this for initialization
	void Start () {
        owningObject = GetComponentInParent<InteractiveObjectController>();
        rend = GetComponent<Renderer>();
        SetHighlightOff(false);
        highlightGrabbed = false;
        highlightSelected = false;
        highlightAttached = false;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if(highlightOn && highlightSelected)
        {
            Collider[] overlappedColliders = Physics.OverlapSphere(this.transform.position, overlapSphereRadius);
            bool overlappedController = false;

            foreach(Collider overlapped in overlappedColliders)
            {
                ManipulatorController controller = overlapped.GetComponent<ManipulatorController>();

                if(controller)
                {
                    overlappedController = true;
                    break;
                }
            }

            if(!overlappedController)
            {
                SetHighlightOff(false);
            }
        }

	}

    public void SetHighlightGrabbed(bool propagateToAttached)
    {
        if (!rend) { return; }

        rend.material.color = grabbedColor;

        if (propagateToAttached && owningObject.GetAttachedObjects().Count > 0)
        {
            foreach (InteractiveObjectController obj in owningObject.GetAttachedObjects())
            {
                obj.highlight.SetHighlightGrabbed(propagateToAttached);
            }
        }

        highlightGrabbed = true;
        highlightSelected = false;
        highlightAttached = false;
    }

    public void SetHighlightSelected(bool propagateToAttached)
    {
        if(!rend) { return; }

        rend.material.color = selectedColor;

        if (propagateToAttached && owningObject.GetAttachedObjects().Count > 0)
        {
            foreach (InteractiveObjectController obj in owningObject.GetAttachedObjects())
            {
                obj.highlight.SetHighlightSelected(propagateToAttached);
            }
        }

        highlightGrabbed = false;
        highlightSelected = true;
        highlightAttached = false;
    }

    public void SetHighlightAttached(bool propagateToAttached)
    {
        if (!rend) { return; }

        rend.material.color = attachedColor;

        if (propagateToAttached && owningObject.GetAttachedObjects().Count > 0)
        {
            foreach (InteractiveObjectController obj in owningObject.GetAttachedObjects())
            {
                obj.highlight.SetHighlightAttached(propagateToAttached);
            }
        }

        highlightGrabbed = false;
        highlightSelected = false;
        highlightAttached = true;
    }

    public void SetHighlightOff(bool propagateToAttached)
    {
        if (!rend) { return; }

        rend.gameObject.SetActive(false);

        if(propagateToAttached && owningObject.GetAttachedObjects().Count > 0)
        {
            foreach (InteractiveObjectController obj in owningObject.GetAttachedObjects())
            {
                obj.highlight.SetHighlightOff(propagateToAttached);
            }
        }

        highlightOn = false;
    }

    public void SetHighlightOn(bool propagateToAttached)
    {
        if (!rend) { return; }

        rend.gameObject.SetActive(true);

        if (propagateToAttached && owningObject.GetAttachedObjects().Count > 0)
        {
            foreach (InteractiveObjectController obj in owningObject.GetAttachedObjects())
            {
                obj.highlight.SetHighlightOn(propagateToAttached);
            }
        }

        highlightOn = true;
    }
}
