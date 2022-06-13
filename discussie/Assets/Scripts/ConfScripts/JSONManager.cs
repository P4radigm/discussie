using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class JSONManager : MonoBehaviour
{
    #region Singleton
    public static JSONManager instance;

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

        InitialiseJSONFiles();
        UpdateReadJSON();
    }
    #endregion

    [System.Serializable]
    public class Definition
	{
        public int index;
        public string definition;
        public string source;
	}

    [System.Serializable]
    public class DefinitionArray
	{
        public Definition[] definitions;
	}

    private DefinitionArray personalDefinitions = new DefinitionArray();
    private DefinitionArray officialDefinitions = new DefinitionArray();

    public List<Definition> personalDefinitionsList = new List<Definition>();
    public List<Definition> officialDefinitionsList = new List<Definition>();

    [SerializeField] private DefinitionArray personalDefinitionsInitial = new DefinitionArray();
    [SerializeField] private DefinitionArray officialDefinitionsInitial = new DefinitionArray();


    private void Start()
	{
        Debug.Log(Application.persistentDataPath + "/personalDefinitions.JSON");
        
    }

    private void InitialiseJSONFiles()
	{
		if (!File.Exists(Application.persistentDataPath + "/personalDefinitions.JSON"))
		{
            string _personalOutput = JsonUtility.ToJson(personalDefinitionsInitial);
            File.WriteAllText(Application.persistentDataPath + "/personalDefinitions.JSON", _personalOutput);
        }
        if (!File.Exists(Application.persistentDataPath + "/officialDefinitions.JSON"))
        {
            string _officialOutput = JsonUtility.ToJson(officialDefinitionsInitial);
            File.WriteAllText(Application.persistentDataPath + "/officialDefinitions.JSON", _officialOutput);
        }
    }

	public void UpdateReadJSON()
	{
		string _personalString = "";
		string _officialString = "";

		_personalString = File.ReadAllText(Application.persistentDataPath + "/personalDefinitions.JSON");
		personalDefinitions = JsonUtility.FromJson<DefinitionArray>(_personalString);
		//Debug.Log(personalDefinitions);
		personalDefinitionsList = new List<Definition>(personalDefinitions.definitions);

		_officialString = File.ReadAllText(Application.persistentDataPath + "/officialDefinitions.JSON");
		officialDefinitions = JsonUtility.FromJson<DefinitionArray>(_officialString);
		officialDefinitionsList = new List<Definition>(officialDefinitions.definitions);
	}

    public void WriteToJSON()
	{
		personalDefinitions.definitions = personalDefinitionsList.ToArray();
		officialDefinitions.definitions = officialDefinitionsList.ToArray();

		string _personalOutput = JsonUtility.ToJson(personalDefinitions);
		string _officialOutput = JsonUtility.ToJson(officialDefinitions);

		File.WriteAllText(Application.persistentDataPath + "/personalDefinitions.JSON", _personalOutput);
		File.WriteAllText(Application.persistentDataPath + "/officialDefinitions.JSON", _officialOutput);
        UpdateReadJSON();

    }

    public string GetNewDefinition(bool isPersonal)
	{
        string _outputDefinition = "";

        if(isPersonal) 
        {
            int _randomIndex = Random.Range(0, personalDefinitionsList.Count);
            _outputDefinition = personalDefinitionsList[_randomIndex].definition + '\n' + personalDefinitionsList[_randomIndex].source;
            personalDefinitionsList.RemoveAt(_randomIndex);
        }
        else 
        {
            int _randomIndex = Random.Range(0, officialDefinitionsList.Count);
            _outputDefinition = officialDefinitionsList[_randomIndex].definition + '\n' + officialDefinitionsList[_randomIndex].source;
            officialDefinitionsList.RemoveAt(_randomIndex);
        }

        return _outputDefinition;
    }

    public string GetFinalDefinition()
	{
        //UpdateReadJSON();
        string _outputDefinition = "";
        int _finalIndex = personalDefinitionsList.Count - 1;
        _outputDefinition = personalDefinitionsList[_finalIndex].definition + '\n' + personalDefinitionsList[_finalIndex].source;
        return _outputDefinition;
    }

 //   private string Format(string inputDefinition, string inputAuthor)
	//{
        
 //   }

    /*
     Definitie 1: def, author
    Definitie 2: def2, author2
    ...
    Defintie n: defn, authorn


     */
}
