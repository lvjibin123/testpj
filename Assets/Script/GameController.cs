using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameController : MonoBehaviour {

    public float dirY = 10;
    public float r = 20;
    public float brickLength = 1.115f;
    public GameObject camera;

    GameObject wall;
    GameObject trail;
    GameObject bg;
    List<GameObject> ballList;
    GAME_STATUS game_status;
    Vector3 originV;
    GameUI gameUI;

    float delVX;
    int dir = 0;
    Vector3 offsetBg;
    Vector3 offsetCamera;
    Loops loopdata;

    //prefab
    GameObject brick0;
    GameObject Boom0;

	void Awake () {
        game_status = GAME_STATUS.READY;
        trail = transform.Find("trail").gameObject;
        wall = transform.Find("wall").gameObject;
        bg = transform.Find("bg").gameObject;
        gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();

        ballList = new List<GameObject>();

        for (int i = 0; i < trail.transform.childCount; i++) {
            ballList.Add(trail.transform.GetChild(i).gameObject);
        }
        offsetBg = bg.transform.position - ballList[0].transform.position;
        offsetCamera = camera.transform.position - ballList[0].transform.position;

        brick0 = (GameObject)Resources.Load("Prefab/brick");
        Boom0 = (GameObject)Resources.Load("FX/Boom");
        loopdata = JsonUtil.LoadColorFromFile();
        genBricks(true);
    }
	
	void Update () {
        if (game_status == GAME_STATUS.START)
        {
            checkClick();
        }
	}

    void LateUpdate()
    {
        if (game_status == GAME_STATUS.START && ballList != null && ballList.Count > 0)
        {
            bg.transform.position = offsetBg + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
            camera.transform.position = offsetCamera + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
        }
    }

    void genBricks(bool isFirst) {
        int level = UnityEngine.Random.Range(10, 20);
        if(isFirst)
            level = UnityEngine.Random.Range(5, 11);
        for (int i = 0; i < 5; i++) {
            float posY = ballList[0].transform.localPosition.y + level * brickLength;
            Vector3 pos = new Vector3((i - 2) * brickLength, posY, 0);

            GameObject brick = GameObject.Instantiate<GameObject>(brick0);
            brick.transform.position = pos;
            brick.transform.SetParent(wall.transform, false);
            int score = UnityEngine.Random.Range(1, 3);

            brick.GetComponent<Brick>().setNumber(score);
            brick.GetComponent<Brick>().setColor(getBrickColor(score));
        }
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
            dir = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            delVX = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - originV).x;
            originV = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0)) {
            delVX = 0;
        }

        moveBalls();
    }

    public void ballMoveStart()
    {
        for (int i = 0; i < ballList.Count; i++) {
            ballList[i].transform.DOLocalMove(ballList[i].transform.localPosition + new Vector3(0, (ballList.Count -1- i) * r * 2, 0), 0.2f)
                .SetEase(Ease.Linear)
                .SetAutoKill(true)
                .SetUpdate(true);
        }
        game_status = GAME_STATUS.START;
    }

    void moveBalls() {
        if (ballList == null || ballList.Count == 0)
            return;

        ballList[0].transform.localPosition = ballList[0].transform.localPosition + new Vector3(delVX, dirY, 0);
        for (int i = 1; i < ballList.Count;i++ )
        {
            Vector3 deltaV = ballList[i-1].transform.localPosition - ballList[i].transform.localPosition;
            Vector3 moveV = deltaV - Vector3.Normalize(deltaV) * r * 2;
            ballList[i].transform.localPosition = ballList[i].transform.localPosition + moveV;
        }
    }

    public int getBallCount()
    {
        if (ballList == null)
            return 0;
        else
            return ballList.Count;
    }

    public void hitBrick(GameObject brick)
    {
        // 减球
        gameUI.updateScore();
        Destroy(ballList[ballList.Count - 1]);
        ballList.RemoveAt(ballList.Count - 1);

        if (ballList.Count == 0)
        {
            game_status = GAME_STATUS.READY;
            return;
        }
        ballList[0].transform.localPosition = ballList[0].transform.localPosition - new Vector3(0, r * 2, 0);

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

            genBricks(false);
        }
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
