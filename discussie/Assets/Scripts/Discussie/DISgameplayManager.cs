using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DISgameplayManager : MonoBehaviour
{
	private DISgameplaySettings settings;
	private DISgameplayInput input;
	private DISscoreResultManager resultManager;
	[HideInInspector] public List<GameObject> elementsInPlayTop = new List<GameObject>();
	[HideInInspector] public List<GameObject> elementsInPlayBot = new List<GameObject>();
	[HideInInspector] public List<GameObject> elementsInPlay = new List<GameObject>();
	[HideInInspector] public List<Transform> anchorsInPlay = new List<Transform>();
	[HideInInspector] public List<GameObject> elementsInConstructionArea = new List<GameObject>();
	public TextMeshProUGUI topScoreDisplay;
	public TextMeshProUGUI bottomScoreDisplay;
	[SerializeField] private RectTransform scoreHolder;
	[SerializeField] private Transform topDashedLine;
	[SerializeField] private Transform botDashedLine;
	[SerializeField] private TextMeshProUGUI timerText;

	[Header("Anim settings")]
	[SerializeField] private float introTimeBeforeFirstSpawn;
	[SerializeField] private float introAnimDuration;
	[SerializeField] private AnimationCurve introAnimCurve;
	[SerializeField] private float popInDuration;
	[SerializeField] private AnimationCurve popInCurve;
	[SerializeField] private float waitToScrollSeconds;
	[SerializeField] private float graphicsIntroAnimDuration;
	[SerializeField] private AnimationCurve graphicsIntroAnimCurve;
	[Space(10)]
	[SerializeField] private float outroAnimDuration;
	[SerializeField] private AnimationCurve outroAnimCurve;
	private Coroutine animateOutRoutine;
	

	private SequenceManager sequenceManager;
	private DataManager dataManager;
	private HighlightColorManager colorManager;

	private bool introDone = false;

	[Header("Score display settings")]
	public int botScore = 0;
	public int topScore = 0;
	private float botDisplayScore = 0;
	private float topDisplayScore = 0;
	public string resultString = "";
	public int difference;
	public int totalScore;

	public void StartUp()
	{
		sequenceManager = SequenceManager.instance;
		dataManager = DataManager.instance;
		colorManager = HighlightColorManager.instance;

		settings = GetComponent<DISgameplaySettings>();
		input = GetComponent<DISgameplayInput>();
		resultManager = GetComponent<DISscoreResultManager>();

		//Generate a set of solvable or random (weighted) elements
		List<GameObject> elementPool = new List<GameObject>();
		if (!settings.scrollEnabled)
		{
			//generate solvable list of elements
			int totalElementsAmount = settings.topLines * settings.elementsPerLine + settings.bottomLines * settings.elementsPerLine;
			if (!settings.containsMiddlePieces)
			{
				//No middle pieces           
				while (totalElementsAmount != 0)
				{
					if (totalElementsAmount > ((settings.leftEdgeElements[0].gameObjects.Length * 2) + 1)) //4*2+1
					{
						//First spawn random pairs
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length - 1);
						elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]);
						elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]);
						totalElementsAmount -= 2;
					}
					else if (totalElementsAmount >= 2) //For sure 9 or lower
					{
						//Make sure each pair is spawned at least once
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Mathf.FloorToInt(((float)totalElementsAmount + 0.03f) / 2f); //Calculation based on totalElementsAmount -> floortoint(9.03/2) -> floortoint(4.5015) -> 4
						elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider - 1]);
						elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider - 1]);
						totalElementsAmount -= 2;
					}
					else
					{
						//Spawn random edge element
						int LeftRightDecider = Random.Range(0, 2);
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						if (LeftRightDecider == 0) { elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]); }
						else { elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]); }
						totalElementsAmount--;
					}
				}
			}
			else
			{
				//Does contain middle pieces
				int loopCounter = 0;
				while (totalElementsAmount != 0)
				{
					float threePieceDecider = Random.value;
					if (threePieceDecider < (settings.groupsWithMiddlePiecesPercentage / 100f) && totalElementsAmount >= 3)
					{
						//Randomly decided to select a three piece and is able to spawn a three piece
						//Generate Random Left edge piece
						int maleFemaleDeciderLeft = Random.Range(0, 2);
						int randomConnectionLeft = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						elementPool.Add(settings.leftEdgeElements[maleFemaleDeciderLeft].gameObjects[randomConnectionLeft]);
						

						//Generate Random Right edge piece
						int maleFemaleDeciderRight = Random.Range(0, 2);
						int randomConnectionRight = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						elementPool.Add(settings.rightEdgeElements[maleFemaleDeciderRight].gameObjects[randomConnectionRight]);

						//Find Fitting middle piece
						elementPool.Add(settings.connectorElements[Mathf.Abs(maleFemaleDeciderLeft - 1)].gameObjects3D[Mathf.Abs(maleFemaleDeciderRight - 1)].gameObjects2D[randomConnectionLeft].gameObjects[randomConnectionRight]);
						totalElementsAmount -= 3;
						Debug.Log($"Spawned 3 piece at loop: {loopCounter}, leftedge = {maleFemaleDeciderLeft}.{randomConnectionLeft}, middlepiece = {Mathf.Abs(maleFemaleDeciderLeft - 1)}.{randomConnectionLeft}-{Mathf.Abs(maleFemaleDeciderRight - 1)}.{randomConnectionRight}, rightedge = {maleFemaleDeciderRight}.{randomConnectionRight}");
						loopCounter++;
					}
					else if (totalElementsAmount >= 2)
					{
						//Will try to spawn a random two piece
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]);
						elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]);
						totalElementsAmount -= 2;
						Debug.Log($"Spawned 2 piece at loop: {loopCounter}, leftedge = {MaleFemaleDecider}.{ConnectionDecider}, rightedge = {Mathf.Abs(MaleFemaleDecider - 1)}.{ConnectionDecider}");
						loopCounter++;
					}
					else
					{
						//Just select a random edge element
						int LeftRightDecider = Random.Range(0, 2);
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						if (LeftRightDecider == 0) { elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]); }
						else { elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]); }
						totalElementsAmount -= 1;
						Debug.Log($"Spawned 1 piece at loop: {loopCounter}");
						loopCounter++;
					}
				}
			}
		}
		else
		{
			//generate random (weighted) starting elements
			int totalElementsAmount = settings.topLines * settings.elementsPerLine + settings.bottomLines * settings.elementsPerLine;
			if (!settings.containsMiddlePieces)
			{
				//Only spawn random edge pieces
				for (int i = 0; i < totalElementsAmount; i++)
				{
					int LeftRightDecider = Random.Range(0, 2);
					int MaleFemaleDecider = Random.Range(0, 2);
					int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
					if (LeftRightDecider == 0) { elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]); }
					else { elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]); }
				}
			}
			else
			{
				//Decide on edge or middle piece based on percentage
				for (int i = 0; i < totalElementsAmount; i++)
				{
					float EdgePieceDecider = Random.value;
					if (EdgePieceDecider < (settings.edgePiecesPercentage / 100f))
					{
						//Spawn random edge piece
						int LeftRightDecider = Random.Range(0, 2);
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						if (LeftRightDecider == 0) { elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]); }
						else { elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]); }
					}
					else
					{
						//Spawn random middle piece
						int maleFemaleDeciderLeft = Random.Range(0, 2);
						int maleFemaleDeciderRight = Random.Range(0, 2);
						int randomConnectionLeft = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						int randomConnectionRight = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						elementPool.Add(settings.connectorElements[maleFemaleDeciderLeft].gameObjects3D[maleFemaleDeciderRight].gameObjects2D[randomConnectionLeft].gameObjects[randomConnectionRight]);
					}
				}
			}
		}

		//Randomise list of elements
		List<GameObject> shuffledElementPool = elementPool;
		if (settings.shuffleElements) { shuffledElementPool = ShuffleList(elementPool); } //Maybe it should be sorted for play section B!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		Debug.Log($"shuffledElementPool Count = {shuffledElementPool.Count}");
		int spawnedCounter = 0;
		//Spawn bot elements
		for (int i = 0; i < settings.bottomLines; i++)
		{
			for (int j = 0; j < settings.elementsPerLine; j++)
			{
				DISelementBehaviour EB = shuffledElementPool[spawnedCounter].GetComponent<DISelementBehaviour>();
				GameObject GO = SpawnElementAndAnchor($"{i}.{j}_{EB.leftConnector}-{EB.rightConnector}", shuffledElementPool[spawnedCounter], new Vector3(settings.xPositions[j], settings.yPosBottomLines[i], 0), colorManager.highlightColorList[0], i, true);

				elementsInPlayBot.Add(GO);
				elementsInPlay.Add(GO);
				spawnedCounter++;
			}
		}
		//Spawn top elements
		for (int i = 0; i < settings.topLines; i++)
		{
			for (int j = 0; j < settings.elementsPerLine; j++)
			{
				DISelementBehaviour EB = shuffledElementPool[spawnedCounter].GetComponent<DISelementBehaviour>();
				GameObject GO = SpawnElementAndAnchor($"{i}.{j}_{EB.leftConnector}-{EB.rightConnector}", shuffledElementPool[spawnedCounter], new Vector3(settings.xPositions[j], settings.yPosTopLines[i], 0), colorManager.highlightColorList[1], settings.bottomLines + i, true);

				elementsInPlayTop.Add(GO);
				elementsInPlay.Add(GO);
				spawnedCounter++;
			}
		}

		//Animate in start elements -> toplist normal (bot to top), botlist reversed (top to bot)
		StartCoroutine(AnimateIn(elementsInPlayBot));
		StartCoroutine(AnimateIn(elementsInPlayTop));

		//Set graphics to correct positions and animate in
		topDashedLine.position = new Vector3(0, settings.yTopEdgeConstruction, 0);
		botDashedLine.position = new Vector3(0, settings.yBotEdgeConstruction, 0);
		float canvasScaleFactor = GetComponentInChildren<Canvas>().scaleFactor;
		//Debug.Log($"canvasScaleFactor = {canvasScaleFactor}");
		scoreHolder.anchoredPosition = new Vector2(0, Camera.main.WorldToScreenPoint(new Vector3(0, settings.yPosConstruction, 0)).y / canvasScaleFactor);
		bottomScoreDisplay.text = "0";
		topScoreDisplay.text = "0";
		StartCoroutine(AnimateInGraphics());
	}

	void Update()
	{
		if(settings == null || !introDone)
		{
			return;
		}

		if (settings.scrollEnabled)
		{
			//Animate scroll based on settings
			List<GameObject> ReplacementElements = new List<GameObject>();
			for (int i = 0; i < anchorsInPlay.Count; i++)
			{
				Transform Anchor = anchorsInPlay[i];
				DISelementBehaviour EB = Anchor.GetComponent<DISanchor>().linkedElement;
				//Might want to have a setting to control the speed per line just like the direction, but idk if necassary
				Anchor.position += new Vector3((float)settings.scrollDirectionList[EB.line] * settings.scrollSpeed * Time.deltaTime, 0, 0);

				if(EB != null)
				{
					if (!EB.isDragged && !EB.isAnimating && !EB.isInConstructionZone)
					{
						EB.transform.position = Anchor.position;
					}
				}


				//Detect whether element is out of range -> needs to be destroyed && Spawn in new element
				if (Anchor.position.x < settings.xPositions[0] - settings.scrollRangeOffset || Anchor.position.x > settings.xPositions[settings.xPositions.Count - 1] + settings.scrollRangeOffset)
				{
					//Anchor is out of range

					//Store Yposition and line for replacement Element
					float xPosAtDestruction = Anchor.position.x;
					int spawnLine = Anchor.GetComponent<DISanchor>().line;

					//Destroy Anchor
					anchorsInPlay.RemoveAt(i);
					Destroy(Anchor.gameObject);
					//Queue attached element for destruction
					if (EB != null)
					{
						if (!EB.isInConstructionZone && !EB.isDragged)
						{
							EB.queForDestruction = true;
						}
					}

					float leftScrollerAdjustedXPos = settings.xPositions[settings.xPositions.Count - 1] - (settings.xPositions[0] - xPosAtDestruction);
					float rigthScrollerAdjustedXPos = settings.xPositions[0] + (xPosAtDestruction - settings.xPositions[settings.xPositions.Count - 1]);

					//Spawn replacement Element on other side of the same line of destroyed element
					float replacementXpos = settings.scrollDirectionList[spawnLine] == -1 ? leftScrollerAdjustedXPos : rigthScrollerAdjustedXPos;
					float replacementYpos = spawnLine < settings.bottomLines ? settings.yPosBottomLines[spawnLine] : settings.yPosTopLines[spawnLine - settings.bottomLines];
					Vector3 replacementSpawnLocation = new Vector3(replacementXpos, replacementYpos, 0);
					//generate random (weighted) element
					GameObject elementPrefab;
					if (!settings.containsMiddlePieces)
					{
						//Only spawn random edge pieces

						int LeftRightDecider = Random.Range(0, 2);
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						if (LeftRightDecider == 0) { elementPrefab = settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]; }
						else { elementPrefab = settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]; }
					}
					else
					{
						//Decide on edge or middle piece based on percentage
						float EdgePieceDecider = Random.value;
						if (EdgePieceDecider < (settings.edgePiecesPercentage / 100f))
						{
							//Spawn random edge piece
							int LeftRightDecider = Random.Range(0, 2);
							int MaleFemaleDecider = Random.Range(0, 2);
							int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
							if (LeftRightDecider == 0) { elementPrefab = settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]; }
							else { elementPrefab = settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]; }
						}
						else
						{
							//Spawn random middle piece
							int maleFemaleDeciderLeft = Random.Range(0, 2);
							int maleFemaleDeciderRight = Random.Range(0, 2);
							int randomConnectionLeft = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
							int randomConnectionRight = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
							elementPrefab = settings.connectorElements[maleFemaleDeciderLeft].gameObjects3D[maleFemaleDeciderRight].gameObjects2D[randomConnectionLeft].gameObjects[randomConnectionRight];
						}
					i--;
					}
					//Instantiate Replacement Object
					int newColorIndex = spawnLine < settings.bottomLines ? 0 : 1;
					GameObject ReplacementElement = SpawnElementAndAnchor($"Replacement.{spawnLine}_{elementPrefab.GetComponent<DISelementBehaviour>().leftConnector}-{elementPrefab.GetComponent<DISelementBehaviour>().rightConnector}", elementPrefab, replacementSpawnLocation, colorManager.highlightColorList[newColorIndex], spawnLine, false);

					if (spawnLine < settings.bottomLines) { elementsInPlayTop.Add(ReplacementElement); }
					else { elementsInPlayTop.Add(ReplacementElement); }
					//Queue to add the new objects after this loop has finished
					ReplacementElements.Add(ReplacementElement);
				}
			}

			for (int i = 0; i < elementsInPlay.Count; i++)
			{
				DISelementBehaviour EB = elementsInPlay[i].GetComponent<DISelementBehaviour>();
				for (int j = 0; j < elementsInPlay.Count; j++)
				{
					DISelementBehaviour OtherEB = elementsInPlay[j].GetComponent<DISelementBehaviour>();
					if (EB.spawnAnchor != null && OtherEB.spawnAnchor != null && EB != OtherEB)
					{
						if (Vector3.Distance(EB.spawnAnchor.position, OtherEB.spawnAnchor.position) <= 0.1f)
						{
							elementsInPlay.RemoveAt(i);
							elementsInPlayBot.Remove(EB.gameObject);
							elementsInPlayTop.Remove(EB.gameObject);
							Destroy(EB.gameObject);
							i--;
							continue;
						}				
					}
				}

				if (EB.queForDestruction && !EB.isAnimating)
				{
					//Remove object from lists and destroy gameObject
					elementsInPlay.RemoveAt(i);
					elementsInPlayBot.Remove(EB.gameObject);
					elementsInPlayTop.Remove(EB.gameObject);
					Destroy(EB.gameObject);
					i--;
				}
			}

			//Add replacement elements to list
			for (int i = 0; i < ReplacementElements.Count; i++)
			{
				elementsInPlay.Add(ReplacementElements[i]);
			}
		}

		if (settings.timerEnabled)
		{
			settings.totalTime -= Time.deltaTime;
			if(settings.totalTime <= 0)
			{
				timerText.text = "0:00";
				if (animateOutRoutine == null) { animateOutRoutine = StartCoroutine(AnimateOutNoScroll()); }
				settings.scrollEnabled = false;
			}
			else 
			{
				timerText.text = $"{Mathf.FloorToInt(settings.totalTime / 60)}:{(Mathf.FloorToInt(settings.totalTime % 60) < 10 ? "0" : "")}{Mathf.FloorToInt(settings.totalTime % 60)}";
			}
		}

		if(botDisplayScore < botScore)
		{
			botDisplayScore += settings.displayIncrementSpeed * Time.deltaTime;
		}
		bottomScoreDisplay.text = Mathf.RoundToInt(botDisplayScore).ToString();
		if (topDisplayScore < topScore)
		{
			topDisplayScore += settings.displayIncrementSpeed * Time.deltaTime;
		}
		topScoreDisplay.text = Mathf.RoundToInt(topDisplayScore).ToString();
	}

	public void EdgeCheck()
	{
		if (settings.scrollEnabled) { return; }
		int edgeCounter = 0;
		for (int i = 0; i < elementsInPlay.Count; i++)
		{
			DISelementBehaviour EB = elementsInPlay[i].GetComponent<DISelementBehaviour>();
			if(EB.leftConnector == DISelementBehaviour.ConnnectionType.edge || EB.rightConnector == DISelementBehaviour.ConnnectionType.edge)
			{
				edgeCounter++;
			}
		}
		if(edgeCounter <= 1)
		{
			//Trigger end anim
			if(animateOutRoutine == null) { animateOutRoutine = StartCoroutine(AnimateOutNoScroll()); }
		}
	}

	private GameObject SpawnElementAndAnchor(string Name, GameObject Element, Vector3 Location, Color color, int line, bool isTransparent)
	{
		GameObject GO = Instantiate(Element, Location, Quaternion.identity, this.transform);
		GO.transform.localScale = Vector3.one * settings.elementScale;
		DISelementBehaviour EB = GO.GetComponent<DISelementBehaviour>();
		GO.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, isTransparent ? 0 : settings.elementTransperancy);
		GameObject Anchor = new GameObject();
		Anchor.transform.position = Location;
		Anchor.transform.parent = this.transform;
		DISanchor anchorComp = Anchor.AddComponent<DISanchor>();
		anchorComp.linkedElement = EB;
		EB.spawnAnchor = Anchor.transform;
		EB.line = line;
		anchorComp.line = line;
		EB.manager = this;
		EB.settings = settings;
		EB.input = input;
		EB.colorType = color == colorManager.highlightColorList[0] ? DISelementBehaviour.Colour.purple : DISelementBehaviour.Colour.green;
		EB.col = new Color(color.r, color.g, color.b, settings.elementTransperancy);
		GO.name = $"{Name}.Element";
		Anchor.name = $"{Name}.Anchor";
		anchorsInPlay.Add(Anchor.transform);
		return GO;
	}

	private IEnumerator AnimateIn(List<GameObject> elements)
	{
		//Animate in start elements -> toplist normal (bot to top), botlist reversed (top to bot)
		float totalCurveTime = 0;
		for (int i = 0; i < elements.Count; i++)
		{
			totalCurveTime += introAnimCurve.Evaluate((float)i / elements.Count);
		}

		yield return new WaitForSeconds(introTimeBeforeFirstSpawn);

		for (int i = 0; i < elements.Count; i++)
		{
			DISelementBehaviour EB = elements[i].GetComponent<DISelementBehaviour>();
			EB.StartCoroutine(EB.AnimateFadeIn(popInDuration, popInCurve));

			yield return new WaitForSeconds(introAnimDuration / (float)elements.Count);
		}

		yield return new WaitForSeconds(waitToScrollSeconds);

		introDone = true;
	}

	private IEnumerator AnimateInGraphics()
	{
		//Dashed lines
		SpriteRenderer[] topDashedRenderer = topDashedLine.gameObject.GetComponentsInChildren<SpriteRenderer>();
		SpriteRenderer[] botDashedRenderer = botDashedLine.gameObject.GetComponentsInChildren<SpriteRenderer>();
		//Score displays
		
		//Background(s)?

		float TimeValue = 0;

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / graphicsIntroAnimDuration;
			float EvaluatedTimeValue = graphicsIntroAnimCurve.Evaluate(TimeValue);
			float NewAlpha = Mathf.Lerp(0, 1, EvaluatedTimeValue);

			for (int i = 0; i < topDashedRenderer.Length; i++)
			{
				topDashedRenderer[i].color = new Color(topDashedRenderer[i].color.r, topDashedRenderer[i].color.g, topDashedRenderer[i].color.b, NewAlpha);
			}
			for (int i = 0; i < botDashedRenderer.Length; i++)
			{
				botDashedRenderer[i].color = new Color(botDashedRenderer[i].color.r, botDashedRenderer[i].color.g, botDashedRenderer[i].color.b, NewAlpha);
			}

			bottomScoreDisplay.color = new Color(bottomScoreDisplay.color.r, bottomScoreDisplay.color.g, bottomScoreDisplay.color.b, NewAlpha);
			topScoreDisplay.color = new Color(topScoreDisplay.color.r, topScoreDisplay.color.g, topScoreDisplay.color.b, NewAlpha);
			if (settings.timerEnabled) { timerText.color = new Color(timerText.color.r, timerText.color.g, timerText.color.b, NewAlpha); }

			yield return null;
		}
	}
	private IEnumerator AnimateOutNoScroll()
	{

		//Buffer for vfx finish
		if (settings.scrollEnabled)
		{
			yield return new WaitForSeconds(settings.vfxReachTime - 0.5f);
			CalculateEndScores();
			CommunicateResult();
		}
		else
		{
			yield return new WaitForSeconds(settings.vfxReachTime + 0.5f);
			CalculateEndScores();
		}

		input.enabled = false;
		//Starts animating the last elements out
		for (int i = 0; i < elementsInPlay.Count; i++)
		{
			DISelementBehaviour EB = elementsInPlay[i].GetComponent<DISelementBehaviour>();
			EB.isDragged = false;
			elementsInPlay[i].GetComponent<DISelementBehaviour>().StartCoroutine(elementsInPlay[i].GetComponent<DISelementBehaviour>().AnimateFadeOut());
		}
		//Dashed lines
		SpriteRenderer[] topDashedRenderer = topDashedLine.gameObject.GetComponentsInChildren<SpriteRenderer>();
		SpriteRenderer[] botDashedRenderer = botDashedLine.gameObject.GetComponentsInChildren<SpriteRenderer>();
		//Score displays

		//Background(s)?

		float TimeValue = 0;

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / outroAnimDuration;
			float EvaluatedTimeValue = outroAnimCurve.Evaluate(TimeValue);
			float NewAlpha = Mathf.Lerp(1, 0, EvaluatedTimeValue);

			for (int i = 0; i < topDashedRenderer.Length; i++)
			{
				topDashedRenderer[i].color = new Color(topDashedRenderer[i].color.r, topDashedRenderer[i].color.g, topDashedRenderer[i].color.b, NewAlpha);
			}
			for (int i = 0; i < botDashedRenderer.Length; i++)
			{
				botDashedRenderer[i].color = new Color(botDashedRenderer[i].color.r, botDashedRenderer[i].color.g, botDashedRenderer[i].color.b, NewAlpha);
			}

			bottomScoreDisplay.color = new Color(bottomScoreDisplay.color.r, bottomScoreDisplay.color.g, bottomScoreDisplay.color.b, NewAlpha);
			topScoreDisplay.color = new Color(topScoreDisplay.color.r, topScoreDisplay.color.g, topScoreDisplay.color.b, NewAlpha);
			if (settings.timerEnabled) { timerText.color = new Color(timerText.color.r, timerText.color.g, timerText.color.b, NewAlpha); }

			yield return null;
		}

		//Start Result Manager sequence
		resultManager.SetText(botScore, topScore, difference, totalScore);
		resultManager.StartCoroutine(resultManager.ResultSequence());
		yield return null;
	}

	public void CalculateEndScores()
	{
		//Calculate all end results
		difference = Mathf.Abs(topScore - botScore);
		totalScore = botScore + topScore - difference;
	}

	public void CommunicateResult()
	{
		//Communicate result to data manager
		dataManager.currentSaveData.gameResult = $"s{totalScore}s{resultString}";
		dataManager.UpdateSaveFile();
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
}
