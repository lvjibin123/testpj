using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GainBall : MonoBehaviour {

    GameController gameController;
    TextMesh ballCount;
    int count = 0;
    void Awake()
    {
        gameController = GameObject.Find("Game").GetComponent<GameController>();
        ballCount = transform.Find("ballCount").GetComponent<TextMesh>();
    }

    public void setBallCount(int count){
        this.count = count;
        ballCount.text = count + "";
    }

    public int getCount() {
        return count;
    }
}
