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
    [HideInInspector] public bool hasSnapped = false;

    [Space(10)]
    public MaleFemaleType leftMF;
    public ConnnectionType leftConnector;
    [HideInInspector] public bool leftHasConnection = false;
    [Space(20)]
    public MaleFemaleType rightMF;
    public ConnnectionType rightConnector;
    [HideInInspector] public bool rightHasConnection = false;

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
        if(leftConnector == ConnnectionType.edge) { leftHasConnection = true; }
        if(rightConnector == ConnnectionType.edge) { rightHasConnection = true; }
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
                    return;
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

        isInConstructionZone = true;

        if (!manager.elementsInConstructionArea.Contains(this.gameObject))
        {
            manager.elementsInConstructionArea.Add(this.gameObject);
        }

        DISelementBehaviour elementToConnectTo = null;
        float rangeToPossibleElement = Mathf.Infinity;
        bool directionBool = false;
        //Search for components that could connect within connection range
        //Debug.Log($"{gameObject.name} is searching for a connection");
        for (int i = 0; i < manager.elementsInConstructionArea.Count; i++)
		{
            //Debug.Log($"{gameObject.name} is tryig a connection with {manager.elementsInConstructionArea[i].gameObject.name}");
            DISelementBehaviour OtherEB = manager.elementsInConstructionArea[i].GetComponent<DISelementBehaviour>();

			if (connectedElements.Contains(OtherEB))
			{
                if (OtherEB != this) 
                { 
                    //Debug.Log($"{gameObject.name} already shares a connection with {manager.elementsInConstructionArea[i].gameObject.name}");
                };
                continue; //Either self or already connected element
			}
            
            if (Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position) > settings.tryConnectionRange)
			{
                //Debug.Log($"{gameObject.name} is too far away from {manager.elementsInConstructionArea[i].gameObject.name}, {Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position)} > {settings.tryConnectionRange}");
                continue; //The element is too far away
			}

            float dotLeft = Vector3.Dot((OtherEB.ownConnectionPivot.position - ownConnectionPivot.position).normalized, Vector3.left);
            bool isLeft = dotLeft > 0 ? true : false; //Checks if this element is left of the element its trying to connect to
            float dotUp = Vector3.Dot((OtherEB.ownConnectionPivot.position - ownConnectionPivot.position).normalized, Vector3.up);
            if (dotUp < settings.minDotUpRange || dotUp > settings.maxDotUpRange)
            {
                //Debug.Log($"{gameObject.name} angled positions is too far off {manager.elementsInConstructionArea[i].gameObject.name}, {dotUp} < {settings.minDotUpRange} || {dotUp} > {settings.maxDotUpRange}");
                continue; //Angle to the piece is too far off
            }

            if ((isLeft ? leftConnector : rightConnector) != (isLeft ? OtherEB.rightConnector : OtherEB.leftConnector) || (isLeft ? leftMF : rightMF) == (isLeft ? OtherEB.rightMF : OtherEB.leftMF))
            {
                //Debug.Log($"{gameObject.name}'s connector is not compatible with {manager.elementsInConstructionArea[i].gameObject.name}, this connector = {(isLeft ? leftMF : rightMF)}.{(isLeft ? leftConnector : rightConnector)}, other connector = {(isLeft ? OtherEB.rightMF : OtherEB.leftMF)}.{(isLeft ? OtherEB.rightConnector : OtherEB.leftConnector)}");
                continue; //Another unconnected element is closer, so connecting to that one instead
            }

			//Check if already connected to another connector on this connection
            if((isLeft ? leftHasConnection : rightHasConnection))
			{
                //Debug.Log($"{gameObject.name} is trying to connect to a connector that has another connection already {manager.elementsInConstructionArea[i].gameObject.name}");
                continue; //The connection is already populated
            }

            if (Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position) >= rangeToPossibleElement)
            {
                //Debug.Log($"{gameObject.name} already has found an element that is closer that {manager.elementsInConstructionArea[i].gameObject.name}, {elementToConnectTo.gameObject.name} is closest");
                continue; //Another unconnected element is closer, so connecting to that one instead
            }

            Debug.Log($"{gameObject.name} has decided that it wants to connect to {manager.elementsInConstructionArea[i].gameObject.name}, this element is to its {(isLeft ? "left" : "right")}");
            //Connectors are compatible
            elementToConnectTo = OtherEB;
            directionBool = isLeft;
            rangeToPossibleElement = Vector3.Distance(OtherEB.ownConnectionPivot.position, ownConnectionPivot.position);
        }

        //Form connection
        if(elementToConnectTo != null)
		{
            //Animate to position
            //Form connection
            //Check for completion
            Debug.Log($"{gameObject.name} is starting FormConnection coroutine");
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
            connectedElements[i].parentElement = this;
            connectedElements[i].CalculateOffsetToParent(); //0
            connectedElements[i].isAnimating = true;
            Vector3 ReleasePosition = connectedElements[i].transform.position;
            int LeftRightMultiplier = isLeft ? 1 : -1;
            Vector3 ConnectPosition = connectElement.ownConnectionPivot.position + new Vector3((settings.connectionDistance * (float)LeftRightMultiplier) - (ownConnectionPivot.position.x - transform.position.x), 0, 0) + connectedElements[i].offsetToParentElement;
            //anchorpos of connectedTo + connectiondistance*leftRightint + anchorpos.localposition
            //Animate to position
            connectedElements[i].StartCoroutine(connectedElements[i].AnimateToSnapPosition(connectElement, ReleasePosition, ConnectPosition));
        }

        yield return new WaitForSeconds(settings.snapToConnectedPositionDuration);
        yield return null;
		for (int i = 0; i < connectedElements.Count; i++)
		{
            if (connectedElements[i].hasSnapped != true) { Debug.Log($"{connectedElements[i].gameObject.name} has not snapped"); yield break; };
            connectedElements[i].hasSnapped = false;
        }


        //Form connection
        List<DISelementBehaviour> NewConnectedElementsList = new();
		for (int i = 0; i < connectElement.connectedElements.Count; i++)
		{
            if (!NewConnectedElementsList.Contains(connectElement.connectedElements[i])) { NewConnectedElementsList.Add(connectElement.connectedElements[i]); }
        }
        for (int i = 0; i < connectedElements.Count; i++)
        {
            if (!NewConnectedElementsList.Contains(connectedElements[i])) { NewConnectedElementsList.Add(connectedElements[i]); }
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

		if (!isLeft)
		{
            connectElement.leftHasConnection = true;
            rightHasConnection = true;
        }
		else
		{
            connectElement.rightHasConnection = true;
            leftHasConnection = true;
        }

        //Check for completion
        if(EdgeCounter >= 2)
		{
			//complete balloon
			for (int i = 0; i < connectedElements.Count; i++)
			{
                connectedElements[i].StartCoroutine(connectedElements[i].PartOfCompletedGroup());
            }
		}
    }

    public void CalculateOffsetToParent()
	{
        //offsetToParentElement = parentElement.transform.position - transform.position;
        offsetToParentElement = transform.position - parentElement.transform.position;
	}

    public IEnumerator PartOfCompletedGroup()
	{
        //Disable collider
        GetComponent<Collider2D>().enabled = false;
        //Remove from all lists
        manager.elementsInConstructionArea.Remove(this.gameObject);
        manager.elementsInPlay.Remove(this.gameObject);
        manager.elementsInPlayBot.Remove(this.gameObject);
        manager.elementsInPlayTop.Remove(this.gameObject);

        //calc own score
        int ownColorElements = -1;
        int otherColorElements = 0;

        List<DISelementBehaviour> SortedList = new();
        SortedList.Add(connectedElements[0]);
		for (int i = 1; i < connectedElements.Count; i++)
		{
            float xpos = connectedElements[i].transform.position.x;
            if(xpos < SortedList[0].transform.position.x)
			{
                SortedList.Insert(0, connectedElements[i]);
                continue;
			}
            else if(xpos > SortedList[SortedList.Count - 1].transform.position.x)
			{
                SortedList.Add(connectedElements[i]);
                continue;
            }
			for (int j = 0; j < SortedList.Count; j++)
			{
                float curIndexXpos = SortedList[j].transform.position.x;
                float nextIndexXpos = SortedList[Mathf.Clamp(j + 1, 0, SortedList.Count - 1)].transform.position.x;
                if(xpos > curIndexXpos && xpos < nextIndexXpos)
				{
                    SortedList.Insert(j + 1, connectedElements[i]);
                    continue;
                }
			}
		}

		for (int i = 0; i < SortedList.Count; i++)
		{
            if(SortedList[i].colorType == colorType) { ownColorElements++; }
			else { otherColorElements++; }
		}
        float ownScoreFloat = settings.defaultElementScore + settings.defaultElementScore * (settings.sameColorMultiplier * (float)ownColorElements + settings.differentColorMultiplier * (float)otherColorElements);
        float otherScoreFloat = settings.defaultElementScore * settings.differentColorMultiplier * (float)otherColorElements;
        int ownScore = Mathf.RoundToInt(ownScoreFloat);
        int otherScore = Mathf.RoundToInt(otherScoreFloat);
        //Spawn vfx object
        GameObject vfxGOown = Instantiate(settings.burstToScoreParticles);
        DISelementParticleBehaviour vfxControllerown = vfxGOown.GetComponent<DISelementParticleBehaviour>();
        //set vfx
        vfxControllerown.spawnAmount = Mathf.RoundToInt((float)ownScore * 1.5f);
        vfxControllerown.parentElement = this;
        vfxControllerown.target = colorType == Colour.purple ? manager.bottomScoreDisplay : manager.topScoreDisplay;
        vfxControllerown.color = colorType == Colour.purple ? settings.botVFXColor : settings.topVFXColor;
        vfxControllerown.topBot = colorType == Colour.purple ? -1 : 1;
        vfxControllerown.InitVFX();

        //Spawn vfx object
        GameObject vfxGOother = Instantiate(settings.burstToScoreParticles);
        DISelementParticleBehaviour vfxControllerother = vfxGOother.GetComponent<DISelementParticleBehaviour>();
        //set vfx
        vfxControllerother.spawnAmount = Mathf.RoundToInt((float)otherScore * 1.5f);
        vfxControllerother.parentElement = this;
        vfxControllerother.target = colorType == Colour.purple ? manager.topScoreDisplay : manager.bottomScoreDisplay;
        vfxControllerother.color = colorType == Colour.purple ? settings.topVFXColor : settings.botVFXColor;
        vfxControllerother.topBot = colorType == Colour.purple ? 1 : -1;
        vfxControllerother.InitVFX();

        //disable spriterenderer
        GetComponent<SpriteRenderer>().enabled = false;

        //burst vfx
        vfxControllerown.FireVFX();
        vfxControllerother.FireVFX();

        //wait for vfx to hit
        yield return new WaitForSeconds(settings.vfxReachTime);
        
        //add to score
        if(colorType == Colour.purple) { manager.botScore += ownScore; manager.topScore += otherScore; }
		else { manager.topScore += ownScore; manager.botScore += otherScore; }

		//Add to string
		if (SortedList[0].parentElement == this)
		{
            manager.resultString += '-';
            for (int i = 0; i < SortedList.Count; i++)
			{
                manager.resultString += SortedList[i].colorType == Colour.purple ? 'b' : 't';
            }

            //Check if any pairs are still possible
            if (!settings.scrollEnabled) { manager.EdgeCheck(); }
		}

        //Destroy this object
        Destroy(this.gameObject);
    }

    public IEnumerator AnimateToSnapPosition(DISelementBehaviour connectElement, Vector3 ReleasePosition, Vector3 SnapPosition)
	{
        float TimeValue = 0;

        while (TimeValue < 1)
        {
			for (int i = 0; i < connectedElements.Count; i++)
			{
				if (connectedElements[i].isDragged) { isAnimating = false; Debug.Log($"{connectedElements[i].gameObject.name} is being dragged"); yield break; }
			}
			for (int i = 0; i < connectElement.connectedElements.Count; i++)
			{
                if (connectElement.connectedElements[i].isDragged) { isAnimating = false; Debug.Log($"{connectElement.connectedElements[i].gameObject.name} is being dragged"); yield break; }
            }
            TimeValue += Time.deltaTime / settings.snapToConnectedPositionDuration;
            float EvaluatedTimeValue = settings.snapToConnectedPositionCurve.Evaluate(TimeValue);
            Vector3 NewPos = Vector3.Lerp(ReleasePosition, SnapPosition, EvaluatedTimeValue);

            transform.position = NewPos;

            yield return null;
        }

        isAnimating = false;
        hasSnapped = true;
    }

    public IEnumerator AnimateBack()
	{
        manager.elementsInConstructionArea.Remove(this.gameObject);
        //Animate back to original position
        isAnimating = true;
        float TimeValue = 0;
        Vector3 ReleasePosition = transform.position;

        while (TimeValue < 1)
        {
            TimeValue += Time.deltaTime / settings.animateBackToAnchorDuration;
            float EvaluatedTimeValue = settings.animateBackToAnchorCurve.Evaluate(TimeValue);
            Vector3 NewPos = Vector3.Lerp(ReleasePosition, spawnAnchor.position, EvaluatedTimeValue);
            transform.position = NewPos;

            yield return null;
        }
       
        isAnimating = false;
        isInConstructionZone = false;
    }

    public IEnumerator AnimateFadeOut()
	{
        manager.elementsInConstructionArea.Remove(this.gameObject);
        //Fade out
        isAnimating = true;
        float TimeValue = 0;
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        Color OriginalColor = SR.color;
        Color TransperantColor = new Color(OriginalColor.r, OriginalColor.g, OriginalColor.b, 0);

        while (TimeValue < 1)
        {
            TimeValue += Time.deltaTime / settings.fadeOutDuration;
            float EvaluatedTimeValue = settings.fadeOutCurve.Evaluate(TimeValue);
            Color NewCol = Color.Lerp(OriginalColor, TransperantColor, EvaluatedTimeValue);
            SR.color = NewCol;

            yield return null;
        }
        queForDestruction = true;
        isAnimating = false;
        this.enabled = false;
    }

    public IEnumerator AnimateFadeIn(float duration, AnimationCurve curve)
    {
        //Fade out
        isAnimating = true;
        float TimeValue = 0;
        SpriteRenderer SR = GetComponent<SpriteRenderer>();
        Color OriginalColor = SR.color;

        while (TimeValue < 1)
        {
            TimeValue += Time.deltaTime / duration;
            float EvaluatedTimeValue = curve.Evaluate(TimeValue);
            Color NewCol = Color.Lerp(OriginalColor, col, EvaluatedTimeValue);
            SR.color = NewCol;

            yield return null;
        }
        isAnimating = false;
    }
}
