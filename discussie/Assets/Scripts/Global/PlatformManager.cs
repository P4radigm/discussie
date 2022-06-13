using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance;

    [Header("Connectivity")]
    public bool hasInternetConnection;
    public bool hasServerConnection;
    //private Coroutine hasInternetRoutine;
    public float internetCheckInterval;
    private float internetCheckTimer;

    [Header("Screen")]
    public Vector2 ScreenResolution;
    public Rect ScreenSafeArea;
    [Space(20)]
    public Vector2 WorldScreenBotLeftCoords;    
    public Vector2 WorldScreenTopLeftCoords;
    public Vector2 WorldScreenTopRightCoords;
    public Vector2 WorldScreenBotRightCoords;
    [Space(20)]
    public Vector2 WorldSafeScreenBotLeftCoords;
    public Vector2 WorldSafeScreenTopLeftCoords;
    public Vector2 WorldSafeScreenTopRightCoords;
    public Vector2 WorldSafeScreenBotRightCoords;

    private void Awake()
    {
		#region Singleton
		if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion

        ServerCheck();
        GetScreenInfo();
    }

	private void Update()
	{
        if(internetCheckTimer <= 0) 
        { 
            ServerCheck();
            internetCheckTimer = internetCheckInterval;
        }
		else
		{
            internetCheckTimer -= Time.deltaTime;
        }
    }

	private void GetScreenInfo()
	{
        ScreenResolution = new Vector2(Screen.width, Screen.height);
        ScreenSafeArea = Screen.safeArea;

        WorldScreenBotLeftCoords = Camera.main.ScreenToWorldPoint(Vector2.zero);
        WorldScreenTopLeftCoords = Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height));
        WorldScreenTopRightCoords = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        WorldScreenBotRightCoords = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0));

        WorldSafeScreenBotLeftCoords = Camera.main.ScreenToWorldPoint(Screen.safeArea.min);
        WorldSafeScreenTopLeftCoords = Camera.main.ScreenToWorldPoint(new Vector2(Screen.safeArea.min.x, Screen.safeArea.max.y));
        WorldSafeScreenTopRightCoords = Camera.main.ScreenToWorldPoint(Screen.safeArea.max);
        WorldSafeScreenBotRightCoords = Camera.main.ScreenToWorldPoint(new Vector2(Screen.safeArea.max.x, Screen.safeArea.min.y));
    }

    private async void ServerCheck()
    {
        hasInternetConnection = Application.internetReachability == NetworkReachability.NotReachable ? false : true;

        //Might want to update this to server ping instead of google ping
        UnityWebRequest request = new UnityWebRequest("https://studenthome.hku.nl/~leon.vanoldenborgh/");
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        hasServerConnection = request.error == null;
    }


}
