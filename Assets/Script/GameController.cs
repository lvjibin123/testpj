﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameController : MonoBehaviour {

    public int ballCount = 4;
    public float dirY = 0.04f;
    public float r = 20;
    public float initY = -1.25f;
    public float trailResolution = 0.1f;
    public GameObject camera;
    // Shield 冷却时间
    public float shieldCooling = 3;
    public Spawner spawner;

    AudioSource audio;
    AudioClipSet audioclip_set; 
    Transform wall;
    GameObject trail;
    GameObject bg;
    List<GameObject> ballList;
    GameUI gameUI;
    TextMesh Text_ballCount;

    Vector3 originV;
    Vector3 relativeV = new Vector3(0, 0.35f, -1);
    Vector3 offsetBg;
    Vector3 offsetCamera;
    List<Vector3> dirList = new List<Vector3>();

    float delVX;
    GAME_STATUS game_status;
    Loops loopdata;
    float sheildTime = -1;
    // 碰砖块时位置
    float stopY = 0;
    bool isCrossing = false;
    // 碰到bar停止移动
    bool isStop = false;
    // 是否第一次玩，决定是否能复活
    bool isFirst = true;
    // rate
    int popRateCount;

    //prefab
    GameObject brick0;
    GameObject Boom0;
    GameObject ball0;
    GameObject bar0;
    GameObject bar2;
    GameObject gainball0;
    GameObject blockball0;


    public GAME_STATUS getGameStatus()
    {
        return game_status;
    }

    public bool getIsFirst() {
        return isFirst;
    }

	void Awake () {
        game_status = GAME_STATUS.READY;
        trail = transform.Find("trail").gameObject;
        wall = transform.Find("wall");
        bg = transform.Find("bg").gameObject;
        gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        Text_ballCount = transform.Find("ballCount").GetComponent<TextMesh>();

        blockball0 = (GameObject)Resources.Load("Prefab/blockball");
        brick0 = (GameObject)Resources.Load("Prefab/brick");
        ball0 = (GameObject)Resources.Load("Prefab/ball2");
        gainball0 = (GameObject)Resources.Load("Prefab/gainball");
        bar0 = (GameObject)Resources.Load("Prefab/bar");
        bar2 = (GameObject)Resources.Load("Prefab/bar2");
        Boom0 = (GameObject)Resources.Load("FX/Boom");
        loopdata = JsonUtil.LoadColorFromFile();


        audio = GetComponent<AudioSource>();
        GameObject go_audioclip = GameObject.Find("AudioClipSet");

        if (go_audioclip)
        {
            audioclip_set = go_audioclip.GetComponent<AudioClipSet>();
        }
    }

    public void ballMoveStart()
    {
        isFirst = true;
        isCrossing = false;
        initBallList(ballCount);
        Text_ballCount.text = getBallCount() + "";
        delVX = 0;

        this.dirList.Clear();
        this.dirList.Add(ballList[0].transform.position);
        game_status = GAME_STATUS.START;

        spawner.OnLoadGame();
    }

    public void reviveStart(int count)
    {
        isFirst = false;
        RemoveWallItems(2);
        isCrossing = false;
        initBallList(count);
        Text_ballCount.text = getBallCount() + "";
        delVX = 0;
        this.dirList.Clear();
        this.dirList.Add(ballList[0].transform.position);
        game_status = GAME_STATUS.START;

    }

    public void useSheild() {
        sheildTime = shieldCooling;
        ballList[0].transform.GetComponent<SpriteRenderer>().color = Color.red;
    }

    // 初始化
    public void initMap()
    {
        for (int i = 0; i < wall.childCount; i++)
        {
            Destroy(wall.GetChild(i).gameObject);
        }

        trail.transform.GetChild(0).localPosition = new Vector3(0, -1.25f, 0);
        camera.transform.localPosition = new Vector3(0, 0, -100);
        bg.transform.localPosition = new Vector3(0, 0, 1);
    }

    void RemoveWallItems(int lineCount) {
        float num = spawner.getLineHeight(lineCount);
        for (int i = 0; i < wall.childCount; i++) {
            if (wall.GetChild(i).localPosition.y < num)
            {
                Destroy(wall.GetChild(i).gameObject);
            }
        }
    }

    void initBallList(int count)
    {
        ballList = new List<GameObject>();
        GameObject firstBall = trail.transform.GetChild(0).gameObject;
        for (int i = 0; i < count; i++)
        {
            GameObject newball = GameObject.Instantiate<GameObject>(ball0);
            newball.transform.SetParent(trail.transform);
            newball.transform.localPosition = firstBall.transform.localPosition;
            newball.transform.GetComponent<CircleCollider2D>().enabled = false;
        }
        for (int i = 0; i < trail.transform.childCount; i++)
        {
            ballList.Add(trail.transform.GetChild(i).gameObject);
        }

        offsetBg = bg.transform.position - ballList[0].transform.position;
        offsetBg.x = 0;
        offsetCamera = camera.transform.position - ballList[0].transform.position;
        offsetCamera.x = 0;
    }

    void FixedUpdate() {
        if (game_status == GAME_STATUS.START)
        {
            checkClick();
            checkSheild();
        }
    }

    void checkSheild() {
        if (Mathf.Abs(sheildTime + 1) > 0.000001f)
        {
            if (sheildTime > 0)
            {
                sheildTime -= Time.deltaTime;
            }
            else
            {
                ballList[0].transform.GetComponent<SpriteRenderer>().color = Color.white;
                sheildTime = -1;
            }
        }
    }

    //float currentVelocity = 0.0f;
    void LateUpdate()
    {
        if (isCrossing)
            return;
        if (game_status == GAME_STATUS.START && ballList != null && ballList.Count > 0)
        {
            bg.transform.position = offsetBg + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
            camera.transform.position = offsetCamera + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
            //Vector3 position = camera.transform.position;
            //position.y = Mathf.SmoothDamp(position.y - offsetCamera.y, ballList[0].transform.position.y, ref currentVelocity, Time.deltaTime) + offsetCamera.y;
            //camera.transform.position = position;
        }
    }

    // 生成一排砖，和砖后面的一组Map
    /// <summary>
    /// 规则：1、除了一排砖以外，每层砖数不超过2
    ///       2、砖块和遮挡条交替出现在每层，一排砖的前后只能放遮挡条
    ///       3、一层中只能有一种遮挡条。
    /// </summary>

    public void initBrick(int number, Vector3 pos) {
        GameObject brick = GameObject.Instantiate<GameObject>(brick0);
        brick.transform.position = pos;
        brick.transform.SetParent(wall, false);

        Brick brickScript = brick.GetComponent<Brick>();
        brickScript.setNumber(number);
        brickScript.setColor(getBrickColor(number));
    }

    public void initGainBall(Vector3 pos, int number)
    {
        GameObject gainball = GameObject.Instantiate<GameObject>(gainball0);
        gainball.transform.position = pos;
        gainball.transform.SetParent(wall, false);
        gainball.GetComponent<GainBall>().setBallCount(number);
    }

    public void initBar(Vector3 pos)
    {
        GameObject bar = GameObject.Instantiate<GameObject>(bar0);
        bar.transform.position = pos;
        bar.transform.SetParent(wall, false);
    }

    public void initBar2(Vector3 pos)
    {
        GameObject bar = GameObject.Instantiate<GameObject>(bar2);
        bar.transform.position = pos;
        bar.transform.SetParent(wall, false);
    }

    public Color getBrickColor(int score)
    {
        return loopdata.getColor(score);
    }

    void checkClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            originV = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            delVX = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - originV).x;
            originV = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else {
            delVX = 0;
        }

        if (delVX != 0)
        {
            isStop = false;
        }

        if (!isStop)
        {
            moveBalls();
            DestroyWallItems();
        }
    }

    public void stopMove() {
        delVX = 0;
        isStop = true;
    }

    private void moveBalls()
    {
        if (ballList == null || ballList.Count == 0)
            return;

        if (ballList[0].transform.localPosition.x > 2.67f)
            ballList[0].transform.localPosition = new Vector3(2.67f, ballList[0].transform.localPosition.y, 0);
        else if (ballList[0].transform.localPosition.x < -2.67f)
            ballList[0].transform.localPosition = new Vector3(-2.67f, ballList[0].transform.localPosition.y, 0);


        ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(0, dirY, 0);
        ballList[0].transform.GetComponent<Ball>().move(new Vector3(delVX, 0, 0));

        //ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(delVX, dirY, 0);
        Text_ballCount.transform.localPosition = ballList[0].transform.localPosition + relativeV;
        if (ballList[0].transform.localPosition.y >= stopY && isCrossing)
        {
            isCrossing = false;
        }

        if (this.dirList.Count <= 0)
            return;
        this.dirList[0] = ballList[0].transform.localPosition;
        if (this.dirList.Count == 1)
            this.dirList.Add(this.dirList[0]);
        else if ((double)(this.dirList[0] - this.dirList[1]).sqrMagnitude >= (double)this.trailResolution * (double)this.trailResolution)
            this.dirList.Insert(1, this.dirList[0]);
        int index1 = 1;
        float spacing = 2 * r;
        int index2;
        for (index2 = 0; index2 < this.dirList.Count - 1 && index1 < this.ballList.Count; ++index2)
        {
            Vector3 point1 = this.dirList[index2];
            Vector3 point2 = this.dirList[index2 + 1];
            float magnitude = (point2 - point1).magnitude;
            if ((double)magnitude > 0.0)
            {
                Vector3 normalized = (point2 - point1).normalized;
                Vector3 vector3 = point1;
                for (; (double)spacing <= (double)magnitude && index1 < this.ballList.Count; spacing = 2 * r)
                {
                    vector3 += normalized * spacing;
                    magnitude -= spacing;
                    this.ballList[index1].transform.localPosition = vector3;
                    ++index1;
                }
                spacing -= magnitude;
            }
        }
        Vector3 point = this.dirList[this.dirList.Count - 1];
        for (int index3 = index1; index3 < this.ballList.Count; ++index3)
            this.ballList[index1].transform.localPosition = point;
        int index4 = index2 + 1;
        if (index4 >= this.dirList.Count)
            return;
        this.dirList.RemoveRange(index4, this.dirList.Count - index4);
    }


    void DestroyWallItems() {
        if (ballList == null || ballList.Count == 0)
            return;

        float ball_y = ballList[0].transform.localPosition.y;
        for (int i = 0; i < wall.childCount; i++)
        {
            if (ball_y - wall.GetChild(i).localPosition.y >= 4.9f)
            {
                Destroy(wall.GetChild(i).gameObject);
            }
            else {
                break;
            }
        }
    }

    public int getBallCount()
    {
        if (ballList == null)
            return 0;
        else
            return ballList.Count - 1;
    }

    public bool isFirstBall(GameObject ball) {
        if (ball == ballList[0])
            return true;
        else
            return false;
    }

    void gameOver() {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.game_over);
        game_status = GAME_STATUS.READY;

        //时间大于一定值，并且五次结束pop rate
        Debug.Log("popRateCount" + popRateCount);
        int isRate = PlayerPrefs.GetInt("isRate");
        if (isRate == 0)
        {
            popRateCount++;
            if (popRateCount >= Constants.RATE_COUNT)
            {
                popRateCount = 0;
                gameUI.popRateWind();
            }
            else
            {
                gameUI.gameOver();
            }
        }
        else
        {
            gameUI.gameOver();
        }
    }

    //碰到砖块
    public void hitBrick(GameObject brick)
    {
        if (ballList.Count == 1)
        {
            gameOver();
            return;
        }

        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.hit);

        delVX = 0;
        // 减球
        stopY = ballList[0].transform.localPosition.y;
        Vector3 vector3 = this.ballList[0].transform.localPosition;
        isCrossing = true;

        gameUI.updateScore();
        changeDirY();
        if (Mathf.Abs(sheildTime + 1) < 0.000001f)
        {
            if (ballList.Count == 1)
            {
                gameOver();
                return;
            }

            Destroy(ballList[0]);
            ballList.RemoveAt(0);

        }
        else
        {
            //sheild
            if (ballList.Count > 1)
                ballList[0].transform.localPosition = new Vector3(ballList[0].transform.localPosition.x, ballList[1].transform.localPosition.y, ballList[0].transform.localPosition.z);
            else
                ballList[0].transform.localPosition = new Vector3(ballList[0].transform.localPosition.x, ballList[0].transform.localPosition.y - r * 2, ballList[0].transform.localPosition.z);
        }

        ballList[0].transform.GetComponent<CircleCollider2D>().enabled = true;
        Vector3 positionVisual = this.ballList[0].transform.localPosition;
        Vector3 normalized = (vector3 - positionVisual).normalized;
        int count = 0;
        for (int index = 0; index < this.dirList.Count && (double)Vector3.Dot(positionVisual - this.dirList[index], normalized) < 0.0; ++index)
            count = index;
        this.dirList.RemoveRange(0, count);
        positionVisual.x = vector3.x;
        ballList[0].transform.localPosition = positionVisual;
        this.dirList[0] = positionVisual;

        Text_ballCount.text = getBallCount() + "";
        brick.GetComponent<Brick>().hit();

        if (brick.GetComponent<Brick>().getNumber() < 1)
        {
            var Fx_boom = GameObject.Instantiate<GameObject>(Boom0);
            Fx_boom.transform.position = brick.transform.position;
            Fx_boom.transform.SetParent(wall.transform, false);
            StartCoroutine(WaitToDestroy(Fx_boom));

            brick.GetComponent<Brick>().destropyBrick();
        }
    }

    void changeDirY() {
        int score = gameUI.getScore();
        if (score < 100)
        {
            dirY = score * 1.0f / 100 * 0.01f + 0.04f;
        }
        else if (score < 200 && score >= 100)
        {
            dirY = (score - 100) * 1.0f / 100 * 0.01f + 0.05f;
        }
        else if (score < 300 && score >= 200)
        {
            dirY = 0.06f;
        }
        else if (score < 500 && score >= 300)
        {
            dirY = (score - 300) * 1.0f / 200 * 0.02f + 0.06f;
        }
        else
        {
            dirY = 0.08f;
        }

    }

    //得到新球
    public void gainBall(GameObject gainball)
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.gain_ball);

        int count = gainball.GetComponent<GainBall>().getCount();
        Destroy(gainball);
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(addBall(i));
        }
    }

    IEnumerator addBall(int i)
    {
        yield return new WaitForSeconds(i * 0.12f);
        GameObject newball = GameObject.Instantiate<GameObject>(ball0);
        newball.transform.SetParent(trail.transform);
        newball.transform.localPosition = ballList[ballList.Count - 1].transform.localPosition;
        ballList.Add(newball);
        Text_ballCount.text = getBallCount() + "";
    }


    IEnumerator WaitToDestroy(GameObject fx)
    {
        yield return new WaitForSeconds(1.5f);
        if (fx != null)
        {
            Destroy(fx);
        }

    }
}
