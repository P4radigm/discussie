using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RatingBehaviour : MonoBehaviour
{
    [SerializeField] private RectTransform targetGraphic;
    [Space(30)]
    [SerializeField] private RectTransform dottedLineUp;
    [SerializeField] private RectTransform dottedLineRight;
    [SerializeField] private RectTransform dottedLineDown;
    [SerializeField] private RectTransform dottedLineLeft;
    [SerializeField] private float dottedLineWidth;
    [Space(30)]

    [SerializeField] private GraphicRaycaster graphicsRaycaster;
    private PointerEventData pointerEventData;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private RectTransform raycastTarget;
    [SerializeField] private Button continueButton;
    [SerializeField] private BaseDefinition baseDefinition;
    [SerializeField] private BaseInputDefinition baseInputDefinition;
    private Canvas parentCanvas;
    private float parentCanvasScaleFactor;

    private Vector2 raycastTargetScreenPos;
	private void Start()
	{
        parentCanvas = GetComponentInParent<Canvas>();
        parentCanvasScaleFactor = parentCanvas.scaleFactor;
        raycastTargetScreenPos = Camera.main.WorldToScreenPoint(raycastTarget.position);
        //Debug.Log($"raycastTargetScreenPos = {raycastTargetScreenPos}");
    }

	void Update()
    {
        if(Input.touchCount == 0) { return; }

		for (int i = 0; i < Input.touches.Length; i++)
		{
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.GetTouch(i).position;

            List<RaycastResult> _results = new List<RaycastResult>();
            graphicsRaycaster.Raycast(pointerEventData, _results);

            foreach(RaycastResult result in _results)
			{
                //Debug.Log(result.gameObject);
                if(result.gameObject.tag == "RatingTarget")
				{
                    if (baseDefinition != null) { continueButton.gameObject.SetActive(true); continueButton.GetComponent<HintBlink>().Activate(); GetComponent<HintBlink>().DeActivate(); }               
                    //Debug.Log($"touchPos = {Input.GetTouch(i).position}, targetPos = {raycastTargetScreenPos}, relativePos = {Input.GetTouch(i).position - raycastTargetScreenPos}");
                    UpdateTargetPosition(Input.GetTouch(i).position - raycastTargetScreenPos);
                    //Debug.LogWarning("Hit Rating Target!");
                }
			}
		}
    }

    private void UpdateTargetPosition(Vector2 relativePos)
	{
        //Debug.Log(relativePos);
        //Vector2 _newTargetPos = Vector2.zero;
        Vector2 _newTargetPos = new Vector2(Mathf.Clamp(relativePos.x / parentCanvasScaleFactor, -200, 200), Mathf.Clamp(relativePos.y / parentCanvasScaleFactor, -200, 200));

        if(baseDefinition != null) { baseDefinition.rating = _newTargetPos / 200; }
        if(baseInputDefinition != null) { baseInputDefinition.ownRating = _newTargetPos / 200; }
        

        targetGraphic.anchoredPosition = _newTargetPos;

        //Update lines to match
        if(_newTargetPos.x > 0)
		{
            dottedLineRight.gameObject.SetActive(true);
            dottedLineLeft.gameObject.SetActive(false);
		}
		else
		{
            dottedLineLeft.gameObject.SetActive(true);
            dottedLineRight.gameObject.SetActive(false);
		}

        if (_newTargetPos.y > 0)
        {
            dottedLineUp.gameObject.SetActive(true);
            dottedLineDown.gameObject.SetActive(false);
        }
        else
        {
            dottedLineDown.gameObject.SetActive(true);
            dottedLineUp.gameObject.SetActive(false);
        }
        float _upLength = _newTargetPos.y - 22;
        dottedLineUp.anchoredPosition = new Vector2(_newTargetPos.x, _upLength / 2);
        dottedLineUp.sizeDelta = new Vector2(_upLength, dottedLineWidth);
        float _rightLength = _newTargetPos.x - 22;
        dottedLineRight.anchoredPosition = new Vector2(_rightLength / 2, _newTargetPos.y);
        dottedLineRight.sizeDelta = new Vector2(_rightLength, dottedLineWidth);
        float _downLength = _newTargetPos.y + 22;
        dottedLineDown.anchoredPosition = new Vector2(_newTargetPos.x, _downLength / 2);
        dottedLineDown.sizeDelta = new Vector2(Mathf.Abs(_downLength), dottedLineWidth);
        float leftLength = _newTargetPos.x + 22;
        dottedLineLeft.anchoredPosition = new Vector2(leftLength / 2, _newTargetPos.y);
        dottedLineLeft.sizeDelta = new Vector2(Mathf.Abs(leftLength), dottedLineWidth);

    }
}
