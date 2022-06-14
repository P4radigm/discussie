using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DISgameplayManager : MonoBehaviour
{
    private DISgameplaySettings settings;
    private List<GameObject> elementsInPlayTop = new List<GameObject>();
    private List<GameObject> elementsInPlayBot = new List<GameObject>();
    private List<GameObject> elementsInPlay = new List<GameObject>();
    public List<GameObject> elementsInConstructionArea = new List<GameObject>();
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
                    if(totalElementsAmount >= 2 && totalElementsAmount > ((settings.leftEdgeElements[0].gameObjects.Length * 2) + 1))
					{
                        //First spawn random pairs
                        int MaleFemaleDecider = Random.Range(0, 2);
                        int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
                        elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]);
                        elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]);
                        totalElementsAmount -= 2;
                    }
                    else if(totalElementsAmount >= 2)
					{
                        //Make sure each pair is spawned at least once
                        int MaleFemaleDecider = Random.Range(0, 2);
                        int ConnectionDecider = Mathf.FloorToInt(((float)totalElementsAmount + 0.03f) / 2f);//Calculation based on totalElementsAmount
                        elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]);
                        elementPool.Add(settings.rightEdgeElements[Mathf.Abs(MaleFemaleDecider - 1)].gameObjects[ConnectionDecider]);
                        totalElementsAmount -= 2;
					}
					else
					{
                        //Spawn random edge element
                        int LeftRightDecider = Random.Range(0, 2);
                        int MaleFemaleDecider = Random.Range(0, 2);
                        int ConnectionDecider = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
                        if(LeftRightDecider == 0) { elementPool.Add(settings.leftEdgeElements[MaleFemaleDecider].gameObjects[ConnectionDecider]); }
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
                    if(threePieceDecider < (settings.groupsWithMiddlePiecesPercentage / 100f) && totalElementsAmount >= 3)
					{
                        //Randomly decided to select a three piece and is able to spawn a three piece
                        //Generate Random Left edge piece
                        int maleFemaleDeciderLeft = Random.Range(0,2);
                        int randomConnectionLeft = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
                        elementPool.Add(settings.leftEdgeElements[maleFemaleDeciderLeft].gameObjects[randomConnectionLeft]);

                        //Generate Random Right edge piece
                        int maleFemaleDeciderRight = Random.Range(0,2);
                        int randomConnectionRight = Random.Range(0, settings.leftEdgeElements[0].gameObjects.Length);
                        elementPool.Add(settings.rightEdgeElements[maleFemaleDeciderRight].gameObjects[randomConnectionRight]);

                        //Find Fitting middle piece
                        elementPool.Add(settings.connectorElements[maleFemaleDeciderLeft].gameObjects3D[maleFemaleDeciderRight].gameObjects2D[randomConnectionLeft].gameObjects[randomConnectionRight]);
                        totalElementsAmount -= 3;

                    }
					else if(totalElementsAmount >= 2)
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

        //Spawn top elements
		for (int i = 0; i < settings.topLines; i++)
		{
			for (int j = 0; j < settings.elementsPerLine; j++)
			{
                GameObject GO = Instantiate(shuffledElementPool[i], new Vector3(settings.yPosTopLines[i], settings.xPositions[j], 0), Quaternion.identity, this.transform);
                DISelementBehaviour EB = GO.GetComponent<DISelementBehaviour>();
                GO.GetComponent<SpriteRenderer>().color = new Color(colorManager.highlightColorList[0].r, colorManager.highlightColorList[0].g, colorManager.highlightColorList[0].b, 0);
                GameObject Anchor = Instantiate(new GameObject(), new Vector3(settings.yPosTopLines[i], settings.xPositions[j], 0), Quaternion.identity, this.transform);
                EB.spawnAnchor = Anchor.transform;
                GO.name = $"{i}.{j}TopElement_{EB.leftConnector}-{EB.rightConnector}";
                Anchor.name = $"{i}.{j}TopAnchor_{EB.leftConnector}-{EB.rightConnector}";

                elementsInPlayTop.Add(GO);
                elementsInPlay.Add(GO);
            }     
		}
        //Spawn bot elements
        for (int i = 0; i < settings.bottomLines; i++)
        {
            for (int j = 0; j < settings.elementsPerLine; j++)
            {
                GameObject GO = Instantiate(shuffledElementPool[i], new Vector3(settings.yPosBottomLines[i], settings.xPositions[j], 0), Quaternion.identity, this.transform);
                DISelementBehaviour EB = GO.GetComponent<DISelementBehaviour>();
                GO.GetComponent<SpriteRenderer>().color = new Color(colorManager.highlightColorList[1].r, colorManager.highlightColorList[1].g, colorManager.highlightColorList[1].b, 0);
                GameObject Anchor = Instantiate(new GameObject(), new Vector3(settings.yPosBottomLines[i], settings.xPositions[j], 0), Quaternion.identity, this.transform);
                EB.spawnAnchor = Anchor.transform;
                GO.name = $"{i}.{j}BotElement_{EB.leftConnector}-{EB.rightConnector}";
                Anchor.name = $"{i}.{j}BotAnchor_{EB.leftConnector}-{EB.rightConnector}";

                elementsInPlayBot.Add(GO);
                elementsInPlay.Add(GO);
            }
        }

        //Animate in start elements -> toplist normal, botlist reversed
    }

    void Update()
    {
        //Animate scroll based on settings

        //Continually spawn elements if scrolling

        //Timer

        //Update score
    }

    public void AddToScore(float scoreAddition)
	{
        //Animate score element
	}

    public void CloseDown()
    {
        //Animate out elements
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
