using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public static GameController gameController;

	public int strokes;
	public int level;

	public bool mStrokeStarted;

	void Awake() {
		if (gameController == null)
		{
			DontDestroyOnLoad(gameObject);
			gameController = this;
			strokes = 0;
			level = 1;
			mStrokeStarted = false;
		}
		else if (gameController != this)
		{
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		setPlayerPosition();
	}
	
	// Update is called once per frame
	void Update () {
		if (mStrokeStarted)
		{
			GameObject golfBall = GameObject.Find("GolfBall");
			Rigidbody golfBallRB = golfBall.GetComponent<Rigidbody>();

			if (golfBallRB.velocity.magnitude < 0.01)
			{
				setPlayerPosition ();
			}
		}
	}

	public void setPlayerPosition()
	{
		GameObject golfBall = GameObject.Find("GolfBall");

		GameObject player = GameObject.Find("Player");
		Vector3 pos = golfBall.transform.position;
		pos.x -= 0.05f;
		pos.y = player.transform.position.y;
		pos.z -= 0.05f;

		player.transform.position = pos;
	}
}
