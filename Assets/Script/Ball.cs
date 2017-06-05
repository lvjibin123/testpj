using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour {

    private CharacterController2D _controller;
    GameController gameController;

    void Awake()
    {
        gameController = GameObject.Find("Game").GetComponent<GameController>();

        _controller = GetComponent<CharacterController2D>();

        // listen to some events for illustration purposes
        _controller.onControllerCollidedEvent += onControllerCollider;
        _controller.onTriggerEnterEvent += onTriggerEnterEvent;
        _controller.onTriggerExitEvent += onTriggerExitEvent;
        _controller.setEventActive(true);
    }

    public void move(Vector3 dir) {
        _controller.move(dir);
    }

    void onControllerCollider(RaycastHit2D hit)
    {
        if (gameController.isFirstBall(gameObject))
        {
            if (hit.transform.name.Contains("brick"))
            {
                // 只能从底部碰撞
                if (hit.transform.localPosition.y - 1.115f / 2 < transform.position.y)
                    return;

                gameController.hitBrick(hit.transform.gameObject);
            }
            else if (hit.transform.name.Contains("gainball"))
            {
                gameController.gainBall(hit.transform.gameObject);
            }
            else if (hit.transform.name.Contains("Bar"))
            {
                Debug.Log("Bar");
            }
        }
    }

    void onTriggerEnterEvent(Collider2D col)
    {
        Debug.Log("onTriggerEnterEvent: " + col.gameObject.name);
    }


    void onTriggerExitEvent(Collider2D col)
    {
        Debug.Log("onTriggerExitEvent: " + col.gameObject.name);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (gameController.isFirstBall(gameObject))
        {
            if (coll.gameObject.name.Contains("brick"))
            {
                // 只能从底部碰撞
                if (coll.transform.localPosition.y - 1.115f / 2 < transform.position.y)
                    return;

                gameController.hitBrick(coll.gameObject);
            }
            else if (coll.gameObject.name.Contains("gainball"))
            {
                gameController.gainBall(coll.gameObject);
            }
        }
    }
}
