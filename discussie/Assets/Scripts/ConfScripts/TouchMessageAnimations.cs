using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TouchMessageAnimations : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fgTouch;
    [SerializeField] private TextMeshProUGUI bgTouch;
    [Space(40)]
    [SerializeField] private Color fgCol;
    [SerializeField] private Color bgCol;
    [Space(20)]
    [SerializeField] private float initialBlinkDelay;
    [Space(10)]
    [SerializeField] private float blinkDuration;
    [SerializeField] private AnimationCurve blinkCurve;
    [Space(10)]
    [SerializeField] private float removeDuration;
    [SerializeField] private AnimationCurve removeCurve;

    private Coroutine blinkLoop;
    private Coroutine removeAnim;
    [HideInInspector] public bool addDelay;

	public void StartBlinkLoop()
	{
        if (removeAnim != null) { return; }
        if (blinkLoop != null) { StopCoroutine(blinkLoop); }
        addDelay = true;
        blinkLoop = StartCoroutine(BlinkLoop());
	}

    public void StartRemoveAnim()
	{
        if(blinkLoop != null) { StopCoroutine(blinkLoop); blinkLoop = null; }
        if(removeAnim != null) { return; }
        removeAnim = StartCoroutine(RemoveAnim());
	}

    private IEnumerator RemoveAnim()
	{
        float _timeValue = 0;
        float _CurrentOpacity = fgTouch.color.a;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / removeDuration;
            float _evaluatedTimeValue = removeCurve.Evaluate(_timeValue);
            float _newOpacity = Mathf.Lerp(_CurrentOpacity, 0f, _evaluatedTimeValue);

            //dit update de material
            bgTouch.color = new Color(bgCol.r, bgCol.g, bgCol.b, _newOpacity);
            fgTouch.color = new Color(fgCol.r, fgCol.g, fgCol.b, _newOpacity);

            yield return null;
        }

        removeAnim = null;
    }

    private IEnumerator BlinkLoop()
	{
        if (addDelay)
        {
            bgTouch.color = new Color(bgCol.r, bgCol.g, bgCol.b, 0);
            fgTouch.color = new Color(fgCol.r, fgCol.g, fgCol.b, 0);
            yield return new WaitForSeconds(initialBlinkDelay);
        }
        addDelay = false;
		
        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / blinkDuration;
            float _evaluatedTimeValue = blinkCurve.Evaluate(_timeValue);
            float _newOpacity = Mathf.Lerp(0f, 1f, _evaluatedTimeValue);

            //dit update de material
            bgTouch.color = new Color(bgCol.r, bgCol.g, bgCol.b, _newOpacity);
            fgTouch.color = new Color(fgCol.r, fgCol.g, fgCol.b, _newOpacity);

            yield return null;
        }

        blinkLoop = StartCoroutine(BlinkLoop());
    }
}
