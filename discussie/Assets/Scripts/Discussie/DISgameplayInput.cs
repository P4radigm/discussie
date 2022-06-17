using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DISgameplayInput : MonoBehaviour
{
    private DISgameplayManager gameplayManager;
    private DISgameplaySettings settings;

    public class InteractedElement
    {
        public int attachedFingerID;
        public GameObject attachedElement;
        public Vector2 hitOffsetVector;
    }

    private List<InteractedElement> interactingElements = new List<InteractedElement>();

    public void StartUp()
    {
        gameplayManager = GetComponent<DISgameplayManager>();
        settings = GetComponent<DISgameplaySettings>();
    }

    private void Update()
    {
		if (Input.touchCount == 0)
		{
            //No finger is touching the screen
            return;
		}

		for (int i = 0; i < Input.touchCount; i++)
		{
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                //Check if a unit was hit
                Ray _Ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                RaycastHit2D hit = Physics2D.Raycast(_Ray.origin, _Ray.direction);
                if (hit == true && hit.collider != null)
                {
                    DISelementBehaviour EB = hit.collider.GetComponent<DISelementBehaviour>();
                    if (EB != null && EB.enabled == true)
					{
                        
                        //Pair the unit & finger
                        InteractedElement _NewInteraction = new();
                        _NewInteraction.attachedFingerID = Input.GetTouch(i).fingerId;
                        _NewInteraction.attachedElement = hit.collider.gameObject;
                        Vector3 _ScreenTouchVector = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                        Vector3 _UnitPosition = hit.collider.transform.position;
                        _NewInteraction.hitOffsetVector = new Vector2(_ScreenTouchVector.x - _UnitPosition.x, _ScreenTouchVector.y - _UnitPosition.y);

                        interactingElements.Add(_NewInteraction);
						//Update isDragged status of Unit
						for (int e = 0; e < EB.connectedElements.Count; e++)
						{
                            EB.connectedElements[e].isDragged = true;
                        }
                        
                        if(EB.parentElement != EB)
						{
							for (int j = 0; j < EB.connectedElements.Count; j++)
							{
                                EB.connectedElements[j].parentElement = EB;
                                EB.connectedElements[j].CalculateOffsetToParent();
                            }
						}
                        //disable unit collider?
                    }
                }
            }
            else if (Input.GetTouch(i).phase == TouchPhase.Moved)
            {
                //Check if the touch is paired to a unit
                int _CurrentUnitPairing = 999;
                for (int j = 0; j < interactingElements.Count; j++)
                {
                    if (Input.GetTouch(i).fingerId == interactingElements[j].attachedFingerID)
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
                for (int j = 0; j < interactingElements.Count; j++)
                {
                    if (Input.GetTouch(i).fingerId == interactingElements[j].attachedFingerID)
                    {
                        _CurrentUnitPairing = j;
                    }
                }

                if (_CurrentUnitPairing != 999)
                {
                    DISelementBehaviour EB = interactingElements[_CurrentUnitPairing].attachedElement.GetComponent<DISelementBehaviour>();
                    for (int e = 0; e < EB.connectedElements.Count; e++)
                    {
                        //Update isDragged status of Unit
                        EB.connectedElements[e].isDragged = false;
                        //Set velocity zero
                        EB.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    }

                    for (int e = 0; e < EB.connectedElements.Count; e++)
                    {
                        //Form Group
                        EB.connectedElements[e].OnRelease();
                    }

                    //Update paired position w/ offset
                    interactingElements.RemoveAt(_CurrentUnitPairing);
                }
            }
        }
    }
    private void UpdateElementPosition(int unitIndex, int currentFingerIndex)
    {
        DISelementBehaviour EB = interactingElements[unitIndex].attachedElement.GetComponent<DISelementBehaviour>();
        Vector2 _TouchWorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(currentFingerIndex).position);

        for (int i = 0; i < EB.connectedElements.Count; i++)
		{
            EB.connectedElements[i].transform.position = new Vector3(_TouchWorldPos.x - interactingElements[unitIndex].hitOffsetVector.x, _TouchWorldPos.y - interactingElements[unitIndex].hitOffsetVector.y, 0) + EB.connectedElements[i].offsetToParentElement;
        }
    }
}
