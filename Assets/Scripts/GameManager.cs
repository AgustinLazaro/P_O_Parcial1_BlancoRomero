using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

using UnityEngine.Advertisements;

public class GameManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    // REFERENCIAS A LA UI 
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI clicksText;
    public TextMeshProUGUI highScoreText;
    public GameObject creditsPanel;

    public Button mainButton;
    public Button rewardAdButton;
    public Button openCreditsButton;
    public Button closeCreditsButton;

    // CONFIGURACION DE ADS
    private string androidGameId = "6112478";
    private bool testMode = true;

    // DATOS DEL JUEGO
    private int clicks = 0;
    private float timeRemaining = 10f;
    private bool isPlaying = false;
    private int highScore = 0;
    private float bonusTime = 0f;

 
    // UNITY LIFECYCLE
   

    void Start()
    {
        mainButton.onClick.AddListener(OnGreenButtonClick);
        openCreditsButton.onClick.AddListener(ToggleCredits);
        closeCreditsButton.onClick.AddListener(ToggleCredits);
        rewardAdButton.onClick.AddListener(ShowRewardedAd);

        highScore = PlayerPrefs.GetInt("HighScore", 0);

        creditsPanel.SetActive(false);

        UpdateUI();

#if UNITY_WEBGL
        rewardAdButton.gameObject.SetActive(false);
        Cursor.visible = true;          
        Cursor.lockState = CursorLockMode.None;  
#endif

#if UNITY_ANDROID
        InitializeAds();
        AndroidNotificationCenter.RequestPermission();
        CreateNotificationChannel();
#endif
    }

    void Update()
    {
        if (!isPlaying) return;

        timeRemaining -= Time.deltaTime;
        UpdateUI();

        if (timeRemaining <= 0)
        {
            EndGame();
        }
    }


    // GAME LOGIC
   
    private void OnGreenButtonClick()
    {
        if (!isPlaying)
        {
           
            isPlaying = true;
            clicks = 1;
            timeRemaining = 10f + bonusTime;
            bonusTime = 0f;
            rewardAdButton.interactable = false;
        }
        else
        {
            
            clicks++;
        }

        UpdateUI();
    }

    private void EndGame()
    {
        isPlaying = false;
        timeRemaining = 0;

        if (clicks > highScore)
        {
           
            highScore = clicks;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        else
        {
           
            ShowInterstitialAd();
        }

        rewardAdButton.interactable = true;
        UpdateUI();

#if UNITY_ANDROID
        ScheduleNotification();
#endif
    }

    private void UpdateUI()
    {
        int seconds = Mathf.FloorToInt(timeRemaining);
        int centiseconds = Mathf.FloorToInt((timeRemaining % 1) * 100);

        timeText.text = "Tiempo: " + seconds.ToString("00") + ":" + centiseconds.ToString("00");
        clicksText.text = clicks.ToString("00") + " clicks";
        highScoreText.text = "High score: " + highScore;
    }

    private void ToggleCredits()
    {
        creditsPanel.SetActive(!creditsPanel.activeSelf);
    }

   
    // ADS

    private void InitializeAds()
    {
        if (Advertisement.isSupported && !Advertisement.isInitialized)
        {
            Advertisement.Initialize(androidGameId, testMode, this);
        }
    }

    
    public void OnInitializationComplete()
    {
      
        Advertisement.Load("Interstitial_Android", this);
        Advertisement.Load("Rewarded_Android", this);

       
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Load("Banner_Android", new BannerLoadOptions
        {
            loadCallback = () => Advertisement.Banner.Show("Banner_Android"),
            errorCallback = (error) => Debug.Log("Banner error: " + error)
        });
    }

    private void ShowInterstitialAd()
    {
        Advertisement.Show("Interstitial_Android", this);
    }

    private void ShowRewardedAd()
    {
        Advertisement.Show("Rewarded_Android", this);
    }

   
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == "Rewarded_Android" && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
           
            bonusTime = 2f;
            rewardAdButton.interactable = false;
        }
    }

 
    public void OnUnityAdsAdLoaded(string placementId) { }
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log("Error loading ad " + placementId + ": " + message);
    }

   
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("Ads initialization error: " + message);
    }
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) { }
    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    
    // NOTIFICATIONS (Android only)
    
    private void CreateNotificationChannel()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = "game_channel",
            Name = "Clicker Notifications",
            Importance = Importance.Default,
            Description = "Reminders to come back and play",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    private void ScheduleNotification()
    {
#if UNITY_ANDROID
        
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        var notification = new AndroidNotification();
        notification.Title = "Come back and play!";
        notification.Text = "Game developed by Agustin Lazaro Blanco Romero";
        notification.FireTime = DateTime.Now.AddMinutes(10);

        AndroidNotificationCenter.SendNotification(notification, "game_channel");
#endif
    }
}