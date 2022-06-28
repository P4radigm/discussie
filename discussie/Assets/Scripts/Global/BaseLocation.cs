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
	[SerializeField] private float animateOutDuration;
	[SerializeField] private AnimationCurve animateOutCurve;
	[NonReorderable]
	[SerializeField] private List<TextMeshProUGUI> filteredOutTexts;
	[NonReorderable]
	[SerializeField] private List<Image> filteredOutImages;

	private bool animateInInit = true;
	private bool animateIn = false;
	private float animateInTimeValue = 0;

	private bool animateOutInit = true;
	private bool animateOut = false;
	private float animateOutTimeValue = 0;

	private List<float> startTextOpacities = new();
	private List<float> startImageOpacities = new();

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
		animateIn = true;
	}

	private void Update()
	{
		AnimIn();
		AnimOut();
	}

	private void AnimIn()
	{
		if (!animateIn) { return; }

		if (animateInInit)
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

			animateInTimeValue = 0;
			animateInInit = false;
		}

		if (animateInTimeValue > 0 && animateInTimeValue < animateInDuration)
		{
			float EvaluatedTimeValue = animateInCurve.Evaluate(animateInTimeValue / animateInDuration);

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
		else if (animateInTimeValue > animateInDuration)
		{
			onAnimatedIn.Invoke();
			active = true;

			animateInInit = true;
			animateIn = false;
		}

		animateInTimeValue += Time.deltaTime;
		Mathf.Clamp(animateInTimeValue, 0, animateInDuration + 0.01f);
	}

	private void AnimOut()
	{
		if (!animateOut) { return; }

		if (animateOutInit)
		{
			onAnimateOut.Invoke();

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

			animateOutTimeValue = 0;
			animateOutInit = false;
		}

		if (animateOutTimeValue > 0 && animateOutTimeValue < animateOutDuration)
		{
			float EvaluatedTimeValue = animateOutCurve.Evaluate(animateOutTimeValue / animateOutDuration);

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
		else if (animateOutTimeValue > animateOutDuration)
		{
			onDeActivated.Invoke();
			active = false;

			animateInInit = true;
			animateIn = false;
			sM.AddToGameState();
			gameObject.SetActive(false);
		}

		animateOutTimeValue += Time.deltaTime;
		Mathf.Clamp(animateOutTimeValue, 0, animateOutDuration + 0.01f);
	}

	public void CloseDown()
	{
		//Communicate chosen location to datamanager
		DataManager.instance.UpdateSaveFile();
		//Animate out
		animateOut = true;
	}

}
