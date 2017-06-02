using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour {

	// Use this for initialization
    float delta = -1;
    int number = 1;
    Vector3 move;
    GameObject Text_number;
    GameObject brick0;
    GameController gameController;
    bool isWall = false;

    public void setNumber(int number) {
        this.number = number;
        Text_number.GetComponent<TextMesh>().text = number + "";
        if (number >= 1000)
        {
            Text_number.GetComponent<TextMesh>().fontSize = 26;
            if (number >= 10000)
            {
                Text_number.GetComponent<TextMesh>().fontSize = 20;
            }
        }
        else {
            Text_number.GetComponent<TextMesh>().fontSize = 36;
        }
    }

    public void setWall() {
        isWall = true;
    }

    public bool getWall() {
        return isWall;
    }

    public void setColor(Color color0)
    {
        brick0.GetComponent<SpriteRenderer>().color = color0;
    }

    public int getNumber() {
        return number;
    }

    void Awake()
    {
        Text_number = transform.Find("number").gameObject;
        brick0 = transform.Find("brick0").gameObject;
        move = Vector3.down;
        gameController = GameObject.Find("Game").GetComponent<GameController>();
    }

	void Start () {
	}
	
	void Update () {
        if (delta >= 0)
        {
            transform.Translate(move * Time.deltaTime * 2, Space.World);
            delta = delta + Time.deltaTime * 2 * Mathf.Abs(move.y);
            if (delta > Mathf.Abs(move.y))
            {
                delta = -1f;
            }
        }
	}

    public void setMove(Vector3 moveV2)
    {
        move = moveV2;
    }

    public void moveDown()
    {
        delta = 0f;
    }

    public void hit() {
        number--;
        if (number > 0) {
            Text_number.GetComponent<TextMesh>().text = number +"";
            if (number >= 1000)
            {
                Text_number.GetComponent<TextMesh>().fontSize = 26;
                if (number >= 10000)
                {
                    Text_number.GetComponent<TextMesh>().fontSize = 20;
                }
            }
            brick0.GetComponent<SpriteRenderer>().color = gameController.getBrickColor(number);
        }
    }

    public void destropyBrick(){
        Destroy(transform.gameObject);
    }

}
