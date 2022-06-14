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

	[Header("Element Spawn Options")]
	public int bottomLines;
	public int topLines;
	public int elementsPerLine;
	public bool scrollEnabled;
	[Range(0, 100)] public float edgePiecesPercentage;
	public bool containsMiddlePieces;
	[Range(0, 100)] public float groupsWithMiddlePiecesPercentage;
	public bool timerEnabled;
	[HideInInspector] public List<float> xPositions = new List<float>(); 
	[HideInInspector] public List<float> yPosBottomLines = new List<float>(); 
	[HideInInspector] public List<float> yPosTopLines = new List<float>(); 
	[HideInInspector] public float yPosConstruction;
	[HideInInspector] public float yTopEdgeConstruction;
	[HideInInspector] public float yBotEdgeConstruction;
	[Tooltip("Time in seconds till game over")]
	public float totalTime;

	[Header("Element Tables")]
	public GameObjectArray4D[] connectorElements;
	public GameObjectArray2D[] leftEdgeElements;
	public GameObjectArray2D[] rightEdgeElements;

	[Header("Element Sizes")]
	public float elementWidth;
	public float elementHeight;
	public float horizontalDistanceBetweeenElements;
	public float minVerticalDistanceBetweeenElements;
	public float comfyVerticalDistanceBetweeenElements;
	public float minVerticalConstructionSpacePadding;
	public float comfyVerticalConstructionSpacePadding;

	private PlatformManager platformManager;

	//Score calculation


	//Element behaviour settings

	private void OnEnable()
	{
		platformManager = PlatformManager.instance;

		CalculateYPositions();
		CalculateXPositions();
	}

	private void CalculateXPositions()
	{
		if((float)elementsPerLine * elementWidth + (float)(elementsPerLine - 1) * horizontalDistanceBetweeenElements < platformManager.WorldScreenBotRightCoords.x - platformManager.WorldScreenBotLeftCoords.x)
		{
			Debug.LogWarning("Cannot fit requested amount of lines of elements within the horizontal screen area");
		}
		// [] - [] - [] - [0] - [] - [] - []
		// [] - [] - [] - 0 - [] - [] - []
		int amountOfPositions = elementsPerLine;
		if (scrollEnabled) { amountOfPositions = 7; }

		float startingXpos = 0 - Mathf.FloorToInt(amountOfPositions / 2) * elementWidth - Mathf.FloorToInt(amountOfPositions / 2) * horizontalDistanceBetweeenElements;
		if(elementsPerLine % 2 == 0)
		{
			//elements per line is even, so centered differently
			startingXpos = 0 - Mathf.FloorToInt(amountOfPositions / 2) * elementWidth + elementWidth / 2f - Mathf.FloorToInt(amountOfPositions / 2) * horizontalDistanceBetweeenElements + horizontalDistanceBetweeenElements / 2f;
		}
		for (int i = 0; i < amountOfPositions; i++)
		{
			xPositions.Add(startingXpos);
			startingXpos += elementWidth + horizontalDistanceBetweeenElements;
		}
	}

	private void CalculateYPositions()
	{
		//Calculate Y positions of rows based on screen safe area

		//Top Lines = 3, Bottom Lines = 3

		//2 * minVerticalDistanceBetweeenElements 
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
		//2 * minVerticalDistanceBetweeenElements

		if(topLines != 0 && bottomLines != 0)
		{
			//Two colour mode, construction area centred between lines
			//Check if minimum height for everything even fits
			float availableScreenSize = platformManager.WorldScreenTopLeftCoords.y - platformManager.WorldScreenBotLeftCoords.y;
			if((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 4f) * minVerticalDistanceBetweeenElements + 2f * minVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogWarning("Cannot fit requested amount of lines of elements within the vertical screen area");
			}
			float extraVerticalSpace = availableScreenSize - ((topLines + bottomLines + 1f) * elementHeight) - (2f * minVerticalConstructionSpacePadding) - (topLines + bottomLines + 4f) * minVerticalDistanceBetweeenElements;
			float adjustedVerticalSpaceBetweenElements = minVerticalDistanceBetweeenElements + (extraVerticalSpace / (2f + topLines + bottomLines + 4f));
			float adjustedVerticalConstructionSpacePadding = minVerticalConstructionSpacePadding + (extraVerticalSpace / (2f + topLines + bottomLines + 4f));
			float yPosForCalc = platformManager.WorldScreenBotLeftCoords.y;
			for (int i = 0; i < bottomLines; i++)
			{
				if(i == 0)
				{
					yPosForCalc += adjustedVerticalSpaceBetweenElements;
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
		}
		else if(topLines == 0)
		{
			//Calc lines from bottom to top, construction area topping the lines
			float availableScreenSize = platformManager.WorldScreenTopLeftCoords.y - platformManager.WorldScreenBotLeftCoords.y;
			if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 4f) * minVerticalDistanceBetweeenElements + 2f * minVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogError("Cannot fit requested amount of lines of elements within the vertical screen area with minimum spacing");
			}
			else if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 4f) * comfyVerticalDistanceBetweeenElements + 2f * comfyVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogError("Cannot fit requested amount of lines of elements within the vertical screen area with comfy spacing");
			}
			else
			{
				float yPosForCalc = platformManager.WorldScreenBotLeftCoords.y;
				for (int i = 0; i < bottomLines; i++)
				{
					if (i == 0)
					{
						yPosForCalc += comfyVerticalDistanceBetweeenElements;
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
		}
		else
		{
			//Calc lines from top to bottom, construction area below the lines
			float availableScreenSize = platformManager.WorldScreenTopLeftCoords.y - platformManager.WorldScreenBotLeftCoords.y;
			if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 4f) * minVerticalDistanceBetweeenElements + 2f * minVerticalConstructionSpacePadding > availableScreenSize)
			{
				Debug.LogWarning("Cannot fit requested amount of lines of elements within the vertical screen area with minimum spacing");
			}
			else if ((topLines + bottomLines + 1f) * elementHeight + (topLines + bottomLines + 4f) * comfyVerticalDistanceBetweeenElements + 2f * comfyVerticalConstructionSpacePadding > availableScreenSize)
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
						yPosForCalc -= comfyVerticalDistanceBetweeenElements;
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
	}

}
