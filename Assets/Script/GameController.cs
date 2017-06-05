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

    //prefab
    GameObject brick0;
    GameObject Boom0;
    GameObject ball0;
    GameObject bar0;
    GameObject bar2;
    GameObject gainball0;

	void Awake () {
        game_status = GAME_STATUS.READY;
        trail = transform.Find("trail").gameObject;
        wall = transform.Find("wall");
        bg = transform.Find("bg").gameObject;
        gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        Text_ballCount = transform.Find("ballCount").GetComponent<TextMesh>();

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
        for (int i = 0; i < ballList.Count; i++)
        {
            Tweener tweener = ballList[i].transform.DOLocalMove(ballList[i].transform.localPosition + new Vector3(0, (0-i) * r * 2, 0), 0.2f)
                .SetEase(Ease.Linear)
                .SetAutoKill(true)
                .SetUpdate(true);
            if (i == ballList.Count - 1)
                tweener.OnComplete(gameStart);
        }

       // game_status = GAME_STATUS.START;
        genBalls();
        genAll(true, ballList[0].transform.localPosition.y);
    }

    void gameStart() {
        game_status = GAME_STATUS.START;
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

    // 生成一排砖，和砖后面的一组Map
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
            number = UnityEngine.Random.Range(0, 3);
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
                type1 = 1 - type1;
                isLVFull = false;
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
                        else if (i != level + genLevel - 1 && i != genLevel + 1)
                        {
                            //brick 一排砖前面一行和后面一行不会有砖
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
                        else if (type2 == 1 && array[ran] != 4)
                        {
                            //bar 
                            pos = pos + new Vector3(0.5f * brickLength + barWidth, 0, 0);
                            initBar(pos);
                        }
                        else if (type2 == 2 && array[ran] != 4 && (level + genLevel - i > 2))
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

    void moveBalls() {
        if (ballList == null || ballList.Count == 0)
            return;

       // ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(delVX, dirY, 0);
        ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(0, dirY, 0);
        ballList[0].transform.GetComponent<Ball>().move(new Vector3(delVX, 0, 0));
        
        if (ballList[0].transform.localPosition.x > 2.7f)
            ballList[0].transform.localPosition = new Vector3(2.7f, ballList[0].transform.localPosition.y, 0);
        else if (ballList[0].transform.localPosition.x < -2.7f)
            ballList[0].transform.localPosition = new Vector3(-2.7f, ballList[0].transform.localPosition.y, 0);

        Text_ballCount.transform.position = ballList[0].transform.localPosition + relativeV;

        if (ballList[0].transform.localPosition.y >= stopY && isCrossing)
        {
            isCrossing = false;
        }
        
        for (int i = 1; i < ballList.Count; i++)
        {
            Vector3 deltaV = ballList[i-1].transform.localPosition - ballList[i].transform.localPosition;
            Vector3 moveV = deltaV - Vector3.Normalize(deltaV) * r * 2;
            ballList[i].transform.localPosition = ballList[i].transform.localPosition + moveV;
            //ballList[i].transform.localPosition = ballList[i].transform.localPosition + new Vector3(0,moveV.y,0);
            //ballList[i].transform.GetComponent<Ball>().move(new Vector3(moveV.x,0, 0));
        }
    }

    void DestroyWallItems() {
        if (ballList == null || ballList.Count == 0)
            return;

        float ball_y = ballList[0].transform.localPosition.y;
        for (int i = 0; i < wall.childCount; i++)
        {
            if (ball_y - wall.GetChild(i).localPosition.y >= 4.365f)
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

        // 减球
        stopY = ballList[0].transform.localPosition.y;
        isCrossing = true;

        gameUI.updateScore();
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

            if (brick.GetComponent<Brick>().getWall())
                genAll(false, brick.transform.localPosition.y);
        }
    }

    //得到新球
    public void gainBall(GameObject gainball)
    {

        //var Fx_frag = GameObject.Instantiate<GameObject>(Frag0);
        //Fx_frag.transform.position = ball.transform.position;
        //Fx_frag.transform.SetParent(wall.transform, false);
        //StartCoroutine(WaitToDestroy(Fx_frag));

        int count = gainball.GetComponent<GainBall>().getCount();
        Destroy(gainball);
        for (int i = 0; i < count; i++)
        {
            GameObject newball = GameObject.Instantiate<GameObject>(ball0);
            newball.transform.SetParent(trail.transform);
            newball.SetActive(false);
           // newball.transform.localPosition = ballList[ballList.Count - 1].transform.localPosition - new Vector3(0, r * 2, 0);
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
