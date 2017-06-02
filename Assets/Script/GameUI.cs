using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    GameController gameController;
    GameObject Button_start;
    Text Text_score;
    int score = 0;

    public int getScore()
    {
        return score;
    }

	void Start () {
        gameController = GameObject.Find("Game").transform.GetComponent<GameController>();
        Button_start = transform.Find("Button_start").gameObject;
        Text_score = transform.Find("Text_score").GetComponent<Text>();
	}
	
	void Update () {
		
	}

    public void updateScore()
    {
        score++;
        int best = PlayerPrefs.GetInt("BestScore");
        if (best < score)
        {
            PlayerPrefs.SetInt("BestScore", score);
            best = score;
        }
        Text_score.text = score + "";
    }

    public void onStartBtnClick() {
        gameController.ballMoveStart();
        Button_start.SetActive(false);
    }
}
