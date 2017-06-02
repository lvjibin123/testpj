using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour {

    GameController gameController;
    TextMesh ballCount;
    void Awake()
    {
        gameController = GameObject.Find("Game").GetComponent<GameController>();
        ballCount = transform.Find("ballCount").GetComponent<TextMesh>();
    }

    void setBallCount(int count){
        ballCount.text = count + "";
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.name.Contains("brick"))
        {
            int count = gameController.getBallCount();
            if (count == 0)
                return;
            if (count > 1)
                setBallCount(count - 2);
            else
                setBallCount(0);

            gameController.hitBrick(coll.gameObject);
        }
        else if (coll.gameObject.name.Contains("gainball"))
        {
          //  gameController.gainBall(coll.gameObject);
        }
    }
}
