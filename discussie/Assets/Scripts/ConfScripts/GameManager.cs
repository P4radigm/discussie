using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public enum GameStates
    {
        menu,
        playing,
        definitionA,
        playingA,
        definitionB,
        playingB,
        end,
        final
	}

    [SerializeField] private InputFieldController iFC;
    [SerializeField] private SmoothFollow sF;
    [SerializeField] public ColorUpdate cU;
    [SerializeField] public PlayerControls pC;
    [SerializeField] private AnimationController aC;
    [SerializeField] private CameraZoom cZ;
    [SerializeField] private TouchMessageAnimations tMA;
    [SerializeField] public RectTransform playerStartPos;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] public Animator animator;
    [Header("TMPRO elements")]
    [SerializeField] private TextMeshProUGUI fgTimer;
    [SerializeField] private TextMeshProUGUI bgTimer;
    [SerializeField] private TextMeshProUGUI fgDefinition;
    [SerializeField] private TextMeshProUGUI bgDefinition;
    [SerializeField] private TextMeshProUGUI fgWord;
    [SerializeField] private TextMeshProUGUI bgWord;
    [SerializeField] private TextMeshProUGUI fgTouch;
    [SerializeField] private TextMeshProUGUI bgTouch;
    [Space(20)]
    [SerializeField] private float firstTimer;
    [SerializeField] private float secondTimer;
    [SerializeField] private float minColSat;
    [SerializeField] private float minColVal;
    //[SerializeField] private Color minBlack;
    //[SerializeField] private Color minWhite;
    [Space(20)]
    public float playTime;
    public GameStates gameState;

    private bool finalAnimIsDone = false;
    private bool hasConformed;
    private JSONManager JSONmanager;

    void Start()
    {
        JSONmanager = JSONManager.instance;
        gameState = GameStates.menu;

        cU.animsOnly = true;
        UpdateDefinition(JSONmanager.GetNewDefinition(false));

        //Set player position
        pC.transform.position = new Vector3(playerStartPos.position.x, playerStartPos.position.y, -1);

        tMA.StartBlinkLoop();

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
            Application.Quit();
		}

		switch (gameState)
		{
			case GameStates.menu:
                sF.enabled = false;                
                if (cU != null)
                {
                    cU.followSpawnPosition = true;
                    cU.animsOnly = true;
                }
                if (pC != null) { pC.enabled = false; }
                aC.enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    animator.SetTrigger("Play");
                }

                break;
			case GameStates.playing:
                sF.enabled = true;
                
                if (cU != null)
                {
                    cU.followSpawnPosition = false;
                    cU.animsOnly = false;
                }
                if (pC != null) { pC.enabled = true; }
                aC.enabled = false;
                playTime += Time.deltaTime;
                fgTimer.text = TimerFormat(firstTimer - playTime);
                bgTimer.text = TimerFormat(firstTimer - playTime);

                if (cU.updateHSVcol.y <= minColSat || cU.updateHSVcol.z <= minColVal)
                {
                    hasConformed = true;
                    Destroy(cU.gameObject);
                    animator.SetTrigger("Definition");
                    UpdateDefinition(JSONmanager.GetNewDefinition(true));
                    UpdateTouch("<color=#FF00D1>Tik</color> om verder te gaan");
                    gameState = GameStates.definitionA;
                }
                if (firstTimer - playTime <= 0)
                {
                    hasConformed = false;
                    animator.SetTrigger("Definition");
                    UpdateDefinition(JSONmanager.GetNewDefinition(true));
                    UpdateTouch("<color=#FF00D1>Tik</color> om verder te gaan");
                    gameState = GameStates.definitionA;
                }

                break;
			case GameStates.definitionA:
                //Put in new definitionA

                sF.enabled = false;
                //cU.enabled = false;
                if (cU != null)
                {
                    cU.followSpawnPosition = true;
                    cU.animsOnly = true;
                }
                if (pC != null) { pC.enabled = false; }
                aC.enabled = true;


                if (!tMA.addDelay && Input.GetMouseButtonDown(0))
                {
                    animator.SetTrigger("Play");
                    playTime = 0;
                }

                break;
			case GameStates.playingA:
                sF.enabled = true;
                
                if (cU != null)
                {
                    cU.followSpawnPosition = false;
                    cU.animsOnly = false;
                }
                if (pC != null) { pC.enabled = true; }
                aC.enabled = false;
                playTime += Time.deltaTime;
                fgTimer.text = TimerFormat(secondTimer - playTime);
                bgTimer.text = TimerFormat(secondTimer - playTime);

                if (cU.updateHSVcol.y <= minColSat || cU.updateHSVcol.z <= minColVal)
                {
                    hasConformed = true;
                    Destroy(cU.gameObject);
                    animator.SetTrigger("Definition");
                    UpdateDefinition(JSONmanager.GetNewDefinition(true));
                    UpdateTouch("<color=#FF00D1>Tik</color> om verder te gaan");
                    gameState = GameStates.definitionB;
                }
                if (firstTimer - playTime <= 0)
                {
                    hasConformed = false;
                    animator.SetTrigger("Definition");
                    UpdateDefinition(JSONmanager.GetNewDefinition(true));
                    UpdateTouch("<color=#FF00D1>Tik</color> om verder te gaan");
                    gameState = GameStates.definitionB;
                }

                break;
			case GameStates.definitionB:
                //Put in new definitionB

                sF.enabled = false;
                
                if (cU != null)
                {
                    cU.followSpawnPosition = true;
                    cU.animsOnly = true;
                }
                if (pC != null) { pC.enabled = false; }
                aC.enabled = true;


                if (!tMA.addDelay && Input.GetMouseButtonDown(0))
                {
                    animator.SetTrigger("Play");
                    playTime = 0;
                }

                break;
			case GameStates.playingB:
                sF.enabled = true;
                //cU.enabled = true;
                if (cU != null)
                {
                    cU.followSpawnPosition = false;
                    cU.animsOnly = false;
                }
                if (pC != null) { pC.enabled = true; }
                aC.enabled = false;
                playTime += Time.deltaTime;
                fgTimer.text = StopwatchFormat(playTime);
                bgTimer.text = StopwatchFormat(playTime);

                if (cU.targetRenderer.color == Color.black || cU.targetRenderer.color == Color.white)
                {
                    animator.SetTrigger("End");
                    UpdateWord("con·for·me·ren");
                    UpdateDefinition("<i>Vul nu zelf je eigen definitie in...</i>");
                    Destroy(cU.gameObject);
                    gameState = GameStates.end;
                }

                break;
			case GameStates.end:
                sF.enabled = false;
                //cU.enabled = false;
                //pC.enabled = false;
                aC.enabled = true;

                //fix dat ze zelf een definitie in moeten vullen

                //if (endAnimIsDone && Input.GetMouseButtonDown(0))
                //{
                //             SceneManager.LoadScene(0);
                //}
                break;
            case GameStates.final:
                sF.enabled = false;
                aC.enabled = true;

                if(finalAnimIsDone && Input.GetMouseButton(0))
				{
                    SceneManager.LoadScene(0);
                }

                break;
			default:
				break;
		}
    }

    public void UpdateDefinition(string _newDefinition)
	{
        fgDefinition.text = _newDefinition;
        bgDefinition.text = _newDefinition;
    }

    public void UpdateToOwnDefinition()
	{
        UpdateTouch("<color=#FF00D1>Tik</color> om naar het <color=#FF00D1>menu</color> te gaan");
        UpdateDefinition(JSONManager.instance.GetFinalDefinition());
    }

    public void UpdateWord(string _newWord)
    {
        fgWord.text = _newWord;
        bgWord.text = _newWord;
    }

    public void UpdateTouch(string _newMessage)
    {
        fgTouch.text = _newMessage;
        bgTouch.text = _newMessage;
    }

	public void FinalAnimFinished()
	{
		finalAnimIsDone = true;
	}

	public void AnimateCameraIn()
	{
        cZ.StartZoomIn();
    }

    public void AnimateCameraOut()
    {
        cZ.StartZoomOut();
    }

    public void AddToGamestate(int addition)
	{
        int _NewState = (int)gameState + addition; 
        gameState = (GameStates)_NewState;
    }

    public void ResetPlayer()
	{
		if (!hasConformed)
		{
            //reset color? & smooth reset position
            cU.followSpawnPosition = true;
            cU.StartColorReset();
		}
		else
		{
            //cU.StartDisappearAnim();
            //Destroy(cU.gameObject);
            GameObject _newPlayer = Instantiate(playerPrefab, new Vector3(playerStartPos.position.x, playerStartPos.position.y, -1), Quaternion.Euler(Vector3.zero));
            _newPlayer.GetComponent<ColorUpdate>().StartAppearAnim();
            _newPlayer.GetComponent<ColorUpdate>().followSpawnPosition = true;
        }
    }

    public void StartDefinitionInput()
	{
        iFC.ShowDefinitionField();
	}

    string TimerFormat(float timeInSeconds)
    {
        timeInSeconds = Mathf.Clamp(timeInSeconds, 0, Mathf.Infinity);
        int minutes = Mathf.FloorToInt((timeInSeconds / 60));
        int seconds = Mathf.CeilToInt((timeInSeconds % 60));
        //Debug.Log($"minutes = {minutes}, secondes = {seconds}");
        string min = minutes.ToString();
        string sec = seconds.ToString();

        if (minutes < 10)
        {
            min = "0" + min;
        } else if(minutes == 60)
        {
            min = "00";
        }
        if (seconds < 10)
        {
            sec = "0" + sec;
        }

        return $"{min}m<color=#FF00D1>{sec}s</color>";
    }
    string StopwatchFormat(float timeInSeconds)
    {
        timeInSeconds = Mathf.Clamp(timeInSeconds, 0, Mathf.Infinity);
        int minutes = Mathf.FloorToInt((timeInSeconds / 60));
        int seconds = Mathf.FloorToInt((timeInSeconds % 60));
        //int miliseconds = Mathf.FloorToInt((timeInSeconds * 1000) % 1000);
        //Debug.Log($"minutes = {minutes}, secondes = {seconds}");
        string min = minutes.ToString();
        string sec = seconds.ToString();
        //string mil = miliseconds.ToString();

        //if (minutes < 0)
        //{
        //    minutes = 0;
        //}
        //if (seconds < 0)
        //{
        //    seconds = 0;
        //}

        if (minutes < 10)
        {
            min = "0" + min;
        }
        if (seconds < 10)
        {
            sec = "0" + sec;
        }


        return $"{min}m<color=#FF00D1>{sec}s</color>";
    }
}
