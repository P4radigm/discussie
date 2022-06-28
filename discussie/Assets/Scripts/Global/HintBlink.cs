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
	private bool animateBlinkInit = true;
	private bool animateBlink = false;
	private float animateBlinkTimeValue = 0;
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

		AnimBlink();
	}

	public void StartBlink()
	{
		if (!animateBlink) { animateBlink = true; }
	}

	private void AnimBlink()
	{
		if(!animateBlink) { return; }

		if (animateBlinkInit)
		{
			animateBlinkTimeValue = 0;
			animateBlinkInit = false;
		}

		if(animateBlinkTimeValue > 0 && animateBlinkTimeValue < blinkDuration)
		{
			float EvaluatedTimeValue = blinkCurve.Evaluate(animateBlinkTimeValue / blinkDuration);

			for (int i = 0; i < subscribedImages.Length; i++)
			{
				subscribedImages[i].color = new Color(subscribedImages[i].color.r, subscribedImages[i].color.g, subscribedImages[i].color.b, Mathf.Lerp((1 - blinkDepth) * imageOpacities[i], imageOpacities[i], EvaluatedTimeValue));
			}
			for (int i = 0; i < subscribedTexts.Length; i++)
			{
				subscribedTexts[i].color = new Color(subscribedTexts[i].color.r, subscribedTexts[i].color.g, subscribedTexts[i].color.b, Mathf.Lerp((1 - blinkDepth) * textOpacities[i], textOpacities[i], EvaluatedTimeValue));
			}
		}
		else if(animateBlinkTimeValue > blinkDuration)
		{
			animateBlinkInit = true;
			animateBlink = false;
		}

		animateBlinkTimeValue += Time.deltaTime;
		Mathf.Clamp(animateBlinkTimeValue, 0, blinkDuration + 0.01f);
	}
}
