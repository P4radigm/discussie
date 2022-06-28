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
	private bool animateLDinit = true;
	private bool animateLD = false;
	private float animateLDtimeValue = 0;

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
	private bool animateSvPinit = true;
	private bool animateSvP = false;
	private float animateSvPtimeValue = 0;
	private RectTransform RT;
	private Vector2 BotAnchoredPos;

	private bool LDtakesLonger = false;

	public void StartUp()
	{
		sM = SequenceManager.instance;
		pM = PlatformManager.instance;
		LDtakesLonger = animateInDelayLD + animateInDurationLD + displayTimeLD + animateOutDurationLD > animateInDelaySvP + animateInDurationSvP + displayTimeSvP + animateOutDurationSvP;

		RT = imageSvP.GetComponent<RectTransform>();

		//Animate
		if (!animateLD) { animateLD = true; }
		if (!animateSvP) { animateSvP = true; }	
	}

	private void Update()
	{
		AnimSvP();
		AnimLD();
	}

	private void AnimSvP()
	{
		if (!animateSvP) { return; }

		if (animateSvPinit)
		{
			animateSvPtimeValue = 0;
			
			//Change anchoring, without moving the image
			Vector2 MidWorldPos = RT.position;
			RT.anchorMax = new Vector2(0.5f, 0f);
			RT.anchorMin = new Vector2(0.5f, 0f);
			RT.position = MidWorldPos;
			BotAnchoredPos = RT.anchoredPosition;
			animateSvPinit = false;
		}

		if(animateSvPtimeValue > 0 && animateSvPtimeValue <= animateInDelaySvP)
		{
			//Wait for potential delay
		}
		else if(animateSvPtimeValue > animateInDelaySvP && animateSvPtimeValue <= animateInDelaySvP + animateInDurationSvP)
		{
			//Fade in
			float EvaluatedTimeValueIn = animateInCurveSvP.Evaluate((animateSvPtimeValue - (animateInDelaySvP)) / animateInDurationSvP);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueIn);
			imageSvP.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);
		}
		else if(animateSvPtimeValue > animateInDelaySvP + animateInDurationSvP && animateSvPtimeValue <= animateInDelaySvP + animateInDurationSvP + displayTimeSvP)
		{
			//Display
		}
		else if(animateSvPtimeValue > animateInDelaySvP + animateInDurationSvP + displayTimeSvP && animateSvPtimeValue <= animateInDelaySvP + animateInDurationSvP + displayTimeSvP + animateOutDurationSvP)
		{
			float EvaluatedTimeValueOut = animateOutCurveSvP.Evaluate((animateSvPtimeValue - (animateInDelaySvP + animateInDurationSvP + displayTimeSvP)) / animateOutDurationSvP);
			Vector2 NewPos = Vector2.Lerp(BotAnchoredPos, bottomAnchoredFinalPosition, EvaluatedTimeValueOut);

			RT.anchoredPosition = NewPos;
		}
		else if(animateSvPtimeValue > animateInDelaySvP + animateInDurationSvP + displayTimeSvP + animateOutDurationSvP && !LDtakesLonger)
		{
			sM.AddToGameState();
			animateSvP = false;
			animateSvPinit = true;
		}

		animateSvPtimeValue += Time.deltaTime;
		Mathf.Clamp(animateSvPtimeValue, 0, animateInDelaySvP + animateInDurationSvP + displayTimeSvP + animateOutDurationSvP + 0.01f);
	}

	private void AnimLD()
	{
		if (!animateLD) { return; }

		if (animateLDinit)
		{
			animateLDtimeValue = 0;
			animateLDinit = false;
		}

		if(animateLDtimeValue > 0 && animateLDtimeValue <= animateInDelayLD)
		{
			//Wait for potential delay
		}
		else if(animateLDtimeValue > animateInDelayLD && animateLDtimeValue <= animateInDelayLD + animateInDurationLD)
		{
			//Fade in
			float EvaluatedTimeValueIn = animateInCurveLD.Evaluate((animateLDtimeValue - (animateInDelayLD)) / animateInDurationLD);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueIn);
			imageLD.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);
		}
		else if(animateLDtimeValue > animateInDelayLD + animateInDurationLD && animateLDtimeValue <= animateInDelayLD + animateInDurationLD + displayTimeLD)
		{
			//Display
		}
		else if(animateLDtimeValue > animateInDelayLD + animateInDurationLD + displayTimeLD && animateLDtimeValue <= animateInDelayLD + animateInDurationLD + displayTimeLD + animateOutDurationLD)
		{
			//Fade out
			float EvaluatedTimeValueOut = animateOutCurveLD.Evaluate((animateLDtimeValue - (animateInDelayLD + animateInDurationLD + displayTimeLD)) / animateOutDurationLD);

			float NewOpacity = Mathf.Lerp(1, 0, EvaluatedTimeValueOut);
			imageLD.color = new Color(imageLD.color.r, imageLD.color.g, imageLD.color.b, NewOpacity);
		}
		else if(animateLDtimeValue > animateInDelayLD + animateInDurationLD + displayTimeLD + animateOutDurationLD && LDtakesLonger)
		{
			sM.AddToGameState();
			animateLD = false;
			animateLDinit = true;
		}

		animateLDtimeValue += Time.deltaTime;
		Mathf.Clamp(animateLDtimeValue, 0, animateInDelayLD + animateInDurationLD + displayTimeLD + animateOutDurationLD + 0.01f);
	}
}
