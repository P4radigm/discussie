using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    #region Singleton
    public static SequenceManager instance;

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
        splashes,
        gameDef,
        menu,
        notices,
        location,
        definitionA,
        playingA,
        definitionB,
        playingB,
        definitionC,
        playingC,
        definitionInput,
        endScreen
    }

    //Variables
    [Header("To Set")]
    [SerializeField] private BaseSplashes baseSplashes;
    [SerializeField] private BaseGameDef baseGameDef;
    [SerializeField] private BaseMenu baseMenu;
    [SerializeField] private BaseNotices baseNotices;
    [SerializeField] private BaseLocation baseLocation;
    [SerializeField] private BaseDefinition baseDefinitionA;
    [SerializeField] private BaseGameplay baseGameplayA;
    [SerializeField] private BaseDefinition baseDefinitionB;
    [SerializeField] private BaseGameplay baseGameplayB;
    [SerializeField] private BaseDefinition baseDefinitionC;
    [SerializeField] private BaseGameplay baseGameplayC;
    [SerializeField] private BaseInputDefinition baseInput;
    [SerializeField] private BaseEnd baseEnd;
    [Space(20)]
    public bool isPhysicalDeploy;
    [SerializeField] private string physicalDeployLocation;
    [Header("To Get")]
    public GameStates currentGamestate;
    private ContinuityManager cM;
    private DataManager dM;



    void Start()
    {
        if (ContinuityManager.instance != null)
        {
            cM = ContinuityManager.instance;
            if (cM.hasBeenToMenu) { currentGamestate = GameStates.menu; }
        }
        dM = DataManager.instance;
    }

    void Update()
    {
        //If played on computer
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        switch (currentGamestate)
        {
            case GameStates.splashes:
                if (baseSplashes.gameObject.activeInHierarchy == false)
                {
                    baseSplashes.gameObject.SetActive(true);
                    baseSplashes.StartUp();
                }
                break;
            case GameStates.gameDef:
                if (baseGameDef.gameObject.activeInHierarchy == false)
                {
                    baseGameDef.gameObject.SetActive(true);
                    baseGameDef.StartUp();
                    baseSplashes.gameObject.SetActive(false);
                }
                break;
            case GameStates.menu:
                if (baseMenu.gameObject.activeInHierarchy == false)
                {
                    baseMenu.gameObject.SetActive(true);
                    baseMenu.StartUp();
                    baseSplashes.gameObject.SetActive(false);
                    baseGameDef.gameObject.SetActive(false);
                }
                break;
            case GameStates.notices:
                if (baseNotices.gameObject.activeInHierarchy == false)
                {
                    if (cM != null)
                    {
                        cM.hasBeenToMenu = true;
                    }
                    baseNotices.gameObject.SetActive(true);
                    baseNotices.StartUp();
                    baseMenu.gameObject.SetActive(false);
                }
                break;
            case GameStates.location:
                if (baseLocation.gameObject.activeInHierarchy == false && !isPhysicalDeploy)
                {
                    baseLocation.gameObject.SetActive(true);
                    baseLocation.StartUp();
                    baseNotices.gameObject.SetActive(false);
                }
                else if (physicalDeployLocation != "")
                {
                    baseNotices.gameObject.SetActive(false);
                    dM.currentSaveData.location = physicalDeployLocation;
                    DataManager.instance.UpdateSaveFile();
                    AddToGameState();
                }
                else
                {
                    baseLocation.gameObject.SetActive(true);
                    baseLocation.StartUp();
                    baseNotices.gameObject.SetActive(false);
                }
                break;
            case GameStates.definitionA:

                if (baseDefinitionA.gameObject.activeInHierarchy == false)
                {
                    baseDefinitionA.gameObject.SetActive(true);
                    baseDefinitionA.StartUp();
                    baseLocation.gameObject.SetActive(false);
                }
                break;
            case GameStates.playingA:
                if (baseGameplayA.gameObject.activeInHierarchy == false)
                {
                    baseGameplayA.gameObject.SetActive(true);
                    baseGameplayA.StartUp();
                    baseDefinitionA.gameObject.SetActive(false);
                }
                break;
            case GameStates.definitionB:
                if (baseDefinitionB.gameObject.activeInHierarchy == false)
                {
                    baseDefinitionB.gameObject.SetActive(true);
                    baseDefinitionB.StartUp();
                    baseGameplayA.gameObject.SetActive(false);
                }
                break;
            case GameStates.playingB:
                if (baseGameplayB.gameObject.activeInHierarchy == false)
                {
                    baseGameplayB.gameObject.SetActive(true);
                    baseGameplayB.StartUp();
                    baseDefinitionB.gameObject.SetActive(false);
                }
                break;
            case GameStates.definitionC:
                if (baseDefinitionC.gameObject.activeInHierarchy == false)
                {
                    baseDefinitionC.gameObject.SetActive(true);
                    baseDefinitionC.StartUp();
                    baseGameplayB.gameObject.SetActive(false);
                }
                break;
            case GameStates.playingC:
                if (baseGameplayC.gameObject.activeInHierarchy == false)
                {
                    baseGameplayC.gameObject.SetActive(true);
                    baseGameplayC.StartUp();
                    baseDefinitionC.gameObject.SetActive(false);
                }
                break;
            case GameStates.definitionInput:
                if (baseInput.gameObject.activeInHierarchy == false)
                {
                    baseInput.gameObject.SetActive(true);
                    baseInput.StartUp();
                    baseGameplayC.gameObject.SetActive(false);
                }
                break;
            case GameStates.endScreen:
                if (baseEnd.gameObject.activeInHierarchy == false)
                {
                    baseEnd.gameObject.SetActive(true);
                    baseEnd.StartUp();
                    baseInput.gameObject.SetActive(false);
                }
                break;
        }
    }

    public void AddToGameState()
    {
        currentGamestate = (GameStates)((int)currentGamestate + 1);
    }
}
