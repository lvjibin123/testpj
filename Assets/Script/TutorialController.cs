using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour {

    GameObject Tutorial;
    GameObject tutorialClose;
    GameObject gesture;
    ScrollRect Scroller;
    AudioSource audio;
    AudioClipSet audioclip_set;

  //  private InAppPurchaseSup purchaseSup;
    bool scrollTag = true;

	void Start () {
        Tutorial = GameObject.Find("Tutorial");
        tutorialClose = Tutorial.transform.Find("close").gameObject;
        gesture = GameObject.Find("gesture");
        Scroller = GameObject.Find("Scroller").GetComponent<ScrollRect>();
        Scroller.onValueChanged.AddListener(ListenerMethod);
        GameObject go_audioclip = GameObject.Find("AudioClipSet");
        audio = GetComponent<AudioSource>();
        if (go_audioclip)
        {
            audioclip_set = go_audioclip.GetComponent<AudioClipSet>();
        }

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
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        transform.localPosition = new Vector3(0, 1920, 0);
    }

    public void onRestoreClick()
    {
   //     purchaseSup.RestoreAppPurchase();
    }
}
