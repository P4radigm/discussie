using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UZKgameplaySettings))]
[RequireComponent(typeof(UZKgameplayInput))]
public class UZKgameplayManager : MonoBehaviour
{
    public class Connections
	{
        public int GOxID;
        public int GOyID;
	}

    [Header("Intro/Outro")]
    [SerializeField] private float introTimeBeforeFirstSpawn;
    [SerializeField] private float totalElementAppearTime;
    [SerializeField] private AnimationCurve elementAppearCurve;
    [HideInInspector] public float elementColliderRadius;
    private bool isInIntro = true;
    //[SerializeField] private float totalElementDisappearTime;
    [SerializeField] private float averageGroupDisappearTime;
    [SerializeField] private AnimationCurve elementDisappearCurve;
    private Coroutine outroRoutine;
    [SerializeField] private bool needsOutroC;
    [SerializeField] private Color defResultColor;
    //[SerializeField] private AnimationCurve unitAppearCurve;

    private UZKgameplaySettings settings;
    private UZKgameplayInput gameplayInput;
    private PlatformManager platformManager;

    [Header("Settings")]
    private float xUpperBoundary; //in world space!
    private float xLowerBoundary; //in world space!
    private float yUpperBoundary; //in world space!
    private float yLowerBoundary; //in world space!
    [Space(10)]
    [SerializeField] private float endButtonAppearTime;
    [SerializeField] private float NonInteractTimeTillButtonAnim;
    [SerializeField] private Button endButton;
    [SerializeField] private TextMeshProUGUI endButtonText;
    [SerializeField] private Image endButtonVisualTop;
    [SerializeField] private Image endButtonVisualBot; //This is also the raycast target
    private float buttonTimerValue;
    [SerializeField] private TextMeshProUGUI groupCounter;

    [Header("Tech")]
    [SerializeField] private GameObject wall;
    [SerializeField] private Vector2 wallWidthHeight;
    
    public List<GameObject> elementsInPlay = new List<GameObject>();
    public List<Vector2Int> edges = new List<Vector2Int>();

    private List<GameObject> groupTwo = new List<GameObject>();

    public int amountOfGroups = -420;
    public int groupIDnumber = 0;

	public void StartPlaying()
    {
        settings = GetComponent<UZKgameplaySettings>();
        gameplayInput = GetComponent<UZKgameplayInput>();
        platformManager = PlatformManager.instance;

        isInIntro = true;
        amountOfGroups = -420;
        groupCounter.text = "0";
        elementColliderRadius = settings.ElementColliderRadius;

        //Calculate spawn area borders
        xUpperBoundary = platformManager.WorldSafeScreenTopRightCoords.x - settings.stockElementPool[0].elementPrefab.transform.localScale.x - settings.extraWorldSpawnBorderAdjustment;
        xLowerBoundary = platformManager.WorldSafeScreenBotLeftCoords.x + settings.stockElementPool[0].elementPrefab.transform.localScale.x + settings.extraWorldSpawnBorderAdjustment;
        yUpperBoundary = platformManager.WorldSafeScreenTopRightCoords.y - settings.stockElementPool[0].elementPrefab.transform.localScale.y - settings.extraWorldSpawnBorderAdjustment;
        yLowerBoundary = platformManager.WorldSafeScreenBotLeftCoords.y + settings.stockElementPool[0].elementPrefab.transform.localScale.y + settings.extraWorldSpawnBorderAdjustment;

        //Generate Borders
        GenerateBorders();

        //Generate stock elements group One
        for (int i = 0; i < settings.stockElementPool.Length; i++)
        {
            GameObject _NewObject = Instantiate(settings.stockElementPool[i].elementPrefab, GenerateSpawnPoint(), Quaternion.identity);
            UZKelementBehaviour _EB = _NewObject.GetComponent<UZKelementBehaviour>();
            //Reference this manager
            _NewObject.name = groupIDnumber.ToString();
            _NewObject.transform.localScale = Vector3.one * settings.ElementSize;
            _NewObject.GetComponent<CircleCollider2D>().radius = settings.ElementColliderRadius;
            _EB.gameplayManager = this;
            _EB.identifierID = groupIDnumber;
            _EB.minGroupRadius = settings.minGroupRadius;
            _EB.springCollisionEnable = settings.springCollisionEnable;
            _EB.springLength = settings.springLength;
            _EB.springDampingRatio = settings.springDampingRatio;
            _EB.springFrequency = settings.springFrequency;
            _EB.springBreakForce = settings.springBreakForce;
            _EB.springGracePeriod = settings.springGracePeriod;
            groupIDnumber++;

            //make sure all elements have sufficient colors in their list
            if(_EB.colorSequence.Count < settings.maxFingers)
			{
				for (int e = 0; e < settings.maxFingers - _EB.colorSequence.Count; e++)
				{
                    _EB.colorSequence.Add(_EB.colorSequence[_EB.colorSequence.Count - 1]);
                }
			}

            //Parent to Manager
            _NewObject.transform.SetParent(this.transform);
            //Add to List
            elementsInPlay.Add(_NewObject);
        }

        //Generate random elements group One
        for (int i = 0; i < (settings.spawnAmount / 2) - settings.stockElementPool.Length; i++)
        {
            int _WeightedCategoryRoll = GenerateCategoryInt();

            GameObject _NewObject = Instantiate(settings.randomElementPool[_WeightedCategoryRoll].elementPrefab, GenerateSpawnPoint(), Quaternion.identity);
            UZKelementBehaviour _EB = _NewObject.GetComponent<UZKelementBehaviour>();
            //Reference this manager
            _NewObject.name = groupIDnumber.ToString();
            _NewObject.transform.localScale = Vector3.one * settings.ElementSize;
            _NewObject.GetComponent<CircleCollider2D>().radius = settings.ElementColliderRadius;
            _EB.gameplayManager = this;
            _EB.identifierID = groupIDnumber;
            _EB.minGroupRadius = settings.minGroupRadius;
            _EB.springCollisionEnable = settings.springCollisionEnable;
            _EB.springLength = settings.springLength;
            _EB.springDampingRatio = settings.springDampingRatio;
            _EB.springFrequency = settings.springFrequency;
            _EB.springBreakForce = settings.springBreakForce;
            _EB.springGracePeriod = settings.springGracePeriod;
            groupIDnumber++;

            //make sure all elements have sufficient colors in their list
            if (_EB.colorSequence.Count < settings.maxFingers)
            {
                for (int e = 0; e < settings.maxFingers - _EB.colorSequence.Count; e++)
                {
                    _EB.colorSequence.Add(_EB.colorSequence[_EB.colorSequence.Count - 1]);
                }
            }

            //Parent to Manager
            _NewObject.transform.SetParent(this.transform);
            //Add to List
            elementsInPlay.Add(_NewObject);
        }

        //Generate units group two
        for (int i = 0; i < elementsInPlay.Count; i++)
        {
            GameObject _NewObject = Instantiate(elementsInPlay[i], GenerateSpawnPoint(), Quaternion.identity);
            UZKelementBehaviour _EB = _NewObject.GetComponent<UZKelementBehaviour>();
            _NewObject.name = groupIDnumber.ToString();
            _NewObject.transform.localScale = Vector3.one * settings.ElementSize;
            _NewObject.GetComponent<CircleCollider2D>().radius = settings.ElementColliderRadius;
            _EB.gameplayManager = this;
            _EB.identifierID = groupIDnumber;
            _EB.minGroupRadius = settings.minGroupRadius;
            _EB.springCollisionEnable = settings.springCollisionEnable;
            _EB.springLength = settings.springLength;
            _EB.springDampingRatio = settings.springDampingRatio;
            _EB.springFrequency = settings.springFrequency;
            _EB.springBreakForce = settings.springBreakForce;
            _EB.springGracePeriod = settings.springGracePeriod;
            groupIDnumber++;

            for (int j = 0; j < _EB.colorSequence.Count; j++)
            {
                for (int g = 0; g < settings.counterColours.Length; g++)
                {
                    if (_EB.colorSequence[j] == settings.counterColours[g].GroupOneCol) { _EB.colorSequence[j] = settings.counterColours[g].GroupTwoCol; }
                }
            }

            _NewObject.transform.SetParent(this.transform);

            groupTwo.Add(_NewObject);
        }

        //Add to elements list
        for (int i = 0; i < groupTwo.Count; i++)
        {
            elementsInPlay.Add(groupTwo[i]);
        }

        for (int i = 0; i < elementsInPlay.Count; i++)
        {
            //Distance all elements from each other   
            elementsInPlay[i].GetComponent<CircleCollider2D>().radius = 1;
        }

        UpdateColors(0);
        SortUnitListByID();
        StartCoroutine(IntroSequence());
    }

	public IEnumerator IntroSequence()
    {
        float totalCurveTime = 0;
        for (int i = 0; i < elementsInPlay.Count; i++)
		{
            totalCurveTime += elementAppearCurve.Evaluate((float)i / elementsInPlay.Count);
        }

        List<GameObject> shuffledList = ShuffleList(elementsInPlay);

        yield return new WaitForSeconds(introTimeBeforeFirstSpawn);

        for (int i = 0; i < elementsInPlay.Count; i++)
        {
            shuffledList[i].GetComponent<SpriteRenderer>().enabled = true;
            shuffledList[i].GetComponent<CircleCollider2D>().radius = elementColliderRadius;
            //yield return new WaitForSeconds(elementAppearCurve.Evaluate((float)i / elementsInPlay.Count) * maxElementAppearTime);
            groupCounter.text = (i + 1).ToString();
            yield return new WaitForSeconds((elementAppearCurve.Evaluate((float)i / (float)elementsInPlay.Count) / totalCurveTime) * totalElementAppearTime);
        }

        isInIntro = false;
    }

	private void LateUpdate()
	{
        if(Input.touchCount != 0)
        {
            if(buttonTimerValue > endButtonAppearTime + NonInteractTimeTillButtonAnim && gameplayInput.fingerHitDoneButton)
			{
                return;
			}
			else
			{
                buttonTimerValue = 0;
                endButtonText.color = new Color(1, 1, 1, 0);
                endButton.interactable = false;
                endButtonVisualBot.raycastTarget = false;
                endButtonVisualTop.fillAmount = 0;
                endButtonVisualBot.fillAmount = 0;
            }
            return; 
        }

        if(amountOfGroups == -420) { return; }

        if(buttonTimerValue > NonInteractTimeTillButtonAnim && buttonTimerValue <= endButtonAppearTime + NonInteractTimeTillButtonAnim)
		{
            //Button anim progress
            endButtonText.color = new Color(0, 0, 0, 0);
            endButton.interactable = false;
            endButtonVisualBot.raycastTarget = false;

        }
        else if(buttonTimerValue > endButtonAppearTime + NonInteractTimeTillButtonAnim)
		{
            //activate button
            endButtonText.color = new Color(0, 0, 0, 1);
            endButton.interactable = true;
            endButtonVisualBot.raycastTarget = true;
        }
		else
		{
            endButtonText.color = new Color(0, 0, 0, 0);
            endButton.interactable = false;
            endButtonVisualBot.raycastTarget = false;
        }

        endButtonVisualTop.fillAmount = Mathf.Clamp((buttonTimerValue - NonInteractTimeTillButtonAnim) / endButtonAppearTime, 0f , 1f);
        endButtonVisualBot.fillAmount = Mathf.Clamp((buttonTimerValue - NonInteractTimeTillButtonAnim) / endButtonAppearTime, 0f , 1f);

        buttonTimerValue += Time.deltaTime;
    }

    public void StopPlaying()
	{
        gameplayInput.currentTouchCount = 0;
        UpdateColors(gameplayInput.currentTouchCount);
        GetComponent<BaseGameplay>().endingGraceTime = averageGroupDisappearTime * amountOfGroups;
        if (outroRoutine != null) { return; }
        StartCoroutine(OutroSequence());
	}

    public IEnumerator OutroSequence()
	{
        float totalCurveTime = 0;
        for (int i = 0; i < amountOfGroups; i++)
        {
            if (i != amountOfGroups - 1) { totalCurveTime += elementDisappearCurve.Evaluate((float)i / ((float)amountOfGroups - 1f)); }  
        }

        float curveArea = GetAreaUnderCurve(elementDisappearCurve, 0.05f);

        endButton.gameObject.SetActive(false);
        gameplayInput.currentTouchCount = 0;
        gameplayInput.enabled = false;
        gameplayInput.currentTouchCount = 0;
        UpdateColors(gameplayInput.currentTouchCount);
        IdentifyGroups();

        Color counterCol = groupCounter.color;

		for (int i = 0; i < amountOfGroups; i++)
		{

            List<GameObject> GO = new List<GameObject>();
			for (int e = 0; e < elementsInPlay.Count; e++)
			{
                if(elementsInPlay[e].GetComponent<UZKelementBehaviour>().groupID == i)
				{
                    GO.Add(elementsInPlay[e]);
				}
			}

			for (int g = 0; g < GO.Count; g++)
			{
				if (!needsOutroC)
				{
                    GO[g].GetComponent<SpriteRenderer>().enabled = false;
                }
				else
				{
                    GO[g].GetComponent<SpriteRenderer>().color = defResultColor;
                }
            }

            groupCounter.color = new Color(counterCol.r, counterCol.g, counterCol.b, counterCol.a * (((float)amountOfGroups - (float)i - 1f) / (float)amountOfGroups));

            //if (i != amountOfGroups - 1) { yield return new WaitForSeconds((elementDisappearCurve.Evaluate((float)i / ((float)amountOfGroups - 1f)) / totalCurveTime) * totalElementDisappearTime); }
            //Debug.Log($"waittime = {(elementDisappearCurve.Evaluate((float)i / (float)(amountOfGroups - 2)) + (1 - curveArea)) * averageGroupDisappearTime}");
            if(amountOfGroups == 2 && i != amountOfGroups - 1) { yield return new WaitForSeconds(averageGroupDisappearTime); }
            else if (i != amountOfGroups - 1) { yield return new WaitForSeconds((elementDisappearCurve.Evaluate((float)i / (float)(amountOfGroups - 2)) + (1 - curveArea)) * averageGroupDisappearTime); }
        }
    }

    private float GetAreaUnderCurve(AnimationCurve myCurve, float stepSize)
	{
        float sum = 0;
        for (int i = 0; i < 1 / stepSize; i++)
        {
            sum += IntegralOnStep(
                        stepSize * i,
                        myCurve.Evaluate(stepSize * i), stepSize * (i + 1),
                        myCurve.Evaluate(stepSize * (i + 1))
                    );
        }
        return sum;
    }

    private float IntegralOnStep(float x0, float y0, float x1, float y1)
    {
        float a = (y1 - y0) / (x1 - x0);
        float b = y0 - a * x0;
        return (a / 2 * x1 * x1 + b * x1) - (a / 2 * x0 * x0 + b * x0);
    }

    private Vector3 GenerateSpawnPoint()
    {
        Vector2 _NewSpawnPoint = new Vector2(Random.Range(xLowerBoundary, xUpperBoundary), Random.Range(yLowerBoundary, yUpperBoundary));
        return new Vector3(_NewSpawnPoint.x, _NewSpawnPoint.y, 0);
    }

    private int GenerateCategoryInt()
    {
        int _returnInt = 0;
        float[] _Weights = new float[settings.randomElementPool.Length];
        float _SumOfWeights = 0;
        float _UnadjustedSumOfWeights = 0;
        //Create array with all weights and get total
		for (int i = 0; i < settings.randomElementPool.Length; i++)
		{
            _Weights[i] = settings.randomElementPool[i].spawnDistributionWeight;
            _UnadjustedSumOfWeights += settings.randomElementPool[i].spawnDistributionWeight;
        }

        float _RandomFloat = Random.Range(0f, _UnadjustedSumOfWeights);

        for (int i = 0; i < _Weights.Length; i++)
        {
            if (_RandomFloat > _SumOfWeights && _RandomFloat <= _SumOfWeights + _Weights[i])
            {
                _returnInt = i;
            }
            _SumOfWeights += _Weights[i];
        }

        return _returnInt;
    }

    void GenerateBorders()
    {
        //Top Wall
        Vector3 _TopPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, 10));
        GameObject _TopWall = Instantiate(wall, new Vector3(_TopPosition.x, _TopPosition.y + (wallWidthHeight.y / 2), 0), Quaternion.identity);
        _TopWall.transform.localScale = new Vector3(wallWidthHeight.x, wallWidthHeight.y);
        //Right Wall
        Vector3 _RightPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2, 10));
        GameObject _RightWall = Instantiate(wall, new Vector3(_RightPosition.x + (wallWidthHeight.y / 2), _RightPosition.y, 0), Quaternion.identity);
        _RightWall.transform.localScale = new Vector3(wallWidthHeight.y, wallWidthHeight.x);
        //Bottom Wall
        Vector3 _BottomPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10));
        GameObject _BottomWall = Instantiate(wall, new Vector3(_BottomPosition.x, _BottomPosition.y - (wallWidthHeight.y / 2), 0), Quaternion.identity);
        _BottomWall.transform.localScale = new Vector3(wallWidthHeight.x, wallWidthHeight.y);
        //Left Wall
        Vector3 _LeftPosition = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, 10));
        GameObject _LeftWall = Instantiate(wall, new Vector3(_LeftPosition.x - (wallWidthHeight.y / 2), _LeftPosition.y, 0), Quaternion.identity);
        _LeftWall.transform.localScale = new Vector3(wallWidthHeight.y, wallWidthHeight.x);
    }

    public void UpdateColors(int _TouchCount)
    {
        for (int i = 0; i < elementsInPlay.Count; i++)
        {
            if (_TouchCount >= settings.maxFingers) { return; }
            elementsInPlay[i].GetComponent<SpriteRenderer>().color = elementsInPlay[i].GetComponent<UZKelementBehaviour>().colorSequence[_TouchCount];
        }
    }

    private List<GameObject> ShuffleList(List<GameObject> list)
    {
        List<GameObject> tempList = new();
        for (int i = 0; i < list.Count; i++)
		{
            tempList.Add(list[i]);
        }
       
        for (int i = 0; i < tempList.Count; i++)
        {
            GameObject temp = tempList[i];
            int randomIndex = Random.Range(i, tempList.Count);
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }
        return tempList;
    }

    private void SortUnitListByID()
	{
        for (int i = 0; i < elementsInPlay.Count; i++)
        {
            GameObject temp = elementsInPlay[i];
            int index = elementsInPlay[i].GetComponent<UZKelementBehaviour>().identifierID;
            elementsInPlay[i] = elementsInPlay[index];
            elementsInPlay[index] = temp;
        }
    }

	#region GroupCounting
	public void CountGroups()
	{
        int n = elementsInPlay.Count;
        int[] ids = new int[n];

		for (int i = 0; i < ids.Length; i++)
		{
            ids[i] = i;
		}
		foreach (Vector2Int edge in edges)
		{
            Union(edge.x, edge.y, ids);
		}

        List<int> uniqueParents = new List<int>();
		for (int i = 0; i < ids.Length; i++)
		{
			if (!uniqueParents.Contains(Find(i, ids)))
			{
                uniqueParents.Add(Find(i, ids));
            }
        }

        amountOfGroups = uniqueParents.Count;
        if (!isInIntro) { groupCounter.text = amountOfGroups.ToString(); }
    }

    private void Union(int edge1, int edge2, int[] ids)
	{
        int parent1 = Find(edge1, ids);
        int parent2 = Find(edge2, ids);
        ids[parent1] = parent2;
	}

    private int Find(int edge, int[] ids)
	{
        if(ids[edge] != edge) { ids[edge] = Find(ids[edge], ids); }
        return ids[edge];
	}

    private void IdentifyGroups()
    {
        int groupID = 0;
        int n = elementsInPlay.Count;
        int[] ids = new int[n];

        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = i;
        }
        foreach (Vector2Int edge in edges)
        {
            Union(edge.x, edge.y, ids);
        }

        List<int> uniqueParents = new List<int>();
        for (int i = 0; i < ids.Length; i++)
        {
            if (!uniqueParents.Contains(Find(i, ids)))
            {
                uniqueParents.Add(Find(i, ids));
            }
        }

		for (int i = 0; i < ids.Length; i++)
		{
			for (int p = 0; p < uniqueParents.Count; p++)
			{
                if (ids[i] == uniqueParents[p])
				{
                    elementsInPlay[i].GetComponent<UZKelementBehaviour>().groupID = p;
				}
            }      
		}
    }
    #endregion

    public void SaveCoords()
	{
        Vector2[] _coords = new Vector2[elementsInPlay.Count];
		for (int i = 0; i < elementsInPlay.Count; i++)
		{
            _coords[i] = new Vector2(elementsInPlay[i].transform.position.x, elementsInPlay[i].transform.position.y);

        }
        DataManager dM = DataManager.instance;
        dM.currentSaveData.gameResult = dM.GetComponent<UZKresultInterperter>().ConvertToServerString(_coords);
        dM.UpdateSaveFile();
	}

    public void LetElementsGroupWhenNearby()
	{
		for (int i = 0; i < elementsInPlay.Count; i++)
		{
            elementsInPlay[i].GetComponent<UZKelementBehaviour>().FormGroupWhenNearby();
        }
	}
}
