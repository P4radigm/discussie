using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public enum CurrentGame
    {
        uitzoeken,
        conformeren,
        discussie,
        verrijken
    }

    public enum NetworkingStatus
	{
        negative,
        trying,
        positive
	}

    [SerializeField] private CurrentGame game;
    //[SerializeField] private ReformatDefinitionString reformat;

    [System.Serializable]
    public class RawDefinitionData
	{
        public int index;
        [TextArea]
        public string definition;
        public string source;
        //public float[] ratingsX;
        //public float[] ratingsY;
        public float averageRatingX;
        public float averageRatingY;
        public string location;
        public bool privacyCheck;
        //public iets player result (Dit moet wss opgeslagen worden als string) -> $"{indexBall0}{xCoord0}{yCoord0}...{indexBalln}{xCoordn}{yCoordn}"
        public string result;
    }

    [System.Serializable]
    public class SaveData
	{
        public bool privacyCheck;

        public string location;

        public int indexRatingA;
        public Vector2 givenRatingA;
        public bool defAIsUploaded;

        public int indexRatingB;
        public Vector2 givenRatingB;
        public bool defBIsUploaded;

        public int indexRatingC;
        public Vector2 givenRatingC;
        public bool defCIsUploaded;

        public string gameResult;
        public string ownDefinition;
        public Vector2 ownRating;
        public string source;
        public bool ownDefIsUploaded;

        public bool allIsUploaded;
    }

    [SerializeField] private SaveData emptySaveData;
    public SaveData currentSaveData;
    private SaveData oldSaveData;

    [SerializeField] private int standardTimeoutValue;
    //[SerializeField] private float retryNetworkingTimer;

    public RawDefinitionData definitionA;
    public RawDefinitionData definitionB;
    public RawDefinitionData definitionC;
    private int randomOfflineOfficialDefA;
    private int randomOfflinePersonalDefB;
    private int randomOfflinePersonalDefC;

    [SerializeField] private BaseDefinition baseDefA;
    [SerializeField] private BaseDefinition baseDefB;
    [SerializeField] private BaseDefinition baseDefC;

    [SerializeField] private RawDefinitionData[] offlineDefinitionBackupsOfficial;
    [SerializeField] private RawDefinitionData[] offlineDefinitionBackupsPersonal;

    [Header("Network flags")]
    //old save data flags
    public NetworkingStatus uploadedOldRatingA = NetworkingStatus.negative;
    public NetworkingStatus uploadedOldRatingB = NetworkingStatus.negative;
    public NetworkingStatus uploadedOldRatingC = NetworkingStatus.negative;
    public NetworkingStatus uploadedOldDefinition = NetworkingStatus.negative;
    //get definitions flags
    public NetworkingStatus downloadedDefinitionA = NetworkingStatus.negative;
    public NetworkingStatus downloadedDefinitionB = NetworkingStatus.negative;
    public NetworkingStatus downloadedDefinitionC = NetworkingStatus.negative;
    //upload current gameplay data flags
    public bool ownDefinitionReady = false;
    public NetworkingStatus uploadedOwnDefinition = NetworkingStatus.negative;
    public bool ownRatingAReady = false;
    public NetworkingStatus uploadedOwnRatingA = NetworkingStatus.negative;
    public bool ownRatingBReady = false;
    public NetworkingStatus uploadedOwnRatingB = NetworkingStatus.negative;
    public bool ownRatingCReady = false;
    public NetworkingStatus uploadedOwnRatingC = NetworkingStatus.negative;

    private bool lastFrameServerConnection;
    private int serverConnectAmount = 0;

    //private Vector2 blacklist = new Vector2(-99, -99);
    private List<int> idBlacklist = new List<int>();

    private string saveFilePath;

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

        //make sure idBlacklist is empty
        idBlacklist.Clear();
    }

    private void Start()
    {
        saveFilePath = Application.persistentDataPath + "/saveFile.JSON";
        Debug.Log($"DM: saveFilePath = {saveFilePath}");
        // --- Check for unuploaded SaveData JSON -> try to upload ---
        CheckUnunploadedSaveData();

        //Offline definition indeces
        randomOfflineOfficialDefA = Random.Range(0, offlineDefinitionBackupsOfficial.Length);
        //Make sure we can't select the same personal definition twice
        List<int> personalIndeces = new();
        for (int i = 0; i < offlineDefinitionBackupsPersonal.Length; i++)
        {
            personalIndeces.Add(i);
        }
        randomOfflinePersonalDefB = Random.Range(0, personalIndeces.Count);
        personalIndeces.RemoveAt(randomOfflinePersonalDefB);
        randomOfflinePersonalDefC = personalIndeces[Random.Range(0, personalIndeces.Count)];
    }

	private void FixedUpdate()
	{
  //      if(PlatformManager.instance.hasServerConnection != lastFrameServerConnection && PlatformManager.instance.hasServerConnection)
		//{
  //          serverConnectAmount++;
  //          OnServerReconnect(); 
  //      }
        TryNetworking();
        UpdateDefinitionObjects();
        //lastFrameServerConnection = PlatformManager.instance.hasServerConnection;
    }

	private void OnServerReconnect()
	{
        TryUploadOldData();
        GetServerDefinitions();
        TryUploadOwnRatingA();
        TryUploadOwnRatingB();
        TryUploadOwnRatingC();
        TryUploadOwnDefinition();
    }

    public void TryNetworking()
	{
        if (!PlatformManager.instance.hasServerConnection)
		{
            return;
		}
        TryUploadOldData();
        GetServerDefinitions();
        TryUploadOwnRatingA();
        TryUploadOwnRatingB();
        TryUploadOwnRatingC();
        TryUploadOwnDefinition();
    }

    private void CheckUnunploadedSaveData()
	{
        //Check if file exists
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("DM: No Save file exists yet");
            //If file does not yet exist, cerate empty save data file
            string _emptySaveDataOutput = JsonUtility.ToJson(emptySaveData);
            currentSaveData = emptySaveData;
            File.WriteAllText(saveFilePath, _emptySaveDataOutput);
            //Set all oldData flags to true
            uploadedOldRatingA = NetworkingStatus.positive;
            uploadedOldRatingB = NetworkingStatus.positive;
            uploadedOldRatingC = NetworkingStatus.positive;
            uploadedOldDefinition = NetworkingStatus.positive;
            Debug.Log($"DM: Created empty savefile at {saveFilePath}");
        }
        else
        {
            Debug.Log("DM: Save file has been locally found");
            //Read file
            string _existingSaveDataOutput = File.ReadAllText(saveFilePath);
            SaveData existingSaveData = JsonUtility.FromJson<SaveData>(_existingSaveDataOutput);
            oldSaveData = JsonUtility.FromJson<SaveData>(_existingSaveDataOutput);
            //Check if file is already uploaded
            if (existingSaveData.allIsUploaded)
            {
                Debug.Log("DM: Local save file was uploaded already, deleting old save file");
                //File is already uploaded, we can overwrite the file
                File.Delete(saveFilePath);
                string _emptySaveDataOutput = JsonUtility.ToJson(emptySaveData);
                currentSaveData = emptySaveData;
                File.WriteAllText(saveFilePath, _emptySaveDataOutput);
                //Set all oldData flags to true
                uploadedOldRatingA = NetworkingStatus.positive;
                uploadedOldRatingB = NetworkingStatus.positive;
                uploadedOldRatingC = NetworkingStatus.positive;
                uploadedOldDefinition = NetworkingStatus.positive;
                Debug.Log($"DM: Created empty savefile at {saveFilePath}");
            }
            else if (existingSaveData.privacyCheck)
            {
                Debug.Log("DM: Some unuploaded local data found, trying to upload...");
                if (existingSaveData.indexRatingA != -1 && existingSaveData.givenRatingA != Vector2.one * -99 && !existingSaveData.defAIsUploaded)
                {
                    Debug.Log("Flagging Rating A from existing save file for upload");
                }
				else
				{
                    uploadedOldRatingA = NetworkingStatus.positive;
                }

                if (existingSaveData.indexRatingB != -1 && existingSaveData.givenRatingB != Vector2.one * -99 && !existingSaveData.defBIsUploaded)
                {
                    Debug.Log("Flagging Rating B from existing save file for upload");
                }
                else
                {
                    uploadedOldRatingB = NetworkingStatus.positive;
                }

                if (existingSaveData.indexRatingC != -1 && existingSaveData.givenRatingC != Vector2.one * -99 && !existingSaveData.defCIsUploaded)
                {
                    Debug.Log("Flagging Rating C from existing save file for upload");
                }
                else
                {
                    uploadedOldRatingC = NetworkingStatus.positive;
                }

                if (existingSaveData.gameResult != "0" && existingSaveData.ownDefinition != "0" && existingSaveData.source != "0" && existingSaveData.ownRating != Vector2.one * -99 && !existingSaveData.ownDefIsUploaded)
                {
                    Debug.Log("Flagging Definition from existing save file for upload");
                }
                else
                {
                    uploadedOldDefinition = NetworkingStatus.positive;
                }

                Debug.Log("Deleting old save file, since all is now uploaded");
                //uploaded all data, we can overwrite the file
                File.Delete(saveFilePath);
                string _emptySaveDataOutput = JsonUtility.ToJson(emptySaveData);
                currentSaveData = emptySaveData;
                File.WriteAllText(saveFilePath, _emptySaveDataOutput);
                Debug.Log($"DM: Created empty savefile at {saveFilePath}");
            }
            else
            {
                Debug.Log($"DM: No permission to upload anything from local file, deleting old save file");
                //No permission to upload data, we can overwrite the file
                File.Delete(saveFilePath);
                string _emptySaveDataOutput = JsonUtility.ToJson(emptySaveData);
                currentSaveData = emptySaveData;
                File.WriteAllText(saveFilePath, _emptySaveDataOutput);
                //Set all oldData flags to true
                uploadedOldRatingA = NetworkingStatus.positive;
                uploadedOldRatingB = NetworkingStatus.positive;
                uploadedOldRatingC = NetworkingStatus.positive;
                uploadedOldDefinition = NetworkingStatus.positive;
                Debug.Log($"DM: Created empty savefile at {saveFilePath}");
            }

        }
    }

    public void CheckToUseOfflineDefinitionA()
    {
        //Debug.Log($"DM: Platform Manager has found no server Connection: {PlatformManager.instance.hasServerConnection}");
        //Get definitions from preshipped list
        if(downloadedDefinitionA == NetworkingStatus.positive)
		{
            return;
		}

        definitionA = offlineDefinitionBackupsOfficial[randomOfflineOfficialDefA];

        baseDefA.definition = definitionA.definition;
        baseDefA.givenName = definitionA.source;
        baseDefA.results = definitionA.result;
        baseDefA.definitionText.text = Reformatter.instance.ReformatDefinition(definitionA);
        Debug.Log($"DM: Offline definition A has been communicated to Base Def A");
    }

    public void CheckToUseOfflineDefinitionB()
    {
        //Debug.Log($"DM: Platform Manager has found no server Connection: {PlatformManager.instance.hasServerConnection}");
        //Get definitions from preshipped list
        if (downloadedDefinitionB == NetworkingStatus.positive)
        {
            return;
        }

        definitionB = offlineDefinitionBackupsPersonal[randomOfflinePersonalDefB];

        baseDefB.definition = definitionB.definition;
        baseDefB.givenName = definitionB.source;
        baseDefB.results = definitionB.result;
        baseDefB.definitionText.text = Reformatter.instance.ReformatDefinition(definitionB);
        Debug.Log($"DM: Offline definition B has been communicated to Base Def B");
    }

    public void CheckToUseOfflineDefinitionC()
    {
        //Debug.Log($"DM: Platform Manager has found no server Connection: {PlatformManager.instance.hasServerConnection}");
        //Get definitions from preshipped list
        if (downloadedDefinitionC == NetworkingStatus.positive)
        {
            return;
        }

        definitionC = offlineDefinitionBackupsPersonal[randomOfflinePersonalDefC];

        baseDefC.definition = definitionC.definition;
        baseDefC.givenName = definitionC.source;
        baseDefC.results = definitionC.result;
        baseDefC.definitionText.text = Reformatter.instance.ReformatDefinition(definitionC);
        Debug.Log($"DM: Offline definition C has been communicated to Base Def C");
    }

    private void TryUploadOldData()
	{
        if(uploadedOldRatingA == NetworkingStatus.positive && uploadedOldRatingB == NetworkingStatus.positive && uploadedOldRatingC == NetworkingStatus.positive && uploadedOldDefinition == NetworkingStatus.positive)
		{
            return;
		}

		if (uploadedOldRatingA == NetworkingStatus.negative)
		{
            AddRating(oldSaveData.indexRatingA, oldSaveData.givenRatingA, 0, oldSaveData.privacyCheck ? 1 : 0, delegate(NetworkingStatus wasSuccesful) { uploadedOldRatingA = wasSuccesful; });
            uploadedOldRatingA = NetworkingStatus.trying;
        }

		if (uploadedOldRatingB == NetworkingStatus.negative)
		{
            AddRating(oldSaveData.indexRatingB, oldSaveData.givenRatingB, 1, oldSaveData.privacyCheck ? 1 : 0, delegate (NetworkingStatus wasSuccesful) { uploadedOldRatingB = wasSuccesful; });
            uploadedOldRatingB = NetworkingStatus.trying;
        }

        if (uploadedOldRatingC == NetworkingStatus.negative)
        {
            AddRating(oldSaveData.indexRatingC, oldSaveData.givenRatingC, 1, oldSaveData.privacyCheck ? 1 : 0, delegate (NetworkingStatus wasSuccesful) { uploadedOldRatingC = wasSuccesful; });
            uploadedOldRatingB = NetworkingStatus.trying;
        }

        if(uploadedOldDefinition == NetworkingStatus.negative)
		{
            AddDefinition(game.ToString(), 1, oldSaveData.ownDefinition, oldSaveData.source, oldSaveData.ownRating, oldSaveData.location, oldSaveData.privacyCheck ? 1 : 0, oldSaveData.gameResult, delegate (NetworkingStatus wasSuccesful) { uploadedOldRatingC = wasSuccesful; });
            uploadedOldDefinition = NetworkingStatus.trying;
        }
    }

    private void GetServerDefinitions()
	{
        
        if(downloadedDefinitionA == NetworkingStatus.positive && downloadedDefinitionB == NetworkingStatus.positive && downloadedDefinitionC == NetworkingStatus.positive)
		{
            return;
		}

        //Get official definition
        if (downloadedDefinitionA == NetworkingStatus.negative)
		{
            GetDefinition(delegate (RawDefinitionData def) { if (def != null) { definitionA = def; } }, 0, delegate (NetworkingStatus wasSuccesful) { downloadedDefinitionA = wasSuccesful; });
            downloadedDefinitionA = NetworkingStatus.trying;
        }

        //Get Personal definition 1
        if (downloadedDefinitionB == NetworkingStatus.negative)
        {
            GetDefinition(delegate (RawDefinitionData def) { if (def != null) { definitionB = def; } }, 1, delegate (NetworkingStatus wasSuccesful) { downloadedDefinitionB = wasSuccesful; });
            downloadedDefinitionB = NetworkingStatus.trying;
        }

        //Get Personal definition 2
        if (downloadedDefinitionC == NetworkingStatus.negative && downloadedDefinitionB == NetworkingStatus.positive)
        {
            GetDefinition(delegate (RawDefinitionData def) { if (def != null) { definitionC = def; } }, 1, delegate (NetworkingStatus wasSuccesful) { downloadedDefinitionC = wasSuccesful; });
            downloadedDefinitionC = NetworkingStatus.trying;
        }
    }

    private void UpdateDefinitionObjects()
	{
        if(baseDefA.results == definitionA.result && baseDefB.results == definitionB.result && baseDefC.results == definitionC.result)
		{
            return;
		}

        baseDefA.definition = definitionA.definition;
        baseDefA.givenName = definitionA.source;
        baseDefA.results = definitionA.result;
        baseDefA.definitionText.text = Reformatter.instance.ReformatDefinition(definitionA);

        baseDefB.definition = definitionB.definition;
        baseDefB.givenName = definitionB.source;
        baseDefB.results = definitionB.result;
        baseDefB.definitionText.text = Reformatter.instance.ReformatDefinition(definitionB);

        baseDefC.definition = definitionC.definition;
        baseDefC.givenName = definitionC.source;
        baseDefC.results = definitionC.result;
        baseDefC.definitionText.text = Reformatter.instance.ReformatDefinition(definitionC);
    }

	private void TryUploadOwnRatingA()
	{
        if(!ownRatingAReady || uploadedOwnRatingA == NetworkingStatus.positive)
		{
            return;
		}

        if(uploadedOwnRatingA == NetworkingStatus.negative)
		{
            AddRating(currentSaveData.indexRatingA, currentSaveData.givenRatingA, 0, currentSaveData.privacyCheck ? 1 : 0, delegate (NetworkingStatus wasSuccesful) { uploadedOwnRatingA = wasSuccesful; if (wasSuccesful == NetworkingStatus.positive) { currentSaveData.defAIsUploaded = true; UpdateSaveFile(); } });
            uploadedOwnRatingA = NetworkingStatus.trying;
        }
	}

    private void TryUploadOwnRatingB()
    {
        if (!ownRatingBReady || uploadedOwnRatingB == NetworkingStatus.positive)
        {
            return;
        }

        if (uploadedOwnRatingB == NetworkingStatus.negative)
        {
            AddRating(currentSaveData.indexRatingB, currentSaveData.givenRatingB, 1, currentSaveData.privacyCheck ? 1 : 0, delegate (NetworkingStatus wasSuccesful) { uploadedOwnRatingB = wasSuccesful; if (wasSuccesful == NetworkingStatus.positive) { currentSaveData.defBIsUploaded = true; UpdateSaveFile(); } });
            uploadedOwnRatingB = NetworkingStatus.trying;
        }
    }

    private void TryUploadOwnRatingC()
    {
        if (!ownRatingCReady || uploadedOwnRatingC == NetworkingStatus.positive)
        {
            return;
        }

        if (uploadedOwnRatingC == NetworkingStatus.negative)
        {
            AddRating(currentSaveData.indexRatingC, currentSaveData.givenRatingC, 1, currentSaveData.privacyCheck ? 1 : 0, delegate (NetworkingStatus wasSuccesful) { uploadedOwnRatingC = wasSuccesful; if (wasSuccesful == NetworkingStatus.positive) { currentSaveData.defCIsUploaded = true; UpdateSaveFile(); } });
            uploadedOwnRatingC = NetworkingStatus.trying;
        }
    }

    private void TryUploadOwnDefinition()
	{
        if(!ownDefinitionReady || uploadedOwnDefinition == NetworkingStatus.positive)
		{
            return;
		}

        if(uploadedOwnDefinition == NetworkingStatus.negative)
		{
            AddDefinition(game.ToString(), 1, currentSaveData.ownDefinition, currentSaveData.source, currentSaveData.ownRating, currentSaveData.location, currentSaveData.privacyCheck ? 1 : 0, currentSaveData.gameResult, delegate (NetworkingStatus wasSuccesful) 
            { 
                uploadedOwnDefinition = wasSuccesful; 
                if (wasSuccesful == NetworkingStatus.positive) 
                {
                    Debug.LogWarning("DM: Uploaded own definition");
                    currentSaveData.ownDefIsUploaded = true; 
                    if(currentSaveData.defAIsUploaded && currentSaveData.defBIsUploaded && currentSaveData.defCIsUploaded)
					{
                        currentSaveData.allIsUploaded = true;
					}
                    UpdateSaveFile();
                } 
            });
            uploadedOwnDefinition = NetworkingStatus.trying;
        }
	}

	public void UpdateSaveFile()
	{
        Debug.Log($"DM: Updating the Local Save file to match CurrentSaveData");
        string _emptySaveDataOutput = JsonUtility.ToJson(currentSaveData);
        File.WriteAllText(saveFilePath, _emptySaveDataOutput);
    }

    private RawDefinitionData GetRawDataFromServerString(string rawServerString, int isPersonal)
    {
        Debug.Log($"DM: Converting raw server data from: {rawServerString}");
        RawDefinitionData _outputData = new RawDefinitionData();
        string[] splitData = rawServerString.Split('\t');
        _outputData.index = int.Parse(splitData[1]);
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[1]}");
        _outputData.definition = splitData[2];
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[2]}");
        _outputData.source = splitData[3];
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[3]}");
        _outputData.averageRatingX = float.Parse(splitData[4]);
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[4]}");
        _outputData.averageRatingY = float.Parse(splitData[5]);
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[5]}");
        _outputData.location = splitData[6];
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[6]}");
        _outputData.privacyCheck = int.Parse(splitData[7]) == 0 ? false : true;
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[7]}");
        _outputData.result = splitData[8];
        Debug.Log($"DM: Converting raw server data from source: {splitData[3]}, index = {splitData[8]}");

        if (isPersonal == 1) 
        {
            Debug.Log($"DM: Retrieved data from a serverstring, adding index: {_outputData.index} to the blacklist");
            idBlacklist.Add(_outputData.index); 
        }
        return _outputData;
    }

	#region Web Functions

    private async void AddRating(int definitionIndex, Vector2 givenRating, int isPersonal, int privacy, System.Action<NetworkingStatus> isSuccesful)
	{
        WWWForm form = new WWWForm();
        form.AddField("game", game.ToString());
        form.AddField("isPersonal", isPersonal);
        form.AddField("index", definitionIndex);
        form.AddField("ratingX", givenRating.x.ToString("F2"));
        form.AddField("ratingY", givenRating.y.ToString("F2"));
        form.AddField("privacy", privacy);
        Debug.Log($"DM: Sending AddRating Webrequest to server from AddRating routine, Form: {form.data}");
        UnityWebRequest req = UnityWebRequest.Post("https://studenthome.hku.nl/~leon.vanoldenborgh/PolarisatieFiles/AddRating.php", form);
        req.timeout = standardTimeoutValue;
        var operation = req.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        Debug.Log($"DM: AddRating Webrequest has finished, Form: {form.data}");
        if(req.result != UnityWebRequest.Result.Success && req.result != UnityWebRequest.Result.ProtocolError) 
        {
            Debug.Log($"DM: Add Rating error with request, result = {req.result}, returnText = {req.downloadHandler.text}");
            isSuccesful(NetworkingStatus.negative);
            req.Dispose();
            return;
        }

        if (req.downloadHandler.text == "0")
		{
            Debug.Log($"DM: Rating {givenRating} for index {definitionIndex} succesfully added");
            isSuccesful(NetworkingStatus.positive);
            req.Dispose();
        }
		else
		{
            Debug.Log($"DM: Rating Addition failed, error code: {req.downloadHandler.text}");
            isSuccesful(NetworkingStatus.negative);
            req.Dispose();
        } 
    }

    private async void GetDefinition(System.Action<RawDefinitionData> callBack, int isPersonal, System.Action<NetworkingStatus> isSuccesful)
    {
        WWWForm form = new WWWForm();
        form.AddField("game", game.ToString());
        form.AddField("isPersonal", isPersonal);

        if (idBlacklist.Count > 0 && isPersonal == 1) 
        { 
            form.AddField("blacklistA", idBlacklist[0]); 
        }
        else
        { 
            form.AddField("blacklistA", -1); 
        }

        if (idBlacklist.Count > 1 && isPersonal == 1) 
        { 
            form.AddField("blacklistB", idBlacklist[1]); 
        }
        else
        { 
            form.AddField("blacklistB", -1); 
        }
        Debug.Log($"DM: Sending GetDefinition Webrequest to server from GetDefinition routine, Form: {form.data}");


        UnityWebRequest req = UnityWebRequest.Post("https://studenthome.hku.nl/~leon.vanoldenborgh/PolarisatieFiles/GetDefinition.php", form);
        req.timeout = standardTimeoutValue;
        var operation = req.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        Debug.Log($"DM: GetDefinition Webrequest has finished, Form: {form.data}");

        if (req.result != UnityWebRequest.Result.Success && req.result != UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"DM: Get Definition error with request, result = {req.result}, returnText = {req.downloadHandler.text}");
            isSuccesful(NetworkingStatus.negative);
            req.Dispose();
            return;
        }

        if (req.downloadHandler.text[0] == '0')
        {
            Debug.Log($"DM: Starting callback function to retrieve usable data from serverstring: {req.downloadHandler.text}");
            callBack(GetRawDataFromServerString(req.downloadHandler.text, isPersonal));
            isSuccesful(NetworkingStatus.positive);
            req.Dispose();
        }
		else
		{
            Debug.Log($"DM: Failed to get a defintion error: {req.downloadHandler.text}");
            isSuccesful(NetworkingStatus.negative);
            req.Dispose();
        }
    }

    private async void AddDefinition(string _game, int _isPersonal, string _definition, string _source, Vector2 _initialRating, string _location, int _privacy, string _gameResult, System.Action<NetworkingStatus> isSuccesful)
    {
        //Debug.Log($"definition = {_definition}");
        WWWForm form = new WWWForm();
        form.AddField("game", _game);
        form.AddField("isPersonal", _isPersonal);
        form.AddField("definition", _definition);
        form.AddField("source", _source);
        form.AddField("initialRatingX", _initialRating.x.ToString());
        form.AddField("initialRatingY", _initialRating.y.ToString());
        form.AddField("location", _location);
        form.AddField("privacy", _privacy);
        form.AddField("gameResult", _gameResult);
        //Debug.Log($"GameResult = {_gameResult}");
        //Debug.Log($"Form Data = {form.data}");
        Debug.Log($"DM: Sending AddDefinition Webrequest to server from AddDefinition routine, Form: {form.data}");
        UnityWebRequest req = UnityWebRequest.Post("https://studenthome.hku.nl/~leon.vanoldenborgh/PolarisatieFiles/AddDefinition.php", form);
        req.timeout = standardTimeoutValue;
        var operation = req.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        Debug.Log($"DM: AddDefinition Webrequest has finished, Form: {form.data}");
        if (req.result != UnityWebRequest.Result.Success && req.result != UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"DM: Add Definition error with request, result = {req.result}, returnText = {req.downloadHandler.text}");
            isSuccesful(NetworkingStatus.negative);
            req.Dispose();
            return;
        }

        if (req.downloadHandler.text[0] == '0')
        {
            Debug.Log("DM: Added definition succesfully");
            isSuccesful(NetworkingStatus.positive);
            req.Dispose();
        }
        else
        {
            Debug.Log($"DM: Failed to add definition error: {req.downloadHandler.text}");
            isSuccesful(NetworkingStatus.negative);
            req.Dispose();
        }
    }

    #endregion
}
