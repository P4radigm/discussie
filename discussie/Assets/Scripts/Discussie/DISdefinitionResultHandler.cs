using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DISdefinitionResultHandler : MonoBehaviour
{
	private HighlightColorManager hCM;
	private BaseDefinition baseDef;
	private DataManager dataManager;
	private string resultScore;
	private string resultBubbles;
	[SerializeField] private bool isFirst;
	[SerializeField] private bool isBaseDef;
	[SerializeField] private bool isInput;
	[SerializeField] private bool isOwnDef;
	[Space(20)]
	[SerializeField][Range(0, 1)] private float elementTransperancy;
	[SerializeField] private TextMeshProUGUI scoreText;
	[SerializeField] private TextMeshProUGUI definitionText;
	[Space(20)]
	[SerializeField] private GameObject leftEdgeEmpty;
	[SerializeField] private GameObject leftEdgeEmptyTriangle;
	[SerializeField] private GameObject middleEmpty;
	[SerializeField] private GameObject rightEdgeEmpty;
	[SerializeField] private GameObject rightEdgeEmptyTriangle;
	[Space(20)]
	[SerializeField] private Transform startSpawnPoint;
	[SerializeField] private Transform startSpawnPointFirst;
	[SerializeField] private float spaceBetweenBubbleXPositions;
	[SerializeField] private float elementScale;
	[SerializeField] private float elementWidthAtScale1;
	[SerializeField] private int maxElementsPerColumn;
	[SerializeField] private Vector3 rotationOfElements;
	[SerializeField] private AnimationCurve introCurve;
	[SerializeField] private AnimationCurve outroCurve;
	private float introDuration;
	private float outroDuration;
	[Space(20)]
	[SerializeField] private float firstLineHeightInPixels;
	[SerializeField] private float normalLineHeightInPixels;
	[SerializeField] private float firsthalfLineHeightInPixels;
	private List<List<SpriteRenderer>> bubbleList = new();


	public void StartUp(float duration)
	{
		if(isFirst && startSpawnPointFirst != null) { startSpawnPointFirst.position = startSpawnPoint.position; }
		introDuration = duration;
		hCM = HighlightColorManager.instance;
		dataManager = DataManager.instance;
		string serverString = "";
		if (isBaseDef) 
		{ 
			baseDef = GetComponent<BaseDefinition>();
			serverString = baseDef.results;
		}
		else if (isInput || isOwnDef)
		{
			serverString = dataManager.currentSaveData.gameResult;
		}

		//interpret result string
		if(!serverString.Contains('s') || !serverString.Contains('-')){ Debug.Log("result string is invalid"); return; }
		resultScore = serverString.Split('s')[1];
		resultBubbles = serverString.Split('s')[2];

		//spawn all bubbles
		string[] bubbles = resultBubbles.Split('-');
		Vector3 spawnPos = startSpawnPoint.position;
		for (int i = 1; i < bubbles.Length; i++)
		{
			List<SpriteRenderer> bubbleComponents = new();
			bool botDominant = bubbles[i].Split('b').Length >= bubbles[i].Split('t').Length ? true : false;
			if ((spawnPos.y - bubbles[i].Length * elementWidthAtScale1 * elementScale) < startSpawnPoint.position.y - maxElementsPerColumn * elementWidthAtScale1 * elementScale)
			{
				spawnPos = new Vector3(spawnPos.x + spaceBetweenBubbleXPositions, startSpawnPoint.position.y, 0);
			}

			for (int j = 0; j < bubbles[i].Length; j++)
			{
				GameObject spawnPrefab = middleEmpty;
				if (j == 0) { spawnPrefab = botDominant ? leftEdgeEmpty : leftEdgeEmptyTriangle; }
				else if (j == bubbles[i].Length - 1) { spawnPrefab = botDominant ? rightEdgeEmptyTriangle : rightEdgeEmpty; }

				GameObject GO = Instantiate(spawnPrefab, spawnPos, Quaternion.Euler(rotationOfElements), transform);
				GO.transform.localScale = Vector3.one * elementScale;
				SpriteRenderer[] SR = GO.GetComponentsInChildren<SpriteRenderer>();
				for (int s = 0; s < SR.Length; s++)
				{
					SR[s].color = bubbles[i][j] == 'b' ? new Color(hCM.highlightColorList[0].r, hCM.highlightColorList[0].g, hCM.highlightColorList[0].b, isInput ? elementTransperancy : 0) : new Color(hCM.highlightColorList[1].r, hCM.highlightColorList[1].g, hCM.highlightColorList[1].b, isInput ? elementTransperancy : 0);
					bubbleComponents.Add(SR[s]);
				}

				spawnPos += new Vector3(0, -elementWidthAtScale1 * elementScale, 0);
			}
			bubbleList.Add(bubbleComponents);
			spawnPos += new Vector3(0, -elementWidthAtScale1 * elementScale, 0);
		}

		if (!isInput)
		{
			//set score
			scoreText.color = new Color(scoreText.color.r, scoreText.color.g, scoreText.color.b, 0);
			scoreText.text = resultScore;
			//calc score position
			definitionText.ForceMeshUpdate();
			float yTopDef = definitionText.GetComponent<RectTransform>().rect.max.y + definitionText.GetComponent<RectTransform>().anchoredPosition.y; Debug.LogWarning($"yTopDef = {yTopDef}");
			RectTransform rtScore = scoreText.GetComponent<RectTransform>();
			rtScore.anchoredPosition = new Vector2(rtScore.anchoredPosition.x, yTopDef - firstLineHeightInPixels - (definitionText.textInfo.lineCount - 2) * normalLineHeightInPixels - firsthalfLineHeightInPixels - rtScore.rect.height / 2);
			Debug.LogWarning($"yPosScoreText = {yTopDef} - {firstLineHeightInPixels} - {definitionText.textInfo.lineCount} * {normalLineHeightInPixels} - {firsthalfLineHeightInPixels} - {rtScore.rect.height / 2}");
			//animate text in
			StartCoroutine(FadeTMPROin(scoreText));
		}

		//animate bubbles in
		if (!isInput)
		{
			for (int i = 0; i < bubbleList.Count; i++)
			{
				for (int j = 0; j < bubbleList[i].Count; j++)
				{
					StartCoroutine(FadeBubbleIn(bubbleList[i][j]));
				}
			}
		}
	}

	public void CloseDown(float duration)
	{
		outroDuration = duration;
		//animate bubbles out
		for (int i = 0; i < bubbleList.Count; i++)
		{
			for (int j = 0; j < bubbleList[i].Count; j++)
			{
				StartCoroutine(FadeBubbleOut(bubbleList[i][j]));
			}
		}

		//animate text out
		StartCoroutine(FadeTMPROout(scoreText));
	}

	private IEnumerator FadeBubbleIn(SpriteRenderer sprite)
	{
		float TimeValue = 0;
		Color OriginalColor = sprite.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, elementTransperancy);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / introDuration;
			float EvaluatedTimeValue = introCurve.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			sprite.color = NewCol;

			yield return null;
		}
	}

	private IEnumerator FadeBubbleOut(SpriteRenderer sprite)
	{
		float TimeValue = 0;
		Color OriginalColor = sprite.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 0f);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / outroDuration;
			float EvaluatedTimeValue = outroCurve.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			sprite.color = NewCol;

			yield return null;
		}
	}

	private IEnumerator FadeTMPROin(TextMeshProUGUI tmpro)
	{
		float TimeValue = 0;
		Color OriginalColor = tmpro.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 1);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / introDuration;
			float EvaluatedTimeValue = introCurve.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			tmpro.color = NewCol;

			yield return null;
		}
	}

	private IEnumerator FadeTMPROout(TextMeshProUGUI tmpro)
	{
		float TimeValue = 0;
		Color OriginalColor = tmpro.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 0);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / outroDuration;
			float EvaluatedTimeValue = introCurve.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			tmpro.color = NewCol;

			yield return null;
		}
	}
}
