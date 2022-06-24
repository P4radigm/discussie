using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class BaseMenu : MonoBehaviour
{
	[SerializeField] private UnityEvent onActivated;
	[SerializeField] private UnityEvent onAnimatedIn;
	[SerializeField] private UnityEvent onAnimateOut;
	[SerializeField] private UnityEvent onDeActivated;

	private GraphicRaycaster raycaster;
	private SequenceManager sM;
	private PlatformManager pM;
	private List<TextMeshProUGUI> texts = new();
	private List<float> textsOpacities = new();
	private List<Image> images = new();
	private List<float> imagesOpacities = new();

	[SerializeField] private RectTransform masterPivot;
	[SerializeField] private Button startButton;
	[SerializeField] private Button artistButton;
	[SerializeField] private Button artistMenuButton;
	[SerializeField] private Button websiteButton;
	[SerializeField] private Button infoButton;
	[SerializeField] private Button infoMenuButton;
	[SerializeField] private Button sysOneButton;
	[SerializeField] private Button sysTwoButton;
	[SerializeField] private Button sysThreeButton;
	private Vector2 artistScreenPos;
	private Vector2 infoScreenPos;
	private Vector2 menuScreenPos;

	[Space(40)]
	[SerializeField] private float fadeInDuration;
	[SerializeField] private AnimationCurve fadeInCurve;
	private Coroutine fadeInRoutine;
	[SerializeField] private float fadeOutDuration;
	[SerializeField] private AnimationCurve fadeOutCurve;
	private Coroutine fadeOutRoutine;
	[Space(20)]
	[SerializeField] private float positionChangeDuration;
	[SerializeField] private AnimationCurve positionChangeCurve;
	private Coroutine positionChangeRoutine;
	[Space(20)]
	[SerializeField] private float buttonChangeDuration;
	[SerializeField] private AnimationCurve buttonChangeCurve;
	[SerializeField] private float backgroundImageOpacity;
	[SerializeField] private float textOpacity;
	[Space(20)]
	[SerializeField] private List<TextMeshProUGUI> filteredOutTexts;
	[SerializeField] private List<Image> filteredOutImages;


	public void StartUp()
	{
		sM = SequenceManager.instance;
		pM = PlatformManager.instance;
		raycaster = GetComponentInChildren<GraphicRaycaster>();

		raycaster.enabled = false;

		//Setup menu positions
		artistScreenPos = new Vector2(920, 920);
		infoScreenPos = new Vector2(-920, -920);
		menuScreenPos = Vector2.zero;

		TextMeshProUGUI[] unfilteredTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
		Image[] unfilteredImages = GetComponentsInChildren<Image>(true);

		for (int i = 0; i < unfilteredTexts.Length; i++)
		{
			if (!filteredOutTexts.Contains(unfilteredTexts[i])) { texts.Add(unfilteredTexts[i]); }
		}

		for (int i = 0; i < unfilteredImages.Length; i++)
		{
			if (!filteredOutImages.Contains(unfilteredImages[i])) { images.Add(unfilteredImages[i]); }
		}

		//Store all transperancy values and set all graphics invisible
		for (int i = 0; i < texts.Count; i++)
		{
			textsOpacities.Add(texts[i].color.a);
			texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, 0);
		}
		for (int i = 0; i < images.Count; i++)
		{
			imagesOpacities.Add(images[i].color.a);
			images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, 0);
		}

		onActivated.Invoke();

		//Animate in
		if (fadeInRoutine == null)
		{
			fadeInRoutine = StartCoroutine(AnimateIn());
		}
	}

	public void PressArtist()
	{
		//Animate to artist screen
		if(positionChangeRoutine == null) 
		{
			websiteButton.GetComponent<HintBlink>().Activate();
			StartCoroutine(AnimateToMenuPosition(artistScreenPos));
			StartCoroutine(AnimateChangeButton(artistMenuButton, artistButton));
		}
	}

	public void PressInfo()
	{
		//Animate to info screen
		if (positionChangeRoutine == null)
		{
			sysOneButton.GetComponent<HintBlink>().Activate();
			sysTwoButton.GetComponent<HintBlink>().Activate();
			sysThreeButton.GetComponent<HintBlink>().Activate();
			StartCoroutine(AnimateToMenuPosition(infoScreenPos));
			StartCoroutine(AnimateChangeButton(infoMenuButton, infoButton));
		}
	}

	public void PressMenuFromInfo()
	{
		//Animate to menu screen
		if (positionChangeRoutine == null)
		{
			sysOneButton.GetComponent<HintBlink>().DeActivate();
			sysTwoButton.GetComponent<HintBlink>().DeActivate();
			sysThreeButton.GetComponent<HintBlink>().DeActivate();
			StartCoroutine(AnimateToMenuPosition(menuScreenPos));
			StartCoroutine(AnimateChangeButton(infoButton, infoMenuButton));
		}
	}

	public void PressMenuFromArtist()
	{
		//Animate to menu screen
		if (positionChangeRoutine == null)
		{
			websiteButton.GetComponent<HintBlink>().DeActivate();
			StartCoroutine(AnimateToMenuPosition(menuScreenPos));
			StartCoroutine(AnimateChangeButton(artistButton, artistMenuButton));
		}
	}

	public void PressStart()
	{
		onAnimateOut.Invoke();
		raycaster.enabled = false;

		//Animate out
		if (fadeOutRoutine == null)
		{
			fadeOutRoutine = StartCoroutine(AnimateOut());
		}
	}

	public void PressWebsite()
	{
		string website = "https://studenthome.hku.nl/~leon.vanoldenborgh/";
		Application.OpenURL(website);
	}

	public void PressConformeren()
	{
		string storelink = pM.android ? "https://studenthome.hku.nl/~leon.vanoldenborgh/AndroidRedirectConformeren" : "https://studenthome.hku.nl/~leon.vanoldenborgh/AppleRedirectConformeren";
		Application.OpenURL(storelink);
	}

	public void PressDiscussie()
	{
		string storelink = pM.android ? "https://studenthome.hku.nl/~leon.vanoldenborgh/AndroidRedirectDiscussie" : "https://studenthome.hku.nl/~leon.vanoldenborgh/AppleRedirectDiscussie";
		Application.OpenURL(storelink);
	}

	public void PressUitzoeken()
	{
		string storelink = pM.android ? "https://studenthome.hku.nl/~leon.vanoldenborgh/AndroidRedirectUitzoeken" : "https://studenthome.hku.nl/~leon.vanoldenborgh/AppleRedirectUitzoeken";
		Application.OpenURL(storelink);
	}

	private IEnumerator AnimateToMenuPosition(Vector2 newScreenPosition)
	{
		raycaster.enabled = false;

		//Change anchoring, without moving the image
		float currentLeftAnchoredPos = masterPivot.offsetMin.x;
		float currentRightAnchoredPos = masterPivot.offsetMax.x;
		//Get position
		float TimeValueOut = 0;
		while (TimeValueOut < 1)
		{
			TimeValueOut += Time.deltaTime / positionChangeDuration;
			float EvaluatedTimeValueOut = positionChangeCurve.Evaluate(TimeValueOut);
			float NewLeftAnchoredPos = Mathf.Lerp(currentLeftAnchoredPos, newScreenPosition.x, EvaluatedTimeValueOut);
			float NewRightAnchoredPos = Mathf.Lerp(currentRightAnchoredPos, newScreenPosition.y, EvaluatedTimeValueOut);

			masterPivot.offsetMin = new Vector2(NewLeftAnchoredPos, masterPivot.offsetMin.y);
			masterPivot.offsetMax = new Vector2(NewRightAnchoredPos, masterPivot.offsetMax.y);

			yield return null;
		}
		raycaster.enabled = true;
		positionChangeRoutine = null;
	}

	private IEnumerator AnimateChangeButton(Button newButton, Button oldButton)
	{
		oldButton.interactable = false;
		oldButton.GetComponent<HintBlink>().DeActivate();
		TextMeshProUGUI[] newTexts = newButton.GetComponentsInChildren<TextMeshProUGUI>();
		Image[] newTmages = newButton.GetComponentsInChildren<Image>();
		Image newOwnImage = newButton.GetComponent<Image>();
		TextMeshProUGUI[] oldTexts = oldButton.GetComponentsInChildren<TextMeshProUGUI>();
		Image[] oldTmages = oldButton.GetComponentsInChildren<Image>();
		Image oldOwnImage = oldButton.GetComponent<Image>();
		oldOwnImage.raycastTarget = false;
		newButton.gameObject.SetActive(true);

		float TimeValueOut = 0;
		while (TimeValueOut < 1)
		{
			TimeValueOut += Time.deltaTime / positionChangeDuration;
			float EvaluatedTimeValueOut = positionChangeCurve.Evaluate(TimeValueOut);
			

			for (int i = 0; i < newTexts.Length; i++)
			{
				newTexts[i].color = new Color(newTexts[i].color.r, newTexts[i].color.g, newTexts[i].color.b, EvaluatedTimeValueOut * textOpacity);
			}
			for (int i = 0; i < newTmages.Length; i++)
			{
				if(newTmages[i] != newOwnImage) { newTmages[i].color = new Color(newTmages[i].color.r, newTmages[i].color.g, newTmages[i].color.b, EvaluatedTimeValueOut * backgroundImageOpacity); }
			}
			for (int i = 0; i < oldTexts.Length; i++)
			{
				oldTexts[i].color = new Color(oldTexts[i].color.r, oldTexts[i].color.g, oldTexts[i].color.b, (1 - EvaluatedTimeValueOut) * textOpacity);
			}
			for (int i = 0; i < oldTmages.Length; i++)
			{
				if (oldTmages[i] != oldOwnImage) { oldTmages[i].color = new Color(oldTmages[i].color.r, oldTmages[i].color.g, oldTmages[i].color.b, (1 - EvaluatedTimeValueOut) * backgroundImageOpacity); }
			}

			yield return null;
		}
		newButton.interactable = true;
		newOwnImage.raycastTarget = true;
		newButton.GetComponent<HintBlink>().Activate();
		oldButton.gameObject.SetActive(false);
	}

	private IEnumerator AnimateIn()
	{
		float _timeValue = 0;
		float[] _startTextOpacities = new float[texts.Count];
		for (int i = 0; i < texts.Count; i++)
		{
			_startTextOpacities[i] = texts[i].color.a;
		}
		float[] _startImageOpacities = new float[images.Count];
		for (int i = 0; i < images.Count; i++)
		{
			_startImageOpacities[i] = images[i].color.a;
		}

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / fadeInDuration;
			float _evaluatedTimeValue = fadeInCurve.Evaluate(_timeValue);

			for (int i = 0; i < texts.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(_startTextOpacities[i], textsOpacities[i], _evaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(_startImageOpacities[i], imagesOpacities[i], _evaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}


			yield return null;
		}
		onAnimatedIn.Invoke();
		raycaster.enabled = true;
		fadeInRoutine = null;
		startButton.GetComponent<HintBlink>().Activate();
		infoButton.GetComponent<HintBlink>().Activate();
		artistButton.GetComponent<HintBlink>().Activate();
	}

	private IEnumerator AnimateOut()
	{
		for (int i = 0; i < filteredOutTexts.Count; i++)
		{
			texts.Add(filteredOutTexts[i]);
		}
		for (int i = 0; i < filteredOutImages.Count; i++)
		{
			images.Add(filteredOutImages[i]);
		}
		float _timeValue = 0;
		float[] _startTextOpacities = new float[texts.Count];
		for (int i = 0; i < texts.Count; i++)
		{
			_startTextOpacities[i] = texts[i].color.a;
		}
		float[] _startImageOpacities = new float[images.Count];
		for (int i = 0; i < images.Count; i++)
		{
			_startImageOpacities[i] = images[i].color.a;
		}

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / fadeOutDuration;
			float _evaluatedTimeValue = fadeOutCurve.Evaluate(_timeValue);

			for (int i = 0; i < texts.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(_startTextOpacities[i], 0, _evaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(_startImageOpacities[i], 0, _evaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}


			yield return null;
		}
		onDeActivated.Invoke();
		sM.AddToGameState();
		startButton.GetComponent<HintBlink>().DeActivate();
		infoButton.GetComponent<HintBlink>().DeActivate();
		artistButton.GetComponent<HintBlink>().DeActivate();
		artistMenuButton.GetComponent<HintBlink>().DeActivate();
		infoMenuButton.GetComponent<HintBlink>().DeActivate();
		fadeOutRoutine = null;
	}
}
