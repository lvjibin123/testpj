using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopController : MonoBehaviour
{

    GameUI gameUI;
  //  private AdvertiseSup adsSup;
  //  private InAppPurchaseSup purchaseSup;
 //   private AdmobBannerNative mAdmobBannerNative;
 //   private GPNativeAd nativeAd;
    private Hashtable coinMaps;
    Text loop_text;
    private float frequent = 0.1f;
    private int step;
    int bestscore;

    GameObject Text_timer;
    GameObject video;
    DateTime aimTime;
    AudioSource audio;
    AudioClipSet audioclip_set;

    void Awake()
    {

    }
    // Use this for initialization
	void Start () {
        gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
        Text_timer = transform.Find("Button_video").transform.Find("Text_timer").gameObject;
        video = transform.Find("Button_video").transform.Find("video").gameObject;

        GameObject go_audioclip = GameObject.Find("AudioClipSet");
        audio = GetComponent<AudioSource>();
        if (go_audioclip)
        {
            audioclip_set = go_audioclip.GetComponent<AudioClipSet>();
        }
       // purchaseSup = GameObject.Find("GamePlus").GetComponent<InAppPurchaseSup>();
       // adsSup = GameObject.Find("GamePlus").GetComponent<AdvertiseSup>();
       // nativeAd = GameObject.Find("GamePlus").GetComponent<GPNativeAd>();
       // mAdmobBannerNative = GameObject.Find("GamePlus").GetComponent<AdmobBannerNative>();
       // purchaseSup.SetPurchaseListner(this);
        coinMaps = new Hashtable();
        //coinMaps.Add(AppConfig.PURCHASE_COIN_200, 200);
        //coinMaps.Add(AppConfig.PURCHASE_COIN_500, 500);
        //coinMaps.Add(AppConfig.PURCHASE_COIN_900, 900);
        //coinMaps.Add(AppConfig.PURCHASE_COIN_2000, 2000);
        //updateUnlockUI();
        initVideoTime();
    }

    void initVideoTime()
    {
        if (PlayerPrefs.GetString("EndTime", "") == "")
        {
            //第一次登陆，应该赠送一次机会。

            Text_timer.SetActive(false);
            video.SetActive(true);
        }
        else
        {
            string timeStr = PlayerPrefs.GetString("EndTime");
            aimTime = Convert.ToDateTime(timeStr);

            compareVideoTime();
        }
    }

    void compareVideoTime()
    {
        DateTime dt = System.DateTime.Now;
        if (dt.CompareTo(aimTime) >= 0)
        {
            //倒计时结束
            Text_timer.SetActive(false);
            video.SetActive(true);
        }
        else
        {
            Text_timer.SetActive(true);
            video.SetActive(false);
            TimeSpan span = aimTime.Subtract(dt);
            Text_timer.GetComponent<Text>().text = span.ToString().Substring(0,8);
        }
    }


    //private static void setNativeUI(NativeAdInfo info, GameObject item)
    //{
    //    if (Application.internetReachability == NetworkReachability.NotReachable)
    //    {
    //        item.SetActive(false);
    //        return;
    //    }

    //    int show_ads = PlayerPrefs.GetInt(Constance.SHOW_ASDS, 0);
    //    if (show_ads == 1)
    //    {
    //        item.SetActive(false);
    //        return;
    //    }

    //    //刷新原生广告UI
    //    if (info != null)
    //    {
    //        if (info.IconImage != null)
    //        {
    //            item.transform.Find("icon").GetComponent<Image>().sprite = info.IconImage;
    //        }
    //        item.transform.Find("title").GetComponent<Text>().text = info.Title;
    //        item.transform.Find("desc").GetComponent<Text>().text = info.SocialContext;
    //    }
    //    else
    //    {
    //        item.SetActive(false);
    //    }
    //}
	
	// Update is called once per frame
	void Update () {
        if (Text_timer.activeSelf && aimTime != null)
        {
            PlayerPrefs.SetString("LastTime", System.DateTime.Now.ToString());
            compareVideoTime();
        }
	}

    public void onBackClick()
    {
        AudioSourcesManager.GetInstance().Play(audio, (audioclip_set == null) ? null : audioclip_set.button_click);
        transform.localPosition = new Vector3(0, 1960, 0);
        // mAdmobBannerNative.show();
       // gameUI.onMenuClick();
    }

    public void onPurchaseCLick(string arguments)
    {
        string[] args = arguments.Split(' ');
        Debug.Log(args[0]);
        Debug.Log(args[1]);
     //   purchaseSup.PurchaseProduct(args[0], args[1]);
    }


    //public void onAdclick() {
    //    if (Text_timer.activeSelf)
    //        return;
    //    AdsUtils.setInterStutas(false);
    //    ShowOptions options = new ShowOptions();
    //    options.resultCallback = AdCallbackhanler;
    //    if (Advertisement.IsReady(AppConfig.UNITY_PLACEMENT_ID))
    //    {
    //        Advertisement.Show(AppConfig.UNITY_PLACEMENT_ID, options);
    //    }
    //    Dictionary<string, object> desc = new Dictionary<string, object>();
    //    desc.Add("status", "clicked");
    //    desc.Add("type", "shop reward ads");
    //    desc.Add("IsReady", Advertisement.IsReady(AppConfig.UNITY_PLACEMENT_ID));
    //    AnalysisSup.fabricLog(EventName.PLAY_VIDEO, desc);
    //    Debug.Log("VedioAd");
    //}

    //void PurchaseListner.PurchaseResult(bool success, string productId)
    //{
    //    Debug.Log("PurchaseResult ======" + success + " " + productId);
    //    if (success)
    //    {
    //        if ("no_ads2".Equals(productId))
    //        {
    //            PlayerPrefs.SetInt(Constance.SHOW_ASDS, 1);
    //            if (adsSup.adView != null) {
    //                adsSup.adView.Dispose();
    //            }
    //            GameObject btm_ad = GameObject.Find("btm_ad");
    //            if (btm_ad != null)
    //            {
    //                btm_ad.SetActive(false);
    //            }
    //            //#if UNITY_ANDROID && UNITY_IPHONE
    //            //    adsSup.adView.Dispose();
    //            //#endif
    //        }
    //        else if (coinMaps.ContainsKey(productId)) {
    //            int coin = (int)coinMaps[productId];
    //            gameUI.addCoinDynamic(coin);
    //        }
    //        else
    //        {
    //            Debug.Log("product ======"+ productId + " initial fail");
    //        }
    //    }
    //}

    //void AdCallbackhanler(ShowResult result)
    //{
    //    switch (result)
    //    {
    //        case ShowResult.Finished:
    //            Debug.Log("Ad Finished. Rewarding player...");
    //            gameUI.addCoinDynamic(BallzConstants.WATCH_VIDEO_COIN);
    //            // 重新计时
    //            aimTime = System.DateTime.Now.AddSeconds(BallzConstants.VIDEO_COUNTDOWN_TIME);
    //            PlayerPrefs.SetString("EndTime", aimTime.ToString());
    //            Text_timer.SetActive(true);
    //            video.SetActive(false);
    //            break;
    //        case ShowResult.Skipped:
    //            Debug.Log("Ad Skipped");
    //            break;
    //        case ShowResult.Failed:
    //            Debug.Log("Ad failed");
    //            break;
    //    }
    //}
}
