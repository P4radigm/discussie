using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DISelementBehaviour : MonoBehaviour
{
    public enum MaleFemaleType
	{
        male,
        female
	}

    public enum ConnnectionType
	{
        square,
        diamond,
        circle,
        hexagon,
        edge
	}

    public enum Colour
	{
        purple,
        green
	}

    [HideInInspector] public bool isDragged = false;
    [HideInInspector] public bool isAnimating = false;
    [HideInInspector] public bool isInConstructionZone = false;
    [HideInInspector] public bool queForDestruction = false;

    [Space(10)]
    public MaleFemaleType leftMF;
    public ConnnectionType leftConnector;
    [Space(20)]
    public MaleFemaleType rightMF;
    public ConnnectionType rightConnector;

    [HideInInspector] public DISelementBehaviour parentElement;
    [HideInInspector] public Vector3 offsetToParentElement;

    [Space(20)]
    public Transform ownConnectionPivot;

    [HideInInspector] public Colour colorType;
    [HideInInspector] public Color col;

    [HideInInspector] public Transform spawnAnchor;

    [HideInInspector] public int line;
    [HideInInspector] public DISgameplayManager manager;
    [HideInInspector] public DISgameplaySettings settings;
    [HideInInspector] public DISgameplayInput input;

    [HideInInspector] public List<DISelementBehaviour> connectedElements = new List<DISelementBehaviour>();

	private void OnEnable()
	{
        parentElement = this;
        offsetToParentElement = Vector3.zero;
        connectedElements.Add(this);
    }

	public void OnRelease()
	{
		//Check if element is within construction area Y = continue, N = AnimateBack
        if(transform.position.y <= settings.yBotEdgeConstruction || transform.position.y >= settings.yTopEdgeConstruction)
		{
			if (manager.elementsInConstructionArea.Contains(this.gameObject)) { manager.elementsInConstructionArea.Remove(this.gameObject); }
            //manager.elementsInConstructionArea.RemoveAll(null);
			//Element released outside of cunstruction area
			if (!isAnimating && spawnAnchor != null)
			{
                //Animate back to original position
                if(connectedElements.Count != 1) 
                {
					//Element is part of a group
					for (int i = 0; i < connectedElements.Count; i++)
					{
                        StartCoroutine(connectedElements[i].AnimateFadeOut());
					}
                }
                StartCoroutine(AnimateBack());                
            }
			else if (!isAnimating)
			{
                //Fade out, since anchor is outside of range
                for (int i = 0; i < connectedElements.Count; i++)
                {
                    StartCoroutine(connectedElements[i].AnimateFadeOut());
                }
            }
            return;
		}

        manager.elementsInConstructionArea.Add(this.gameObject);

        DISelementBehaviour elementToConnectTo = null;
        float rangeToPossibleElement = Mathf.Infinity;
        bool directionBool = false;
        //Search for components that could connect within connection range
        for (int i = 0; i < manager.elementsInConstructionArea.Count; i++)
		{
            DISelementBehaviour OtherEB = manager.elementsInConstructionArea[i].GetComponent<DISelementBehaviour>();

			if (connectedElements.Contains(OtherEB))
			{
                continue; //Either self or already connected element
			}
            
            if (Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position) > settings.connectionDistance)
			{
                continue; //The element is too far away
			}

            float dotLeft = Vector3.Dot((OtherEB.ownConnectionPivot.position - ownConnectionPivot.position).normalized, Vector3.left);
            bool isLeft = dotLeft > 0 ? true : false; //Checks if this element is left of the element its trying to connect to
            float dotUp = Vector3.Dot((OtherEB.ownConnectionPivot.position - ownConnectionPivot.position).normalized, Vector3.up);
            if (dotUp < settings.minDotUpRange && dotUp > settings.maxDotUpRange)
            {
                continue; //Angle to the piece is too far off
            }

            if (Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position) >= rangeToPossibleElement)
            {
                continue; //Another unconnected element is closer, so connecting to that one instead
            }

            if ((isLeft ? leftConnector : rightConnector) == (isLeft ? OtherEB.leftConnector : OtherEB.rightConnector) && (isLeft ? leftMF : rightMF) != (isLeft ? OtherEB.leftMF : OtherEB.rightMF) && (isLeft ? leftConnector : rightConnector) != ConnnectionType.edge)
            {
                //Connectors are compatible
                elementToConnectTo = OtherEB;
                directionBool = isLeft;
                rangeToPossibleElement = Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position);
            }
        }

        //Form connection
        if(elementToConnectTo != null)
		{
            //Animate to position
            //Form connection
            //Check for completion
            StartCoroutine(FormConnection(elementToConnectTo, directionBool));
		}
    }

	private void Update()
	{
		if (isDragged)
        {
			for (int i = 0; i < connectedElements.Count; i++)
			{
                connectedElements[i].StopAllCoroutines();
                connectedElements[i].GetComponent<SpriteRenderer>().color = connectedElements[i].col; 
            }
        }
	}

	private IEnumerator FormConnection(DISelementBehaviour connectElement, bool isLeft)
	{
		for (int i = 0; i < connectedElements.Count; i++)
		{
            connectedElements[i].CalculateOffsetToParent();
            connectedElements[i].isAnimating = true;
            Vector3 ReleasePosition = connectedElements[i].transform.position;
            int LeftRightMultiplier = isLeft ? 1 : -1;
            Vector3 ConnectPosition = connectElement.transform.position + new Vector3(settings.connectionDistance * (float)LeftRightMultiplier, 0, 0) + connectedElements[i].offsetToParentElement;
            //Animate to position
            float TimeValue = 0;

            while (TimeValue < 1)
            {
                if (connectElement.isDragged) { yield break; } //element this is trying to connect to is being moved
                TimeValue += Time.deltaTime / settings.snapToConnectedPositionDuration;
                float EvaluatedTimeValue = settings.snapToConnectedPositionCurve.Evaluate(TimeValue);
                Vector3 NewPos = Vector3.Lerp(ReleasePosition, ConnectPosition, EvaluatedTimeValue);

                connectedElements[i].transform.position = NewPos;

                yield return null;
            }

            connectedElements[i].isAnimating = false;
        }
        
        //Form connection
        List<DISelementBehaviour> NewConnectedElementsList = new();
		for (int i = 0; i < connectElement.connectedElements.Count; i++)
		{
            if (!NewConnectedElementsList.Contains(connectedElements[i])) { NewConnectedElementsList.Add(connectElement.connectedElements[i]); }
        }
        if (!NewConnectedElementsList.Contains(this)) { NewConnectedElementsList.Add(this); }

        int EdgeCounter = 0;
		for (int i = 0; i < NewConnectedElementsList.Count; i++)
		{
            NewConnectedElementsList[i].connectedElements = NewConnectedElementsList;
            NewConnectedElementsList[i].parentElement = connectElement;
            NewConnectedElementsList[i].CalculateOffsetToParent();
            if (NewConnectedElementsList[i].leftConnector == ConnnectionType.edge || NewConnectedElementsList[i].rightConnector == ConnnectionType.edge) { EdgeCounter++; }
        }

        //Check for completion
        if(EdgeCounter >= 2)
		{
			//complete balloon
			for (int i = 0; i < connectedElements.Count; i++)
			{
                connectedElements[i].PartOfCompletedGroup();
            }
		}
    }

    public void CalculateOffsetToParent()
	{
        offsetToParentElement = parentElement.transform.position - transform.position;
	}

    public void PartOfCompletedGroup()
	{
        Debug.Log("Completed a group");
	}

    public IEnumerator AnimateBack()
	{
		//Animate back to original position
        isAnimating = true;
        float TimeValue = 0;
        Vector3 ReleasePosition = transform.position;
        Vector3 AnchorPosition = spawnAnchor.position;

        while (TimeValue < 1)
        {
            TimeValue += Time.deltaTime / settings.animateBackToAnchorDuration;
            float EvaluatedTimeValue = settings.animateBackToAnchorCurve.Evaluate(TimeValue);
            Vector3 NewPos = Vector3.Lerp(ReleasePosition, AnchorPosition, EvaluatedTimeValue);
            transform.position = NewPos;

            yield return null;
        }

        isAnimating = false;
    }

    public IEnumerator AnimateFadeOut()
	{
        //Fade out
        isAnimating = true;
        float TimeValue = 0;
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        Color OriginalColor = SR.color;
        Color TransperantColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 0);

        while (TimeValue < 1)
        {
            TimeValue += Time.deltaTime / settings.animateBackToAnchorDuration;
            float EvaluatedTimeValue = settings.animateBackToAnchorCurve.Evaluate(TimeValue);
            Color NewCol = Color.Lerp(OriginalColor, TransperantColor, EvaluatedTimeValue);
            SR.color = NewCol;

            yield return null;
        }
        queForDestruction = true;
        isAnimating = false;
        this.enabled = false;
    }
}
