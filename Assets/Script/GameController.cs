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

    float delVX;
    int dir = 0;
    Vector3 offsetBg;
    Vector3 offsetCamera;
    Loops loopdata;

    //prefab
    GameObject brick0;

	void Awake () {
        game_status = GAME_STATUS.READY;
        trail = transform.Find("trail").gameObject;
        wall = transform.Find("wall").gameObject;
        bg = transform.Find("bg").gameObject;

        ballList = new List<GameObject>();

        for (int i = 0; i < trail.transform.childCount; i++) {
            ballList.Add(trail.transform.GetChild(i).gameObject);
        }
        offsetBg = bg.transform.position - ballList[0].transform.position;
        offsetCamera = camera.transform.position - ballList[0].transform.position;

        brick0 = (GameObject)Resources.Load("Prefab/brick");
        loopdata = JsonUtil.LoadColorFromFile();
        genBricks();
    }
	
	void Update () {
        if (game_status == GAME_STATUS.START)
        {
            checkClick();
        }
	}

    void LateUpdate() {
        bg.transform.position = offsetBg + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
        camera.transform.position = offsetCamera + new Vector3(0, ballList[0].transform.position.y, ballList[0].transform.position.z);
    }


    void genBricks() {
        for (int i = 0; i < 5; i++) {
            Vector3 pos = new Vector3((i - 2) * brickLength, 10, 0);

            GameObject brick = GameObject.Instantiate<GameObject>(brick0);
            brick.transform.position = pos;
            brick.transform.SetParent(wall.transform, false);
            int score = 5;

            brick.GetComponent<Brick>().setNumber(score);
         //   brick.GetComponent<Brick>().setColor(getBrickColor(score));
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
}
