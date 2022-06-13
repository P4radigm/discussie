using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightColorManager : MonoBehaviour
{
    #region Singleton
    public static HighlightColorManager instance;

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

    public enum ColorDistribution
	{
        sequential,
        random,
        randomNoDuplicates
	}

    [NonReorderable]
    public Color[] highlightColorList;
    [SerializeField] private ColorDistribution distributionKind;
    private int returnIndex = 0;

	private void Start()
	{
        returnIndex = highlightColorList.Length - 1;
        ChooseNextIndex();
    }

	public Color getHighlightColor()
    {
        ChooseNextIndex();
        return highlightColorList[returnIndex];
    }

    public string GetHighlightHex()
	{
        return ColorUtility.ToHtmlStringRGB(getHighlightColor());
    }

    private void ChooseNextIndex()
	{
		switch (distributionKind)
		{
			case ColorDistribution.sequential:
                returnIndex++;
                if(returnIndex >= highlightColorList.Length) { returnIndex = 0; }
                break;
			case ColorDistribution.random:
                returnIndex = Random.Range(0, highlightColorList.Length - 1);
				break;
            case ColorDistribution.randomNoDuplicates:
                int oldIndex = returnIndex;
                List<int> choosableIndeces = new List<int>();
				for (int i = 0; i < highlightColorList.Length; i++)
				{
                    choosableIndeces.Add(i);
                }
                choosableIndeces.RemoveAt(returnIndex);
                returnIndex = choosableIndeces[Random.Range(0, choosableIndeces.Count - 1)];
                break;
			default:
                returnIndex = 0;
                Debug.LogWarning("Unknown color distribution");
                break;
		}
	}
}
