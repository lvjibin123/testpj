﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public static float base_height = 1920;
    public static float base_width = 1080;
    public int rate_reward = 20;

    GameController gameController;
    Transform Menu;
    Text Text_score;
    AudioSource audio;
    AudioClipSet audioclip_set;
    GameObject Tutorial;
    GameObject Rate;
    GameObject Shop;

    int score = 0;
    int deltaCoin = -1;
    int coinFrame = 1;

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
        Rate = transform.Find("Rate").gameObject;
        Shop = transform.Find("Shop").gameObject;

        initMenu();

        audio = GetComponent<AudioSource>();
        GameObject go_audioclip = GameObject.Find("AudioClipSet");
        Tutorial = GameObject.Find("Tutorial");
        Tutorial.SetActive(false);
        Rate.SetActive(false);
        Shop.transform.localPosition = new Vector3(0, 1960, 0);

        if (go_audioclip)
        {
            audioclip_set = go_audioclip.GetComponent<AudioClipSet>();
        }
	}

    public void onStoreClick()
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        Shop.transform.localPosition = new Vector3(0, 0, 0);
    }

    void Update()
    {
        //addCoin();
    }

    // 动态增长金币。
    //void addCoin()
    //{
    //    if (deltaCoin >= 0)
    //    {
    //        deltaCoin = deltaCoin - coinFrame;
    //        int loop = PlayerPrefs.GetInt(BallzConstants.LOOP_COUNT);
    //        showLoop(loop + rate_reward - deltaCoin);
    //        if (deltaCoin == 0)
    //        {
    //            saveLoop(loop + rate_reward - deltaCoin);
    //            deltaCoin = -1;
    //        }
    //    }
    //}

    public void popRateWind()
    {
        Rate.SetActive(true);
    }

    public void onRateClick()
    {
     //   AdsUtils.setInterStutas(false);
     //   rateSup.rate();
    }

    public void onRateLater()
    {
        onRateOver(false);
    }

    public void onRateOver(bool isAdd)
    {
        if (isAdd)
        {
    //        addCoinDynamic(BallzConstants.RATE_COIN);
        }
        Rate.SetActive(false);
   //     onGameOver();
    }

    //增加金币
    public void addCoinDynamic(int coin)
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.spend_money);

        rate_reward = coin;
        if (rate_reward >= 100)
        {
            coinFrame = rate_reward / 50;
        }
        else
        {
            coinFrame = 1;
        }
        deltaCoin = rate_reward;
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
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);

        gameController.ballMoveStart();
        Menu.localPosition = new Vector3(0, 1920, 0);
    }

    public void initMenu()
    {
        Menu.localPosition = new Vector3(0, 0, 0);
        score = 0;
        Text_score.text = score + "";
    }

    public void onTutorialClick()
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        Tutorial.SetActive(true);
    }
}
