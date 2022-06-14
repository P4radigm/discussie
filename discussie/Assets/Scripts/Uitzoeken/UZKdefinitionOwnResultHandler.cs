using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseEnd))]
public class UZKdefinitionOwnResultHandler : MonoBehaviour
{
    [SerializeField] private UZKgameplaySettings playCSettings;
    private BaseEnd baseEnd;
    private Vector2[] resultCoords;
    [SerializeField] private GameObject displayElementPrefab;
    private List<GameObject> displayElements = new List<GameObject>();
    [SerializeField] private Color displayElementColor;

    [Header("AnimateInSettings")]
    private Coroutine animateInRoutine;
    [Header("AnimateOutSettings")]
    private Coroutine animateOutRoutine;

    public void StartUp()
    {
        baseEnd = GetComponent<BaseEnd>();

        //interpret result string
        string serverString = baseEnd.results;
        if (!serverString.Contains('c') && !serverString.Contains('n')) { Debug.Log("def Coords string is invalid"); return; }
        resultCoords = DataManager.instance.GetComponent<UZKresultInterperter>().ConvertToUsableVar(serverString);

        //Spawn all display elements
		for (int i = 0; i < resultCoords.Length; i++)
		{
            GameObject _NewDisplayElement = Instantiate(displayElementPrefab, new Vector3(resultCoords[i].x, resultCoords[i].y, 1), Quaternion.identity);
            _NewDisplayElement.transform.SetParent(transform);
            _NewDisplayElement.name = $"DisplayElement {i}";
            _NewDisplayElement.transform.localScale = Vector3.one * playCSettings.ElementSize;
            _NewDisplayElement.GetComponent<SpriteRenderer>().color = new Color(displayElementColor.r, displayElementColor.g, displayElementColor.b, 0);
            _NewDisplayElement.GetComponent<SpriteRenderer>().enabled = true;
            displayElements.Add(_NewDisplayElement);
        }

        //Start to Animate display elements in
        if (animateInRoutine == null)
        {
            animateInRoutine = StartCoroutine(AnimateIn());
        }
    }

    private IEnumerator AnimateIn()
	{
        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / baseEnd.animateInDuration;
            float _evaluatedTimeValue = baseEnd.animateInCurve.Evaluate(_timeValue);

            for (int i = 0; i < displayElements.Count; i++)
            {
                float _newOpacity = Mathf.Lerp(0, displayElementColor.a, _evaluatedTimeValue);
                displayElements[i].GetComponent<SpriteRenderer>().color = new Color(displayElementColor.r, displayElementColor.g, displayElementColor.b, _newOpacity);
            }
            yield return null;
        }
        animateInRoutine = null;
    }

    public void CloseDown()
    {
        //Animate elements out
        if (animateOutRoutine == null)
        {
            animateOutRoutine = StartCoroutine(AnimateOut());
        }
    }

    private IEnumerator AnimateOut()
    {
        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / baseEnd.animateOutDuration;
            float _evaluatedTimeValue = baseEnd.animateOutCurve.Evaluate(_timeValue);

            for (int i = 0; i < displayElements.Count; i++)
            {
                float _newOpacity = Mathf.Lerp(displayElementColor.a, 0, _evaluatedTimeValue);
                displayElements[i].GetComponent<SpriteRenderer>().color = new Color(displayElementColor.r, displayElementColor.g, displayElementColor.b, _newOpacity);
            }
            yield return null;
        }
        animateOutRoutine = null;
    }
}
