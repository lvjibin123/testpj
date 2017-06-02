using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour {

    GameController gameController;
    void Awake()
    {
        gameController = GameObject.Find("Game").GetComponent<GameController>();
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.name.Contains("brick"))
        {
            // 只能从底部碰撞
            if (coll.transform.localPosition.y - 1.115f/2 < transform.position.y)
                return;
            int count = gameController.getBallCount();
            
            gameController.hitBrick(coll.gameObject);
        }
        else if (coll.gameObject.name.Contains("gainball"))
        {
            gameController.gainBall(coll.gameObject);
        }
    }
}
