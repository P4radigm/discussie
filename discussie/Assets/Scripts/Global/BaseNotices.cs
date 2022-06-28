using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class BaseNotices : MonoBehaviour
{
	private SequenceManager sM;

	public bool active;

	[SerializeField] private UnityEvent onPrivacyActivated;
	[SerializeField] private UnityEvent onPrivacyAnimatedIn;
	[SerializeField] private UnityEvent onPrivacyAnimateOut;
	[SerializeField] private UnityEvent onInternetActivated;
	[SerializeField] private UnityEvent onInternetAnimatedIn;
	[SerializeField] private UnityEvent onInternetAnimateOut;
	[SerializeField] private UnityEvent onDeActivated;

	[SerializeField] private GameObject privacyContainer;
	[SerializeField] private Button privacyButton;
	private TextMeshProUGUI[] privacyTexts;
	private List<float> privacyTextsOpacities = new();
	private Image[] privacyImages;
	private List<float> privacyImagesOpacities = new();
	[SerializeField] private GameObject internetContainer;
	[SerializeField] private Button internetButton;
	private TextMeshProUGUI[] internetTexts;
	private List<float> internetTextsOpacities = new();
	private Image[] internetImages;
	private List<float> internetImagesOpacities = new();

	//private Coroutine animateInRoutine;
	[SerializeField] private float animateInDuration;
	[SerializeField] private AnimationCurve animateInCurve;

	[SerializeField] private float continueSeconds;
	//private Coroutine animateOutRoutine;
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;

	private bool animatePrivacyInInit = true;
	private bool animatePrivacyIn = false;
	private float animatePrivacyInTimeValue = 0;

	private bool animatePrivacyOutInit = true;
	private bool animatePrivacyOut = false;
	private float animatePrivacyOutTimeValue = 0;

	private bool animateInternetInInit = true;
	private bool animateInternetIn = false;
	private float animateInternetInTimeValue = 0;

	private bool animateInternetOutInit = true;
	private bool animateInternetOut = false;
	private float animateInternetOutTimeValue = 0;

	private List<float> startTextOpacities = new();
	private List<float> startImageOpacities = new();

	private float ogInternetCheckInterval;
	private bool internetStatusLastFrame;
	private string internetHighlightOne;
	private string internetHighlightTwo;
	private string internetHighlightThree;
	private string internetHighlightSource;

	[Header("Text")]
	[NonReorderable]
	[TextArea]
	[SerializeField] private string[] privacyText;
	[NonReorderable]
	[TextArea]
	[SerializeField] private string[] internetText;
	[SerializeField] private string source;
	[SerializeField] private TextMeshProUGUI privacyBody;
	[SerializeField] private TextMeshProUGUI internetBody;

	public void StartUp()
	{
		HighlightColorManager hCM = HighlightColorManager.instance;
		internetHighlightOne = hCM.GetHighlightHex();
		internetHighlightTwo = hCM.GetHighlightHex();
		internetHighlightThree = hCM.GetHighlightHex();
		internetHighlightSource = hCM.GetHighlightHex();

		privacyBody.text = FormatPrivacyText();
		internetBody.text = UpdateInternetText(internetHighlightOne, internetHighlightTwo, internetHighlightThree, internetHighlightSource);


		sM = SequenceManager.instance;

		privacyContainer.SetActive(false);
		internetContainer.SetActive(false);

		privacyTexts = privacyContainer.GetComponentsInChildren<TextMeshProUGUI>(true);
		privacyImages = privacyContainer.GetComponentsInChildren<Image>(true);
		internetTexts = internetContainer.GetComponentsInChildren<TextMeshProUGUI>(true);
		internetImages = internetContainer.GetComponentsInChildren<Image>(true);


		//Store all opacities and set all graphics invisible
		for (int i = 0; i < privacyTexts.Length; i++)
		{
			privacyTextsOpacities.Add(privacyTexts[i].color.a);
			privacyTexts[i].color = new Color(privacyTexts[i].color.r, privacyTexts[i].color.g, privacyTexts[i].color.b, 0);
		}
		for (int i = 0; i < privacyImages.Length; i++)
		{
			privacyImagesOpacities.Add(privacyImages[i].color.a);
			privacyImages[i].color = new Color(privacyImages[i].color.r, privacyImages[i].color.g, privacyImages[i].color.b, 0);
		}
		for (int i = 0; i < internetTexts.Length; i++)
		{
			internetTextsOpacities.Add(internetTexts[i].color.a);
			internetTexts[i].color = new Color(internetTexts[i].color.r, internetTexts[i].color.g, internetTexts[i].color.b, 0);
		}
		for (int i = 0; i < internetImages.Length; i++)
		{
			internetImagesOpacities.Add(internetImages[i].color.a);
			internetImages[i].color = new Color(internetImages[i].color.r, internetImages[i].color.g, internetImages[i].color.b, 0);
		}

		//Animate privacy in
		animatePrivacyIn = true;
	}

	public void TransitionToInternet()
	{
		ogInternetCheckInterval = PlatformManager.instance.internetCheckInterval;
		PlatformManager.instance.internetCheckInterval = 1;
		DataManager.instance.currentSaveData.privacyCheck = true;
		DataManager.instance.UpdateSaveFile();

		animatePrivacyOut = true;
	}

	private void Update()
	{
		AnimPrivacyIn();
		AnimPrivacyOut();
		AnimInternetIn();
		AnimInternetOut();

		if (internetStatusLastFrame == PlatformManager.instance.hasInternetConnection) { return; }

		internetBody.text = UpdateInternetText(internetHighlightOne, internetHighlightTwo, internetHighlightThree, internetHighlightSource);

		internetStatusLastFrame = PlatformManager.instance.hasInternetConnection;
	}

	public void CloseDown()
	{
		PlatformManager.instance.internetCheckInterval = ogInternetCheckInterval;
		animateInternetOut = true;
	}

	private string FormatPrivacyText()
	{
		HighlightColorManager hCM = HighlightColorManager.instance;
		Reformatter reformatter = Reformatter.instance;
		string _outputString = "";
		for (int i = 0; i < privacyText.Length; i++)
		{
			_outputString += $"<color=#{hCM.GetHighlightHex()}>{i + 1}.</color> <indent={reformatter.indentPercentage}%>{privacyText[i]}</indent>" + '\n';
		}
		_outputString += $"<size={reformatter.sourceScalePercentage}%><align=right>Gegeven door: <color=#{hCM.GetHighlightHex()}>{source}</size></align></color>";

		return _outputString;
	}

	private string UpdateInternetText(string hL1, string hL2, string hL3, string hLsource)
	{
		HighlightColorManager hCM = HighlightColorManager.instance;
		Reformatter reformatter = Reformatter.instance;
		PlatformManager pM = PlatformManager.instance;
		string yN = pM.hasInternetConnection ? "wel" : "niet";
		string _outputString = "";

		_outputString += $"<color=#{hL1}>1.</color> <indent={reformatter.indentPercentage}%>{internetText[0]}</indent>" + '\n';
		_outputString += $"<color=#{hL2}>2.</color> <indent={reformatter.indentPercentage}%>{internetText[1]} <color=#{hCM.GetHighlightHex()}>{yN}</color> {internetText[2]}</indent>" + '\n';
		_outputString += $"<color=#{hL3}>3.</color> <indent={reformatter.indentPercentage}%>{internetText[3]}</indent>" + '\n';
		_outputString += $"<size={reformatter.sourceScalePercentage}%><align=right>Gegeven door: <color=#{hLsource}>{source}</size></align></color>";

		return _outputString;
	}

	private void AnimPrivacyIn()
	{
		if (!animatePrivacyIn) { return; }

		if (animatePrivacyInInit)
		{
			privacyContainer.SetActive(true);
			onPrivacyActivated.Invoke();

			startTextOpacities.Clear();
			startImageOpacities.Clear();

			for (int i = 0; i < privacyTexts.Length; i++)
			{
				startTextOpacities.Add(privacyTexts[i].color.a);
			}
			for (int i = 0; i < privacyImages.Length; i++)
			{
				startImageOpacities.Add(privacyImages[i].color.a);
			}

			animatePrivacyInTimeValue = 0;
			animatePrivacyInInit = false;
		}

		if (animatePrivacyInTimeValue > 0 && animatePrivacyInTimeValue < animateInDuration)
		{
			float EvaluatedTimeValue = animateInCurve.Evaluate(animatePrivacyInTimeValue / animateInDuration);

			for (int i = 0; i < privacyTexts.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startTextOpacities[i], privacyTextsOpacities[i], EvaluatedTimeValue);
				privacyTexts[i].color = new Color(privacyTexts[i].color.r, privacyTexts[i].color.g, privacyTexts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < privacyImages.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startImageOpacities[i], privacyImagesOpacities[i], EvaluatedTimeValue);
				privacyImages[i].color = new Color(privacyImages[i].color.r, privacyImages[i].color.g, privacyImages[i].color.b, _newOpacity);
			}
		}
		else if (animatePrivacyInTimeValue > animateInDuration)
		{
			onPrivacyAnimatedIn.Invoke();
			active = true;

			privacyButton.gameObject.SetActive(true);
			privacyButton.gameObject.GetComponent<HintBlink>().Activate();

			animatePrivacyInInit = true;
			animatePrivacyIn = false;
		}

		animatePrivacyInTimeValue += Time.deltaTime;
		Mathf.Clamp(animatePrivacyInTimeValue, 0, animateInDuration + 0.01f);
	}

	private void AnimPrivacyOut()
	{
		if (!animatePrivacyOut) { return; }

		if (animatePrivacyOutInit)
		{
			onPrivacyAnimateOut.Invoke();
			privacyButton.GetComponent<HintBlink>().DeActivate();

			startTextOpacities.Clear();
			startImageOpacities.Clear();

			for (int i = 0; i < privacyTexts.Length; i++)
			{
				startTextOpacities.Add(privacyTexts[i].color.a);
			}
			for (int i = 0; i < privacyImages.Length; i++)
			{
				startImageOpacities.Add(privacyImages[i].color.a);
			}

			animatePrivacyOutTimeValue = 0;
			animatePrivacyOutInit = false;
		}

		if (animatePrivacyOutTimeValue > 0 && animatePrivacyOutTimeValue < animateOutDuration)
		{
			float EvaluatedTimeValue = animateOutCurve.Evaluate(animatePrivacyOutTimeValue / animateOutDuration);

			for (int i = 0; i < privacyTexts.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startTextOpacities[i], 0, EvaluatedTimeValue);
				privacyTexts[i].color = new Color(privacyTexts[i].color.r, privacyTexts[i].color.g, privacyTexts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < privacyImages.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startImageOpacities[i], 0, EvaluatedTimeValue);
				privacyImages[i].color = new Color(privacyImages[i].color.r, privacyImages[i].color.g, privacyImages[i].color.b, _newOpacity);
			}
		}
		else if (animatePrivacyOutTimeValue > animateOutDuration)
		{
			privacyContainer.SetActive(false);
			animateInternetIn = true;

			animatePrivacyOutInit = true;
			animatePrivacyOut = false;
		}

		animatePrivacyOutTimeValue += Time.deltaTime;
		Mathf.Clamp(animatePrivacyOutTimeValue, 0, animateOutDuration + 0.01f);
	}

	private void AnimInternetIn()
	{
		if (!animateInternetIn) { return; }

		if (animateInternetInInit)
		{
			internetContainer.SetActive(true);
			onInternetActivated.Invoke();

			startTextOpacities.Clear();
			startImageOpacities.Clear();

			for (int i = 0; i < internetTexts.Length; i++)
			{
				startTextOpacities.Add(internetTexts[i].color.a);
			}
			for (int i = 0; i < internetImages.Length; i++)
			{
				startImageOpacities.Add(internetImages[i].color.a);
			}

			animateInternetInTimeValue = 0;
			animateInternetInInit = false;
		}

		if (animateInternetInTimeValue > 0 && animateInternetInTimeValue < animateInDuration)
		{
			float EvaluatedTimeValue = animateInCurve.Evaluate(animateInternetInTimeValue / animateInDuration);

			for (int i = 0; i < internetTexts.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startTextOpacities[i], internetTextsOpacities[i], EvaluatedTimeValue);
				internetTexts[i].color = new Color(internetTexts[i].color.r, internetTexts[i].color.g, internetTexts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < internetImages.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startImageOpacities[i], internetImagesOpacities[i], EvaluatedTimeValue);
				internetImages[i].color = new Color(internetImages[i].color.r, internetImages[i].color.g, internetImages[i].color.b, _newOpacity);
			}
		}
		else if (animateInternetInTimeValue > animateInDuration)
		{
			onInternetAnimatedIn.Invoke();

			internetButton.gameObject.SetActive(true);
			internetButton.gameObject.GetComponent<HintBlink>().Activate();

			animatePrivacyInInit = true;
			animatePrivacyIn = false;
		}

		animateInternetInTimeValue += Time.deltaTime;
		Mathf.Clamp(animateInternetInTimeValue, 0, animateInDuration + 0.01f);
	}

	private void AnimInternetOut()
	{
		if (!animateInternetOut) { return; }

		if (animateInternetOutInit)
		{
			onInternetAnimateOut.Invoke();
			internetButton.GetComponent<HintBlink>().DeActivate();

			startTextOpacities.Clear();
			startImageOpacities.Clear();

			for (int i = 0; i < internetTexts.Length; i++)
			{
				startTextOpacities.Add(internetTexts[i].color.a);
			}
			for (int i = 0; i < internetImages.Length; i++)
			{
				startImageOpacities.Add(internetImages[i].color.a);
			}

			animateInternetOutTimeValue = 0;
			animateInternetOutInit = false;
		}

		if (animateInternetOutTimeValue > 0 && animateInternetOutTimeValue < animateOutDuration)
		{
			float EvaluatedTimeValue = animateOutCurve.Evaluate(animateInternetOutTimeValue / animateOutDuration);

			for (int i = 0; i < internetTexts.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startTextOpacities[i], 0, EvaluatedTimeValue);
				internetTexts[i].color = new Color(internetTexts[i].color.r, internetTexts[i].color.g, internetTexts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < internetImages.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(startImageOpacities[i], 0, EvaluatedTimeValue);
				internetImages[i].color = new Color(internetImages[i].color.r, internetImages[i].color.g, internetImages[i].color.b, _newOpacity);
			}
		}
		else if (animateInternetOutTimeValue > animateOutDuration)
		{
			onDeActivated.Invoke();
			active = false;

			animateInternetOutInit = true;
			animateInternetOut = false;

			sM.AddToGameState();
			gameObject.SetActive(false);
		}

		animateInternetOutTimeValue += Time.deltaTime;
		Mathf.Clamp(animateInternetOutTimeValue, 0, animateOutDuration + 0.01f);
	}
}
