using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerTractorBeam : MonoBehaviour {
    //public ParticleSystem sparksPrimary;
    public bool activated = false;
    public int blocksLayer = 10;
    private int layerMask = 1;
    public GameObject localOrb;
    public GameObject distantOrb;
    public ParticleSystem localEffects;
    public ParticleSystem distantEffects;
    public ParticleSystem beamEffects;
    public InteractiveObjectController hitObject = null;
    private bool distantOn = false;
    private bool localOn = false;
    public float sparksEmissionRate = 10;
    public float beamEmmissionRate = 10;

    // Use this for initialization
	void Start () {
        layerMask = 1 << blocksLayer;
        localEffects.Stop();
        distantEffects.Stop();
        beamEffects.Stop();
        localOrb.SetActive(false);
        distantOrb.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (activated)
        {
            if(!localOn) { LocalOn(); }
            Beam();
        }
        else
        {
            if (localOn) { LocalOff(); }
            if (distantOn) { DistantOff(); }
        }
    }


    private void Beam()
    {
        RaycastHit hit;

        if (Physics.Raycast(localOrb.transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            if(!distantOn) { DistantOn(); }
            distantEffects.transform.position = hit.point;
            hitObject = hit.collider.GetComponentInParent<InteractiveObjectController>();
        }
        else
        {
            if(distantOn) { DistantOff(); }
            hitObject = null;
        }
    }

   

    private void DistantOn()
    {
        distantEffects.Play();
        beamEffects.Play();
        localEffects.Play();

        if (!distantOrb.activeSelf) { distantOrb.SetActive(true); }
        distantOn = true;
    }

    private void DistantOff()
    {
        distantEffects.Stop();
        beamEffects.Stop();
        localEffects.Stop();

        if (distantOrb.activeSelf) { distantOrb.SetActive(false); }
        distantOn = false;
    }

    private void LocalOn()
    {
        if (!localOrb.activeSelf) { localOrb.SetActive(true); }
        localOn = true;
    }

    private void LocalOff()
    {
        if (localOrb.activeSelf) { localOrb.SetActive(false); }
        localOn = false;
    }


    public void Activate()
    {
        activated = true;
    }

    public void Deactivate()
    {
        activated = false;
    }
}
