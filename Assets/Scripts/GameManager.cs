using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

using UnityEngine.Advertisements;

public class GameManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsShowListener
{
    // ========================================================
    // 1. TEXTOS DE LA PANTALLA
    // ========================================================
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI clicksText;
    public TextMeshProUGUI highScoreText;
    public GameObject creditsPanel;

    // ========================================================
    // 2. BOTONES
    // ========================================================
    public Button mainButton;
    public Button rewardAdButton;
    public Button openCreditsButton;
    public Button closeCreditsButton;

    // ========================================================
    // 3. VARIABLES DE CONFIGURACIÓN
    // ========================================================
    private string androidGameId = "6112478"; 
    private bool testMode = false; 

    // ========================================================
    // 4. DATOS DEL JUEGO
    // ========================================================
    private int clicks = 0; 
    private float timeRemaining = 10f; 
    private bool isPlaying = false; 
    private int highScore = 0; 
    private float bonusTime = 0f; 

    void Start()
    {
       
        mainButton.onClick.AddListener(HacerClickEnBotonVerde);
        openCreditsButton.onClick.AddListener(PrenderOApagarCreditos);
        closeCreditsButton.onClick.AddListener(PrenderOApagarCreditos);
        rewardAdButton.onClick.AddListener(MostrarAnuncioConRecompensa);

     
        if (Advertisement.isSupported == true && Advertisement.isInitialized == false)
        {
            Advertisement.Initialize(androidGameId, testMode, this);
        }

       
        highScore = PlayerPrefs.GetInt("HighScore", 0);

       
        creditsPanel.SetActive(false);

       
        ActualizarTextosEnPantalla();

      
#if UNITY_WEBGL
        rewardAdButton.gameObject.SetActive(false);
#endif

     
#if UNITY_ANDROID
        PrepararCanalDeNotificaciones();
#endif
    }

    void Update()
    {
      
       
        if (isPlaying == true)
        {
           
            timeRemaining = timeRemaining - Time.deltaTime;

            ActualizarTextosEnPantalla();

           
            if (timeRemaining <= 0)
            {
                TerminarElJuego();
            }
        }
    }

    private void HacerClickEnBotonVerde()
    {
      
        if (isPlaying == false)
        {
            isPlaying = true; 
            clicks = 1; 

        
            timeRemaining = 10f + bonusTime;
            bonusTime = 0f; 

          
            rewardAdButton.interactable = false;
        }
       
        else
        {
            clicks = clicks + 1;
        }

        ActualizarTextosEnPantalla();
    }

    private void TerminarElJuego()
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
           
            MostrarAnuncioPantallaCompleta();
        }

      
        rewardAdButton.interactable = true;
        ActualizarTextosEnPantalla();

        
#if UNITY_ANDROID
        MandarNotificacionAlCelular();
#endif
    }

    private void ActualizarTextosEnPantalla()
    {
       
        int segundos = Mathf.FloorToInt(timeRemaining);
        int centesimas = Mathf.FloorToInt((timeRemaining % 1) * 100);

 
        timeText.text = "Tiempo: " + segundos.ToString("00") + ":" + centesimas.ToString("00");
        clicksText.text = clicks.ToString("00") + " clicks";
        highScoreText.text = "High score: " + highScore;
    }

    private void PrenderOApagarCreditos()
    {
       
        bool estaPrendido = creditsPanel.activeSelf;

     
        creditsPanel.SetActive(!estaPrendido);
    }


    // =========================================================================
    // SECCIÓN DE ANUNCIOS Y NOTIFICACIONES
    // =========================================================================

    private void MostrarAnuncioConRecompensa()
    {
        Advertisement.Show("Rewarded_Android", this); 
    }

    private void MostrarAnuncioPantallaCompleta()
    {
        Advertisement.Show("Interstitial_Android", this); 
    }

    
    public void OnInitializationComplete()
    {
        
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);

        
        Advertisement.Banner.Load("Banner_Android", new BannerLoadOptions
        {
            loadCallback = () => Advertisement.Banner.Show("Banner_Android")
        });
    }

 
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        
        if (placementId == "Rewarded_Android" && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            bonusTime = 2f; 
            rewardAdButton.interactable = false; 
        }
    }

   
    public void OnInitializationFailed(UnityAdsInitializationError error, string message) { }
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) { }
    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    // --- NOTIFICACIONES ---

    private void PrepararCanalDeNotificaciones()
    {
#if UNITY_ANDROID
      
        var channel = new AndroidNotificationChannel()
        {
            Id = "game_channel",
            Name = "Notificaciones Clicker",
            Importance = Importance.Default,
            Description = "Recordatorios",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    private void MandarNotificacionAlCelular()
    {
#if UNITY_ANDROID
     
        var notification = new AndroidNotification();
        notification.Title = "¡Volvé a jugar!";
        notification.Text = "Juego desarrollado por Agustín Lázaro Blanco Romero";

       
        notification.FireTime = DateTime.Now.AddMinutes(10);

      
        AndroidNotificationCenter.SendNotification(notification, "game_channel");
#endif
    }
}