using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour, IObjectController, IAttachable {
    
    private Renderer rend;
    public Color blockColor = Color.blue;
    private HighlightController highlighter { get { return GetComponent<HighlightController>(); } }
    public bool randomColor = false;
    private bool attached = false;
    public float blockMass = 2.0f;
    private Rigidbody thisRigidBody { get { return GetComponent<Rigidbody>(); } }
    private ObjectMover mover { get { return GetComponent<ObjectMover>(); } }

    // attachment stuff
    AttachmentPlug attachedTo = null;
    BlockController rootBlock = null;
    HashSet<BlockController> blocksBelow = new HashSet<BlockController>();
    HashSet<BlockController> blocksAbove = new HashSet<BlockController>();
    public BlockDetector bottomDetector;
    private bool busy = false;
    private float attachmentGrabOffset = 0.25f;

    // Use this for initialization
    void Start ()
    {
        rend = GetComponent<Renderer>();
        if(randomColor) { blockColor = Random.ColorHSV(0, 1, 0.5f, 1, 0.75f, 1); }
        rend.material.color = blockColor;
        thisRigidBody.mass = blockMass;
    }
	
	// Update is called once per frame
	void Update () {
	}

    // events

    public void Activate()
    {
        if(attached) { Detach(); }
    }

    public void Deactivate()
    {
        // placeholder
    }

    

    public bool AttachTo(GameObject otherObject, IAttachmentPoint attachmentPoint)
    {
        // check for null pointers
        if(!otherObject) { return false; }
        BlockController otherBlock = otherObject.GetComponent<BlockController>();
        if(!otherBlock) { return false; }
        AttachmentPlug plug = attachmentPoint.GetGameObject().GetComponent<AttachmentPlug>();
        if(!plug) { return false; }

        attachedTo = plug;
        DestroyRigidbody();
        EnableCollision();
        BlockController newRoot = null;

        if (otherBlock.rootBlock)
        {
            newRoot = otherBlock.rootBlock;
        }
        else
        {
            newRoot = otherBlock;
        }

        Reroot(newRoot);
        attached = true;
        return true;
    }

    public void Detach()
    {
        Reroot(this);
        attached = false;
        attachedTo.Activate(null);
    }

    public void ConnectBlocks()
    {
        blocksBelow = bottomDetector.GetDetectedBlocks();

        foreach(BlockController below in blocksBelow)
        {
            if (!below.blocksAbove.Contains(this)) { below.blocksAbove.Add(this); }

            foreach(BlockController above in blocksAbove)
            {
                if (!below.blocksAbove.Contains(above)) { below.blocksAbove.Add(above); }
            }
        }
    }

    public void DisconnectBlocks()
    {
        foreach(BlockController below in blocksBelow)
        {
            below.blocksAbove.Remove(this);

            foreach (BlockController above in blocksAbove)
            {
                below.blocksAbove.Remove(above);
            }
        }
    }

    public void Reroot(BlockController newRoot)
    {
        if(newRoot == this)
        {
            transform.SetParent(null);
            rootBlock = null;
        }
        else
        {
            rootBlock = newRoot;
            transform.SetParent(rootBlock.transform);
        }

        this.GetComponent<Collider>().isTrigger = false;

        if (blocksAbove.Count > 0)
        {
            foreach(BlockController above in blocksAbove)
            {
                above.Reroot(newRoot);
            }
        }
    }

    // helpers

    public void InitializeRigidbody()
    {
        if (!thisRigidBody) { gameObject.AddComponent<Rigidbody>(); }
        if (thisRigidBody.mass != blockMass) { thisRigidBody.mass = blockMass; }
    }

    public void DestroyRigidbody()
    {
        if (thisRigidBody) { Destroy(this.GetComponent<Rigidbody>()); }
    }

    public void KillPhysics()
    {
        if(thisRigidBody) { thisRigidBody.isKinematic = true; }
        //Debug.Log("Kill Physics, rigidbody is: " + thisRigidBody.isKinematic);
    }

    public void EnablePhysics()
    {
        InitializeRigidbody();
        thisRigidBody.isKinematic = false;
        //Debug.Log("Enable Physics, rigidbody is: " + thisRigidBody.isKinematic);
    }

    public void KillCollision()
    {
        this.GetComponent<Collider>().isTrigger = true;
    }

    public void EnableCollision()
    {
        this.GetComponent<Collider>().isTrigger = false;
    }

    public void IgnoreCollision(Collider otherCollider, bool ignore)
    {
        Physics.IgnoreCollision(this.GetComponent<BoxCollider>(), otherCollider, ignore);
    }

    public void SetBusy(bool newBusy)
    {
        busy = newBusy;
    }

    // getters

    public bool GetAttached()
    {
        return attached;
    }

    public IObjectController GetRootObject()
    {
        if(rootBlock) { return rootBlock; }
        else { return this; }
    }

    public ObjectMover GetObjectMover()
    {
        return mover;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public bool GetBusy() // giggity.
    {
        return busy;
    }

    public float GetAttachmentGrabOffset()
    {
        return attachmentGrabOffset;
    }
    
    
}
