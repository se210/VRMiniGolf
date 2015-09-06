using UnityEngine;
using System.Collections;

public class InitialBallPush : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();
		rb.velocity = new Vector3 (0, 0, 1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
