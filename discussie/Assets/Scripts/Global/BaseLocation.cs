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
	private TextMeshProUGUI[] texts;
	private Image[] images;

	[Header("AnimationSettings")]
	[SerializeField] private float animateInDuration;
	[SerializeField] private AnimationCurve animateInCurve;
	private Coroutine animateInRoutine;
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;
	private Coroutine animateOutRoutine;

	private float[] endOpacitiesText;
	private float[] endOpacitiesImages;

	public void StartUp()
	{
		//Instantiate selectable locations objects
		onActivated.Invoke();

		sM = SequenceManager.instance;

		texts = GetComponentsInChildren<TextMeshProUGUI>(true);
		Image[] unfilteredImages = GetComponentsInChildren<Image>(true);

		images = new Image[unfilteredImages.Length - 2];
		int compensation = 0;
		for (int i = 0; i < unfilteredImages.Length; i++)
		{
			if (unfilteredImages[i].gameObject.tag != "RatingTarget" && unfilteredImages[i].GetComponent<Button>() == null)
			{
				images[i - compensation] = unfilteredImages[i];
			}
			else
			{
				compensation++;
			}
		}
		endOpacitiesText = new float[texts.Length];
		endOpacitiesImages = new float[images.Length];
		//Set all end opacities of graphics
		for (int i = 0; i < texts.Length; i++)
		{
			endOpacitiesText[i] = texts[i].color.a;
		}
		for (int i = 0; i < images.Length; i++)
		{
			endOpacitiesImages[i] = images[i].color.a;
		}

		//Set all graphics invisible
		for (int i = 0; i < texts.Length; i++)
		{
			texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, 0);
		}
		for (int i = 0; i < images.Length; i++)
		{
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
				float _newOpacity = Mathf.Lerp(_startTextOpacities[i], endOpacitiesText[i], _evaluatedTimeValue);
				texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, _newOpacity);
			}
			for (int i = 0; i < images.Length; i++)
			{
				float _newOpacity = Mathf.Lerp(_startImageOpacities[i], endOpacitiesImages[i], _evaluatedTimeValue);
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
		onDeActivated.Invoke();
		active = false;
		sM.AddToGameState();
		animateOutRoutine = null;
		gameObject.SetActive(false);
	}
}
