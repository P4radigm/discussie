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
    [Header("To Get")]
    public GameStates currentGamestate;


    void Start()
    {
        
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
            case GameStates.notices:
                if (baseNotices.gameObject.activeInHierarchy == false)
                {
                    baseNotices.gameObject.SetActive(true);
                    baseNotices.StartUp();
                }
                break;
            case GameStates.location:
                if (baseLocation.gameObject.activeInHierarchy == false)
                {
                    baseLocation.gameObject.SetActive(true);
                    baseLocation.StartUp();
                }
                break;
            case GameStates.definitionA:
                
                if(baseDefinitionA.gameObject.activeInHierarchy == false)
				{
                    baseDefinitionA.gameObject.SetActive(true);
                    baseDefinitionA.StartUp();
				}
                break;
            case GameStates.playingA:
                if (baseGameplayA.gameObject.activeInHierarchy == false)
                {
                    baseGameplayA.gameObject.SetActive(true);
                    baseGameplayA.StartUp();
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
                }
                break;
        }
    }

    public void AddToGameState()
	{
        currentGamestate = (GameStates)((int)currentGamestate + 1);
    }
}
