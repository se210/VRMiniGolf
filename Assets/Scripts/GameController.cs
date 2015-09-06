using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public static GameController gameController;

	void Awake() {
		if (gameController == null)
		{
			DontDestroyOnLoad(gameObject);
			gameController = this;
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
