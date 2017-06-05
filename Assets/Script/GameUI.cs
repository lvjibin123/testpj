using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    GameController gameController;
    Transform Menu;
    Text Text_score;
    int score = 0;
    public static float base_height = 1920;
    public static float base_width = 1080;

    public int getScore()
    {
        return score;
    }

    void Awake()
    {
        adapterCanvas();
    }

	void Start () {
        gameController = GameObject.Find("Game").transform.GetComponent<GameController>();
        Menu = transform.Find("Menu");
        Text_score = transform.Find("GameScene").Find("Text_score").GetComponent<Text>();

        initMenu();
	}

    //Canvas适配
    void adapterCanvas()
    {
        float scale = 1;

        if (base_height / base_width > Screen.height * 1.0f / Screen.width)
        {
            scale = Screen.height / base_height;
        }
        else
        {
            scale = Screen.width / base_width;
        }
        transform.GetComponent<CanvasScaler>().scaleFactor = scale;
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
