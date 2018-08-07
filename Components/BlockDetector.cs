using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDetector : MonoBehaviour {

    BlockController owningBlock { get { return GetComponentInParent<BlockController>(); } }
    HashSet<BlockController> detectedBlocks = new HashSet<BlockController>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        BlockController otherBlock = other.GetComponent<BlockController>();

        if(otherBlock && otherBlock.GetAttached() && otherBlock != owningBlock)
        {
            detectedBlocks.Add(otherBlock);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BlockController otherBlock = other.GetComponent<BlockController>();

        if (otherBlock)
        {
            detectedBlocks.Remove(otherBlock);
        }
    }

    public HashSet<BlockController> GetDetectedBlocks()
    {
        return detectedBlocks;
    }
}
