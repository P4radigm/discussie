using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class BaseDefinition : MonoBehaviour
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
	public float animateInDuration;
	public AnimationCurve animateInCurve;
	private Coroutine animateInRoutine;	
	public float animateOutDuration;
	public AnimationCurve animateOutCurve;
	private Coroutine animateOutRoutine;
	[SerializeField] private float continueSeconds;
	[NonReorderable]
	[SerializeField] private List<TextMeshProUGUI> filteredOutTexts;
	[NonReorderable]
	[SerializeField] private List<Image> filteredOutImages;

	private SequenceManager sM;


	public void StartUp()
	{
		//Update textcontainers to match data

		sM = SequenceManager.instance;

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
		if(animateInRoutine == null)
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

	public void TryUploadA()
	{
		//Save rating to local save file
		DataManager.instance.currentSaveData.indexRatingA = DataManager.instance.definitionA.index;
		DataManager.instance.currentSaveData.givenRatingA = rating;

		//flag rating to be uploaded to server
		DataManager.instance.ownRatingAReady = true;
		DataManager.instance.TryNetworking();

		DataManager.instance.UpdateSaveFile();
	}

	public void TryUploadB()
	{
		//Save rating to local save file
		DataManager.instance.currentSaveData.indexRatingB = DataManager.instance.definitionB.index;
		DataManager.instance.currentSaveData.givenRatingB = rating;

		//try uploading rating to server
		DataManager.instance.ownRatingBReady = true;
		DataManager.instance.TryNetworking();

		DataManager.instance.UpdateSaveFile();
	}

	public void TryUploadC()
	{
		//Save rating to local save file
		DataManager.instance.currentSaveData.indexRatingC = DataManager.instance.definitionC.index;
		DataManager.instance.currentSaveData.givenRatingC = rating;

		//try uploading rating to server
		DataManager.instance.ownRatingCReady = true;
		DataManager.instance.TryNetworking();

		DataManager.instance.UpdateSaveFile();
	}

	private void Update()
	{
		if(!active) { return; }
		//Animate blinking stuff
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
		active = true;
		yield return new WaitForSeconds(continueSeconds);
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
		sM.AddToGameState();
		animateOutRoutine = null;
		gameObject.SetActive(false);
	}

}
