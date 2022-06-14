using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseInputDefinition))]
public class UZKownResultHandler : MonoBehaviour
{
    [SerializeField] private UZKgameplaySettings playCSettings;
    private DataManager dM;
    private BaseInputDefinition bID;
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
        dM = DataManager.instance;
        bID = GetComponent<BaseInputDefinition>();

        //interpret result string
        string resultString = dM.currentSaveData.gameResult;
        if (!resultString.Contains('c') && !resultString.Contains('n')) { Debug.Log("def Coords string is invalid"); return; }
        resultCoords = DataManager.instance.GetComponent<UZKresultInterperter>().ConvertToUsableVar(resultString);

        //Spawn all display elements
        for (int i = 0; i < resultCoords.Length; i++)
        {
            GameObject _NewDisplayElement = Instantiate(displayElementPrefab, new Vector3(resultCoords[i].x, resultCoords[i].y, 1), Quaternion.identity);
            _NewDisplayElement.transform.SetParent(transform);
            _NewDisplayElement.name = $"DisplayElement {i}";
            _NewDisplayElement.transform.localScale = Vector3.one * playCSettings.ElementSize;
            _NewDisplayElement.GetComponent<SpriteRenderer>().color = displayElementColor;
            _NewDisplayElement.GetComponent<SpriteRenderer>().enabled = true;
            displayElements.Add(_NewDisplayElement);
        }
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
            _timeValue += Time.deltaTime / bID.animateNameOutDuration;
            float _evaluatedTimeValue = bID.animateNameOutCurve.Evaluate(_timeValue);

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
