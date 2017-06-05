using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    GameController gameController;
    Transform Menu;
    Text Text_score;
    int score = 0;

    public int getScore()
    {
        return score;
    }

	void Start () {
        gameController = GameObject.Find("Game").transform.GetComponent<GameController>();
        Menu = transform.Find("Menu");
        Text_score = transform.Find("GameScene").Find("Text_score").GetComponent<Text>();

        initMenu();
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
        Menu.localPosition = new Vector3(0, 1920, 0);
    }

    public void initMenu()
    {
        Menu.localPosition = new Vector3(0, 0, 0);
        score = 0;
        Text_score.text = score + "";
    }
}
