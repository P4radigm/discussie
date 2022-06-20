using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DISscoreResultManager : MonoBehaviour
{
	private DISgameplayManager gameplayManager;
	private DISgameplaySettings settings;
	private SequenceManager sequenceManager;
	private BaseGameplay baseGameplay;
	private HighlightColorManager hCM;
	[SerializeField] private bool isPlayA = false;
	[SerializeField] private bool isPlayB = false;
	[SerializeField] private bool isPlayC = false;
	[Space(20)]
	[SerializeField] private GameObject leftEdgeEmpty;
	[SerializeField] private GameObject leftEdgeEmptyTriangle;
	[SerializeField] private GameObject middleEmpty;
	[SerializeField] private GameObject rightEdgeEmpty;
	[SerializeField] private GameObject rightEdgeEmptyTriangle;
	[Space(20)]
	[SerializeField] private Transform startSpawnPoint;
	[SerializeField] private float spaceBetweenBubbleXPositions;
	[SerializeField] private float elementScale;
	[SerializeField] private float elementWidthAtScale1;
	[SerializeField] private int maxElementsPerColumn;
	[SerializeField] private Vector3 rotationOfElements;
	[Space(20)]
	[SerializeField] private float displayTime;
	[Space(20)]
	[SerializeField] private float appearTimePerElement;
	[SerializeField] private float textAppearTimeMultiplier;
	[SerializeField] private AnimationCurve appearCurvePerElement;
	[Space(20)]
	[SerializeField] private float disappearTimePerElement;
	[SerializeField] private AnimationCurve disappearCurvePerElement;
	[Space(20)]
	[SerializeField] private float fadeOutTime;
	[SerializeField] private AnimationCurve fadeOutCurve;
	[Space(20)]
	[SerializeField][Range(0,1)] private float elementTransperancy;
	[SerializeField] private TextMeshProUGUI botScore;
	[SerializeField] private TextMeshProUGUI botScoreText;
	[SerializeField] private TextMeshProUGUI topScore;
	[SerializeField] private TextMeshProUGUI topScoreText;
	[SerializeField] private TextMeshProUGUI difference;
	[SerializeField] private TextMeshProUGUI differenceText;
	[SerializeField] private TextMeshProUGUI totalScore;
	[SerializeField] private TextMeshProUGUI totalText;
	private List<List<SpriteRenderer>> bubbleList = new();

	public void SetText(int bScore, int tScore, int diff, int tot)
	{
		hCM = HighlightColorManager.instance;
		sequenceManager = SequenceManager.instance;
		gameplayManager = GetComponent<DISgameplayManager>();
		settings = GetComponent<DISgameplaySettings>();
		baseGameplay = GetComponent<BaseGameplay>();

		botScore.text = bScore.ToString();
		topScore.text = tScore.ToString();
		if(bScore >= tScore)
		{
			difference.text = $"<color=#{ColorUtility.ToHtmlStringRGB(hCM.highlightColorList[0])}>{bScore}</color><color=#000000> - </color><color=#{ColorUtility.ToHtmlStringRGB(hCM.highlightColorList[1])}>{tScore}</color><color=#000000> = </color>{diff}";
		}
		else
		{
			difference.text = $"<color=#{ColorUtility.ToHtmlStringRGB(hCM.highlightColorList[1])}>{tScore}</color><color=#000000> - </color><color=#{ColorUtility.ToHtmlStringRGB(hCM.highlightColorList[0])}>{bScore}</color><color=#000000> = </color>{diff}";
		}

		totalScore.text = isPlayA ? bScore.ToString() : tot.ToString();
	}

	public IEnumerator ResultSequence()
	{
		yield return StartCoroutine(SpawnBubbles());

		StartCoroutine(FadeTMPROin(botScoreText));
		yield return StartCoroutine(FadeTMPROin(botScore));

		if (!isPlayA)
		{
			StartCoroutine(FadeTMPROin(topScoreText));
			yield return StartCoroutine(FadeTMPROin(topScore));

			StartCoroutine(FadeTMPROin(differenceText));
			yield return StartCoroutine(FadeTMPROin(difference));
		}

		StartCoroutine(FadeTMPROin(totalText));
		yield return StartCoroutine(FadeTMPROin(totalScore));

		yield return new WaitForSeconds(displayTime);

		for (int i = 0; i < bubbleList.Count; i++)
		{
			for (int j = 0; j < bubbleList[i].Count; j++)
			{
				StartCoroutine(FadeBubbleOut(bubbleList[i][j]));
			}
		}

		StartCoroutine(FadeTMPROout(botScoreText));
		StartCoroutine(FadeTMPROout(botScore));
		StartCoroutine(FadeTMPROout(topScoreText));
		StartCoroutine(FadeTMPROout(topScore));
		StartCoroutine(FadeTMPROout(differenceText));
		StartCoroutine(FadeTMPROout(difference));
		StartCoroutine(FadeTMPROout(totalText));
		StartCoroutine(FadeTMPROout(botScoreText));
		yield return StartCoroutine(FadeTMPROout(totalScore));
		baseGameplay.StartCloseDown();
	}

	private IEnumerator SpawnBubbles()
	{
		string[] bubbles = gameplayManager.resultString.Split('-');
		Vector3 spawnPos = startSpawnPoint.position;
		for (int i = 1; i < bubbles.Length; i++)
		{
			List<SpriteRenderer> bubbleComponents = new();
			bool botDominant = bubbles[i].Split('b').Length >= bubbles[i].Split('t').Length ? true : false;
			if((spawnPos.y - bubbles[i].Length * elementWidthAtScale1 * elementScale) < startSpawnPoint.position.y - maxElementsPerColumn * elementWidthAtScale1 * elementScale)
			{
				spawnPos = new Vector3(spawnPos.x + spaceBetweenBubbleXPositions, startSpawnPoint.position.y, 0);
			}

			for (int j = 0; j < bubbles[i].Length; j++)
			{
				GameObject spawnPrefab = middleEmpty;
				if(j == 0) { spawnPrefab = botDominant ? leftEdgeEmpty : leftEdgeEmptyTriangle; }
				else if(j == bubbles[i].Length - 1) { spawnPrefab = botDominant ? rightEdgeEmptyTriangle : rightEdgeEmpty; }

				GameObject GO = Instantiate(spawnPrefab, spawnPos, Quaternion.Euler(rotationOfElements), transform);
				GO.transform.localScale = Vector3.one * elementScale;
				SpriteRenderer[] SR = GO.GetComponentsInChildren<SpriteRenderer>();
				for (int s = 0; s < SR.Length; s++)
				{
					SR[s].color = bubbles[i][j] == 'b' ? new Color(hCM.highlightColorList[0].r, hCM.highlightColorList[0].g, hCM.highlightColorList[0].b, 0) : new Color(hCM.highlightColorList[1].r, hCM.highlightColorList[1].g, hCM.highlightColorList[1].b, 0);
					bubbleComponents.Add(SR[s]);
				}
				
				spawnPos += new Vector3(0, -elementWidthAtScale1 * elementScale, 0);
			}
			bubbleList.Add(bubbleComponents);
			spawnPos += new Vector3(0, -elementWidthAtScale1 * elementScale, 0);
		}

		for (int i = 0; i < bubbleList.Count; i++)
		{
			for (int j = 0; j < bubbleList[i].Count; j++)
			{
				StartCoroutine(FadeBubbleIn(bubbleList[i][j]));
			}
			yield return new WaitForSeconds(appearTimePerElement);
		}
	}

	private IEnumerator FadeTMPROin(TextMeshProUGUI tmpro)
	{
		float TimeValue = 0;
		Color OriginalColor = tmpro.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 1);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / (appearTimePerElement * textAppearTimeMultiplier);
			float EvaluatedTimeValue = appearCurvePerElement.Evaluate(TimeValue);
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
			TimeValue += Time.deltaTime / fadeOutTime;
			float EvaluatedTimeValue = fadeOutCurve.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			tmpro.color = NewCol;

			yield return null;
		}
	}

	private IEnumerator FadeBubbleIn(SpriteRenderer sprite)
	{
		float TimeValue = 0;
		Color OriginalColor = sprite.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, elementTransperancy);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / appearTimePerElement;
			float EvaluatedTimeValue = appearCurvePerElement.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			sprite.color = NewCol;

			yield return null;
		}
	}

	private IEnumerator FadeBubbleOut(SpriteRenderer sprite)
	{
		float TimeValue = 0;
		Color OriginalColor = sprite.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, isPlayC ? 0.25f : 0f);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / disappearTimePerElement;
			float EvaluatedTimeValue = disappearCurvePerElement.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			sprite.color = NewCol;

			yield return null;
		}
	}

	private IEnumerator AnimateScorePlayC(TextMeshProUGUI tmpro)
	{
		float TimeValue = 0;
		Color OriginalColor = tmpro.color;
		Color NewColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 0.5f);

		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / appearTimePerElement;
			float EvaluatedTimeValue = appearCurvePerElement.Evaluate(TimeValue);
			Color NewCol = Color.Lerp(OriginalColor, NewColor, EvaluatedTimeValue);
			tmpro.color = NewCol;

			yield return null;
		}
	}
}
