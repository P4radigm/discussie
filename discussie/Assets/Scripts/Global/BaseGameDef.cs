using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class BaseGameDef : MonoBehaviour
{
	private SequenceManager sM;
	private HighlightColorManager hCM;

	[System.Serializable]
	public class GameDefinitie
	{
		public int index;
		[TextArea]
		public string definitie;
		public string auteur;
		public string boek;
		public string jaar;
	}

	[Header("Objects")]
	[SerializeField] private TextMeshProUGUI spel;
	[SerializeField] private TextMeshProUGUI het;
	private float hetOpacity;
	[SerializeField] private TextMeshProUGUI nummer;
	[SerializeField] private TextMeshProUGUI definitie;
	[SerializeField] private Button doorgaanKnop;
	[SerializeField] private Image doorgaanKnopBG;
	private float doorgaanKnopBGopacity;
	[SerializeField] private TextMeshProUGUI doorgaanKnopText;

	[Header("Scroll Animation Settings")]
	[SerializeField] private bool scroll;
	[SerializeField] private bool useEnglish;
	[SerializeField] private List<GameDefinitie> engelsDefs;
	[SerializeField] private List<GameDefinitie> nederlandsDefs;
	[SerializeField] private int endDefIndex;
	[SerializeField] private float timeBetweenScroll;
	[SerializeField] private float scrollDuration;
	private bool animateScrollInit = true;
	private bool animateScroll = false;
	private float animateScrollTimeValue = 0;
	private float newScrollTimeValue = 0;

	[Header("Fade Animation Settings")]
	[SerializeField] private float delayBeforeAnimate;
	[SerializeField] private float animateInDuration;
	[SerializeField] private float animateInDurationButton;
	[SerializeField] private AnimationCurve animateInCurve;
	private bool animateFadeInInit = true;
	private bool animateFadeIn = false;
	private float animateFadeInTimeValue = 0;
	private float extraScrollWaitTime = 0;
	[Space(10)]
	[SerializeField] private float displayDuration;
	[Space(10)]
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;
	private bool animateFadeOutInit = true;
	private bool animateFadeOut = false;
	private float animateFadeOutTimeValue = 0;

	private int returnIndex;

	public void StartUp()
	{
		sM = SequenceManager.instance;
		hCM = HighlightColorManager.instance;

		
		hetOpacity = het.color.a;
		doorgaanKnopBGopacity = doorgaanKnopBG.color.a;
		doorgaanKnop.interactable = false;
		doorgaanKnopBG.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, 0);
		doorgaanKnopText.color = new Color(doorgaanKnopText.color.r, doorgaanKnopText.color.g, doorgaanKnopText.color.b, 0);
		spel.color = new Color(spel.color.r, spel.color.g, spel.color.b, 0);
		het.color = new Color(spel.color.r, spel.color.g, spel.color.b, 0);
		nummer.color = new Color(spel.color.r, spel.color.g, spel.color.b, 0);
		definitie.color = new Color(spel.color.r, spel.color.g, spel.color.b, 0);
		returnIndex = Random.Range(0, useEnglish ? engelsDefs.Count : nederlandsDefs.Count);

		GameDefinitie endDefinition = useEnglish ? engelsDefs[endDefIndex] : nederlandsDefs[endDefIndex];
		nummer.text = $"<color=#{hCM.GetHighlightHex()}>{endDefinition.index}.</color>";
		definitie.text = ReformatDefinition(endDefinition);

		if (scroll) { extraScrollWaitTime = Mathf.Clamp(scrollDuration - animateInDuration, 0, Mathf.Infinity); }

		if (!animateFadeIn) { animateFadeIn = true; }
		if (scroll) { if (!animateScroll) { animateScroll = true; } }
	}

	private void Update()
	{
		AnimFadeIn();
		AnimScroll();
		AnimFadeOut();
	}

	private void AnimFadeIn()
	{		
		if (!animateFadeIn) { return; }

		if (animateFadeInInit)
		{
			animateFadeInTimeValue = 0;
			animateFadeInInit = false;
		}

		if (animateFadeInTimeValue > 0 && animateFadeInTimeValue <= delayBeforeAnimate)
		{
			//Wait for potential delay
		}
		else if (animateFadeInTimeValue > delayBeforeAnimate && animateFadeInTimeValue <= delayBeforeAnimate + animateInDuration)
		{
			//Fade in
			float EvaluatedTimeValueIn = animateInCurve.Evaluate((animateFadeInTimeValue - delayBeforeAnimate) / animateInDuration);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueIn);
			spel.color = new Color(spel.color.r, spel.color.g, spel.color.b, NewOpacity);
			het.color = new Color(het.color.r, het.color.g, het.color.b, NewOpacity * hetOpacity);
			nummer.color = new Color(nummer.color.r, nummer.color.g, nummer.color.b, NewOpacity);
			definitie.color = new Color(definitie.color.r, definitie.color.g, definitie.color.b, NewOpacity);
		}
		else if (animateFadeInTimeValue > delayBeforeAnimate + animateInDuration && animateFadeInTimeValue <= delayBeforeAnimate + animateInDuration + displayDuration + extraScrollWaitTime)
		{
			//Wait for scroll to finish and display
		}
		else if(animateFadeInTimeValue > delayBeforeAnimate + animateInDuration + displayDuration + extraScrollWaitTime && animateFadeInTimeValue <= delayBeforeAnimate + animateInDuration + displayDuration + extraScrollWaitTime + animateInDurationButton)
		{
			float EvaluatedTimeValueInButton = animateInCurve.Evaluate((animateFadeInTimeValue - (delayBeforeAnimate + animateInDuration + displayDuration + extraScrollWaitTime)) / animateInDurationButton);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueInButton);
			doorgaanKnopBG.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity * doorgaanKnopBGopacity);
			doorgaanKnopText.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity);
		}
		else if (animateFadeInTimeValue > delayBeforeAnimate + animateInDuration + displayDuration + extraScrollWaitTime + animateInDurationButton)
		{
			doorgaanKnop.interactable = true;
			doorgaanKnop.GetComponent<HintBlink>().Activate();
			animateFadeInInit = true;
			animateFadeIn = false;
		}

		animateFadeInTimeValue += Time.deltaTime;
		Mathf.Clamp(animateFadeInTimeValue, 0, delayBeforeAnimate + animateInDuration + displayDuration + extraScrollWaitTime + animateInDurationButton + 0.01f);
	}

	public void FadeOut()
	{
		doorgaanKnop.interactable = false;
		doorgaanKnop.GetComponent<HintBlink>().DeActivate();
		animateFadeOut = true;
		//StartCoroutine(AnimateFadeOut());
	}

	private void AnimFadeOut()
	{
		if (!animateFadeOut) { return; }

		if (animateFadeOutInit)
		{
			animateFadeOutTimeValue = 0;
			animateFadeOutInit = false;
		}

		if(animateFadeOutTimeValue > 0 && animateFadeOutTimeValue <= animateOutDuration)
		{
			float EvaluatedTimeValueOut = animateOutCurve.Evaluate(animateFadeOutTimeValue / animateOutDuration);

			float NewOpacity = Mathf.Lerp(1, 0, EvaluatedTimeValueOut);
			spel.color = new Color(spel.color.r, spel.color.g, spel.color.b, NewOpacity);
			het.color = new Color(het.color.r, het.color.g, het.color.b, NewOpacity * hetOpacity);
			nummer.color = new Color(nummer.color.r, nummer.color.g, nummer.color.b, NewOpacity);
			definitie.color = new Color(definitie.color.r, definitie.color.g, definitie.color.b, NewOpacity);
			doorgaanKnopBG.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity * doorgaanKnopBGopacity);
			doorgaanKnopText.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity);
		}
		else if(animateFadeOutTimeValue > animateOutDuration)
		{
			sM.AddToGameState();
			animateFadeOutInit = true;
			animateFadeOut = false;
		}

		animateFadeOutTimeValue += Time.deltaTime;
		Mathf.Clamp(animateFadeOutTimeValue, 0, animateOutDuration + 0.01f);

	}

	private void AnimScroll()
	{
		if (!animateScroll) { return; }

		if (animateScrollInit)
		{
			newScrollTimeValue = 0;
			animateScrollTimeValue = 0;
			animateScrollInit = false;
		}

		if (animateScrollTimeValue > 0 && animateScrollTimeValue <= scrollDuration)
		{
			if(newScrollTimeValue > timeBetweenScroll)
			{
				SetNewDefinition();
				newScrollTimeValue = 0;
			}
			newScrollTimeValue += Time.deltaTime;
		}
		else if (animateScrollTimeValue > scrollDuration)
		{
			GameDefinitie endDefinition = useEnglish ? engelsDefs[endDefIndex] : nederlandsDefs[endDefIndex];
			nummer.text = $"<color=#{hCM.GetHighlightHex()}>{endDefinition.index}.</color>";
			definitie.text = ReformatDefinition(endDefinition);

			animateScrollInit = true;
			animateScroll = false;
		}

		animateScrollTimeValue += Time.deltaTime;
		Mathf.Clamp(animateScrollTimeValue, 0, scrollDuration + 0.01f);
	}

	private void SetNewDefinition()
	{
		GameDefinitie NewDefinition = GetNewDefinition();
		nummer.text = $"<color=#{hCM.GetHighlightHex()}>{NewDefinition.index}.</color>";
		definitie.text = ReformatDefinition(NewDefinition);
	}

	private GameDefinitie GetNewDefinition()
	{
		List<int> choosableIndeces = new List<int>();
		for (int i = 0; i < (useEnglish ? engelsDefs.Count : nederlandsDefs.Count); i++)
		{
			choosableIndeces.Add(i);
		}
		choosableIndeces.RemoveAt(returnIndex);
		returnIndex = choosableIndeces[Random.Range(0, choosableIndeces.Count - 1)];
		GameDefinitie outputDef = engelsDefs[Random.Range(0, engelsDefs.Count)];
		return outputDef;
	}

	private string ReformatDefinition(GameDefinitie def)
	{
		string outputString = $"{def.definitie}{'\n'}<size=50%><align=right>Gegeven  door: <color=#{hCM.GetHighlightHex()}>{def.auteur}</color></size></align>{'\n'}<size=50%><align=right>In{(def.boek == "" ? "" : " ")}<color=#{hCM.GetHighlightHex()}><smallcaps>{def.boek}</smallcaps></color> {def.jaar}</size></align>";
		return outputString;
	}
}
