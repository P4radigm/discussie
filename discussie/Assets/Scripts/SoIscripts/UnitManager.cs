using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    #region Singleton
    public static UnitManager instance;

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

    [System.Serializable] 
    public class Categories
	{
        public int groupID;
        [NonReorderable] public SwitchMoment[] switches;
        public Color originalColor;
	}

    [System.Serializable]
    public class ColorCorrections
	{
        public int groupID;
        public int colorPlace;
        public Color newColor;
	}

    [System.Serializable]
    public class SwitchMoment
	{
        public int switchMoment;
        public Color switchColor;
    }

    [System.Serializable]
    public class CounterColours
	{
        public Color GroupOneCol;
        public Color GroupTwoCol;
	}

    [Header("Intro/Outro")]
    [SerializeField] private float maxUnitAppearTime;
    [SerializeField] private AnimationCurve unitAppearCurve;
    //[SerializeField] private AnimationCurve unitAppearCurve;

    [Header("Settings")]
    [SerializeField] public int amntColorsAtStart;
    [SerializeField] private int maxFingers;
    [SerializeField] private AnimationCurve weightDistributionCurve;
    [SerializeField] public int spawnAmount;
    [SerializeField] private float spriteSize;
    //[SerializeField] private float targetDotSize;
    private int xBoundary;
    private int yBoundary;

    [Header("Tech")]
    [SerializeField] private GameObject wall;
    [SerializeField] private Vector2 wallWidthHeight;
    [SerializeField] private GameObject sampleUnit;
    [NonReorderable] [SerializeField] public CounterColours[] counterColours;
    [HideInInspector] public List<GameObject> units = new List<GameObject>();
    private List<GameObject> groupTwo = new List<GameObject>();
    [NonReorderable] [SerializeField] private Categories[] categories;
    [HideInInspector] public int groupIDnumber = 0;
    [NonReorderable][SerializeField] private ColorCorrections[] corrections;
    public int amountOfGroups = -420;


    void Start()
    {
        amountOfGroups = -420;
        xBoundary = Screen.width - (int)spriteSize;
        yBoundary = Screen.height - (int)spriteSize;

        //Generate Borders
        GenerateBorders();

        //Generate stock units One
        for (int i = 0; i < categories.Length; i++)
		{
            GameObject _NewObject = Instantiate(sampleUnit, GenerateSpawnPoint(), Quaternion.identity);
            UnitBehaviour _UB = _NewObject.GetComponent<UnitBehaviour>();
            
            //Initialise colour arrays
            _UB.groupID = categories[i].groupID;

            _UB.color = new Color[maxFingers];
			for (int j = 0; j < _UB.color.Length; j++)
			{
				for (int g = 0; g < categories[i].switches.Length; g++)
				{
                    if(g == 0) { _UB.color[j] = categories[i].originalColor; }
                    if (categories[i].switches[g].switchMoment <= j) { _UB.color[j] = categories[i].switches[g].switchColor; }
                }
            }
            //Parent to Manager
            _NewObject.transform.SetParent(this.transform);
            //Add to List
            units.Add(_NewObject);
        }

        //Generate random units One
        for (int i = 0; i < (spawnAmount / 2)-categories.Length; i++)
        {
            GameObject _NewObject = Instantiate(sampleUnit, GenerateSpawnPoint(), Quaternion.identity);
            UnitBehaviour _UB = _NewObject.GetComponent<UnitBehaviour>();

            int _WeightedCategoryRoll = GenerateCategoryInt();

            //Initialise colour arrays
            _UB.groupID = categories[_WeightedCategoryRoll].groupID;

            _UB.color = new Color[maxFingers];
            for (int j = 0; j < _UB.color.Length; j++)
            {
                for (int g = 0; g < categories[_WeightedCategoryRoll].switches.Length; g++)
                {
                    if (g == 0) { _UB.color[j] = categories[_WeightedCategoryRoll].originalColor; }
                    if (categories[_WeightedCategoryRoll].switches[g].switchMoment <= j) { _UB.color[j] = categories[_WeightedCategoryRoll].switches[g].switchColor; }
                }
            }
            //Parent to Manager
            _NewObject.transform.SetParent(this.transform);
            //Add to List
            units.Add(_NewObject);
        }

		//Generate units group two
		for (int i = 0; i < units.Count; i++)
		{
            GameObject _NewObject = Instantiate(units[i], GenerateSpawnPoint(), Quaternion.identity);
            UnitBehaviour _UB = _NewObject.GetComponent<UnitBehaviour>();

            for (int j = 0; j < _UB.color.Length; j++)
			{
				for (int g = 0; g < counterColours.Length; g++)
				{
                    if(_UB.color[j] == counterColours[g].GroupOneCol) { _UB.color[j] = counterColours[g].GroupTwoCol; }
				}
            }

            _UB.groupID += 10;

            _NewObject.transform.SetParent(this.transform);

            groupTwo.Add(_NewObject);
        }

		//Add to units list
		for (int i = 0; i < groupTwo.Count; i++)
		{
            units.Add(groupTwo[i]);
		}

		for (int i = 0; i < units.Count; i++)
		{
            //Calculate size adjustment for target resolution
            //units[i].transform.localScale = Vector3.one * targetDotSize * (1440f/Screen.height);

            //Assign each unit unique sortedGRoupID
            units[i].GetComponent<UnitBehaviour>().sortedGroupID = groupIDnumber;
            groupIDnumber++;

			//Color corrections
			for (int j = 0; j < corrections.Length; j++)
			{
                if(units[i].GetComponent<UnitBehaviour>().groupID == corrections[j].groupID)
				{
                    units[i].GetComponent<UnitBehaviour>().color[corrections[j].colorPlace] = corrections[j].newColor;
                }
			}

            //Distance them all from each other
            units[i].GetComponent<CircleCollider2D>().radius = 1;
        }

        UpdateColors(0);
    }

	private Vector2 GenerateSpawnPoint()
	{
        Vector2 _NewSpawnPoint = new Vector2(Random.Range(spriteSize, xBoundary), Random.Range(spriteSize, yBoundary));
        return Camera.main.ScreenToWorldPoint(new Vector3(_NewSpawnPoint.x, _NewSpawnPoint.y, 0));
    }

    private int GenerateCategoryInt()
	{
        int _MaxCat = 0;
        float[] _Weights = new float[categories.Length];
        float _UnadjustedTotal = 0;
		for (int i = 0; i < _Weights.Length; i++)
		{
            _Weights[i] = weightDistributionCurve.Evaluate((float)i / (float)categories.Length);
            _UnadjustedTotal += _Weights[i];
        }

        float _RandomFloat = Random.Range(0f, 1f);
        float _SumOfWeights = 0;
        for (int i = 0; i < _Weights.Length; i++)
		{
            _Weights[i] = _Weights[i] / _UnadjustedTotal;
            //Debug.Log($"Weight{i} = {_Weights[i]}, Sum = {_SumOfWeights}, NextSum = {_SumOfWeights + _Weights[i]}, Random Number = {_RandomFloat}");
            if(_RandomFloat > _SumOfWeights && _RandomFloat <= _SumOfWeights + _Weights[i])
			{
                _MaxCat = i;
            }
            _SumOfWeights += _Weights[i];
        }
          
        return _MaxCat;
    }

    void GenerateBorders()
	{
        //Top Wall
        Vector3 _TopPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height, 10));
        GameObject _TopWall = Instantiate(wall, new Vector3(_TopPosition.x, _TopPosition.y + (wallWidthHeight.y / 2), 0), Quaternion.identity);
        _TopWall.transform.localScale = new Vector3(wallWidthHeight.x, wallWidthHeight.y);
        //Right Wall
        Vector3 _RightPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height/2, 10));
        GameObject _RightWall = Instantiate(wall, new Vector3(_RightPosition.x + (wallWidthHeight.y / 2), _RightPosition.y, 0), Quaternion.identity);
        _RightWall.transform.localScale = new Vector3(wallWidthHeight.y, wallWidthHeight.x);
        //Bottom Wall
        Vector3 _BottomPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
        GameObject _BottomWall = Instantiate(wall, new Vector3(_BottomPosition.x, _BottomPosition.y - (wallWidthHeight.y / 2), 0), Quaternion.identity);
        _BottomWall.transform.localScale = new Vector3(wallWidthHeight.x, wallWidthHeight.y);
        //Left Wall
        Vector3 _LeftPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height/2, 10));
        GameObject _LeftWall = Instantiate(wall, new Vector3(_LeftPosition.x - (wallWidthHeight.y / 2), _LeftPosition.y, 0), Quaternion.identity);
        _LeftWall.transform.localScale = new Vector3(wallWidthHeight.y, wallWidthHeight.x);
    }

    public void UpdateColors(int _TouchCount)
	{
		for (int i = 0; i < units.Count; i++)
		{
            if (_TouchCount >= maxFingers) { return; }
            units[i].GetComponent<SpriteRenderer>().color = units[i].GetComponent<UnitBehaviour>().color[_TouchCount];
        }
	}

    public void FixGrouping(int wrongGroupID, int correctGroupID)
	{
		for (int i = 0; i < units.Count; i++)
		{
            if(units[i].GetComponent<UnitBehaviour>().sortedGroupID == wrongGroupID)
			{
                units[i].GetComponent<UnitBehaviour>().sortedGroupID = correctGroupID;
            }
		}
        //groupIDnumber++;
	}

    public IEnumerator IntroSequence()
	{
        ShuffleUnitList();

        for (int i = 0; i < units.Count; i++)
		{
            units[i].GetComponent<SpriteRenderer>().enabled = true;
            units[i].GetComponent<CircleCollider2D>().radius = 0.6f;
            yield return new WaitForSeconds(unitAppearCurve.Evaluate((float)i/units.Count) * maxUnitAppearTime);
		}
        GameManagerSoI.instance.gameState = GameManagerSoI.GameStates.playing;
	}

    private void ShuffleUnitList()
	{
        for (int i = 0; i < units.Count; i++)
        {
            GameObject temp = units[i];
            int randomIndex = Random.Range(i, units.Count);
            units[i] = units[randomIndex];
            units[randomIndex] = temp;
        }
    }

    public void CountGroups()
	{
        List<int> _Groups = new List<int>();
        
		for (int u = 0; u < units.Count; u++)
		{
            bool _IsInList = false;
            for (int i = 0; i < _Groups.Count; i++)
			{
                if(units[u].GetComponent<UnitBehaviour>().sortedGroupID == _Groups[i])
				{
                    _IsInList = true;
                }
			}
			if (!_IsInList)
			{
                _Groups.Add(units[u].GetComponent<UnitBehaviour>().sortedGroupID);
            }            
		}

        amountOfGroups = _Groups.Count;
        Debug.Log($"amountOfGroups = {amountOfGroups}");
	}
}
