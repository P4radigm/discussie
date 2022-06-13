using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    [HideInInspector] public int groupID;
    [NonReorderable] public Color[] color;
    [HideInInspector] public bool isDragged = false;
    public int sortedGroupID;
    //public float Speed;

    private UnitManager uM;
    private InputManager iM;

    //Check For Group Stuff
    [Header("Spring Joint Stuff")]
    [SerializeField] private float minGroupRadius;
    [Space(30)]
    [SerializeField] private bool springCollisionEnable;
    [SerializeField] private float springLength;
    [SerializeField] private float springDampingRatio;
    [SerializeField] private float springFrequency;
    [SerializeField] private float springBreakForce;
    

    [HideInInspector] public List<GameObject> groupedObjects = new List<GameObject>();

    //Unit Movement stuff
    //[SerializeField] private float convergingRadius;
    //[SerializeField] private float minConvergingRadius;
    //[SerializeField] private AnimationCurve convergingSpeedCurve;
    //[SerializeField] private float maxConvergingSpeed;

    //private List<GameObject> convergingObjects = new List<GameObject>();
    //private List<GameObject> connectedObjects = new List<GameObject>();

    void Start()
    {
        uM = UnitManager.instance;
        iM = InputManager.instance;
    }

    void Update()
    {
        //limit position to screen area

        //attract to others of type overlapping in cretain range
        //UpdateConvergingObjects();

    }

    public void FormGroup()
	{
        //uM.FixGrouping(sortedGroupID, uM.groupIDnumber);

        groupedObjects.Clear();
        //Check if nearby unit(s) are available
        for (int u = 0; u < uM.units.Count; u++)
        {
            float _Distance = Vector2.Distance(new Vector2(uM.units[u].transform.position.x, uM.units[u].transform.position.y), new Vector2(transform.position.x, transform.position.y));
            if (_Distance < minGroupRadius)
            {
                if(uM.units[u] != this.gameObject)
				{
                    groupedObjects.Add(uM.units[u]);
                    sortedGroupID = uM.units[u].GetComponent<UnitBehaviour>().sortedGroupID;
                }             
            }
        }
		//Connect nearby unit(s) (spring joint 2d + update group var)
		
        if(groupedObjects.Count == 0)
        {
            //Set new sortedGroupID
            sortedGroupID = uM.groupIDnumber;
            uM.groupIDnumber++;
        }

		for (int u = 0; u < groupedObjects.Count; u++)
		{
            if(groupedObjects[u].GetComponent<UnitBehaviour>().sortedGroupID != sortedGroupID)
			{
                uM.FixGrouping(groupedObjects[u].GetComponent<UnitBehaviour>().sortedGroupID, sortedGroupID);
            }

            //groupedObjects[u].GetComponent<UnitBehaviour>().sortedGroupID = sortedGroupID;
            //Check for spring joints with newly paired unit
            bool _IsNotYetConnected = true;

            SpringJoint2D[] _OwnJoints = GetComponents<SpringJoint2D>();
            SpringJoint2D[] _OtherJoints = groupedObjects[u].GetComponents<SpringJoint2D>();

            //Check for self -> other
            for (int i = 0; i < _OwnJoints.Length; i++)
            {
                if (_OwnJoints[i].connectedBody == groupedObjects[u].GetComponent<Rigidbody2D>())
				{
                    _IsNotYetConnected = false;
                }
			}
            //Check for other -> self
			for (int i = 0; i < _OtherJoints.Length; i++)
			{
				if (_OtherJoints[i].connectedBody == GetComponent<Rigidbody2D>())
				{
                    _IsNotYetConnected = false;
                }
			}

            //Make spring joint
            if (_IsNotYetConnected)
			{
                SpringJoint2D _SpringJoint = gameObject.AddComponent<SpringJoint2D>();
                _SpringJoint.breakForce = Mathf.Infinity;
                _SpringJoint.autoConfigureConnectedAnchor = false;
                _SpringJoint.autoConfigureDistance = false;
                _SpringJoint.dampingRatio = springDampingRatio;
                _SpringJoint.frequency = springFrequency;
                _SpringJoint.distance = springLength;

                _SpringJoint.connectedBody = groupedObjects[u].GetComponent<Rigidbody2D>();

                _SpringJoint.breakForce = springBreakForce;
                _SpringJoint.enableCollision = springCollisionEnable;

            }

            //Add potential rest of group to list
   //         UnitBehaviour _CurUnitBehaviour = groupedObjects[u].GetComponent<UnitBehaviour>();

   //         for (int i = 0; i < _CurUnitBehaviour.groupedObjects.Count; i++)
			//{
   //             bool _IsNotInList = true;
			//	for (int j = 0; j < groupedObjects.Count; j++)
			//	{
   //                 if (_CurUnitBehaviour.groupedObjects[i] == groupedObjects[j]) { _IsNotInList = false; }
   //             }

			//	if (_IsNotInList)
			//	{
   //                 groupedObjects.Add(_CurUnitBehaviour.groupedObjects[i]);
   //             }
			//}
        }

        

		//??Recalculate every unit's groups??
		//for (int i = 0; i < groupedObjects.Count; i++)
		//{
  //          groupedObjects[i].GetComponent<UnitBehaviour>().FormGroup();
  //      }
    }

    public void UpdateGroupedListComplete()
	{

	}

    //   private void UpdateConvergingObjects()
    //{
    //       //Calculate middle point of relevant units  
    //       for (int i = 0; i < uM.units.Count; i++)
    //       {
    //           float _Distance = Vector2.Distance(new Vector2(uM.units[i].transform.position.x, uM.units[i].transform.position.y), new Vector2(transform.position.x, transform.position.y));
    //           if (_Distance < convergingRadius && color[iM.currentTouchCount] == uM.units[i].GetComponent<UnitBehaviour>().color[iM.currentTouchCount])
    //           {
    //               convergingObjects.Add(uM.units[i]);
    //           }
    //       }
    //       if(convergingObjects.Count == 0) { return; }
    //       //Vector2 _MiddlePoint = new Vector2(transform.position.x, transform.position.y);
    //       Vector2 _MiddlePoint = Vector2.zero;
    //       for (int j = 0; j < convergingObjects.Count; j++)
    //       {
    //           _MiddlePoint += new Vector2(convergingObjects[j].transform.position.x, convergingObjects[j].transform.position.y);
    //       }

    //       _MiddlePoint = _MiddlePoint / (convergingObjects.Count + 1);
    //       Debug.DrawLine(transform.position, new Vector3(_MiddlePoint.x, _MiddlePoint.y, 0), Color.red);


    //       //Move to middle point
    //       if (isDragged) { GetComponent<Rigidbody2D>().velocity = Vector2.zero; return; }

    //       float _DistanceToMiddle = Vector2.Distance(_MiddlePoint, new Vector2(transform.position.x, transform.position.y));
    //       float _EvaluatedSpeed = convergingSpeedCurve.Evaluate((_DistanceToMiddle - minConvergingRadius)/(convergingRadius - minConvergingRadius)) * maxConvergingSpeed;
    //       Vector2 _DirectionVector = new Vector2(_MiddlePoint.x - transform.position.x, _MiddlePoint.y - transform.position.y).normalized;
    //       GetComponent<Rigidbody2D>().velocity = new Vector2(_DirectionVector.x * _EvaluatedSpeed, _DirectionVector.y * _EvaluatedSpeed * Time.deltaTime);
    //       Speed = _EvaluatedSpeed * Time.deltaTime;
    //       convergingObjects.Clear();
    //   }

    //private void OnDrawGizmosSelected()
    //{
    //       Gizmos.color = Color.green;
    //       Gizmos.DrawWireSphere(transform.position, minConvergingRadius);
    //       Gizmos.color = Color.magenta;
    //       Gizmos.DrawWireSphere(transform.position, convergingRadius);
    //   }
}
