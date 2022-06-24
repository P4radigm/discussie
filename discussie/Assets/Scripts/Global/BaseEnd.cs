using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BaseEnd : MonoBehaviour
{
	private bool active = false;
	private List<TextMeshProUGUI> texts = new();
	private List<float> textsOpacities = new();
	private List<Image> images = new();
	private List<float> imagesOpacities = new();

	[SerializeField] private UnityEvent onActivated;
	[SerializeField] private UnityEvent onAnimatedIn;
	[SerializeField] private UnityEvent onAnimateOut;
	[SerializeField] private UnityEvent onDeActivated;
	[TextArea]
	public string definition;
	[TextArea]
	public string givenName;
	public Vector2 rating;
	public string results;
	[Space(50)]
	public TextMeshProUGUI definitionText;
	[Space(10)]
	[SerializeField] private List<Image> filteredOutImages = new();
	[SerializeField] private List<TextMeshProUGUI> filteredOutTexts = new();
	[Space(10)]
	public float animateInDuration;
	public AnimationCurve animateInCurve;
	private Coroutine animateInRoutine;
	public float animateOutDuration;
	public AnimationCurve animateOutCurve;
	private Coroutine animateOutRoutine;
	[SerializeField] private float continueSeconds;
	private bool timeOut = false;
	[SerializeField] private float timeOutTime = 5;

	private SequenceManager sM;
	private DataManager dM;


	private void Update()
	{
		if (active)
		{
			if (DataManager.instance.uploadedOwnDefinition == DataManager.NetworkingStatus.positive || timeOut)
			{
				CloseDown();
			}
		}
	}

	public void StartUp()
	{
		//Update textcontainers to match data

		sM = SequenceManager.instance;
		dM = DataManager.instance;

		definition = dM.currentSaveData.ownDefinition;
		givenName = dM.currentSaveData.source;
		rating = dM.currentSaveData.ownRating;
		results = dM.currentSaveData.gameResult;
		definitionText.text = Reformatter.instance.ReformatEndDefinition(definition, givenName);

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

		//Animate in
		if (animateInRoutine == null)
		{
			animateInRoutine = StartCoroutine(AnimateIn());
		}
	}

	public void CloseDown()
	{
		//Animate out
		if (animateOutRoutine == null)
		{
			animateOutRoutine = StartCoroutine(AnimateOut());
		}
	}

	private IEnumerator AnimateIn()
	{
		onActivated.Invoke();
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
			_timeValue += Time.deltaTime / animateInDuration;
			float _evaluatedTimeValue = animateInCurve.Evaluate(_timeValue);

			for (int i = 0; i < texts.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(_startTextOpacities[i], 1, _evaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Count; i++)
			{
				float _newOpacity = Mathf.Lerp(_startImageOpacities[i], 1, _evaluatedTimeValue);
				images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, _newOpacity);
			}


			yield return null;
		}
		onAnimatedIn.Invoke();
		active = true;
		yield return new WaitForSeconds(continueSeconds);
		yield return new WaitForSeconds(timeOutTime);
		timeOut = true;
		animateInRoutine = null;
	}

	private IEnumerator AnimateOut()
	{
		onAnimateOut.Invoke();
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
			_timeValue += Time.deltaTime / animateOutDuration;
			float _evaluatedTimeValue = animateOutCurve.Evaluate(_timeValue);

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
		active = false;
		animateOutRoutine = null;
		SceneManager.LoadScene(0);
		gameObject.SetActive(false);
	}
}
