using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
	[SerializeField] private Camera mainCam;
	[SerializeField] private Camera renderCam;

	[SerializeField] private float definitionSize;
	[SerializeField] private float playingSize;

	[Header("Zoom In")]
	[SerializeField] private float zInDuration;
	[SerializeField] private AnimationCurve zInCurve;
	private Coroutine zInRoutine;

	[Header("Zoom Out")]
	[SerializeField] private float zOutDuration;
	[SerializeField] private AnimationCurve zOutCurve;
	private Coroutine zOutRoutine;

	public void StartZoomIn()
	{
		if (zInRoutine != null || zOutRoutine != null) { return; }
		zInRoutine = StartCoroutine(ZoomIn());
	}

	public void StartZoomOut()
	{
		if (zInRoutine != null || zOutRoutine != null) { return; }
		zOutRoutine = StartCoroutine(ZoomOut());
	}

	private IEnumerator ZoomIn()
	{
		float _timeValue = 0;
		float _startSize = mainCam.orthographicSize;

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / zInDuration;
			float _evaluatedTimeValue = zInCurve.Evaluate(_timeValue);
			float _mijnFloat = Mathf.Lerp(_startSize, playingSize, _evaluatedTimeValue);

			//dit update de material
			mainCam.orthographicSize = _mijnFloat;
			renderCam.orthographicSize = _mijnFloat;

			yield return null;
		}

		zInRoutine = null;
	}

	private IEnumerator ZoomOut()
	{
		float _timeValue = 0;
		float _startSize = mainCam.orthographicSize;

		while (_timeValue < 1)
		{
			_timeValue += Time.deltaTime / zOutDuration;
			float _evaluatedTimeValue = zOutCurve.Evaluate(_timeValue);
			float _mijnFloat = Mathf.Lerp(_startSize, definitionSize, _evaluatedTimeValue);

			//dit update de material
			mainCam.orthographicSize = _mijnFloat;
			renderCam.orthographicSize = _mijnFloat;

			yield return null;
		}

		zOutRoutine = null;
	}
}
