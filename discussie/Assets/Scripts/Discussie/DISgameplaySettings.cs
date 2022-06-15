using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DISgameplaySettings : MonoBehaviour
{
	[System.Serializable]
	public class GameObjectArray2D
	{
		public string name;
		public GameObject[] gameObjects;
	}

	[System.Serializable]
	public class GameObjectArray3D
	{
		public string name;
		public GameObjectArray2D[] gameObjects2D;
	}

	[System.Serializable]
	public class GameObjectArray4D
	{
		public string name;
		public GameObjectArray3D[] gameObjects3D;
	}

	public enum ScrollDirectionModes
	{
		oneSide,
		alternating,
		alternatingSymmetrical, //When amnt of top & bot lines are equal based on bottom lines
		random,
		randomSymmetrical //When amnt of top & bot lines are equal based on bottom lines
	}

	[Header("Element Spawn Options")]
	public int bottomLines;
	public int topLines;
	public int elementsPerLine;
	[Range(0, 100)] public float edgePiecesPercentage;
	public bool containsMiddlePieces;
	[Range(0, 100)] public float groupsWithMiddlePiecesPercentage;
	[HideInInspector] public List<float> xPositions = new List<float>(); 
	[HideInInspector] public List<float> yPosBottomLines = new List<float>(); 
	[HideInInspector] public List<float> yPosTopLines = new List<float>(); 
	[HideInInspector] public float yPosConstruction;
	[HideInInspector] public float yTopEdgeConstruction;
	[HideInInspector] public float yBotEdgeConstruction;
	

	[Header("Scroll Settings")]
	public bool scrollEnabled;
	public float scrollSpeed; //Might need to scale the speed based on horizontal screen space
	public ScrollDirectionModes scrollMode;
	[HideInInspector] public List<int> scrollDirectionList = new List<int>(); //From bottom to top lines

	[Header("Timer Settings")]
	public bool timerEnabled;
	[Tooltip("Time in seconds till game over")]
	public float totalTime;

	[Header("Element Tables")]
	public GameObjectArray4D[] connectorElements;
	public GameObjectArray2D[] leftEdgeElements;
	public GameObjectArray2D[] rightEdgeElements;

	[Header("Element Connection Options")]
	[Tooltip("Distance between the ConnectionAnchors for perfect spacing")]
	[SerializeField] private float connectionDistanceAtScale1;
	[HideInInspector] public float connectionDistance;
	[SerializeField] private float tryConnectionRangeAtScale1;
	[HideInInspector] public float tryConnectionRange;
	public float minDotUpRange;
	public float maxDotUpRange;
	[Space(30)]
	public float snapToConnectedPositionDuration;
	public AnimationCurve snapToConnectedPositionCurve;
	public float animateBackToAnchorDuration;
	public AnimationCurve animateBackToAnchorCurve;
	public float fadeOutDuration;
	public AnimationCurve fadeOutCurve;

	[Header("Element Sizes")]
	[SerializeField] private float elementWidthAtScale1;
	[HideInInspector] public float elementWidth;
	[SerializeField] private float elementHeightAtScale1;
	[HideInInspector] public float elementHeight;
	[HideInInspector] public float elementScale;
	public float horizontalDistanceBetweeenElements;
	public float minVerticalDistanceBetweeenElements;
	public float comfyVerticalDistanceBetweeenElements;
	public float minVerticalConstructionSpacePadding;
	public float comfyVerticalConstructionSpacePadding;

	private PlatformManager platformManager;

	//Score calculation


	//Element behaviour settings

	public void StartUp()
	{
		platformManager = PlatformManager.instance;


		AdjustMeasurementsForScreenAspect();
		CalculateYPositions();
		CalculateXPositions();
		CalculateScrollDirections();
	}

	private void AdjustMeasurementsForScreenAspect()
	{
		//Scale adjustments based on screen aspect ratio (vertical screen world space), elementWidth & elementHeight
		float horizontalWorldScreenDistance = platformManager.WorldScreenBotRightCoords.x - platformManager.WorldScreenBotLeftCoords.x;
		elementWidth = (horizontalWorldScreenDistance - (horizontalDistanceBetweeenElements * ((float)elementsPerLine + 2f))) / elementsPerLine; //4.6 - (0 * (3-1) / 3)
		elementScale = elementWidth / elementWidthAtScale1;
		Debug.Log($"elementScale = {elementScale}");
		elementHeight = (elementScale) * elementHeightAtScale1;
		connectionDistance = connectionDistanceAtScale1 * elementScale;
		tryConnectionRange = tryConnectionRangeAtScale1 * elementScale;
	}

	private void CalculateXPositions()
	{
		if((float)elementsPerLine * elementWidth + (float)(elementsPerLine - 1) * horizontalDistanceBetweeenElements < platformManager.WorldScreenBotRightCoords.x - platformManager.WorldScreenBotLeftCoords.x)
		{
			Debug.LogWarning("Cannot fit requested amount of lines of elements within the horizontal screen area");
		}
		// \- [] - [] - [] - [0] - [] - [] - [] -/
		// \- [] - [] - [] - 0 - [] - [] - [] -/
		int amountOfPositions = elementsPerLine;
		if (scrollEnabled) { amountOfPositions = 7; }

		float startingXpos = 0 - Mathf.FloorToInt(amountOfPositions / 2) * elementWidth - Mathf.FloorToInt((amountOfPositions + 2f) / 2) * horizontalDistanceBetweeenElements;
		if(elementsPerLine % 2 == 0)
		{
			//elements per line is even, so centered differently
			startingXpos = 0 - Mathf.FloorToInt(amountOfPositions / 2) * elementWidth + elementWidth / 2f - (Mathf.FloorToInt(amountOfPositions / 2) + 2f) * horizontalDistanceBetweeenElements + horizontalDistanceBetweeenElements / 2f;
		}

		for (int i = 0; i < amountOfPositions; i++)
		{
			if(i == 0) { startingXpos += horizontalDistanceBetweeenElements; }
			xPositions.Add(startingXpos);
			startingXpos += elementWidth + horizontalDistanceBetweeenElements;
		}
	}

	private void CalculateYPositions()
	{
		//Calculate Y positions of rows based on screen safe area

		//Top Lines = 3, Bottom Lines = 3

		//1.5* minVerticalDistanceBetweeenElements 
		//Element -> Line
		//minVerticalDistanceBetweeenElements
		//Element -> Line
		//minVerticalDistanceBetweeenElements
		//Element -> Line
		//minVerticalDistanceBetweeenElements
		//minVerticalConstructionSpacePadding	|
		//ELement height						| > Construction area
		//minVerticalConstructionSpacePadding	|
		//minVerticalDistanceBetweeenElements
		//Element -> Line
		//minVerticalDistanceBetweeenElements
		//Element -> Line
		//minVerticalDistanceBetweeenElements
		//Element -> Line
		//1.5 * minVerticalDistanceBetweeenElements

		if(topLines != 0 && bottomLines != 0)
		{
			//Two colour mode, construction area centred between lines
			//Check if minimum height for everything even fits
			float availableScreenSize = platformManager.WorldScreenTopLeftCoords.y - platformManager.WorldScreenBotLeftCoords.y;
			if((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 3f) * minVerticalDistanceBetweeenElements + 2f * minVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogWarning("Cannot fit requested amount of lines of elements within the vertical screen area");
			}
			float extraVerticalSpace = availableScreenSize - ((topLines + bottomLines + 1f) * elementHeight) - (2f * minVerticalConstructionSpacePadding) - (topLines + bottomLines + 3f) * minVerticalDistanceBetweeenElements;
			float adjustedVerticalSpaceBetweenElements = minVerticalDistanceBetweeenElements + (extraVerticalSpace / (2f + topLines + bottomLines + 3f));
			float adjustedVerticalConstructionSpacePadding = minVerticalConstructionSpacePadding + (extraVerticalSpace / (2f + topLines + bottomLines + 3f));
			float yPosForCalc = platformManager.WorldScreenBotLeftCoords.y;
			for (int i = 0; i < bottomLines; i++)
			{
				if(i == 0)
				{
					yPosForCalc += adjustedVerticalSpaceBetweenElements * 0.5f;
				}
				yPosForCalc += adjustedVerticalSpaceBetweenElements;
				yPosForCalc += elementHeight / 2f;
				yPosBottomLines.Add(yPosForCalc);
				yPosForCalc += elementHeight / 2f;
			}
			yPosForCalc += adjustedVerticalSpaceBetweenElements;
			yBotEdgeConstruction = yPosForCalc;
			yPosForCalc += adjustedVerticalConstructionSpacePadding;
			yPosForCalc += elementHeight / 2f;
			yPosConstruction = yPosForCalc;
			yPosForCalc += elementHeight / 2f;
			yPosForCalc += adjustedVerticalConstructionSpacePadding;
			yTopEdgeConstruction = yPosForCalc;
			yPosForCalc += adjustedVerticalSpaceBetweenElements;
			for (int i = 0; i < topLines; i++)
			{
				yPosForCalc += elementHeight / 2f;
				yPosTopLines.Add(yPosForCalc);
				yPosForCalc += elementHeight / 2f;
				yPosForCalc += adjustedVerticalSpaceBetweenElements;
			}

			//List -> Bot to Top everything
		}
		else if(topLines == 0)
		{
			//Calc lines from bottom to top, construction area topping the lines
			float availableScreenSize = platformManager.WorldScreenTopLeftCoords.y - platformManager.WorldScreenBotLeftCoords.y;
			if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 3f) * minVerticalDistanceBetweeenElements + 2f * minVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogError($"Cannot fit requested amount of lines of elements ({elementHeight}) within the vertical screen area ({availableScreenSize}) with minimum spacing");
			}
			else if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 3f) * comfyVerticalDistanceBetweeenElements + 2f * comfyVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogError($"Cannot fit requested amount of lines of elements ({elementHeight}) within the vertical screen area ({availableScreenSize}) with comfy spacing");
			}
			else
			{
				float yPosForCalc = platformManager.WorldScreenBotLeftCoords.y;
				for (int i = 0; i < bottomLines; i++)
				{
					if (i == 0)
					{
						yPosForCalc += comfyVerticalDistanceBetweeenElements * 0.5f;
					}
					yPosForCalc += comfyVerticalConstructionSpacePadding;
					yPosForCalc += elementHeight / 2f;
					yPosBottomLines.Add(yPosForCalc);
					yPosForCalc += elementHeight / 2f;
				}
				yPosForCalc += comfyVerticalDistanceBetweeenElements;
				yBotEdgeConstruction = yPosForCalc;
				yTopEdgeConstruction = platformManager.WorldScreenTopLeftCoords.y - 2f * comfyVerticalDistanceBetweeenElements;
				yPosConstruction = yBotEdgeConstruction + ((yTopEdgeConstruction - yBotEdgeConstruction) / 2f);
				return;
			}

			float yPosForCalcMin = platformManager.WorldScreenBotLeftCoords.y;
			for (int i = 0; i < bottomLines; i++)
			{
				if (i == 0)
				{
					yPosForCalcMin += minVerticalDistanceBetweeenElements;
				}
				yPosForCalcMin += minVerticalConstructionSpacePadding;
				yPosForCalcMin += elementHeight / 2f;
				yPosBottomLines.Add(yPosForCalcMin);
				yPosForCalcMin += elementHeight / 2f;
			}
			yPosForCalcMin += minVerticalDistanceBetweeenElements;
			yBotEdgeConstruction = yPosForCalcMin;
			yTopEdgeConstruction = platformManager.WorldScreenTopLeftCoords.y - 2f * minVerticalDistanceBetweeenElements;
			yPosConstruction = yBotEdgeConstruction + ((yTopEdgeConstruction - yBotEdgeConstruction) / 2f);

			//List bot to top Botlines (toplines don't exist)
		}
		else
		{
			//Calc lines from top to bottom, construction area below the lines
			float availableScreenSize = platformManager.WorldScreenTopLeftCoords.y - platformManager.WorldScreenBotLeftCoords.y;
			if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 3f) * minVerticalDistanceBetweeenElements + 2f * minVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogWarning("Cannot fit requested amount of lines of elements within the vertical screen area with minimum spacing");
			}
			else if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 3f) * comfyVerticalDistanceBetweeenElements + 2f * comfyVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogWarning("Cannot fit requested amount of lines of elements within the vertical screen area with comfy spacing");
			}
			else
			{
				float yPosForCalc = platformManager.WorldScreenTopLeftCoords.y;
				for (int i = 0; i < topLines; i++)
				{
					if (i == 0)
					{
						yPosForCalc -= comfyVerticalDistanceBetweeenElements * 0.5f;
					}
					yPosForCalc -= comfyVerticalConstructionSpacePadding;
					yPosForCalc -= elementHeight / 2f;
					yPosBottomLines.Add(yPosForCalc);
					yPosForCalc -= elementHeight / 2f;
				}
				yPosForCalc -= comfyVerticalDistanceBetweeenElements;
				yTopEdgeConstruction = yPosForCalc;
				yBotEdgeConstruction = platformManager.WorldScreenBotLeftCoords.y + 2f * comfyVerticalDistanceBetweeenElements;
				yPosConstruction = yBotEdgeConstruction + ((yTopEdgeConstruction - yBotEdgeConstruction) / 2f);
				return;
			}

			float yPosForCalcMin = platformManager.WorldScreenTopLeftCoords.y;
			for (int i = 0; i < topLines; i++)
			{
				if (i == 0)
				{
					yPosForCalcMin -= minVerticalDistanceBetweeenElements;
				}
				yPosForCalcMin -= minVerticalConstructionSpacePadding;
				yPosForCalcMin -= elementHeight / 2f;
				yPosBottomLines.Add(yPosForCalcMin);
				yPosForCalcMin -= elementHeight / 2f;
			}
			yPosForCalcMin -= minVerticalDistanceBetweeenElements;
			yTopEdgeConstruction = yPosForCalcMin;
			yBotEdgeConstruction = platformManager.WorldScreenBotLeftCoords.y + 2f * minVerticalDistanceBetweeenElements;
			yPosConstruction = yBotEdgeConstruction + ((yTopEdgeConstruction - yBotEdgeConstruction) / 2f);
		}

		//List top to bot Toplines (botlines don't exist)
		yPosTopLines.Reverse();
		//List bot to top Toplines (botlines don't exist)
	}

	private void CalculateScrollDirections()
	{
		int startDirection = Random.Range(0, 2) == 0 ? -1 : 1;
		switch (scrollMode)
		{
			case ScrollDirectionModes.oneSide:

				for (int i = 0; i < bottomLines; i++)
				{
					scrollDirectionList.Add(startDirection);
				}
				for (int i = 0; i < topLines; i++)
				{
					scrollDirectionList.Add(startDirection * -1);
				}
				break;
			case ScrollDirectionModes.alternating:
				for (int i = 0; i < bottomLines + topLines; i++)
				{
					if (i % 2 == 0)
					{
						scrollDirectionList.Add(startDirection);
					}
					else
					{
						scrollDirectionList.Add(startDirection * -1);
					}
				}
				break;
			case ScrollDirectionModes.alternatingSymmetrical:
				for (int i = 0; i < (topLines <= bottomLines ? bottomLines : topLines); i++)
				{
					if (i % 2 == 0)
					{
						scrollDirectionList.Add(startDirection);
					}
					else
					{
						scrollDirectionList.Add(startDirection * -1);
					}
				}

				for (int i = 0; i < (topLines <= bottomLines ? topLines : bottomLines); i++)
				{
					scrollDirectionList.Add(scrollDirectionList[(topLines <= bottomLines ? bottomLines : topLines) - i]);
				}
				break;
			case ScrollDirectionModes.random:
				for (int i = 0; i < bottomLines + topLines; i++)
				{
					int randomDirection = Random.Range(0, 2) == 0 ? -1 : 1;
					scrollDirectionList.Add(randomDirection);
				}
				break;
			case ScrollDirectionModes.randomSymmetrical:
				for (int i = 0; i < (topLines <= bottomLines ? bottomLines : topLines); i++)
				{
					int randomDirection = Random.Range(0, 2) == 0 ? -1 : 1;
					scrollDirectionList.Add(randomDirection);
				}

				for (int i = 0; i < (topLines <= bottomLines ? topLines : bottomLines); i++)
				{
					scrollDirectionList.Add(scrollDirectionList[(topLines <= bottomLines ? bottomLines : topLines) - i]);
				}
				break;
			default:
				break;
		}
	}
}
