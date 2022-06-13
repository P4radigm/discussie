using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reformatter : MonoBehaviour
{
    private HighlightColorManager hCM;
    [SerializeField] public float indentPercentage;
    [SerializeField] public float sourceScalePercentage;

    #region Singleton
    public static Reformatter instance;

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

	private void Start()
	{
        hCM = HighlightColorManager.instance;
    }

	public string ReformatDefinition(DataManager.RawDefinitionData data)
	{
		string[] _splitDefinitions = data.definition.Split('\n');
        string _outputString = "";
		for (int i = 0; i < _splitDefinitions.Length; i++)
		{
            _outputString += $"<color=#{hCM.GetHighlightHex()}>{i+1}.</color><indent={indentPercentage}%>{_splitDefinitions[i]}</indent>" + '\n';
        }
        _outputString += $"<size={sourceScalePercentage}%><align=right>Gegeven door: <color=#{hCM.GetHighlightHex()}>{data.source}</size></align></color>";
        _outputString += '\n';
        if (data.location != "Official") { _outputString += $"<size={sourceScalePercentage}%><align=right>@ <color=#{hCM.GetHighlightHex()}>{data.location}</size></align></color>"; }
        

        return _outputString;
	}

    public string ReformatEndDefinition(string definition, string source)
    {
        string[] _splitDefinitions = definition.Split('\n');
        string _outputString = "";
        for (int i = 0; i < _splitDefinitions.Length; i++)
        {
            _outputString += $"<color=#{hCM.GetHighlightHex()}>{i + 1}.</color><indent={indentPercentage}%>{_splitDefinitions[i]}</indent>" + '\n';
        }
        _outputString += $"<size={sourceScalePercentage}%><align=right>Gegeven door: <color=#{hCM.GetHighlightHex()}>{source}</size></align></color>";
        _outputString += '\n';
        _outputString += $"<size={sourceScalePercentage}%><align=right>@ <color=#{hCM.GetHighlightHex()}>{DataManager.instance.currentSaveData.location}</size></align></color>";

        return _outputString;
    }

    public string ReformatInputDefinitionField(string definitionInput, string[] defHighs)
	{
        string[] _splitDefinitions = definitionInput.Split('\n');
        string _outputString = "";
        for (int i = 0; i < _splitDefinitions.Length; i++)
        {
            _outputString += $"<color=#{defHighs[i]}>{i + 1}.</color><indent={indentPercentage}%>{_splitDefinitions[i]}</indent>";
            if(i != _splitDefinitions.Length - 1) { _outputString += '\n'; }
        }

        return _outputString;
    }

    public string ReformatInputSourceField(string sourceInput, string sourceHigh)
	{
        string _outputString = $"<indent={indentPercentage}%>Ik vul graag <color=#{sourceHigh}>{sourceInput}</color> in als naam.</indent>";
        return _outputString;
    }
}
