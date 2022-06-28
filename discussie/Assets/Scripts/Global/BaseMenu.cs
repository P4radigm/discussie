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
	private bool animateFadeInInit = true;
	private bool animateFadeIn = false;
	private float animateFadeInTimeValue = 0;
	private List<float> startTextOpacities = new();
	private List<float> startImageOpacities = new();

	[SerializeField] private float fadeOutDuration;
	[SerializeField] private AnimationCurve fadeOutCurve;
	private bool animateFadeOutInit = true;
	private bool animateFadeOut = false;
	private float animateFadeOutTimeValue = 0;
	[Space(20)]
	[SerializeField] private float positionChangeDuration;
	[SerializeField] private AnimationCurve positionChangeCurve;
	private bool animatePositionInit = true;
	private bool animatePosition = false;
	private float animatePositionTimeValue = 0;
	private Vector2 oldPosition;
	private Vector2 targetPosition;
	[Space(20)]
	[SerializeField] private float buttonChangeDuration;
	[SerializeField] private AnimationCurve buttonChangeCurve;
	[SerializeField] private float backgroundImageOpacity;
	[SerializeField] private float textOpacity;
	private bool animateChangeButtonInit = true;
	private bool animateChangeButton = false;
	private float animateChangeButtonTimeValue = 0;
	private Button newButton;
	private List<TextMeshProUGUI> newButtonTexts = new();
	private List<Image> newButtonImages = new();
	private Button oldButton;
	private List<TextMeshProUGUI> oldButtonTexts = new();
	private List<Image> oldButtonImages = new();
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
		if (animateFadeOut) { animateFadeOut = false; animateFadeOutInit = true; }
		animateFadeIn = true;
	}

	private void Update()
	{
		AnimFadeIn();
		AnimFadeOut();
		AnimToMenuPosition();
		AnimChangeButton();
	}

	public void PressArtist()
	{
		//Animate to artist screen
		if (animatePosition) { return; }

		targetPosition = artistScreenPos;
		animatePositionInit = true;
		animatePosition = true;

		websiteButton.GetComponent<HintBlink>().Activate();

		oldButton = artistButton;
		newButton = artistMenuButton;
		animateChangeButtonInit = true;
		animateChangeButton = true;
	}

	public void PressInfo()
	{
		//Animate to info screen
		if (animatePosition) { return; }

		targetPosition = infoScreenPos;
		animatePositionInit = true;
		animatePosition = true;

		sysOneButton.GetComponent<HintBlink>().Activate();
		sysTwoButton.GetComponent<HintBlink>().Activate();
		sysThreeButton.GetComponent<HintBlink>().Activate();

		oldButton = infoButton;
		newButton = infoMenuButton;
		animateChangeButtonInit = true;
		animateChangeButton = true;
	}

	public void PressMenuFromInfo()
	{
		if (animatePosition) { return; }

		//Animate to menu screen
		targetPosition = menuScreenPos;
		animatePositionInit = true;
		animatePosition = true;

		sysOneButton.GetComponent<HintBlink>().DeActivate();
		sysTwoButton.GetComponent<HintBlink>().DeActivate();
		sysThreeButton.GetComponent<HintBlink>().DeActivate();

		oldButton = infoMenuButton;
		newButton = infoButton;
		animateChangeButtonInit = true;
		animateChangeButton = true;
	}

	public void PressMenuFromArtist()
	{
		if (animatePosition) { return; }

		//Animate to menu screen
		targetPosition = menuScreenPos;
		animatePositionInit = true;
		animatePosition = true;

		websiteButton.GetComponent<HintBlink>().DeActivate();

		oldButton = artistMenuButton;
		newButton = artistButton;
		animateChangeButtonInit = true;
		animateChangeButton = true;
	}

	public void PressStart()
	{
		onAnimateOut.Invoke();
		raycaster.enabled = false;

		//Animate out
		if (animateFadeIn) { animateFadeIn = false; animateFadeInInit = true; }
		animateFadeOut = true;
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

	private void AnimToMenuPosition()
	{
		if (!animatePosition) { return; }

		if (animatePositionInit)
		{
			oldPosition = new Vector2(masterPivot.offsetMin.x, masterPivot.offsetMax.x);
			raycaster.enabled = false;

			animatePositionTimeValue = 0;
			animatePositionInit = false;
		}

		if (animatePositionTimeValue > 0 && animatePositionTimeValue <= positionChangeDuration)
		{
			float EvaluatedTimeValue = positionChangeCurve.Evaluate(animatePositionTimeValue / positionChangeDuration);

			float NewLeftAnchoredPos = Mathf.Lerp(oldPosition.x, targetPosition.x, EvaluatedTimeValue);
			float NewRightAnchoredPos = Mathf.Lerp(oldPosition.y, targetPosition.y, EvaluatedTimeValue);

			masterPivot.offsetMin = new Vector2(NewLeftAnchoredPos, masterPivot.offsetMin.y);
			masterPivot.offsetMax = new Vector2(NewRightAnchoredPos, masterPivot.offsetMax.y);

		}
		else if (animatePositionTimeValue > positionChangeDuration)
		{
			raycaster.enabled = true;
			animatePositionInit = true;
			animatePosition = false;
		}

		animatePositionTimeValue += Time.deltaTime;
		Mathf.Clamp(animatePositionTimeValue, 0, positionChangeDuration + 0.01f);

	}

	private void AnimChangeButton()
	{
		if (!animateChangeButton) { return; }

		if (animateChangeButtonInit)
		{
			newButtonTexts.Clear();
			newButtonImages.Clear();
			oldButtonTexts.Clear();
			oldButtonImages.Clear();

			oldButton.interactable = false;
			oldButton.GetComponent<HintBlink>().DeActivate();

			TextMeshProUGUI[] newTexts = newButton.GetComponentsInChildren<TextMeshProUGUI>();
			Image[] newTmages = newButton.GetComponentsInChildren<Image>();
			Image newOwnImage = newButton.GetComponent<Image>();
			for (int i = 0; i < newTexts.Length; i++)
			{
				newButtonTexts.Add(newTexts[i]);
			}
			for (int i = 0; i < newTmages.Length; i++)
			{
				if (newTmages[i] != newOwnImage) { newButtonImages.Add(newTmages[i]); }
			}

			TextMeshProUGUI[] oldTexts = oldButton.GetComponentsInChildren<TextMeshProUGUI>();
			Image[] oldTmages = oldButton.GetComponentsInChildren<Image>();
			Image oldOwnImage = oldButton.GetComponent<Image>();
			for (int i = 0; i < oldTexts.Length; i++)
			{
				oldButtonTexts.Add(oldTexts[i]);
			}
			for (int i = 0; i < oldTmages.Length; i++)
			{
				if (oldTmages[i] != oldOwnImage) { oldButtonImages.Add(oldTmages[i]); }
			}
			oldOwnImage.raycastTarget = false;
			newButton.gameObject.SetActive(true);

			animateChangeButtonTimeValue = 0;
			animateChangeButtonInit = false;
		}

		if(animateChangeButtonTimeValue > 0 && animateChangeButtonTimeValue <= buttonChangeDuration)
		{
			float EvaluatedTimeValue = buttonChangeCurve.Evaluate(animateChangeButtonTimeValue / buttonChangeDuration);

			for (int i = 0; i < newButtonTexts.Count; i++)
			{
				newButtonTexts[i].color = new Color(newButtonTexts[i].color.r, newButtonTexts[i].color.g, newButtonTexts[i].color.b, EvaluatedTimeValue * textOpacity);
			}
			for (int i = 0; i < newButtonImages.Count; i++)
			{
				newButtonImages[i].color = new Color(newButtonImages[i].color.r, newButtonImages[i].color.g, newButtonImages[i].color.b, EvaluatedTimeValue * backgroundImageOpacity);
			}
			for (int i = 0; i < oldButtonTexts.Count; i++)
			{
				oldButtonTexts[i].color = new Color(oldButtonTexts[i].color.r, oldButtonTexts[i].color.g, oldButtonTexts[i].color.b, (1 - EvaluatedTimeValue) * textOpacity);
			}
			for (int i = 0; i < oldButtonImages.Count; i++)
			{
				oldButtonImages[i].color = new Color(oldButtonImages[i].color.r, oldButtonImages[i].color.g, oldButtonImages[i].color.b, (1 - EvaluatedTimeValue) * backgroundImageOpacity);
			}

		}
		else if (animateChangeButtonTimeValue > buttonChangeDuration)
		{
			newButton.interactable = true;
			newButton.GetComponent<Image>().raycastTarget = true;
			newButton.GetComponent<HintBlink>().Activate();
			oldButton.gameObject.SetActive(false);

			animateChangeButtonInit = true;
			animateChangeButton = false;
		}

		animateChangeButtonTimeValue += Time.deltaTime;
		Mathf.Clamp(animateChangeButtonTimeValue, 0, buttonChangeDuration + 0.01f);
	}

	private void AnimFadeIn()
	{
		if(!animateFadeIn) { return; }

		if (animateFadeInInit)
		{
			startTextOpacities.Clear();
			startImageOpacities.Clear();

			for (int i = 0; i < texts.Count; i++)
			{
				startTextOpacities.Add(texts[i].color.a);
			}
			for (int i = 0; i < images.Count; i++)
			{
				startImageOpacities.Add(images[i].color.a);
			}

			animateFadeInTimeValue = 0;
			animateFadeInInit = false;
		}

		if(animateFadeInTimeValue > 0 && animateFadeInTimeValue <= fadeInDuration)
		{
			float EvaluatedTimeValue = fadeInCurve.Evaluate(animateFadeInTimeValue / fadeInDuration);

			for (int i = 0; i < texts.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(startTextOpacities[i], textsOpacities[i], EvaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(startImageOpacities[i], imagesOpacities[i], EvaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}
		}
		else if(animateFadeInTimeValue > fadeInDuration)
		{
			onAnimatedIn.Invoke();
			raycaster.enabled = true;
			startButton.GetComponent<HintBlink>().Activate();
			infoButton.GetComponent<HintBlink>().Activate();
			artistButton.GetComponent<HintBlink>().Activate();

			animateFadeInInit = true;
			animateFadeIn = false;
		}

		animateFadeInTimeValue += Time.deltaTime;
		Mathf.Clamp(animateFadeInTimeValue, 0, fadeInDuration + 0.01f);
	}

	private void AnimFadeOut()
	{
		if (!animateFadeOut) { return; }

		if (animateFadeOutInit)
		{
			for (int i = 0; i < filteredOutTexts.Count; i++)
			{
				texts.Add(filteredOutTexts[i]);
			}
			for (int i = 0; i < filteredOutImages.Count; i++)
			{
				images.Add(filteredOutImages[i]);
			}

			startTextOpacities.Clear();
			startImageOpacities.Clear();

			for (int i = 0; i < texts.Count; i++)
			{
				startTextOpacities.Add(texts[i].color.a);
			}
			for (int i = 0; i < images.Count; i++)
			{
				startImageOpacities.Add(images[i].color.a);
			}

			animateFadeOutTimeValue = 0;
			animateFadeOutInit = false;
		}

		if (animateFadeOutTimeValue > 0 && animateFadeOutTimeValue <= fadeOutDuration)
		{
			float EvaluatedTimeValue = fadeOutCurve.Evaluate(animateFadeOutTimeValue / fadeOutDuration);

			for (int i = 0; i < texts.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(startTextOpacities[i], 0, EvaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(startImageOpacities[i], 0, EvaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}
		}
		else if (animateFadeOutTimeValue > fadeOutDuration)
		{
			onDeActivated.Invoke();
			startButton.GetComponent<HintBlink>().DeActivate();
			infoButton.GetComponent<HintBlink>().DeActivate();
			artistButton.GetComponent<HintBlink>().DeActivate();
			artistMenuButton.GetComponent<HintBlink>().DeActivate();
			infoMenuButton.GetComponent<HintBlink>().DeActivate();
			sM.AddToGameState();

			animateFadeInInit = true;
			animateFadeIn = false;
		}

		animateFadeOutTimeValue += Time.deltaTime;
		Mathf.Clamp(animateFadeOutTimeValue, 0, fadeOutDuration + 0.01f);
	}
}
