using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HintBlink : MonoBehaviour
{
	public bool isActive;
	[SerializeField] private Image[] subscribedImages;
	private List<float> imageOpacities = new();
	[SerializeField] private TextMeshProUGUI[] subscribedTexts;
	private List<float> textOpacities = new();
	[Space(20)]
	[SerializeField] private float initiatingTime;
	[SerializeField] private float loopTime;
	private float timer;
	private Coroutine blinkRoutine;
	[Range(0,1)] [SerializeField] private float blinkDepth;
	[SerializeField] private float blinkDuration;
	[SerializeField] private AnimationCurve blinkCurve;

	public void Activate()
	{
		isActive = true;
		timer = loopTime + initiatingTime;
		for (int i = 0; i < subscribedImages.Length; i++)
		{
			imageOpacities.Add(subscribedImages[i].color.a);
		}
		for (int i = 0; i < subscribedTexts.Length; i++)
		{
			textOpacities.Add(subscribedTexts[i].color.a);
		}
	}

	public void DeActivate()
	{
		isActive = false;
	}

	private void Update()
	{
		if(timer < 0 && isActive) 
		{ 
			StartBlink(); 
			timer = loopTime; 
		}
		else if(isActive)
		{
			timer -= Time.deltaTime;
		}
		else
		{
			timer = loopTime;
		}
	}

	public void StartBlink()
	{
		if(blinkRoutine == null)
		{
			blinkRoutine = StartCoroutine(Blink());
		}
	}

	private IEnumerator Blink()
	{
		float TimeValue = 0;
		while (TimeValue < 1)
		{
			TimeValue += Time.deltaTime / blinkDuration;
			float EvaluatedTimeValue = blinkCurve.Evaluate(TimeValue);

			for (int i = 0; i < subscribedImages.Length; i++)
			{
				subscribedImages[i].color = new Color(subscribedImages[i].color.r, subscribedImages[i].color.g, subscribedImages[i].color.b, Mathf.Lerp((1 - blinkDepth) * imageOpacities[i], imageOpacities[i], EvaluatedTimeValue));
			}
			for (int i = 0; i < subscribedTexts.Length; i++)
			{
				subscribedTexts[i].color = new Color(subscribedTexts[i].color.r, subscribedTexts[i].color.g, subscribedTexts[i].color.b, Mathf.Lerp((1 - blinkDepth) * textOpacities[i], textOpacities[i], EvaluatedTimeValue));
			}

			yield return null;
		}
		blinkRoutine = null;
	}
}
