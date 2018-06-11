using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFloorController : MonoBehaviour {
    public InteractiveObjectController floorObject;
    private int lastCount = 0;
    public ObjectSpawner spawner;
    public int allowableBlockOverflowAmount = 2;
    public int minimumSpawnedBlocks = 2;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        DetectSpawn();
	}

    private void DetectSpawn()
    {

        //need to change this to some sort of event based system
        int currentCount = floorObject.GetAttachedObjects().Count;
        HashSet<GameObject> spawnedObjects = spawner.spawnedObjects;
        //Debug.Log("spawned objects: " + spawnedObjects.Count + ", minimum blocks: " + minimumSpawnedBlocks);

        if (spawnedObjects.Count < minimumSpawnedBlocks)
        {
            Debug.Log("BELOW MINIMUM!");
            for (int i = 0; i < minimumSpawnedBlocks; i++)
            {
                spawner.SpawnRandom();
            }
        }
        else
        {
            if (lastCount != currentCount)
            {
                if (currentCount > lastCount)
                {
                    spawner.SpawnRandom();
                }

                if (currentCount < lastCount)
                {
                    if (spawnedObjects.Count - currentCount > allowableBlockOverflowAmount)
                    {
                        foreach (GameObject spawnedObject in spawnedObjects)
                        {
                            InteractiveObjectController interactiveObject = spawnedObject.GetComponent<InteractiveObjectController>();

                            if (!interactiveObject.grabbed && !interactiveObject.attached)
                            {
                                spawner.DeleteObject(spawnedObject);
                                break;
                            }
                        }
                    }
                }
            }
        }

        lastCount = currentCount;
    }
}
