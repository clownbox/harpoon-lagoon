using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
public class TestAds : MonoBehaviour {

    //public GameObject MenuWithAds;
  //  public GameObject MenuWithoutAds;
    string gameID = "1562108";
    public static int adStatus;
    void Awake()
    {
        Advertisement.Initialize(gameID, true);
        
    }
    void Start()
    {
       
        adStatus = 1;
        PlayerPrefs.SetInt("Status",1);
        adStatus=PlayerPrefs.GetInt("Status");
        if (adStatus == 2)
        {
            Debug.Log("AdsRemove.....");
          //  MenuWithAds.SetActive(false);
          //  MenuWithoutAds.SetActive(true);
        }
        else
        {
            Debug.Log("Ads Button Show....");
        //    MenuWithAds.SetActive(true);
        //    MenuWithoutAds.SetActive(false);
        }
       
    #if UNITY_EDITOR 
        StartCoroutine(WaitForAd());
    #endif

       // StartCoroutine(AdsShow());

    }
    IEnumerator AdsShow()
    {
        while (!Advertisement.IsReady())
        {
            yield return null;
        }
        Advertisement.Show();
    }
    IEnumerator WaitForAd()
    {
        float currentTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        yield return null;
        while(Advertisement.isShowing)
        {
            yield return null;
        }
        Time.timeScale = currentTimeScale;
    }

    //public void removeAds()
    //{
    //    //////Android/////
    //    //UnityIAP.Instance.BuyNonConsumable();

    //    //////IOS Test/////
    //    iapPanel.SetActive(true);
    //   // adStatus = 2;
    //}
    public void RemoveAdPurchase()
    {

        UnityIAP.Instance.BuyNonConsumable();
    }

    public void tripOver()
    {
        
        DashedLine.enableHoldLine = false;
    }


    //public void onClickIAPButtons()
    //{
    //    iapPanel.SetActive(false);
    //    iapMessage.SetActive(true);
    //    Invoke("IAPMessage", 5f);
    //}
    //void IAPMessage()
    //{
    //    iapMessage.SetActive(false);
    //}
}
