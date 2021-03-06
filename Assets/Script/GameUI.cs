﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public static float base_height = 1920;
    public static float base_width = 1080;
    public int rate_reward = 20;
    public int reviveBall = 8;

    GameController gameController;
    Transform Menu;
    Transform GameOver;
    Transform Shop;
    Transform Tutorial;
    Transform Rate;
    Transform GameScene;
    Text Text_best;
    Text Text_score;
    Text Text_sheildCount;
    Text Over_score;
    Text Over_best;
    Transform Button_more;
    Text Over_more;
    AudioSource audio;
    AudioClipSet audioclip_set;
    AudioSource music;

    int score = 0;
    int deltaCoin = -1;
    int coinFrame = 1;
    float reviveDelta = -1;
    int currentReviveBall;

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
        GameOver = transform.Find("GameOver");
        Rate = transform.Find("Rate");
        GameScene = transform.Find("GameScene");
        Shop = transform.Find("Shop");
        Tutorial = transform.Find("Tutorial");
        Over_score = GameOver.Find("Text_score").GetComponent<Text>();
        Over_best = GameOver.Find("Text_best").GetComponent<Text>();
        Button_more = GameOver.Find("Button_more");
        Over_more = Button_more.Find("Text_more").GetComponent<Text>();
        Text_best = Menu.Find("Text_best").GetComponent<Text>();
        Text_score = GameScene.Find("Text_score").GetComponent<Text>();
        Text_sheildCount = GameScene.Find("Text_count").GetComponent<Text>();

        audio = GetComponent<AudioSource>();
        GameObject go_audioclip = GameObject.Find("AudioClipSet");
        Shop.transform.localPosition = new Vector3(0, 1920, 0);
        GameOver.localPosition = new Vector3(0, 1920, 0);
        Tutorial.localPosition = new Vector3(0, 1920, 0);
        Rate.localPosition = new Vector3(0, 1920, 0);
        GameScene.localPosition = new Vector3(0, 1920, 0);
        Menu.localPosition = Vector3.zero;
        music = GameObject.Find("Music").GetComponent<AudioSource>();
        music.Stop();

        if (go_audioclip)
        {
            audioclip_set = go_audioclip.GetComponent<AudioClipSet>();
        }

        initMenu();
    }

    public void onStoreClick()
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        Shop.transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        //addCoin();
        checkRevive();
    }

    void checkRevive() {
        if (Mathf.Abs(reviveDelta + 1) > 0.000001f) {
            if (reviveDelta > 0)
            {
                reviveDelta -= Time.deltaTime;
                if (currentReviveBall != (int)Mathf.Ceil(reviveDelta)) {
                    currentReviveBall = (int)Mathf.Ceil(reviveDelta);
                    Over_more.text = "One More Life With " + currentReviveBall + " Balls";
                }
            }
            else {
                reviveDelta = -1;
                Button_more.gameObject.SetActive(false);
                AudioSourcesManager.GetInstance().Stop(audio);
            }
        }
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
        Rate.localPosition = Vector3.zero;
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
            addCoinDynamic(Constants.RATE_SHIELD);
        }
        Rate.localPosition = new Vector3(0, 1920, 0);
        gameOver();
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

    public void onShieldClick() {
        int count = PlayerPrefs.GetInt("ShieldCount", 10);
        if (count > 0) {
            count--;
            PlayerPrefs.SetInt("ShieldCount", count);
            Text_sheildCount.text = count + "";
            gameController.useSheild();
        }
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
        music.Play();
        score = 0;
        gameController.ballMoveStart();
        Menu.localPosition = new Vector3(0, 1920, 0);
        GameScene.localPosition = Vector3.zero;
    }

    public void initMenu()
    {
        Text_best.text = PlayerPrefs.GetInt("BestScore", 0) + "";
        score = 0;
        Text_score.text = score + "";
        Text_sheildCount.text = PlayerPrefs.GetInt("ShieldCount", 10) + "";
        gameController.initMap();
    }

    public void onRestartClick() {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        Menu.localPosition = Vector3.zero;
        GameOver.localPosition = new Vector3(0, 1920, 0);
        GameScene.localPosition = new Vector3(0, 1920, 0);
        initMenu();
    }

    public void gameOver() {
        music.Stop();
        // 倒计时
        GameOver.localPosition = Vector3.zero;
        Over_score.text = score + "";
        Over_best.text = PlayerPrefs.GetInt("BestScore", 0) + "";
        if (gameController.getIsFirst())
        {
            AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.time_down);
            reviveDelta = reviveBall;
            currentReviveBall = reviveBall;
            Over_more.text = "One More Life With " + currentReviveBall + " Balls";
            Button_more.gameObject.SetActive(true);
        }
        else {
            Button_more.gameObject.SetActive(false);
        }
    }

    public void moreLifeClick()
    {
        // 复活
        music.Play();
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        GameOver.localPosition = new Vector3(0, 1920, 0);
        int count = (int)Mathf.Ceil(reviveDelta);
        if (count > 0)
            gameController.reviveStart(count);
    }

    public void onTutorialClick()
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        Tutorial.localPosition = Vector3.zero;
    }
}
