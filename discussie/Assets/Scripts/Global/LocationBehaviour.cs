using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class LocationBehaviour : MonoBehaviour
{
	private DataManager dM;
	[SerializeField] private BaseLocation bL;

	[Header("Element spawn settings")]
	[SerializeField] private Transform locationParentObject;
	[SerializeField] private GameObject textElementPrefab;
	[SerializeField] private float textElementWidth;
	private List<GameObject> textElements = new List<GameObject>();
	private List<float> textElementXpositions = new List<float>();
	private Vector2 initialTouchPos;

	[Header("Other settings")]
	[SerializeField] private GraphicRaycaster graphicsRaycaster;
	private PointerEventData pointerEventData;
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private RectTransform raycastTarget;
	[SerializeField] private Button continueButton;
	private int parentedTouchId = -1;
	[SerializeField] private TextMeshProUGUI staticText;
	[SerializeField] private float alphaZeroDistance;
	[SerializeField] private AnimationCurve colorChangeCurve;
	[SerializeField] private Vector2 middleRange;

	[Header("Animation settings")]
	[SerializeField] private float toCenterDuration;
	[SerializeField] private AnimationCurve toCenterCurve;

	private bool animateToCenterInit = true;
	private bool animateToCenter = false;
	private float animateToCenterTimeValue = 0;
	private float distanceNeeded;
	private List<float> initialXPositions = new();

	//private Coroutine animateToCenterRoutine;
	//private Coroutine animateResidualForceRoutine;

	public void CreateTextElements()
	{
		dM = DataManager.instance;

		float newXpos = 0;
		for (int i = 0; i < bL.selectableLocations.Length; i++)
		{
			Vector3 position = new Vector3(newXpos, 0, 0);

			//Spawn element
			GameObject newElement = Instantiate(textElementPrefab, Vector3.zero, Quaternion.identity, locationParentObject);
			newElement.name = $"{bL.selectableLocations[i].location}";
			newElement.GetComponent<RectTransform>().anchoredPosition = new Vector2(newXpos, 0);
			newElement.GetComponent<TextMeshProUGUI>().text = bL.selectableLocations[i].location;
			textElements.Add(newElement);

			//Calc new position
			newXpos += textElementWidth;
			if(newXpos >= (float)Mathf.CeilToInt((float)bL.selectableLocations.Length / 2f) * textElementWidth)
			{
				newXpos = (float)Mathf.FloorToInt((float)bL.selectableLocations.Length / 2f) * textElementWidth * -1f;
			}
		}

		UpdateTextElementColors();
		UpdateLidwoord();
	}

	private void Update()
	{
		AnimToCenter();
		if (bL.active)
		{
			UpdateLidwoord();
			UpdateTextElementColors();
		}

		if (Input.touchCount == 0) 
		{
			//Check if an element is at 0, display continue button and set this as 
			bool isAtZero = false;
			for (int q = 0; q < textElements.Count; q++)
			{
				if(textElements[q].GetComponent<RectTransform>().anchoredPosition.x <= middleRange.y && textElements[q].GetComponent<RectTransform>().anchoredPosition.x >= middleRange.x)
				{
					isAtZero = true;
				}
			}

			if (isAtZero)
			{
				continueButton.gameObject.SetActive(true);
				dM.currentSaveData.location = bL.selectableLocations[getClosestElementToMiddleIndex()].location;
				
			}
			else
			{
				continueButton.gameObject.SetActive(false);
			}
			return; 
		}

		if (!bL.active) { return; }

		//Finger input
		for (int i = 0; i < Input.touches.Length; i++)
		{
			//Get parent finger
			if(Input.touches[i].phase == TouchPhase.Began)
			{
				pointerEventData = new PointerEventData(eventSystem);
				pointerEventData.position = Input.GetTouch(i).position;

				List<RaycastResult> _results = new List<RaycastResult>();
				graphicsRaycaster.Raycast(pointerEventData, _results);

				foreach (RaycastResult result in _results)
				{
					if (result.gameObject.tag == "RatingTarget" && parentedTouchId == -1)
					{
						//Finger has hit target
						parentedTouchId = Input.touches[i].fingerId;
						if(animateToCenter) { animateToCenter = false; animateToCenterInit = true; }
						continueButton.gameObject.SetActive(false);

						for (int t = 0; t < textElements.Count; t++)
						{
							textElementXpositions.Add(textElements[t].GetComponent<RectTransform>().anchoredPosition.x);
						}
						initialTouchPos = Input.touches[i].position;
					}
				}
			}


			//Textelements follow parent finger
			if(parentedTouchId != -1 && parentedTouchId == Input.touches[i].fingerId)
			{
				switch (Input.touches[i].phase)
				{
					case TouchPhase.Began:
						//Store initial starting point
						break;
					case TouchPhase.Moved:
						//Update positions of text elements to match movement from finger
						for (int t = 0; t < textElements.Count; t++)
						{
							float predictedNewXpos = textElementXpositions[t] + Input.touches[i].position.x - initialTouchPos.x;
							float newXpos;
							if(predictedNewXpos > (float)(bL.selectableLocations.Length / 2) * textElementWidth)
							{
								//Out of upper limit
								newXpos = (float)(bL.selectableLocations.Length / 2) * -textElementWidth + (predictedNewXpos - (float)(bL.selectableLocations.Length / 2) * textElementWidth);
							}
							else if (predictedNewXpos < (float)(bL.selectableLocations.Length / 2) * -textElementWidth)
							{
								//Out of lower limit
								newXpos = (float)(bL.selectableLocations.Length / 2) * textElementWidth + (predictedNewXpos + (float)(bL.selectableLocations.Length / 2) * textElementWidth);
							}
							else
							{
								//In limits
								newXpos = predictedNewXpos;
							}

							textElements[t].GetComponent<RectTransform>().anchoredPosition = new Vector2(newXpos, 0);
						}
						break;
					case TouchPhase.Stationary:
						//Do nothing
						break;
					case TouchPhase.Ended:
						//Calculate speed of movement before 
						//Initiate residual force animation -> Initiate Animate closest text element to center
						parentedTouchId = -1;
						textElementXpositions.Clear();
						StartAnimateToCenter();
						break;
					case TouchPhase.Canceled:
						//Do nothing
						parentedTouchId = -1;
						textElementXpositions.Clear();
						StartAnimateToCenter();
						break;
					default:
						break;
				}
			}
		}
		
	}

	private void StartAnimateToCenter()
	{
		animateToCenterInit = true;
		animateToCenter = true;
	}

	private void AnimToCenter()
	{
		if(!animateToCenter) { return; }

		if (animateToCenterInit)
		{
			distanceNeeded = -textElements[getClosestElementToMiddleIndex()].GetComponent<RectTransform>().anchoredPosition.x;

			initialXPositions.Clear();

			for (int i = 0; i < textElements.Count; i++)
			{
				initialXPositions.Add(textElements[i].GetComponent<RectTransform>().anchoredPosition.x);
			}

			animateToCenterTimeValue = 0;
			animateToCenterInit = false;
		}

		if(animateToCenterTimeValue > 0 && animateToCenterTimeValue <= toCenterDuration)
		{
			float EvaluatedTimeValue = toCenterCurve.Evaluate(animateToCenterTimeValue / toCenterDuration);

			for (int i = 0; i < textElements.Count; i++)
			{
				textElements[i].GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(new Vector2(initialXPositions[i], 0), new Vector2(initialXPositions[i] + distanceNeeded, 0), EvaluatedTimeValue);
			}
		}
		else if(animateToCenterTimeValue > toCenterDuration)
		{
			animateToCenterInit = true;
			animateToCenter = false;
		}

		animateToCenterTimeValue += Time.deltaTime;
		Mathf.Clamp(animateToCenterTimeValue, 0, toCenterDuration + 0.01f);

	}

	private void UpdateLidwoord()
	{
		staticText.text = "ik ben deze ervaring bij " + bL.selectableLocations[getClosestElementToMiddleIndex()].lidwoord + '\n' + '\n' + '\n' + "tegen gekomen";
	}

	private void UpdateTextElementColors() 
	{
		for (int i = 0; i < textElements.Count; i++)
		{
			float newAlpha = colorChangeCurve.Evaluate(Mathf.Clamp((alphaZeroDistance - Mathf.Abs(textElements[i].GetComponent<RectTransform>().anchoredPosition.x)) / alphaZeroDistance, 0f, 1f));
			textElements[i].GetComponent<TextMeshProUGUI>().color = new Color(textElements[i].GetComponent<TextMeshProUGUI>().color.r, textElements[i].GetComponent<TextMeshProUGUI>().color.g, textElements[i].GetComponent<TextMeshProUGUI>().color.b, newAlpha);
		}
	}

	private int getClosestElementToMiddleIndex()
	{
		int returnObject = -1;
		float closest = Mathf.Infinity;
		for (int i = 0; i < textElements.Count; i++)
		{
			if(Mathf.Abs(textElements[i].GetComponent<RectTransform>().anchoredPosition.x) < closest)
			{
				closest = Mathf.Abs(textElements[i].GetComponent<RectTransform>().anchoredPosition.x);
				returnObject = i;
			}
		}
		return returnObject;
	}
}
