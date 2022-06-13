using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFieldController : MonoBehaviour
{
    [SerializeField] private TMP_InputField definitionInputField;
    [SerializeField] private TextMeshProUGUI definitionDisplay;
    [SerializeField] private string definitionPlaceholder;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private string namePlaceholder;
    [SerializeField] private string highlightColHex;
    [SerializeField] private float indentPercentage;
    [SerializeField] private Button sendDefButton;
    [SerializeField] private Button sendNameButton;
    [SerializeField] private Image button;
    private string newDefinition = "";
    private string newName = "";

    private bool atDefinition = true;
    //[SerializeField] private string enterReplacement;

    // Start is called before the first frame update
    void Start()
    {
        //definitionInputField = GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.instance.gameState == GameManager.GameStates.end)
		{            
			if (atDefinition && newDefinition != "")
            {
                button.enabled = true;
                sendDefButton.gameObject.SetActive(true); 
                sendNameButton.gameObject.SetActive(false); 
            }
			else if (newName != "")
            {
                button.enabled = true;
                sendDefButton.gameObject.SetActive(false); 
                sendNameButton.gameObject.SetActive(true); 
            }
		}
		else
		{
            sendDefButton.gameObject.SetActive(false);
            sendNameButton.gameObject.SetActive(false);
            button.enabled = false;
        }
    }

    public void ShowDefinitionField()
	{
        definitionInputField.gameObject.SetActive(true);
        definitionInputField.enabled = true;
        definitionInputField.GetComponent<Image>().raycastTarget = true;
        definitionInputField.Select();
        atDefinition = true;
    }

    public void OnNewInput(string _inputString)
	{
        if(_inputString == "") { definitionDisplay.text = definitionPlaceholder; newDefinition = ""; return; }
        //Debug.Log(_inputString);
        //Update text with reformatted message.
        definitionDisplay.text = ReformatInput(_inputString);
        newDefinition = ReformatInput(_inputString);
    }

    public void DefinitionDone()
	{
        if(newDefinition == "") 
        {
            //Hier moet mss nog wel een visible error voor de speler komen
            return; 
        }
        definitionDisplay.text = "";
        nameInputField.gameObject.SetActive(true);
        nameInputField.enabled = true;
        nameInputField.GetComponent<Image>().raycastTarget = true;
        definitionInputField.enabled = false;
        definitionInputField.GetComponent<Image>().raycastTarget = false;
        definitionInputField.gameObject.SetActive(false);
        GameManager.instance.UpdateDefinition("<indent=6%><i>Schrijf hier je naam...</i></indent>");
        nameInputField.Select();
        atDefinition = false;
    }

    public void OnNewNameInput(string _inputString)
	{
        if (_inputString == "") { definitionDisplay.text = namePlaceholder; newName = ""; return; }
        definitionDisplay.text = ReformatNameInput(_inputString);
        newName = $"<size=50%><align=right>Gegeven door: <color=#{highlightColHex}>{_inputString}</size></align></color>";
    }

    public void NameDone()
	{
        if (newName == "")
        {
            //Hier moet mss nog wel een visible error voor de speler komen
            return;
        }
        nameInputField.enabled = false;
        nameInputField.GetComponent<Image>().raycastTarget = false;
        nameInputField.gameObject.SetActive(false);
        AddNewDef();
        //trigger final animation
        GameManager.instance.animator.SetTrigger("Final");
        GameManager.instance.gameState = GameManager.GameStates.final;
    }

    public void AddNewDef()
	{
        JSONManager jM = JSONManager.instance;
        jM.UpdateReadJSON();
        JSONManager.Definition _newDefinition = new JSONManager.Definition();
        _newDefinition.index = jM.personalDefinitionsList[jM.personalDefinitionsList.Count - 1].index + 1;
        _newDefinition.definition = newDefinition;
        _newDefinition.source = newName;
        jM.personalDefinitionsList.Add(_newDefinition);
        jM.WriteToJSON();
    }

    private string ReformatInput(string _inputString)
	{
        string _outputstring = "";
        int _enterAmnt = 0;

        string[] _string = _inputString.Split('\n');

		for (int i = 0; i < _string.Length; i++)
		{
            _enterAmnt++;

            _outputstring += $"<color=#{highlightColHex}>{_enterAmnt}.<indent={indentPercentage}%></color>" + _string[i];
            if(i != _string.Length - 1) { _outputstring += "</indent>\n"; }
			else { _outputstring += "</indent>"; }	
        }
        return _outputstring;
	}

    private string ReformatNameInput(string _inputString)
	{
        string _outputstring = "";
        _outputstring = $"<indent={indentPercentage}%><color=#{highlightColHex}>" + _inputString + "</color> <i>vul ik graag in als naam</i></indent>";
        return _outputstring;
    }

    public void SendDebug(string debugText)
	{
        Debug.Log(debugText);
	}
}
