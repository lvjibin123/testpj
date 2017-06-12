using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour {
    GameObject Tutorial;
    GameObject tutorialClose;
    GameObject gesture;
    ScrollRect Scroller;
    bool scrollTag = true;
  //  private InAppPurchaseSup purchaseSup;
    GameObject soundBtn;
    // Use this for initialization
	void Start () {
     //   soundBtn = GameObject.Find("soundBtn");
        Tutorial = GameObject.Find("Tutorial");
        tutorialClose = Tutorial.transform.Find("close").gameObject;
        gesture = GameObject.Find("gesture");
        Scroller = GameObject.Find("Scroller").GetComponent<ScrollRect>();
        Scroller.onValueChanged.AddListener(ListenerMethod);
 //       purchaseSup = GameObject.Find("GamePlus").GetComponent<InAppPurchaseSup>();
	}

    public void ListenerMethod(Vector2 value)
    {
        //Debug.Log("ListenerMethod: " + value.x);
        if (value.x < 0 && scrollTag)
        {
            scrollTag = false;
        }
        else if (!scrollTag && value.y > 1)
        {
            scrollTag = true;
        }
    }

	// Update is called once per frame
	void Update () {
        
        if (gesture.transform.localPosition.y < -500)
        {
            gesture.transform.localPosition = new Vector3(191, -212, 0);
        }
        else
        {
            gesture.transform.Translate(Vector3.down * Time.deltaTime);
        }
	}

    public void onCloseClick()
    {
        transform.localPosition = new Vector3(0, 1920, 0);
  //      soundBtn.GetComponent<AudioController>().playSound();
    }

    public void onRestoreClick()
    {
   //     purchaseSup.RestoreAppPurchase();
    }
}
