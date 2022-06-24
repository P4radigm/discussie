using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BaseLocation : MonoBehaviour
{
	public bool active;
	[SerializeField] private UnityEvent onActivated;
	[SerializeField] private UnityEvent onAnimatedIn;
	[SerializeField] private UnityEvent onAnimateOut;
	[SerializeField] private UnityEvent onDeActivated;

	[System.Serializable]
    public class locations
	{
        public string location;
        public string lidwoord;
	}

    public locations[] selectableLocations;
	private SequenceManager sM;
	private List<TextMeshProUGUI> texts = new();
	private List<float> textsOpacities = new();
	private List<Image> images = new();
	private List<float> imagesOpacities = new();

	[Header("AnimationSettings")]
	[SerializeField] private float animateInDuration;
	[SerializeField] private AnimationCurve animateInCurve;
	private Coroutine animateInRoutine;
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;
	private Coroutine animateOutRoutine;
	[NonReorderable]
	[SerializeField] private List<TextMeshProUGUI> filteredOutTexts;
	[NonReorderable]
	[SerializeField] private List<Image> filteredOutImages;

	public void StartUp()
	{
		//Instantiate selectable locations objects
		onActivated.Invoke();

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
		if (animateInRoutine == null)
		{
			animateInRoutine = StartCoroutine(AnimateIn());
		}
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
		animateInRoutine = null;
	}

	public void CloseDown()
	{
		//Communicate chosen location to datamanager
		DataManager.instance.UpdateSaveFile();
		//Animate out
		if (animateOutRoutine == null)
		{
			animateOutRoutine = StartCoroutine(AnimateOut());
		}
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
