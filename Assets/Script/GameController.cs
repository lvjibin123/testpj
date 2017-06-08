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
    public GameObject camera;

    Transform wall;
    GameObject trail;
    GameObject bg;
    List<GameObject> ballList;
    GAME_STATUS game_status;
    Vector3 originV;
    GameUI gameUI;
    TextMesh Text_ballCount;

    Vector3 relativeV = new Vector3(0, 0.35f, 0);
    float delVX;
    Vector3 offsetBg;
    Vector3 offsetCamera;
    Loops loopdata;
    float stopY = 0;
    bool isCrossing = false;
    int genLevel = 0;
    bool isStop = false;
    // 为排除一排撞两个
    float brickP = -2;

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

        for (int i = 0; i < ballList.Count; i++)
        {
            Tweener tweener = ballList[i].transform.DOLocalMove(ballList[i].transform.localPosition + new Vector3(0, (0-i) * r * 2, 0), 0.2f)
                .SetEase(Ease.Linear)
                .SetAutoKill(true)
                .SetUpdate(true);
            if (i == ballList.Count - 1)
                tweener.OnComplete(gameStart);
        }

        genBalls();
        genAll(true, ballList[0].transform.localPosition.y);
    }

    void gameStart() {
        game_status = GAME_STATUS.START;
    } 

   // List<float> moveList = new List<float>() {0.1f,0.2f,0.3f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,-0.5f,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
   // List<float> moveList = new List<float>() { 0.03124992f, 0.0520833f, 0.03645843f, 0.05208347f, 0.0781247f, 0.04687528f, 0.03124976f, 0.01041669f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    List<float> moveList = new List<float>() { 0.1875f, 0.3489586f, 0.2864583f, 0.1458334f, 0, 0, -0.2760419f, -0.7604166f, -0.8802083f,
        -0.03125f, 0, 0.1197917f, 0.583333f, 1.208333f, 0.06770831f, 0, 0, -0.2187499f, -0.3645833f, -0.01041669f, 0, 0.03125008f, 
        0.1458334f, 0.1093748f, 0, -0.1093748f, -0.6510419f, -0.7552083f, -0.2083334f, 0, 0.01041663f, 0.1614585f, 0.04687488f, 0.04687488f,
        0.04687488f, 0.04687488f, 0.04687488f };
    int moveIndex = 0;

    public void onMoveClick() {
        if (moveList.Count > moveIndex) {
            delVX = moveList[moveIndex];
            moveIndex++;
            moveBalls();
        }
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
        }
        for (int i = 0; i < trail.transform.childCount; i++)
        {
            ballList.Add(trail.transform.GetChild(i).gameObject);
        }
        offsetBg = bg.transform.position - ballList[0].transform.position;
        offsetCamera = camera.transform.position - ballList[0].transform.position;
    }

    void FixedUpdate() {
        //if (game_status == GAME_STATUS.START)
        //{
        //    checkClick();
        //}
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
        else if (Input.GetMouseButtonUp(0)) {
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

    List<Vector3> dirList = new List<Vector3>();
    List<Vector3> newPos = new List<Vector3>();

    int dirIndex = 0;

    void moveBalls()
    {
       // Debug.Log("delVX : " + delVX);
        if (ballList == null || ballList.Count == 0)
            return;

        newPos.Clear();
        newPos.Add(ballList[0].transform.localPosition + new Vector3(delVX, dirY, 0));

        Text_ballCount.transform.position = newPos[0] + relativeV;

        if (newPos[0].y >= stopY && isCrossing)
        {
            isCrossing = false;
        }

        Vector3 vector = new Vector3(delVX, dirY, 0);

        if (dirList.Count == 0)
        {
            if (Mathf.Abs(delVX) > 0.000001f)
            {
                dirList.Add(ballList[0].transform.localPosition);
                dirList.Add(newPos[0]);
            }
        }
        else
        {
            // 矫正
            dirList[dirList.Count - 1] = ballList[0].transform.localPosition;
            if (dirList.Count == 1)
            {
                if (Mathf.Abs(delVX) > 0.000001f)
                {
                    dirList.Add(newPos[0]);
                }
            }
            else
            {
                if (Mathf.Abs(delVX) < 0.000001f && Mathf.Abs(dirList[dirList.Count - 1].x - newPos[0].x) < 0.000001f)
                    {
                    //垂直
                    dirList[dirList.Count - 1] = newPos[0];
                }
                else if (Mathf.Abs((dirList[dirList.Count - 1].x - dirList[dirList.Count - 2].x) * dirY - (dirList[dirList.Count - 1].y - dirList[dirList.Count - 2].y) * delVX) > 0.000001f)
                {
                    dirList.Add(newPos[0]);
                }
                else
                {
                    dirList[dirList.Count - 1] = newPos[0];
                }
            }
        }

        int index = 0;
        dirIndex = 0;
        if (dirList.Count > 1)
        {
            for (int i = 1; i < ballList.Count; i++)
            {
                Vector3 moveV = Vector3.Normalize(vector) * r * 2 * i;
                if (Mathf.Abs(moveV.x) < Mathf.Abs(vector.x))
                {
                    Vector3 vec = newPos[0] - moveV;
                    newPos.Add(vec);
                }
                else
                {
                    if (Mathf.Abs(ballList[index].transform.localPosition.x - newPos[i - 1].x) < r * 2)
                    {
                        // 转角
                        Vector3 ball1 = newPos[i - 1];
                        Vector3 ball2 = Vector3.zero;
                        Vector3 ball3 = Vector3.zero;
                        Vector3 endP = Vector3.zero;
                        calculateBallPos(ref ball1, ref ball2, ref ball3, ref endP);

                        Vector3 ptInter1 = Vector3.zero;
                        Vector3 ptInter2 = Vector3.zero;
                        LineInterCircle(ball3, endP, ball1, 2 * 2 * r * r, ref ptInter1, ref ptInter2);
                        //if (ptInter1.x < 65536 && ptInter1.y < 65536)
                        //    newPos.Add(ptInter1);
                        //else 
                        if (ptInter2.x < 65536 && ptInter2.y < 65536)
                            newPos.Add(ptInter2);
                        else
                        {
                            newPos.Add(ballList[index + 1].transform.localPosition + new Vector3(delVX, dirY, 0));
                        }

                        index++;
                    }
                    else
                    {
                        newPos.Add(ballList[index + 1].transform.localPosition + new Vector3(delVX, dirY, 0));
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < ballList.Count; i++)
            {
                newPos.Add(ballList[i].transform.localPosition + new Vector3(0, dirY, 0));
            }
        }

        ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(0, dirY, 0);
        ballList[0].transform.GetComponent<Ball>().move(new Vector3(newPos[0].x - ballList[0].transform.localPosition.x, 0, 0));
        for (int i = 1; i < newPos.Count; i++)
        {
            if (i < ballList.Count)
                ballList[i].transform.localPosition = newPos[i];
            else
                break;
        }

        // 清除向量
        while (dirList.Count > 0 && ballList[ballList.Count - 1].transform.position.y > dirList[0].y)
        {
            dirList.RemoveAt(0);
        }
    }

    void calculateBallPos(ref Vector3 ball1, ref Vector3 ball2, ref Vector3 ball3, ref Vector3 endP) {
        if (dirList.Count - dirIndex >= 2)
        {
            ball2 = dirList[dirList.Count - 2 - dirIndex];
            ball3 = dirList[dirList.Count - 1 - dirIndex];
            // endP = ball2;
            endP = ball3 + (ball2 - ball3) * 10;

            Vector3 ballV = ball1 - ball2;
            if ((ballV.x * ballV.x + ballV.y * ballV.y) < r * 2 * r * 2)
            {
                dirIndex++;
                if (dirList.Count - dirIndex >= 2)
                {
                    ball2 = dirList[dirList.Count - 2 - dirIndex];
                    ball3 = dirList[dirList.Count - 1 - dirIndex];
                    // endP = ball2;
                    endP = ball3 + (ball2 - ball3) * 10;

                    calculateBallPos(ref ball1, ref ball2, ref ball3, ref endP);
                }
                else if (dirList.Count - dirIndex >= 1)
                {
                    ball3 = dirList[dirList.Count - 1 - dirIndex];
                    endP = ball3 + new Vector3(0, -10, 0);
                }
            }
        }
        else
        {
            ball3 = dirList[dirList.Count - 1 - dirIndex];
            endP = ball3 + new Vector3(0, -10, 0);
        }
    }

    //void moveBalls()
    //{
    //    if (ballList == null || ballList.Count == 0)
    //        return;

    //    newPos.Clear();
    //    dirList.Clear();

    //    for (int i = 0; i < ballList.Count - 1; i++)
    //    {
    //        dirList.Add(ballList[i].transform.localPosition - ballList[i + 1].transform.localPosition);
    //    }

    //    newPos.Add(ballList[0].transform.localPosition + new Vector3(delVX, dirY, 0));

    //    //if (newPos[0].x > 2.7f)
    //    //    newPos[0] = new Vector3(2.7f, newPos[0].y, 0);
    //    //else if (newPos[0].x < -2.7f)
    //    //    newPos[0] = new Vector3(-2.7f, newPos[0].y, 0);

    //    Text_ballCount.transform.position = newPos[0] + relativeV;

    //    if (newPos[0].y >= stopY && isCrossing)
    //    {
    //        isCrossing = false;
    //    }

    //    Vector3 vector = new Vector3(delVX, dirY, 0);
    //    int index = 0;

    //    for (int i = 1; i < ballList.Count; i++)
    //    {
    //        Vector3 moveV = Vector3.Normalize(vector) * r * 2 * i;
    //        if (Mathf.Abs(moveV.x) < Mathf.Abs(vector.x))
    //        {
    //            Vector3 vec = newPos[0] - moveV;
    //            newPos.Add(vec);
    //        }
    //        else
    //        {
    //            if (Mathf.Abs(ballList[index].transform.localPosition.x - newPos[i - 1].x) < r * 2)
    //            {
    //                // 转角
    //                Vector3 ball1 = newPos[i - 1];
    //                Vector3 ball2 = ballList[index].transform.localPosition;
    //                Vector3 ball3 = ballList[index + 1].transform.localPosition;

    //                Vector3 ptInter1 = Vector3.zero;
    //                Vector3 ptInter2 = Vector3.zero;
    //                Vector3 endP = ball2 + (ball3 - ball2) * 10;
    //                LineInterCircle(ball2, endP, ball1, 2 * 2 * r * r, ref ptInter1, ref ptInter2);
    //                if (ptInter1.x < 65536 && ptInter1.y < 65536)
    //                    newPos.Add(ptInter1);
    //                else if (ptInter2.x < 65536 && ptInter2.y < 65536)
    //                    newPos.Add(ptInter2);
    //                else
    //                {
    //                    newPos.Add(ball3);
    //                }

    //                index++;
    //            }
    //            else
    //            {
    //                newPos.Add(ballList[index + 1].transform.localPosition);
    //            }
    //        }
    //    }

    //    ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(0, dirY, 0);
    //    ballList[0].transform.GetComponent<Ball>().move(new Vector3(newPos[0].x-ballList[0].transform.localPosition.x, 0, 0));
    //    for (int i = 1; i < newPos.Count; i++)
    //    {
    //        if (i < ballList.Count)
    //            ballList[i].transform.localPosition = newPos[i];
    //        else
    //            break;
    //    }
    //}

    /// <summary>
    /// 线段与圆的交点
    /// </summary>
    /// <param name="ptStart">线段起点</param>
    /// <param name="ptEnd">线段终点</param>
    /// <param name="ptCenter">圆心坐标</param>
    /// <param name="Radius2">圆半径平方</param>
    /// <param name="ptInter1">交点1(若不存在返回65536)</param>
    /// <param name="ptInter2">交点2(若不存在返回65536)</param>
    protected bool LineInterCircle(Vector3 ptStart, Vector3 ptEnd, Vector3 ptCenter, float Radius2,
        ref Vector3 ptInter1, ref Vector3 ptInter2)
    {
        ptInter1.x = ptInter2.x = 65536.0f;
        ptInter2.y = ptInter2.y = 65536.0f;
        float fDis = (float)Mathf.Sqrt((ptEnd.x - ptStart.x) * (ptEnd.x - ptStart.x) + (ptEnd.y - ptStart.y) * (ptEnd.y - ptStart.y));
        Vector3 d = new Vector3();
        d.x = (ptEnd.x - ptStart.x) / fDis;
        d.y = (ptEnd.y - ptStart.y) / fDis;
        Vector3 E = new Vector3();
        E.x = ptCenter.x - ptStart.x;
        E.y = ptCenter.y - ptStart.y;
        float a = E.x * d.x + E.y * d.y;
        float a2 = a * a;
        float e2 = E.x * E.x + E.y * E.y;
        if ((Radius2 - e2 + a2) < 0)
        {
            return false;
        }
        else
        {
            float f = Mathf.Sqrt(Radius2 - e2 + a2);
            float t = a - f;
            if (((t - 0.0) > -0.000001f) && (t - fDis) < 0.000001f)
            {
                ptInter1.x = ptStart.x + t * d.x;
                ptInter1.y = ptStart.y + t * d.y;
            }
            t = a + f;
            if (((t - 0.0) > -0.000001f) && (t - fDis) < 0.000001f)
            {
                ptInter2.x = ptStart.x + t * d.x;
                ptInter2.y = ptStart.y + t * d.y;
            }
            return true;
        }
    }

    // 钉子
    //void moveBalls()
    //{
    //    if (ballList == null || ballList.Count == 0)
    //        return;

    //    Vector3 dir1 = Vector3.Normalize(new Vector3(delVX, dirY, 0));
    //    Vector3 dir2 = Vector3.Normalize(ballList[1].transform.localPosition - ballList[0].transform.localPosition);
    //    Vector3 dir3 = Vector3.Normalize(dir1 + dir2) * r * 2;
    //    Vector3 pos = dir3 + ballList[0].transform.localPosition;

    //    // ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(delVX, dirY, 0);

    //    ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(0, dirY, 0);
    //    ballList[0].transform.GetComponent<Ball>().move(new Vector3(delVX, 0, 0));

    //    if (ballList[0].transform.localPosition.x > 2.7f)
    //        ballList[0].transform.localPosition = new Vector3(2.7f, ballList[0].transform.localPosition.y, 0);
    //    else if (ballList[0].transform.localPosition.x < -2.7f)
    //        ballList[0].transform.localPosition = new Vector3(-2.7f, ballList[0].transform.localPosition.y, 0);

    //    Text_ballCount.transform.position = ballList[0].transform.localPosition + relativeV;

    //    if (ballList[0].transform.localPosition.y >= stopY && isCrossing)
    //    {
    //        isCrossing = false;
    //    }

    //    for (int i = 1; i < ballList.Count; i++)
    //    {
    //        Vector3 deltaV = ballList[i - 1].transform.localPosition - ballList[i].transform.localPosition;
    //        Vector3 moveV = deltaV - Vector3.Normalize(deltaV) * r * 2;
    //        ballList[i].transform.localPosition = ballList[i].transform.localPosition + moveV;
    //    }
    //    if (delVX != 0)
    //    {
    //        // blockball
    //        GameObject blockball = GameObject.Instantiate<GameObject>(blockball0);
    //        blockball.transform.position = pos;
    //        blockball.transform.SetParent(wall, false);
    //    }
    //}

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
        isCrossing = true;

        gameUI.updateScore();

        for (int i = ballList.Count - 1; i >=1; i--)
        {
            ballList[i].transform.localPosition = new Vector3(ballList[i - 1].transform.localPosition.x, ballList[i].transform.localPosition.y, 0);
        }

        Destroy(ballList[0]);
        ballList.RemoveAt(0);

        if (ballList.Count == 0)
        {
            gameOver();
            return;
        }
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
            GameObject newball = GameObject.Instantiate<GameObject>(ball0);
            newball.transform.SetParent(trail.transform);
            newball.SetActive(false);
            StartCoroutine(addBall(newball, i, ballList.Count - 1));

            ballList.Add(newball);
            Text_ballCount.text = getBallCount() + "";
        }
    }

    IEnumerator addBall(GameObject newball, int i, int lastIndex)
    {
        yield return new WaitForSeconds(i * 0.1f);
        newball.SetActive(true);
        if (lastIndex > ballList.Count - 1)
            lastIndex = ballList.Count - 1;
        newball.transform.localPosition = ballList[lastIndex].transform.localPosition;
        //Vector3 aa = ballList[lastIndex].transform.localPosition - new Vector3(0, r * 2, 0);
        //Debug.Log("aa:" + aa);
        //newball.transform.DOLocalMove(newball.transform.localPosition - new Vector3(0, r * 2, 0), 0.1f)
        //   .SetEase(Ease.Linear)
        //   .SetAutoKill(true)
        //   .SetUpdate(true);
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
