using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UZKgameplayManager))]
[RequireComponent(typeof(UZKgameplaySettings))]
public class UZKgameplayInput : MonoBehaviour
{
    private UZKgameplayManager gameplayManager;
    [HideInInspector] public int currentTouchCount;
    [HideInInspector] public int lastFrameTouchCount;
    [HideInInspector] public bool fingerHitDoneButton;

    [SerializeField] private GraphicRaycaster graphicsRaycaster;
    private PointerEventData pointerEventData;
    [SerializeField] private EventSystem eventSystem;

    public class InteractedElement
    {
        public int attachedFingerID;
        public GameObject attachedUnit;
        public Vector2 hitOffsetVector;
    }

    private List<InteractedElement> intElementList = new List<InteractedElement>();

    public void StartPlaying()
	{
        gameplayManager = GetComponent<UZKgameplayManager>();
    }

	void Update()
    {
        currentTouchCount = Input.touchCount;
        if (currentTouchCount != lastFrameTouchCount)
        {
            gameplayManager.UpdateColors(currentTouchCount);
        }

        //public done button hit checker
        int _DoneButtonChecker = 0;

        for (int i = 0; i < Input.touchCount; i++)
        { 
            //Check if finger hit the 'done' button, if not continue as normal
            bool _fingerHitDoneButton = false;

            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.GetTouch(i).position;

            List<RaycastResult> _results = new List<RaycastResult>();
            graphicsRaycaster.Raycast(pointerEventData, _results);

            foreach (RaycastResult result in _results)
            {
                //Debug.Log($"Raycastresult = {result.gameObject}");
                if (result.gameObject.tag == "DoneButton")
                {
                    _fingerHitDoneButton = true;
                }
            }
            
            if (_fingerHitDoneButton)
			{
                //do nothing with this input
                _DoneButtonChecker++;
            }
            else if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                

                //Check if a unit was hit
                Ray _Ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                RaycastHit2D hit = Physics2D.Raycast(_Ray.origin, _Ray.direction);
                if (hit == true && hit.collider != null)
                {
                    //Pair the unit & finger
                    InteractedElement _NewInteraction = new();
                    _NewInteraction.attachedFingerID = Input.GetTouch(i).fingerId;
                    _NewInteraction.attachedUnit = hit.collider.gameObject;
                    Vector3 _ScreenTouchVector = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                    Vector3 _UnitPosition = hit.collider.transform.position;
                    _NewInteraction.hitOffsetVector = new Vector2(_ScreenTouchVector.x - _UnitPosition.x, _ScreenTouchVector.y - _UnitPosition.y);

                    intElementList.Add(_NewInteraction);
                    //Update isDragged status of Unit
                    hit.collider.gameObject.GetComponent<UZKelementBehaviour>().isDragged = true;
                    //disable unit collider?
                }
            }
            else if (Input.GetTouch(i).phase == TouchPhase.Moved)
            {
                //Check if the touch is paired to a unit
                int _CurrentUnitPairing = 999;
                for (int j = 0; j < intElementList.Count; j++)
                {
                    if (Input.GetTouch(i).fingerId == intElementList[j].attachedFingerID)
                    {
                        _CurrentUnitPairing = j;
                    }
                }
                if (_CurrentUnitPairing != 999)
                {
                    //Update paired position w/ offset
                    UpdateElementPosition(_CurrentUnitPairing, i);
                }

            }
            else if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {

                //Remove link from list
                int _CurrentUnitPairing = 999;
                for (int j = 0; j < intElementList.Count; j++)
                {
                    if (Input.GetTouch(i).fingerId == intElementList[j].attachedFingerID)
                    {
                        _CurrentUnitPairing = j;
                    }
                }

                if (_CurrentUnitPairing != 999)
                {
                    //Update isDragged status of Unit
                    intElementList[_CurrentUnitPairing].attachedUnit.GetComponent<UZKelementBehaviour>().isDragged = false;
                    //Form Group
                    intElementList[_CurrentUnitPairing].attachedUnit.GetComponent<UZKelementBehaviour>().FormGroupOnRelease();
                    //Set velocity zero
                    intElementList[_CurrentUnitPairing].attachedUnit.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    //enable unit collider?

                    //Update paired position w/ offset
                    intElementList.RemoveAt(_CurrentUnitPairing);
                }

                gameplayManager.CountGroups();
                gameplayManager.LetElementsGroupWhenNearby();
            }
        }

        fingerHitDoneButton = _DoneButtonChecker == 0 ? false : true;

        lastFrameTouchCount = currentTouchCount;
    }

    private void UpdateElementPosition(int unitIndex, int currentFingerIndex)
    {
        Vector2 _TouchWorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(currentFingerIndex).position);
        intElementList[unitIndex].attachedUnit.transform.position = new Vector3(_TouchWorldPos.x - intElementList[unitIndex].hitOffsetVector.x, _TouchWorldPos.y - intElementList[unitIndex].hitOffsetVector.y, 0);
    }
}
