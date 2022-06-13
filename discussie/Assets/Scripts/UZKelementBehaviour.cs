using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UZKelementBehaviour : MonoBehaviour
{
    //public int groupID;
    [NonReorderable]
    public List<Color> colorSequence;
    [HideInInspector] public bool isDragged = false;
    [Space(20)]
    public int identifierID;
    public int groupParentID;
    public int groupID;

    [HideInInspector] public UZKgameplayManager gameplayManager;
    //private InputManager iM;

    //Check For Group Stuff
    [HideInInspector] public float minGroupRadius;
    [HideInInspector] public bool springCollisionEnable;
    [HideInInspector] public float springLength;
    [HideInInspector] public float springDampingRatio;
    [HideInInspector] public float springFrequency;
    [HideInInspector] public float springBreakForce;
    [HideInInspector] public float springGracePeriod;


    public List<GameObject> directlyConnectedObjects = new List<GameObject>();

    //Unit Movement stuff
    //[SerializeField] private float convergingRadius;
    //[SerializeField] private float minConvergingRadius;
    //[SerializeField] private AnimationCurve convergingSpeedCurve;
    //[SerializeField] private float maxConvergingSpeed;

    //private List<GameObject> convergingObjects = new List<GameObject>();
    //private List<GameObject> connectedObjects = new List<GameObject>();

    void Update()
    {
        //limit position to screen area

        //attract to others of type overlapping in cretain range
        //UpdateConvergingObjects();

    }

    public void FormGroupOnRelease()
    {
        //Check if nearby unit(s) are available
        for (int u = 0; u < gameplayManager.elementsInPlay.Count; u++)
        {
            float _Distance = Vector2.Distance(new Vector2(gameplayManager.elementsInPlay[u].transform.position.x, gameplayManager.elementsInPlay[u].transform.position.y), new Vector2(transform.position.x, transform.position.y));
            if (_Distance < minGroupRadius)
            {
                if (gameplayManager.elementsInPlay[u] != this.gameObject && !directlyConnectedObjects.Contains(gameplayManager.elementsInPlay[u]))
                {
                    directlyConnectedObjects.Add(gameplayManager.elementsInPlay[u]);
					if (!gameplayManager.elementsInPlay[u].GetComponent<UZKelementBehaviour>().directlyConnectedObjects.Contains(gameObject))
					{
                        gameplayManager.elementsInPlay[u].GetComponent<UZKelementBehaviour>().directlyConnectedObjects.Add(gameObject);
                    }           
                }
            }
        }

        for (int u = 0; u < directlyConnectedObjects.Count; u++)
        {
            //Check for spring joints with newly paired unit
            bool _IsNotYetConnected = true;

            SpringJoint2D[] _OwnJoints = GetComponents<SpringJoint2D>();
            SpringJoint2D[] _OtherJoints = directlyConnectedObjects[u].GetComponents<SpringJoint2D>();

            //Check for self -> other
            for (int i = 0; i < _OwnJoints.Length; i++)
            {
                if (_OwnJoints[i].connectedBody == directlyConnectedObjects[u].GetComponent<Rigidbody2D>())
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
                StartCoroutine(CreateSpringJoint(directlyConnectedObjects[u]));
            }
        }

        gameplayManager.CountGroups();
    }

    private IEnumerator CreateSpringJoint(GameObject connectedObject)
	{
        SpringJoint2D _SpringJoint = gameObject.AddComponent<SpringJoint2D>();
        _SpringJoint.breakForce = Mathf.Infinity;
        _SpringJoint.autoConfigureConnectedAnchor = false;
        _SpringJoint.autoConfigureDistance = false;
        _SpringJoint.dampingRatio = springDampingRatio;
        _SpringJoint.frequency = springFrequency;
        _SpringJoint.distance = springLength;

        _SpringJoint.connectedBody = connectedObject.GetComponent<Rigidbody2D>();

        _SpringJoint.breakForce = Mathf.Infinity;
        _SpringJoint.enableCollision = springCollisionEnable;

        //Add the new edge to global list of edges
        Vector2Int newEdge = new Vector2Int(identifierID, connectedObject.GetComponent<UZKelementBehaviour>().identifierID);
        Vector2Int invertedEdge = new Vector2Int(newEdge.y, newEdge.x);
        if (!gameplayManager.edges.Contains(newEdge) || !gameplayManager.edges.Contains(invertedEdge)) { gameplayManager.edges.Add(newEdge); }

        yield return new WaitForSeconds(springGracePeriod);
        _SpringJoint.breakForce = springBreakForce;
    }

    public void FormGroupWhenNearby()
    {
        //Check if nearby unit(s) are available
        for (int u = 0; u < gameplayManager.elementsInPlay.Count; u++)
        {
            float _Distance = Vector2.Distance(new Vector2(gameplayManager.elementsInPlay[u].transform.position.x, gameplayManager.elementsInPlay[u].transform.position.y), new Vector2(transform.position.x, transform.position.y));
            if (_Distance <= minGroupRadius)
            {
                if (gameplayManager.elementsInPlay[u] != this.gameObject && !directlyConnectedObjects.Contains(gameplayManager.elementsInPlay[u]))
                {
                    directlyConnectedObjects.Add(gameplayManager.elementsInPlay[u]);
                    if (!gameplayManager.elementsInPlay[u].GetComponent<UZKelementBehaviour>().directlyConnectedObjects.Contains(gameObject))
                    {
                        gameplayManager.elementsInPlay[u].GetComponent<UZKelementBehaviour>().directlyConnectedObjects.Add(gameObject);
                    }
                }
            }
        }

        for (int u = 0; u < directlyConnectedObjects.Count; u++)
        {
            //Check for spring joints with newly paired unit
            bool _IsNotYetConnected = true;

            SpringJoint2D[] _OwnJoints = GetComponents<SpringJoint2D>();
            SpringJoint2D[] _OtherJoints = directlyConnectedObjects[u].GetComponents<SpringJoint2D>();

            //Check for self -> other
            for (int i = 0; i < _OwnJoints.Length; i++)
            {
                if (_OwnJoints[i].connectedBody == directlyConnectedObjects[u].GetComponent<Rigidbody2D>())
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
                StartCoroutine(CreateSpringJoint(directlyConnectedObjects[u]));
            }
        }

        gameplayManager.CountGroups();
    }

	private void OnJointBreak2D(Joint2D joint)
	{
        //Debug.Log($"1: joint broke on {gameObject} that was connected with {joint.connectedBody.gameObject}");
        joint.connectedBody.GetComponent<UZKelementBehaviour>().JointBroke(joint);

        JointBroke(joint);
    }

	public void JointBroke(Joint2D jointThatJustBroke)
	{
        //Remove this edge from the global edge list
        Vector2Int newEdge = new Vector2Int(identifierID, jointThatJustBroke.connectedBody.GetComponent<UZKelementBehaviour>().identifierID);
        Vector2Int invertedEdge = new Vector2Int(newEdge.y, newEdge.x);
        gameplayManager.edges.Remove(newEdge);
        gameplayManager.edges.Remove(invertedEdge);

        //Remove from directlyConnectedObjects
        directlyConnectedObjects.Remove(jointThatJustBroke.connectedBody.gameObject);
        jointThatJustBroke.connectedBody.GetComponent<UZKelementBehaviour>().directlyConnectedObjects.Remove(gameObject);

        gameplayManager.CountGroups();
    }
}
