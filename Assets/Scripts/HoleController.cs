using UnityEngine;
using System.Collections;

public class HoleController : MonoBehaviour {
	public bool isBallIn = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider other) {
		Debug.Log ("Object entered the trigger");
		Rigidbody rb = other.GetComponent<Rigidbody> ();
		rb.velocity = new Vector3 (0, 0, 0);
		rb.detectCollisions = false;
	}

	void OnTriggerStay (Collider other) {
		Debug.Log ("Object is within trigger");
		float ballX = other.transform.position.x;
		float ballY = other.transform.position.y;
		float ballZ = other.transform.position.z;
		Rigidbody rb = other.GetComponent<Rigidbody> ();
		if (ballY > -0.1) {
			rb.MovePosition(new Vector3(ballX, ballY - 0.01f, ballZ));
		} else {
			isBallIn = true;
			rb.velocity = new Vector3 (0, 0, 0);
		}
	}

	void OnTriggerExit (Collider other) {
		Debug.Log ("Object exited trigger");
	}
}
