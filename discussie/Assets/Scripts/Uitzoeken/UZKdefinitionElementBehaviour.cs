using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UZKdefinitionElementBehaviour : MonoBehaviour
{
    private List<Color> colorSequence = new List<Color>();
	private HighlightColorManager hCM;
	[SerializeField] private float[] skipStepOdds; //Between 0-1
	[SerializeField] private bool red;
	private List<Color> availableColorOptions = new List<Color>();
	private SpriteRenderer sP;
	[SerializeField] private AnimationCurve animateInCurve;
	private Coroutine animateInRoutine;
	[SerializeField] private AnimationCurve animateOutCurve;
	private Coroutine animateOutRoutine;

	private void OnEnable()
	{
		hCM = HighlightColorManager.instance;
		sP = GetComponent<SpriteRenderer>();

		//Initiate colorsequence
		if (red)
		{
			availableColorOptions.Add(hCM.highlightColorList[0]);
			availableColorOptions.Add(hCM.highlightColorList[2]);
			availableColorOptions.Add(hCM.highlightColorList[3]);
		}
		else
		{
			availableColorOptions.Add(hCM.highlightColorList[6]);
			availableColorOptions.Add(hCM.highlightColorList[4]);
			availableColorOptions.Add(hCM.highlightColorList[3]);
		}

		for (int i = 0; i < skipStepOdds.Length; i++)
		{
			if (i == 0)
			{
				int _initialIndex = red ? 1 : 5;
				colorSequence.Add(hCM.highlightColorList[_initialIndex]);
			}
			else
			{
				float _randomFactor = Random.value;
				if(_randomFactor < skipStepOdds[i] || availableColorOptions.Count == 0)
				{
					colorSequence.Add(colorSequence[colorSequence.Count - 1]);
				}
				else
				{
					int _randomIndex = Random.Range(0, availableColorOptions.Count);
					colorSequence.Add(availableColorOptions[_randomIndex]);
					if(availableColorOptions[_randomIndex] == hCM.highlightColorList[0] || availableColorOptions[_randomIndex] == hCM.highlightColorList[6])
					{
						availableColorOptions.Clear();
					}
					else
					{
						availableColorOptions.Remove(availableColorOptions[_randomIndex]);
						if (availableColorOptions.Count != 0) { availableColorOptions.RemoveAt(0); }
					}
				}
			}
		}
	}

	private void Update()
	{
		Color _newCol = colorSequence[Mathf.Clamp(Input.touchCount, 0, colorSequence.Count - 1)];
		sP.color = new Color(_newCol.r, _newCol.g, _newCol.b, sP.color.a);
	}

	public void StartAnimateIn(float animationLength)
	{
		if(animateInRoutine != null) { return; }
		StartCoroutine(AnimateIn(animationLength));
	}

	private IEnumerator AnimateIn(float animationLength)
	{
		float _timeValue = 0;

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / animationLength;
			float _evaluatedTimeValue = animateInCurve.Evaluate(_timeValue);

			float _newOpacity = Mathf.Lerp(0, 1, _evaluatedTimeValue);
			sP.color = new Color(sP.color.r, sP.color.g, sP.color.b, _newOpacity);

			yield return null;
		}
		animateInRoutine = null;
	}

	public void StartAnimateOut(float animationLength)
	{
		if (animateOutRoutine != null) { return; }
		StartCoroutine(AnimateOut(animationLength));
	}

	private IEnumerator AnimateOut(float animationLength)
	{
		float _timeValue = 0;

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / animationLength;
			float _evaluatedTimeValue = animateOutCurve.Evaluate(_timeValue);

			float _newOpacity = Mathf.Lerp(1, 0, _evaluatedTimeValue);
			sP.color = new Color(sP.color.r, sP.color.g, sP.color.b, _newOpacity);

			yield return null;
		}
		animateOutRoutine = null;
	}
}
