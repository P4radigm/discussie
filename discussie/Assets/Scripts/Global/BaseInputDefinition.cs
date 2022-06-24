using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class BaseInputDefinition : MonoBehaviour
{
    private DataManager dataManager;
    private Reformatter reformatter;
    private PlatformManager platformManager;
    private SequenceManager sequenceManager;
    private HighlightColorManager highlightColorManager;

    [SerializeField] private UnityEvent onDefinitionActivated;
    [SerializeField] private UnityEvent onDefinitionAnimatedIn;
    [SerializeField] private UnityEvent onDefinitionAnimateOut;
    [SerializeField] private UnityEvent onNameActivated;
    [SerializeField] private UnityEvent onNameAnimatedIn;
    [SerializeField] private UnityEvent onNameAnimateOut;
    [SerializeField] private UnityEvent onDeActivated;

    [Header("Input")]
    [SerializeField] private TMP_InputField definitionInputField;
    [SerializeField] private TextMeshProUGUI definitionDisplay;
    [SerializeField] private string definitionPlaceholder;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private string namePlaceholder;
    private string[] defHighlightCols = new string[10];
    private string sourceHighlightCol;


    [SerializeField] private Button sendDefButton;
    [SerializeField] private Button sendNameButton;
    
    [Space(30)]
    [SerializeField] private TextMeshProUGUI[] toBeAnimatedGlobalTexts;
    [SerializeField] private List<float> toBeAnimatedGlobalTextsOpacities;
    [SerializeField] private Image[] toBeAnimatedGlobalImages;
    [SerializeField] private List<float> toBeAnimatedGlobalImagesOpacities;

    [Header("Animate Def In")]
    private Coroutine defInRoutine;
    [SerializeField] private TextMeshProUGUI[] toBeAnimatedDefTexts;
    [SerializeField] private List<float> toBeAnimatedDefTextsOpacities;
    [SerializeField] private Image[] toBeAnimatedDefImages;
    [SerializeField] private List<float> toBeAnimatedDefImagesOpacities;
    [Space(10)]
    [SerializeField] private float animateDefInDuration;
    [SerializeField] private AnimationCurve animateDefInCurve;
    [Header("Animate Def Out")]
    private Coroutine defOutRoutine;
    [SerializeField] private float animateDefOutDuration;
    [SerializeField] private AnimationCurve animateDefOutCurve;
    [Header("Animate Name In")]
    [SerializeField] private TextMeshProUGUI[] toBeAnimatedNameTexts;
    [SerializeField] private List<float> toBeAnimatedNameTextsOpacities;
    [SerializeField] private Image[] toBeAnimatedNameImages;
    [SerializeField] private List<float> toBeAnimatedNameImagesOpacities;
    [Space(10)]
    private Coroutine nameInRoutine;
    [SerializeField] private float animateNameInDuration;
    [SerializeField] private AnimationCurve animateNameInCurve;
    [Header("Animate Name Out")]
    private Coroutine nameOutRoutine;
    public float animateNameOutDuration;
    public AnimationCurve animateNameOutCurve;
    private bool inTransition;

    [Header("Values")]
    private bool atDefinition = true;
    private bool delivered = false;

    public string ownDefinition = "";
    public string ownSource = "";
    public Vector2 ownRating = Vector2.one * -99;

    public void StartUp()
    {
        dataManager = DataManager.instance;
        reformatter = Reformatter.instance;
        platformManager = PlatformManager.instance;
        sequenceManager = SequenceManager.instance;
        highlightColorManager = HighlightColorManager.instance;

        //Generate random highloight colors
        for (int i = 0; i < defHighlightCols.Length; i++)
		{
            defHighlightCols[i] = highlightColorManager.GetHighlightHex();
        }
        sourceHighlightCol = highlightColorManager.GetHighlightHex();

        //Set all input stuff in correct state
        highlightColorManager = HighlightColorManager.instance;
        definitionInputField.gameObject.SetActive(false);
        definitionInputField.enabled = false;
        definitionInputField.GetComponent<Image>().raycastTarget = false;
        nameInputField.gameObject.SetActive(false);
        nameInputField.enabled = false;
        nameInputField.GetComponent<Image>().raycastTarget = false;

		//Get all original opacities set everything to transparant
		for (int i = 0; i < toBeAnimatedGlobalTexts.Length; i++)
		{
            toBeAnimatedGlobalTextsOpacities.Add(toBeAnimatedGlobalTexts[i].color.a);
            toBeAnimatedGlobalTexts[i].color = new Color(toBeAnimatedGlobalTexts[i].color.r, toBeAnimatedGlobalTexts[i].color.g, toBeAnimatedGlobalTexts[i].color.b, toBeAnimatedGlobalTexts[i].color.a);
        }
        for (int i = 0; i < toBeAnimatedGlobalImages.Length; i++)
        {
            toBeAnimatedGlobalImagesOpacities.Add(toBeAnimatedGlobalImages[i].color.a);
            toBeAnimatedGlobalImages[i].color = new Color(toBeAnimatedGlobalImages[i].color.r, toBeAnimatedGlobalImages[i].color.g, toBeAnimatedGlobalImages[i].color.b, toBeAnimatedGlobalImages[i].color.a);
        }

        for (int i = 0; i < toBeAnimatedDefTexts.Length; i++)
        {
            toBeAnimatedDefTextsOpacities.Add(toBeAnimatedDefTexts[i].color.a);
            toBeAnimatedDefTexts[i].color = new Color(toBeAnimatedDefTexts[i].color.r, toBeAnimatedDefTexts[i].color.g, toBeAnimatedDefTexts[i].color.b, toBeAnimatedDefTexts[i].color.a);
        }
        for (int i = 0; i < toBeAnimatedDefImages.Length; i++)
        {
            toBeAnimatedDefImagesOpacities.Add(toBeAnimatedDefImages[i].color.a);
            toBeAnimatedDefImages[i].color = new Color(toBeAnimatedDefImages[i].color.r, toBeAnimatedDefImages[i].color.g, toBeAnimatedDefImages[i].color.b, toBeAnimatedDefImages[i].color.a);
        }

        for (int i = 0; i < toBeAnimatedNameTexts.Length; i++)
        {
            toBeAnimatedNameTextsOpacities.Add(toBeAnimatedNameTexts[i].color.a);
            toBeAnimatedNameTexts[i].color = new Color(toBeAnimatedNameTexts[i].color.r, toBeAnimatedNameTexts[i].color.g, toBeAnimatedNameTexts[i].color.b, toBeAnimatedNameTexts[i].color.a);
        }
        for (int i = 0; i < toBeAnimatedNameImages.Length; i++)
        {
            toBeAnimatedNameImagesOpacities.Add(toBeAnimatedNameImages[i].color.a);
            toBeAnimatedNameImages[i].color = new Color(toBeAnimatedNameImages[i].color.r, toBeAnimatedNameImages[i].color.g, toBeAnimatedNameImages[i].color.b, toBeAnimatedNameImages[i].color.a);
        }

        //Animate In
        if (defInRoutine != null) { return; }
        StartCoroutine(AnimateDefIn());
    }

    void Update()
    {
        if(ownDefinition != "" && ownRating != Vector2.one * -99 && atDefinition && !inTransition) 
        { 
            sendDefButton.gameObject.SetActive(true); 
            sendNameButton.gameObject.SetActive(false);
        }
        else if (ownSource != "" && !atDefinition && !delivered && !inTransition)
		{
            sendDefButton.gameObject.SetActive(false); 
            sendNameButton.gameObject.SetActive(true);
        }
		else if(!inTransition)
		{
            sendDefButton.gameObject.SetActive(false);
            sendNameButton.gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateDefIn()
	{
        inTransition = true;
        onDefinitionActivated.Invoke();
        definitionInputField.gameObject.SetActive(true);
        definitionInputField.enabled = true;
        

        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / animateDefInDuration;
            float _evaluatedTimeValue = animateDefInCurve.Evaluate(_timeValue);

            for (int i = 0; i < toBeAnimatedGlobalTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedGlobalTextsOpacities[i], _evaluatedTimeValue);
                toBeAnimatedGlobalTexts[i].color = new Color(toBeAnimatedGlobalTexts[i].color.r, toBeAnimatedGlobalTexts[i].color.g, toBeAnimatedGlobalTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedDefTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedDefTextsOpacities[i], _evaluatedTimeValue);
                toBeAnimatedDefTexts[i].color = new Color(toBeAnimatedDefTexts[i].color.r, toBeAnimatedDefTexts[i].color.g, toBeAnimatedDefTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedGlobalImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedGlobalImagesOpacities[i], _evaluatedTimeValue);
                toBeAnimatedGlobalImages[i].color = new Color(toBeAnimatedGlobalImages[i].color.r, toBeAnimatedGlobalImages[i].color.g, toBeAnimatedGlobalImages[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedDefImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedDefImagesOpacities[i], _evaluatedTimeValue);
                toBeAnimatedDefImages[i].color = new Color(toBeAnimatedDefImages[i].color.r, toBeAnimatedDefImages[i].color.g, toBeAnimatedDefImages[i].color.b, _newOpacity);
            }
            yield return null;
        }


        definitionInputField.GetComponent<Image>().raycastTarget = true;
        definitionInputField.Select();
        onDefinitionAnimatedIn.Invoke();
        inTransition = false;
        defInRoutine = null;
    }

    public void StartDefOut()
	{
        dataManager.currentSaveData.ownDefinition = ownDefinition;
        dataManager.UpdateSaveFile();
        if (defOutRoutine != null) { return; }
        StartCoroutine(AnimateDefOut());
    }

    private IEnumerator AnimateDefOut()
    {
        inTransition = true;
        atDefinition = false;
        onDefinitionAnimateOut.Invoke();
        definitionInputField.enabled = false;
        definitionInputField.GetComponent<Image>().raycastTarget = false;

        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / animateDefInDuration;
            float _evaluatedTimeValue = animateDefInCurve.Evaluate(_timeValue);

            for (int i = 0; i < toBeAnimatedDefTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedDefTextsOpacities[i], 0, _evaluatedTimeValue);
                toBeAnimatedDefTexts[i].color = new Color(toBeAnimatedDefTexts[i].color.r, toBeAnimatedDefTexts[i].color.g, toBeAnimatedDefTexts[i].color.b, _newOpacity);
            }

            for (int i = 0; i < toBeAnimatedDefImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedDefImagesOpacities[i], 0, _evaluatedTimeValue);
                toBeAnimatedDefImages[i].color = new Color(toBeAnimatedDefImages[i].color.r, toBeAnimatedDefImages[i].color.g, toBeAnimatedDefImages[i].color.b, _newOpacity);
            }
            yield return null;
        }

        definitionInputField.gameObject.SetActive(false);
        StartNameIn();
        inTransition = false;
        defOutRoutine = null;
    }

    public void StartNameIn()
    {
        
        if (nameInRoutine != null) { return; }
        StartCoroutine(AnimateNameIn());
    }

    private IEnumerator AnimateNameIn()
    {
        inTransition = true;
        onNameActivated.Invoke();
        nameInputField.gameObject.SetActive(true);
        nameInputField.enabled = true;

        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / animateDefInDuration;
            float _evaluatedTimeValue = animateDefInCurve.Evaluate(_timeValue);


            for (int i = 0; i < toBeAnimatedNameTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedNameTextsOpacities[i], _evaluatedTimeValue);
                toBeAnimatedNameTexts[i].color = new Color(toBeAnimatedNameTexts[i].color.r, toBeAnimatedNameTexts[i].color.g, toBeAnimatedNameTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedNameImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedNameImagesOpacities[i], _evaluatedTimeValue);
                toBeAnimatedNameImages[i].color = new Color(toBeAnimatedNameImages[i].color.r, toBeAnimatedNameImages[i].color.g, toBeAnimatedNameImages[i].color.b, _newOpacity);
            }
            yield return null;
        }


        nameInputField.GetComponent<Image>().raycastTarget = true;
        nameInputField.Select();
        onNameAnimatedIn.Invoke();
        inTransition = false;
        nameInRoutine = null;
    }

    public void CloseDown()
    {
        delivered = true;
        dataManager.currentSaveData.source = ownSource;
        dataManager.currentSaveData.ownRating = ownRating;
        dataManager.UpdateSaveFile();
        TrySendOwnDefinition();

        if (nameOutRoutine != null) { return; }
        StartCoroutine(AnimateNameOut());
    }

    private IEnumerator AnimateNameOut()
    {
        inTransition = true;
        onNameAnimateOut.Invoke();
        nameInputField.enabled = false;
        nameInputField.GetComponent<Image>().raycastTarget = false;

        float _timeValue = 0;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / animateNameOutDuration;
            float _evaluatedTimeValue = animateNameOutCurve.Evaluate(_timeValue);

            for (int i = 0; i < toBeAnimatedGlobalTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedGlobalTextsOpacities[i], 0, _evaluatedTimeValue);
                toBeAnimatedGlobalTexts[i].color = new Color(toBeAnimatedGlobalTexts[i].color.r, toBeAnimatedGlobalTexts[i].color.g, toBeAnimatedGlobalTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedNameTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedNameTextsOpacities[i], 0, _evaluatedTimeValue);
                toBeAnimatedNameTexts[i].color = new Color(toBeAnimatedNameTexts[i].color.r, toBeAnimatedNameTexts[i].color.g, toBeAnimatedNameTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedGlobalImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedGlobalImagesOpacities[i], 0, _evaluatedTimeValue);
                toBeAnimatedGlobalImages[i].color = new Color(toBeAnimatedGlobalImages[i].color.r, toBeAnimatedGlobalImages[i].color.g, toBeAnimatedGlobalImages[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedNameImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedNameImagesOpacities[i], 0, _evaluatedTimeValue);
                toBeAnimatedNameImages[i].color = new Color(toBeAnimatedNameImages[i].color.r, toBeAnimatedNameImages[i].color.g, toBeAnimatedNameImages[i].color.b, _newOpacity);
            }
            yield return null;
        }

        nameInputField.gameObject.SetActive(false);
        sequenceManager.AddToGameState();
        inTransition = false;
        nameOutRoutine = null;
    }

    public void TrySendOwnDefinition()
	{
        //try uploading rating to server
        dataManager.ownDefinitionReady = true;
        //dataManager.TryNetworking();

        dataManager.UpdateSaveFile();
    }

    public void OnNewDefInput(string _inputString)
	{
        if(_inputString == "") { definitionDisplay.text = definitionPlaceholder; ownDefinition = ""; definitionInputField.caretColor = new Color(0.8f, 0, 0, 0); return; }

        definitionInputField.caretColor = new Color(0.8f, 0, 0, 1);
        ownDefinition = _inputString;
        definitionDisplay.text = reformatter.ReformatInputDefinitionField(_inputString, defHighlightCols);
	}

    public void OnNewNameInput(string _inputString)
    {
        if (_inputString == "" || _inputString == null) { nameDisplay.text = namePlaceholder; ownSource = ""; nameInputField.caretColor = new Color(0, 0.4f, 0.8f, 0); return; }

        nameInputField.caretColor = new Color(0, 0.4f, 0.8f, 1);
        ownSource = _inputString;
        nameDisplay.text = reformatter.ReformatInputSourceField(_inputString, sourceHighlightCol);
    }
}
