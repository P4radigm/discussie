using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DISmenuElementBehaviour : MonoBehaviour
{
	private HighlightColorManager hCM;
	private SpriteRenderer sP;
	private Coroutine animateInRoutine;
	[SerializeField] private AnimationCurve animateInCurve;
	private Coroutine animateOutRoutine;
	[SerializeField] private AnimationCurve animateOutCurve;

	private void OnEnable()
	{
		hCM = HighlightColorManager.instance;
		sP = GetComponent<SpriteRenderer>();

		sP.color = hCM.getHighlightColor();
	}

	public void StartAnimateIn(float animationLength)
	{
		if (animateInRoutine != null) { return; }
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
