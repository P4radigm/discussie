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

	[Header("Fade Animation Settings")]
	[SerializeField] private float delayBeforeAnimate;
	[SerializeField] private float animateInDuration;
	[SerializeField] private float animateInDurationButton;
	[SerializeField] private AnimationCurve animateInCurve;
	[Space(10)]
	[SerializeField] private float displayDuration;
	[Space(10)]
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;
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

		StartCoroutine(AnimateFadeIn());
		if (scroll) { StartCoroutine(AnimateScroll()); }
	}

	private IEnumerator AnimateFadeIn()
	{
		//Wait for potential delay
		yield return new WaitForSeconds(delayBeforeAnimate);

		//Fade in
		float TimeValueIn = 0;
		while (TimeValueIn < 1)
		{
			TimeValueIn += Time.deltaTime / animateInDuration;
			float EvaluatedTimeValueIn = animateInCurve.Evaluate(TimeValueIn);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueIn);
			spel.color = new Color(spel.color.r, spel.color.g, spel.color.b, NewOpacity);
			het.color = new Color(het.color.r, het.color.g, het.color.b, NewOpacity * hetOpacity);
			nummer.color = new Color(nummer.color.r, nummer.color.g, nummer.color.b, NewOpacity);
			definitie.color = new Color(definitie.color.r, definitie.color.g, definitie.color.b, NewOpacity);


			yield return null;
		}

		//Display
		if (scroll) { yield return new WaitForSeconds(Mathf.Clamp(scrollDuration - animateInDuration, 0, Mathf.Infinity)); }
		yield return new WaitForSeconds(displayDuration);	

		float TimeValueInButton = 0;
		while (TimeValueInButton < 1)
		{
			TimeValueInButton += Time.deltaTime / animateInDuration;
			float EvaluatedTimeValueInButton = animateInCurve.Evaluate(TimeValueInButton);

			float NewOpacity = Mathf.Lerp(0, 1, EvaluatedTimeValueInButton);
			doorgaanKnopBG.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity * doorgaanKnopBGopacity);
			doorgaanKnopText.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity);

			yield return null;
		}
		doorgaanKnop.interactable = true;
		doorgaanKnop.GetComponent<HintBlink>().Activate();
	}

	public void FadeOut()
	{
		doorgaanKnop.interactable = false;
		doorgaanKnop.GetComponent<HintBlink>().DeActivate();
		StartCoroutine(AnimateFadeOut());
	}

	private IEnumerator AnimateFadeOut()
	{
		float TimeValueOut = 0;
		while (TimeValueOut < 1)
		{
			TimeValueOut += Time.deltaTime / animateOutDuration;
			float EvaluatedTimeValueOut = animateOutCurve.Evaluate(TimeValueOut);

			float NewOpacity = Mathf.Lerp(1, 0, EvaluatedTimeValueOut);
			spel.color = new Color(spel.color.r, spel.color.g, spel.color.b, NewOpacity);
			het.color = new Color(het.color.r, het.color.g, het.color.b, NewOpacity * hetOpacity);
			nummer.color = new Color(nummer.color.r, nummer.color.g, nummer.color.b, NewOpacity);
			definitie.color = new Color(definitie.color.r, definitie.color.g, definitie.color.b, NewOpacity);
			doorgaanKnopBG.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity * doorgaanKnopBGopacity);
			doorgaanKnopText.color = new Color(doorgaanKnopBG.color.r, doorgaanKnopBG.color.g, doorgaanKnopBG.color.b, NewOpacity);

			yield return null;
		}


		sM.AddToGameState();
	}

	private IEnumerator AnimateScroll()
	{
		float TimeValue = 0;
		while (TimeValue < scrollDuration)
		{
			SetNewDefinition();
			TimeValue += timeBetweenScroll;
			yield return new WaitForSeconds(timeBetweenScroll);
		}

		GameDefinitie endDefinition = useEnglish ? engelsDefs[endDefIndex] : nederlandsDefs[endDefIndex];
		nummer.text = $"<color=#{hCM.GetHighlightHex()}>{endDefinition.index}.</color>";
		definitie.text = ReformatDefinition(endDefinition);
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
