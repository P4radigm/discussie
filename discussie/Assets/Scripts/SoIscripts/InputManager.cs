using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
	#region Singleton
	public static InputManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
	#endregion

	public class InteractedUnit
	{
        public int attachedFingerID;
        public GameObject attachedUnit;
        public Vector2 hitOffsetVector;
	}

	public int currentTouchCount;
    private UnitManager uM;

    [SerializeField] private float timeTillEnd;
    [SerializeField] private float activationTime;
    [SerializeField] private float graceTime;
    [SerializeField] private Slider endTimerTop;
    [SerializeField] private Slider endTimerBot;
    private float endTimerValue;

    private List<InteractedUnit> intUnitList = new List<InteractedUnit>();

    // Start is called before the first frame update
    void Start()
    {
        uM = UnitManager.instance;
        endTimerValue = 0;
    }

    // Update is called once per frame
    void Update()
    {

        currentTouchCount = Input.touchCount + uM.amntColorsAtStart;
        uM.UpdateColors(currentTouchCount);

        if(GameManagerSoI.instance.gameState != GameManagerSoI.GameStates.playing) { return; }

        if(endTimerValue >= timeTillEnd + graceTime && uM.amountOfGroups != -420) 
        {
            //endTimerTop.gameObject.SetActive(false); 
            //endTimerBot.gameObject.SetActive(false); 
            GameManagerSoI.instance.StartEnd();
            GameManagerSoI.instance.gameState = GameManagerSoI.GameStates.end; 
        }

        if(Input.touchCount == 0) 
        {
            if(uM.amountOfGroups != -420)
			{
                //Update slider               
                endTimerTop.value = Mathf.Clamp(((endTimerValue - activationTime) / (timeTillEnd - activationTime)), 0f, 1f);
                endTimerBot.value = Mathf.Clamp(((endTimerValue - activationTime) / (timeTillEnd - activationTime)), 0f, 1f);
            }
            
            endTimerValue += Time.deltaTime;
            return; 
        }
        endTimerValue = 0;
        endTimerTop.value = 0;
        endTimerBot.value = 0;


        for (int i = 0; i < Input.touchCount; i++)
		{
            if(Input.GetTouch(i).phase == TouchPhase.Began)
			{
                //Check if a unit was hit
                Ray _Ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                RaycastHit2D hit = Physics2D.Raycast(_Ray.origin, _Ray.direction);
                if(hit == true && hit.collider != null)
                {
                    //Pair the unit & finger
                    InteractedUnit _NewInteraction = new();
                    _NewInteraction.attachedFingerID = Input.GetTouch(i).fingerId;
                    _NewInteraction.attachedUnit = hit.collider.gameObject;
                    Vector3 _ScreenTouchVector = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                    Vector3 _UnitPosition = hit.collider.transform.position;
                    _NewInteraction.hitOffsetVector = new Vector2(_ScreenTouchVector.x - _UnitPosition.x, _ScreenTouchVector.y - _UnitPosition.y);

                    intUnitList.Add(_NewInteraction);
                    //Update isDragged status of Unit
                    hit.collider.gameObject.GetComponent<UnitBehaviour>().isDragged = true;
                    //disable unit collider?
                }            
			}
            else if(Input.GetTouch(i).phase == TouchPhase.Moved)
			{
                //Check if the touch is paired to a unit
                int _CurrentUnitPairing = 999;
				for (int j = 0; j < intUnitList.Count; j++)
				{
                    if(Input.GetTouch(i).fingerId == intUnitList[j].attachedFingerID)
					{
                        _CurrentUnitPairing = j;
                    }
				}
                if (_CurrentUnitPairing != 999)
				{
                    //Update paired position w/ offset
                    UpdateUnitPosition(_CurrentUnitPairing, i);
				}
                
			}
            else if(Input.GetTouch(i).phase == TouchPhase.Ended)
			{

                //Remove link from list
                int _CurrentUnitPairing = 999;
                for (int j = 0; j < intUnitList.Count; j++)
                {
                    if (Input.GetTouch(i).fingerId == intUnitList[j].attachedFingerID)
                    {
                        _CurrentUnitPairing = j;
                    }
                }
                
                if (_CurrentUnitPairing != 999)
                {
                    //Update isDragged status of Unit
                    intUnitList[_CurrentUnitPairing].attachedUnit.GetComponent<UnitBehaviour>().isDragged = false;
                    //Form Group
                    intUnitList[_CurrentUnitPairing].attachedUnit.GetComponent<UnitBehaviour>().FormGroup();
                    //Set velocity zero
                    intUnitList[_CurrentUnitPairing].attachedUnit.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    //enable unit collider?

                    //Update paired position w/ offset
                    intUnitList.RemoveAt(_CurrentUnitPairing);
                }
                uM.CountGroups();
            }
		}
    }

	private void UpdateUnitPosition(int unitIndex, int currentFingerIndex)
	{
        Vector2 _TouchWorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(currentFingerIndex).position);
        intUnitList[unitIndex].attachedUnit.transform.position = new Vector3(_TouchWorldPos.x + intUnitList[unitIndex].hitOffsetVector.x, _TouchWorldPos.y + intUnitList[unitIndex].hitOffsetVector.y, 0);
	}
}
