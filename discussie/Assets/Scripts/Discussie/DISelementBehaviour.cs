using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DISelementBehaviour : MonoBehaviour
{
    public enum ConnnectionType
	{
        maleSquare,
        maleDiamond,
        maleCircle,
        maleHexagon,
        femaleSquare,
        femaleDiamond,
        femaleCircle,
        femaleHexagon,
        edge
	}

    public enum Colour
	{
        purple,
        green
	}

    [HideInInspector] public bool isDragged = false;

    public ConnnectionType leftConnector;
    public ConnnectionType rightConnector;

    public Transform connectionPivot;
    public Transform equalDistancePivot;

    public Colour col;

    public Transform spawnAnchor;

    public List<GameObject> connectedElements = new List<GameObject>();

    public void TryConnection()
	{
        //Search for components that could connect and connect to them

        //If connection is possible, animate to correct connection position
        //Form link

        //Check for completion
	}

    public void AnimateBack()
	{
        //placed outside of construction zone, so animate back to original position
	}
}
