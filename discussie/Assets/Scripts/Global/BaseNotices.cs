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
	private Image[] privacyImages;
	[SerializeField] private GameObject internetContainer;
	[SerializeField] private Button internetButton;
	private TextMeshProUGUI[] internetTexts;
	private Image[] internetImages;

	private Coroutine animateInRoutine;
	[SerializeField] private float animateInDuration;
	[SerializeField] private AnimationCurve animateInCurve;
	[SerializeField] private float continueSeconds;
	private Coroutine animateOutRoutine;
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;
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


		//Set all graphics invisible
		for (int i = 0; i < privacyTexts.Length; i++)
		{
			privacyTexts[i].color = new Color(privacyTexts[i].color.r, privacyTexts[i].color.g, privacyTexts[i].color.b, 0);
		}
		for (int i = 0; i < privacyImages.Length; i++)
		{
			privacyImages[i].color = new Color(privacyImages[i].color.r, privacyImages[i].color.g, privacyImages[i].color.b, 0);
		}
		for (int i = 0; i < internetTexts.Length; i++)
		{
			internetTexts[i].color = new Color(internetTexts[i].color.r, internetTexts[i].color.g, internetTexts[i].color.b, 0);
		}
		for (int i = 0; i < internetImages.Length; i++)
		{
			internetImages[i].color = new Color(internetImages[i].color.r, internetImages[i].color.g, internetImages[i].color.b, 0);
		}

		

		//Animate privacy in
		if (animateInRoutine == null)
		{
			animateInRoutine = StartCoroutine(AnimateIn(privacyTexts, privacyImages, privacyButton));
		}
	}

	public void TransitionToInternet()
	{
		ogInternetCheckInterval = PlatformManager.instance.internetCheckInterval;
		PlatformManager.instance.internetCheckInterval = 1;
		DataManager.instance.currentSaveData.privacyCheck = true;
		DataManager.instance.UpdateSaveFile();
		StartCoroutine(AnimateOut(privacyTexts, privacyImages));
	}

	private void Update()
	{
		if(internetStatusLastFrame == PlatformManager.instance.hasInternetConnection) { return; }

		internetBody.text = UpdateInternetText(internetHighlightOne, internetHighlightTwo, internetHighlightThree, internetHighlightSource);

		internetStatusLastFrame = PlatformManager.instance.hasInternetConnection;
	}

	public void CloseDown()
	{
		PlatformManager.instance.internetCheckInterval = ogInternetCheckInterval;
		if (animateOutRoutine != null) { return; } 
		StartCoroutine(AnimateOut(internetTexts, internetImages));
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

	private IEnumerator AnimateIn(TextMeshProUGUI[] texts, Image[] images, Button button)
	{
		if (texts == privacyTexts)
		{
			privacyContainer.SetActive(true);
			onPrivacyActivated.Invoke();
		}
		else
		{
			internetContainer.SetActive(true);
			onInternetActivated.Invoke();
		}
		float _timeValue = 0;
		float[] _startTextOpacities = new float[texts.Length];
		for (int i = 0; i < texts.Length; i++)
		{
			_startTextOpacities[i] = texts[i].color.a;
		}
		float[] _startImageOpacities = new float[images.Length];
		for (int i = 0; i < images.Length; i++)
		{
			_startImageOpacities[i] = images[i].color.a;
		}

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / animateInDuration;
			float _evaluatedTimeValue = animateInCurve.Evaluate(_timeValue);

			for (int i = 0; i < texts.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(_startTextOpacities[i], 1, _evaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(_startImageOpacities[i], 1, _evaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}


			yield return null;
		}
		if(texts == privacyTexts) { onPrivacyAnimatedIn.Invoke(); active = true; }
		else { onInternetAnimatedIn.Invoke(); }
		yield return new WaitForSeconds(continueSeconds);
		button.gameObject.SetActive(true);
		animateInRoutine = null;
	}

	private IEnumerator AnimateOut(TextMeshProUGUI[] texts, Image[] images)
	{
		if (texts == privacyTexts)
		{
			onPrivacyAnimateOut.Invoke();
		}
		else
		{
			onInternetAnimateOut.Invoke();
		}
		float _timeValue = 0;
		float[] _startTextOpacities = new float[texts.Length];
		for (int i = 0; i < texts.Length; i++)
		{
			_startTextOpacities[i] = texts[i].color.a;
		}
		float[] _startImageOpacities = new float[images.Length];
		for (int i = 0; i < images.Length; i++)
		{
			_startImageOpacities[i] = images[i].color.a;
		}

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / animateOutDuration;
			float _evaluatedTimeValue = animateOutCurve.Evaluate(_timeValue);

			for (int i = 0; i < texts.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(_startTextOpacities[i], 0, _evaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(_startImageOpacities[i], 0, _evaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}


			yield return null;
		}


		if(texts == internetTexts)
		{
			onDeActivated.Invoke();
			active = false;
			sM.AddToGameState();
			animateOutRoutine = null;
			gameObject.SetActive(false);
		}
		else
		{
			privacyContainer.SetActive(false);
			StartCoroutine(AnimateIn(internetTexts, internetImages, internetButton));
			animateOutRoutine = null;
		}
	}
}
