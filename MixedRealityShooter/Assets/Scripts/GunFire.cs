using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFire : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButtonDown("Fire1"))
	    {
            var gunSound = GetComponent<AudioSource>();
	        gunSound.Play();

	        GetComponent<Animation>().Play("Gunshot");
	    }
	}
}
