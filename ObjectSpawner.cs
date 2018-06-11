using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour {
    public HashSet<GameObject> spawnedObjects = new HashSet<GameObject>();
    public List<GameObject> objectsToSpawn = new List<GameObject>();
    public GameObject owningObject;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void SpawnElement(int element, Quaternion spawnRot)
    {
        if(objectsToSpawn[element])
        {
            GameObject newObject = Instantiate(objectsToSpawn[element], this.transform.position, spawnRot);
            spawnedObjects.Add(newObject);
        }
        else
        {
            Debug.LogError("Element " + element + " does not exist in objects spawner for " + owningObject.name + ".");
        }
    }

    public void SpawnRandom()
    {
        int randomElement = Random.Range(0, objectsToSpawn.Count);
        Quaternion spawnRot = Random.rotation;
        SpawnElement(randomElement, spawnRot);
    }

    public void DeleteObject(GameObject objectToDelete)
    {
        spawnedObjects.Remove(objectToDelete);
        Destroy(objectToDelete);
    }
}
