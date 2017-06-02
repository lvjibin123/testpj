using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour {

    GameController gameController;
    GameObject Button_start;

	void Start () {
        gameController = GameObject.Find("Game").transform.GetComponent<GameController>();
        Button_start = transform.Find("Button_start").gameObject;
	}
	
	void Update () {
		
	}

    public void onStartBtnClick() {
        gameController.ballMoveStart();
        Button_start.SetActive(false);
    }
}
