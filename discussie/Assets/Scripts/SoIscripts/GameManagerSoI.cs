using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManagerSoI: MonoBehaviour
{
    #region Singleton
    public static GameManagerSoI instance;

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
        end
    }

    private Coroutine unitAppearSequence;
    [SerializeField] private TextMeshProUGUI toPlayText;
    public GameStates gameState;
    [SerializeField] private TextMeshProUGUI groupResultText;
    [SerializeField] private TextMeshProUGUI groupText;
    private bool endTapEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameStates.menu;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameState == GameStates.menu)
		{
            if(Input.touchCount > 0)
			{
                GetComponent<Animator>().SetTrigger("Opaque");
            }
			else
			{
                GetComponent<Animator>().SetTrigger("ReturnOpaque");
            }

            if(Input.touchCount == 0)
			{
                toPlayText.text = $"PRESS  &  HOLD  WITH  ONE  FINGER  TO  PLAY";
            }
            else if (Input.touchCount == 1)
			{
                toPlayText.text = $"PRESS  &  HOLD  WITH  TWO  FINGERS  TO  PLAY";
            }
            else if (Input.touchCount == 2)
			{
                toPlayText.text = $"PRESS  &  HOLD  WITH  THREE  FINGERS  TO  PLAY";
            }
            else if (Input.touchCount == 3)
			{
                toPlayText.text = $"PRESS  &  HOLD  WITH  FOUR  FINGERS  TO  PLAY";
            }
            else if(Input.touchCount >= 4 && unitAppearSequence == null)
			{
                //unitAppearSequence = StartCoroutine(UnitManager.instance.IntroSequence());
                GetComponent<Animator>().SetTrigger("Intro");
            }
		}
        else if(gameState == GameStates.end)
		{
			if (endTapEnabled && Input.touchCount > 0)
			{
                SceneManager.LoadScene(0);
			}
		}
    }

    public void StartAppearCoroutine()
	{
        unitAppearSequence = StartCoroutine(UnitManager.instance.IntroSequence());
    }

    public void StartEnd()
	{
        UnitManager.instance.CountGroups();
        groupResultText.text = $"{UnitManager.instance.amountOfGroups}";
		if (UnitManager.instance.amountOfGroups == 1)
		{
            groupText.text = "GROUP";
        }
		else
		{
            groupText.text = "GROUPS";
        }
        InputManager.instance.enabled = false;
        UnitManager.instance.enabled = false;
        GetComponent<Animator>().SetTrigger("End");
    }

    public void ResultsDone()
	{
        endTapEnabled = true;
    }
}
