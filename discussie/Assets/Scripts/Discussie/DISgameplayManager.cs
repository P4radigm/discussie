using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DISgameplayManager : MonoBehaviour
{
	private DISgameplaySettings settings;
	private DISgameplayInput input;
	private List<GameObject> elementsInPlayTop = new List<GameObject>();
	private List<GameObject> elementsInPlayBot = new List<GameObject>();
	private List<GameObject> elementsInPlay = new List<GameObject>();
	[HideInInspector] public List<GameObject> elementsInConstructionArea = new List<GameObject>();
	[SerializeField] private TextMeshProUGUI topScoreDisplay;
	[SerializeField] private TextMeshProUGUI bottomScoreDisplay;

	private SequenceManager sequenceManager;
	private DataManager dataManager;
	private HighlightColorManager colorManager;

	public void StartUp()
	{
		sequenceManager = SequenceManager.instance;
		dataManager = DataManager.instance;
		colorManager = HighlightColorManager.instance;

		settings = GetComponent<DISgameplaySettings>();
		input = GetComponent<DISgameplayInput>();

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
						elementPool.Add(settings.connectorElements[maleFemaleDeciderLeft].gameObjects3D[maleFemaleDeciderRight].gameObjects2D[randomConnectionLeft].gameObjects[randomConnectionRight]);
						totalElementsAmount -= 3;

					}
					else if (totalElementsAmount >= 2)
					{
						//Will try to spawn a random two piece
						int MaleFemaleDecider = Random.Range(0, 2);
						int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
						elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]);
						elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]);
						totalElementsAmount -= 2;
						totalElementsAmount -= 2;
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
					if (EdgePieceDecider < settings.edgePiecesPercentage)
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
		List<GameObject> shuffledElementPool = ShuffleList(elementPool); //Maybe it should be sorted for play section B!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
				DISelementBehaviour EB = shuffledElementPool[i + elementsInPlayBot.Count].GetComponent<DISelementBehaviour>();
				GameObject GO = SpawnElementAndAnchor($"{i}.{j}_{EB.leftConnector}-{EB.rightConnector}", shuffledElementPool[i + elementsInPlayBot.Count], new Vector3(settings.xPositions[j], settings.yPosTopLines[i], 0), colorManager.highlightColorList[1], settings.bottomLines + i, true);

				elementsInPlayTop.Add(GO);
				elementsInPlay.Add(GO);
			}
		}

		//Animate in start elements -> toplist normal (bot to top), botlist reversed (top to bot)
		StartCoroutine(AnimateIn());
	}

	void Update()
	{
		if (settings.scrollEnabled)
		{
			//Animate scroll based on settings
			List<GameObject> ReplacementElements = new List<GameObject>();
			for (int i = 0; i < elementsInPlay.Count; i++)
			{
				DISelementBehaviour EB = elementsInPlay[i].GetComponent<DISelementBehaviour>();
				if (EB.spawnAnchor == null) { continue; } //Element does not ever need to scroll anymore
				Transform Anchor = EB.spawnAnchor;

				//Might want to have a setting to control the speed per line just like the direction, but idk if necassary
				Anchor.position += new Vector3((float)settings.scrollDirectionList[EB.line] * settings.scrollSpeed * Time.deltaTime, 0, 0);
				if (!EB.isDragged && !EB.isAnimating && !EB.isInConstructionZone)
				{
					EB.transform.position = Anchor.position;
				}

				//Detect whether element is out of range -> needs to be destroyed && Spawn in new element
				if (Anchor.position.x < settings.xPositions[0] || Anchor.position.x > settings.xPositions[settings.xPositions.Count - 1])
				{
					//Anchor is out of range

					//Store Yposition and line for replacement Element
					float xPosAtDestruction = Anchor.position.y;
					int spawnLine = EB.line;

					//Destroy Anchor
					Destroy(Anchor.gameObject);
					//Queue attached element for destruction
					if (!EB.isInConstructionZone && !EB.isDragged)
					{
						EB.queForDestruction = true;
					}

					//Spawn replacement Element on other side of the same line of destroyed element
					float replacementXpos = settings.scrollDirectionList[spawnLine] == -1 ? settings.xPositions[settings.xPositions.Count - 1] - (xPosAtDestruction - settings.xPositions[0]) : settings.xPositions[0] + (xPosAtDestruction - settings.xPositions[settings.xPositions.Count - 1]);
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
						if (EdgePieceDecider < settings.edgePiecesPercentage)
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
					}
					//Instantiate Replacement Object
					int newColorIndex = spawnLine < settings.bottomLines ? 0 : 1;
					GameObject ReplacementElement = SpawnElementAndAnchor($"Replacement.{spawnLine}_{EB.leftConnector}-{EB.rightConnector}", elementPrefab, replacementSpawnLocation, colorManager.highlightColorList[newColorIndex], spawnLine, false);

					if (spawnLine < settings.bottomLines) { elementsInPlayTop.Add(ReplacementElement); }
					else { elementsInPlayTop.Add(ReplacementElement); }
					//Queue to add the new objects after this loop has finished
					ReplacementElements.Add(ReplacementElement);
				}
			}

			for (int i = 0; i < elementsInPlay.Count; i++)
			{
				DISelementBehaviour EB = elementsInPlay[i].GetComponent<DISelementBehaviour>();
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
			//Update timer
		}

		//Update score?
	}

	private GameObject SpawnElementAndAnchor(string Name, GameObject Element, Vector3 Location, Color color, int line, bool isTransparent)
	{
		GameObject GO = Instantiate(Element, Location, Quaternion.identity, this.transform);
		GO.transform.localScale = Vector3.one * settings.elementScale;
		DISelementBehaviour EB = GO.GetComponent<DISelementBehaviour>();
		GO.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, isTransparent ? 0 : 1);
		GameObject Anchor = Instantiate(new GameObject(), Location, Quaternion.identity, this.transform);
		EB.spawnAnchor = Anchor.transform;
		EB.line = line;
		EB.manager = this;
		EB.settings = settings;
		EB.input = input;
		EB.colorType = color == colorManager.highlightColorList[0] ? DISelementBehaviour.Colour.green : DISelementBehaviour.Colour.purple;
		EB.col = color;
		GO.name = $"{Name}.Element";
		Anchor.name = $"{Name}.Anchor";
		return GO;
	}

	private IEnumerator AnimateIn()
	{
		//Animate in start elements -> toplist normal (bot to top), botlist reversed (top to bot)
		yield return null;
	}

	public void AddToScore(float scoreAddition)
	{
		//Animate score element
	}

	public void CloseDown()
	{
		//Animate out elements
	}

	private IEnumerator AnimateOut()
	{
		yield return null;
	}

	public void CommunicateResult()
	{
		//Communicate result to data manager
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
