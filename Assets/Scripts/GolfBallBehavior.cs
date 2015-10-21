using UnityEngine;
using System.Collections;

public class GolfBallBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		if (rb.velocity.magnitude < 0.1)
		{
			rb.velocity = new Vector3(0,0,0);
			rb.angularVelocity = new Vector3(0,0,0);
		}
	}
}
