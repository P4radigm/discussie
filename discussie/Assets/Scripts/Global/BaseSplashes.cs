using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseSplashes : MonoBehaviour
{
	private SequenceManager sM;
	private PlatformManager pM;

	[Header("Leonardo Design Anim")]
	[SerializeField] private Image imageLD;
	[Space(10)]
	[SerializeField] private float animateInDelayLD;
	[SerializeField] private float animateInDurationLD;
	[SerializeField] private AnimationCurve animateInCurveLD;
	[Space(10)]
	[SerializeField] private float displayTimeLD;
	[Space(10)]
	[SerializeField] private float animateOutDurationLD;
	[SerializeField] private AnimationCurve animateOutCurveLD;

	[Header("Systemen van Polarisatie Anim")]
	[SerializeField] private Image imageSvP;
	[Space(10)]
	[SerializeField] private float animateInDelaySvP;
	[SerializeField] private float animateInDurationSvP;
	[SerializeField] private AnimationCurve animateInCurveSvP;
	[Space(10)]
	[SerializeField] private float displayTimeSvP;
	[Space(10)]
	[SerializeField] private float animateOutDurationSvP;
	[SerializeField] private AnimationCurve animateOutCurveSvP;
	[SerializeField] private Vector2 bottomAnchoredFinalPosition;
	private bool LDtakesLonger = false;

	public void StartUp()
	{
		sM = SequenceManager.instance;
		pM = PlatformManager.instance;
		LDtakesLonger = animateInDelayLD + animateInDurationLD + displayTimeLD + animateOutDurationLD > animateInDelaySvP + animateInDurationSvP + displayTimeSvP + animateOutDurationSvP;

		//Animate
		StartCoroutine(AnimateLD());
		StartCoroutine(AnimateSvP());
	}

	private IEnumerator AnimateLD()
	{
		//Wait for potential delay
		yield return new WaitForSeconds(animateInDelayLD);

		//Fade in
		float TimeValueIn = 0;
		while (TimeValueIn < 1)
		{
			TimeValueIn += Time.deltaTime / animateInDurationLD;
			float EvaluatedTimeValueIn = animateInCurveLD.Evaluate(TimeValueIn);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueIn);
			imageLD.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);

			yield return null;
		}

		//Display
		yield return new WaitForSeconds(displayTimeLD);

		//Fade Out
		float TimeValueOut = 0;
		while (TimeValueOut < 1)
		{
			TimeValueOut += Time.deltaTime / animateOutDurationLD;
			float EvaluatedTimeValueOut = animateOutCurveLD.Evaluate(TimeValueOut);

			float NewOpacity = Mathf.Lerp(1, 0, EvaluatedTimeValueOut);
			imageLD.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);

			yield return null;
		}

		if (LDtakesLonger)
		{
			sM.AddToGameState();
		}
	}

	private IEnumerator AnimateSvP()
	{
		//Wait for potential delay
		yield return new WaitForSeconds(animateInDelaySvP);

		//Fade in
		float TimeValueIn = 0;
		while (TimeValueIn < 1)
		{
			TimeValueIn += Time.deltaTime / animateInDurationSvP;
			float EvaluatedTimeValueIn = animateInCurveSvP.Evaluate(TimeValueIn);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueIn);
			imageSvP.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);

			yield return null;
		}

		//Display
		yield return new WaitForSeconds(displayTimeSvP);

		////Fade Out
		//float TimeValueOut = 0;
		//while (TimeValueOut < 1)
		//{
		//	TimeValueOut += Time.deltaTime / animateOutDurationSvP;
		//	float EvaluatedTimeValueIn = animateOutCurveSvP.Evaluate(TimeValueOut);

		//	float NewOpacity = Mathf.Lerp(1, 0, EvaluatedTimeValueIn);
		//	imageSvP.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);

		//	yield return null;
		//}

		//Animate to bottom position
		RectTransform RT = imageSvP.GetComponent<RectTransform>();
		//Change anchoring, without moving the image
		Vector2 MidWorldPos = RT.position;
		RT.anchorMax = new Vector2(0.5f, 0f);
		RT.anchorMin = new Vector2(0.5f, 0f);
		RT.position = MidWorldPos;
		Vector2 BotAnchoredPos = RT.anchoredPosition;
		//Get position
		float TimeValueOut = 0;
		while (TimeValueOut < 1)
		{
			TimeValueOut += Time.deltaTime / animateOutDurationSvP;
			float EvaluatedTimeValueOut = animateOutCurveSvP.Evaluate(TimeValueOut);
			Vector2 NewPos = Vector2.Lerp(BotAnchoredPos, bottomAnchoredFinalPosition, EvaluatedTimeValueOut);

			RT.anchoredPosition = NewPos;

			yield return null;
		}

		if (!LDtakesLonger)
		{
			sM.AddToGameState();
		}
	}

}
