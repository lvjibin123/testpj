using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameController : MonoBehaviour {

    public int ballCount = 4;
    public float dirY = 0.04f;
    public float r = 20;
    public float brickLength = 1.115f;
    public float barWidth = 0.006f;
    public float trailResolution = 0.1f;
    public GameObject camera;

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
    float stopY = 0;
    bool isCrossing = false;
    int genLevel = 0;
    bool isStop = false;
    // 为排除一排撞两个
    float brickP = -2;
    int dirIndex = 0;

    //prefab
    GameObject brick0;
    GameObject Boom0;
    GameObject ball0;
    GameObject bar0;
    GameObject bar2;
    GameObject gainball0;
    GameObject blockball0;

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

    }

    public void ballMoveStart()
    {
        isCrossing = false;
        initBallList();
        Text_ballCount.text = getBallCount() + "";
        delVX = 0;
        brickP = -2;

        //for (int i = 0; i < ballList.Count; i++)
        //{
        //    Tweener tweener = ballList[i].transform.DOLocalMove(ballList[i].transform.localPosition + new Vector3(0, (0 - i) * r * 2, 0), 0.2f)
        //        .SetEase(Ease.Linear)
        //        .SetAutoKill(true)
        //        .SetUpdate(true);
        //    if (i == ballList.Count - 1)
        //        tweener.OnComplete(gameStart);
        //}

        genBalls();
        genAll(true, ballList[0].transform.localPosition.y);

        this.dirList.Clear();
        this.dirList.Add(ballList[0].transform.position);
        game_status = GAME_STATUS.START;
    }

    void gameStart() {
        this.dirList.Clear();
        game_status = GAME_STATUS.START;
        this.dirList.Add(ballList[0].transform.position);
        this.dirList.Add(this.dirList[0]);
    } 

    void initBallList() {
        ballList = new List<GameObject>();
        GameObject firstBall = trail.transform.GetChild(0).gameObject;
        firstBall.transform.localPosition = new Vector3(0, -1.25f, 0);
        for (int i = 0; i < ballCount; i++)
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
        offsetCamera = camera.transform.position - ballList[0].transform.position;
    }

    void FixedUpdate() {
        if (game_status == GAME_STATUS.START)
        {
            checkClick();
        }
    }

    void LateUpdate()
    {
        if (isCrossing)
            return;
        if (game_status == GAME_STATUS.START && ballList != null && ballList.Count > 0)
        {
            bg.transform.position = offsetBg + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
            camera.transform.position = offsetCamera + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
        }
    }

    void genBalls()
    {
        // 起始5层
        genLevel = 5;
        int number = 0;
        List<int> array;
        float startY = ballList[0].transform.localPosition.y;
        for (int i = 6; i < 5+genLevel; i++)
        {
            array = new List<int>() { 0, 1, 2, 3, 4 };
            number = UnityEngine.Random.Range(0, 3);
            for (int j = 0; j < number; j++)
            {
                int ran = UnityEngine.Random.Range(0, array.Count);
                float posY = startY + i * brickLength;
                Vector3 pos = new Vector3((array[ran] - 2) * brickLength, posY, 0);
                initGainBall(pos);

                array.RemoveAt(ran);

            }
        }
        genLevel += 5;
    }

    int getRandomNum() {
        List<int> brickNum = new List<int>();
        for (int i = 0; i < 4 ; i++)
        {
            brickNum.Add(i);
            if (i > 0)
            {
                brickNum.Add(i);
            }
        }
        int index = UnityEngine.Random.Range(0, brickNum.Count);
        return brickNum[index];
    }

    // 生成一排砖，和砖后面的一组Map
    /// <summary>
    /// 规则：1、除了一排砖以外，每层砖数不超过2
    ///       2、砖块和遮挡条交替出现在每层，一排砖的前后只能放遮挡条
    ///       3、一层中只能有一种遮挡条。
    /// </summary>

    void genAll(bool isFirst, float startY)
    {
        // 一排砖
        genBlockWall(isFirst, startY);

        int level = UnityEngine.Random.Range(10, 20);

        // middle
        // 大种类 0-砖块，1-遮挡物
        int type1 = 0;
        int type2 = 0;
        // 每一层放多少个
        int number = 0;
        // 下层是否放满
        bool isLVFull = false;
        float posY = 0;
        List<int> array;
        
        // 中间的每一层
        for (int i = genLevel + 1; i < level + genLevel; i++)
        {
            array = new List<int>() { 0, 1, 2, 3, 4 };
            number = getRandomNum();
            posY = startY + i * brickLength;
            if (isLVFull)
            {
                isLVFull = false;
                // 只放小球
                for (int j = 0; j < number; j++)
                {
                    // 小种类,球的概率1/3
                    type2 = UnityEngine.Random.Range(0, 3);
                    int ran = UnityEngine.Random.Range(0, array.Count);
                    Vector3 pos = new Vector3((array[ran] - 2) * brickLength, posY, 0);
                    if (type2 == 0)
                    {
                        //gainball
                        initGainBall(pos);
                    }
                }
            }
            else
            {
                isLVFull = false;
                type1 = 1 - type1;
                if (type1 == 1) {
                    // 遮挡物只有4个位置
                    array.Remove(4);
                }
                for (int j = 0; j < number; j++)
                {
                    if (type1 == 1 && j > 0 && type2 != 0)
                    {
                        //遮挡物同一type
                    }
                    else
                    {
                        // 小种类,球的概率1/3
                        type2 = UnityEngine.Random.Range(0, 3);
                    }
                    int ran = UnityEngine.Random.Range(0, array.Count);
                    Vector3 pos = new Vector3((array[ran] - 2) * brickLength, posY, 0);

                    if (type1 == 0)
                    {
                        // 砖
                        if (type2 == 0)
                        {
                            //gainball
                            initGainBall(pos);
                        }
                        else if (i != level + genLevel - 1 && i != genLevel + 1 && j < 2)
                        {
                            //brick 一排砖前面一行和后面一行不会有砖
                            //一行最多两个
                            int score = UnityEngine.Random.Range(10, 50);
                            initBrick(score, pos, false);
                        }
                    }
                    else
                    {
                        // 遮挡物
                        if (type2 == 0)
                        {
                            //gainball
                            initGainBall(pos);
                        }
                        else if (type2 == 1)
                        {
                            //bar 
                            pos = pos + new Vector3(0.5f * brickLength + barWidth, 0, 0);
                            initBar(pos);
                        }
                        else if (type2 == 2 && (level + genLevel - i > 2))
                        {
                            //bar2 
                            pos = pos + new Vector3(0.5f * brickLength + barWidth, brickLength / 2, 0);
                            initBar2(pos);
                            // 下一层只能有小球
                            isLVFull = true;
                        }
                    }

                    array.RemoveAt(ran);
                }
            }
        }

        genLevel = level;
    }

    void genBlockWall(bool isFirst, float startY)
    {
        int smallPos = UnityEngine.Random.Range(0, 5);
        float posY = startY + genLevel * brickLength;
        // 一行墙
        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = new Vector3((i - 2) * brickLength, posY, 0);

            int score = UnityEngine.Random.Range(1, 3);
            if (!isFirst)
            {
                // 高层的数字中，有一个最小，其余的随机。
                if (i == smallPos)
                {
                    score = UnityEngine.Random.Range(1, getBallCount() / 2 + 1);
                }
                else
                {
                    score = UnityEngine.Random.Range(10, 50);
                }
            }
            initBrick(score, pos, true);
        }
    }

    void initBrick(int number, Vector3 pos, bool isWall) {
        GameObject brick = GameObject.Instantiate<GameObject>(brick0);
        brick.transform.position = pos;
        brick.transform.SetParent(wall, false);

        Brick brickScript = brick.GetComponent<Brick>();
        brickScript.setNumber(number);
        brickScript.setColor(getBrickColor(number));
        if (isWall)
            brickScript.setWall();
    }

    void initGainBall(Vector3 pos)
    {
        int count = UnityEngine.Random.Range(1, 6);
        GameObject gainball = GameObject.Instantiate<GameObject>(gainball0);
        gainball.transform.position = pos;
        gainball.transform.SetParent(wall, false);
        gainball.GetComponent<GainBall>().setBallCount(count);
    }

    void initBar(Vector3 pos)
    {
        GameObject bar = GameObject.Instantiate<GameObject>(bar0);
        bar.transform.position = pos;
        bar.transform.SetParent(wall, false);
    }

    void initBar2(Vector3 pos)
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
       // else if (Input.GetMouseButtonUp(0)) {
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

        ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(0, dirY, 0);
        ballList[0].transform.GetComponent<Ball>().move(new Vector3(delVX, 0, 0));

        if (ballList[0].transform.localPosition.x > 2.67f)
            ballList[0].transform.localPosition = new Vector3(2.67f, ballList[0].transform.localPosition.y, 0);
        else if (ballList[0].transform.localPosition.x < -2.67f)
            ballList[0].transform.localPosition = new Vector3(-2.67f, ballList[0].transform.localPosition.y, 0);

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
        game_status = GAME_STATUS.READY;
        Text_ballCount.text = "";
        destroyWallChild();
        destroyBallChild();
        gameUI.initMenu();
    }

    void destroyWallChild()
    {
        for (int i = 0; i < wall.transform.childCount; i++)
        {
            Destroy(wall.transform.GetChild(i).gameObject);
        }
    }

    void destroyBallChild()
    {
        if (ballList == null || ballList.Count == 0)
        {

            GameObject newball = GameObject.Instantiate<GameObject>(ball0);
            newball.transform.SetParent(trail.transform);
            newball.transform.localPosition = new Vector3(0, -1.25f, 0);

            bg.transform.position = offsetBg + new Vector3(0, newball.transform.position.y, newball.transform.position.z);
            camera.transform.position = offsetCamera + new Vector3(0, newball.transform.position.y, newball.transform.position.z);
            return;
        }

        for (int i = 1; i < ballList.Count; i++)
        {
            Destroy(ballList[i]);
        }
        ballList.Clear();
    }

    //碰到砖块
    public void hitBrick(GameObject brick)
    {
        delVX = 0;
        // 减球
        stopY = ballList[0].transform.localPosition.y;
        Vector3 vector3 = this.ballList[0].transform.localPosition;
        isCrossing = true;

        gameUI.updateScore();
        Destroy(ballList[0]);
        ballList.RemoveAt(0);

        if (ballList.Count == 0)
        {
            gameOver();
            return;
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
        //if (!soundHit.GetComponent<AudioController>().getIsPlaying())
        //    soundHit.GetComponent<AudioController>().playSound();
        if (brick.GetComponent<Brick>().getNumber() < 1)
        {
            var Fx_boom = GameObject.Instantiate<GameObject>(Boom0);
            Fx_boom.transform.position = brick.transform.position;
            Fx_boom.transform.SetParent(wall.transform, false);
            StartCoroutine(WaitToDestroy(Fx_boom));

            brick.GetComponent<Brick>().destropyBrick();

            if (brick.GetComponent<Brick>().getWall() && brickP < brick.transform.localPosition.y)
            {
                brickP = brick.transform.localPosition.y;
                genAll(false, brickP);
            }
        }
    }

    //得到新球
    public void gainBall(GameObject gainball)
    {
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
