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
	//private Coroutine animateInRoutine;
	public float animateOutDuration;
	public AnimationCurve animateOutCurve;
	//private Coroutine animateOutRoutine;
	[SerializeField] private float continueSeconds;
	private bool timeOut = false;
	[SerializeField] private float timeOutTime = 5;

	private bool animateInInit = true;
	private bool animateIn = false;
	private float animateInTimeValue = 0;

	private bool animateOutInit = true;
	private bool animateOut = false;
	private float animateOutTimeValue = 0;

	private List<float> startTextOpacities = new();
	private List<float> startImageOpacities = new();

	private SequenceManager sM;
	private DataManager dM;


	private void Update()
	{
		AnimIn();
		AnimOut();

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
		animateIn = true;
	}

	public void CloseDown()
	{
		//Animate out
		animateOut = true;
	}

	private void AnimIn()
	{
		if (!animateIn) { return; }

		if (animateInInit)
		{
			onActivated.Invoke();

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
		else if (animateInTimeValue > animateInDuration && animateInTimeValue <= animateInDuration + continueSeconds + timeOutTime)
		{

			onAnimatedIn.Invoke();
			active = true;

		}
		else if (animateInTimeValue > animateInDuration + continueSeconds + timeOutTime)
		{
			timeOut = true;

			animateInInit = true;
			animateIn = false;
		}

		animateInTimeValue += Time.deltaTime;
		Mathf.Clamp(animateInTimeValue, 0, animateInDuration + continueSeconds + timeOutTime + 0.01f);
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
			SceneManager.LoadScene(0);
			gameObject.SetActive(false);

			animateOutInit = true;
			animateOut = false;
		}

		animateOutTimeValue += Time.deltaTime;
		Mathf.Clamp(animateOutTimeValue, 0, animateOutDuration + 0.01f);
	}	
}
