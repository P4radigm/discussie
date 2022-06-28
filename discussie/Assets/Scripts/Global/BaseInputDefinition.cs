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
    [SerializeField] private TextMeshProUGUI[] toBeAnimatedDefTexts;
    [SerializeField] private List<float> toBeAnimatedDefTextsOpacities;
    [SerializeField] private Image[] toBeAnimatedDefImages;
    [SerializeField] private List<float> toBeAnimatedDefImagesOpacities;
    [Space(10)]
    [SerializeField] private float animateDefInDuration;
    [SerializeField] private AnimationCurve animateDefInCurve;
    [Header("Animate Def Out")]
    [SerializeField] private float animateDefOutDuration;
    [SerializeField] private AnimationCurve animateDefOutCurve;
    [Header("Animate Name In")]
    [SerializeField] private TextMeshProUGUI[] toBeAnimatedNameTexts;
    [SerializeField] private List<float> toBeAnimatedNameTextsOpacities;
    [SerializeField] private Image[] toBeAnimatedNameImages;
    [SerializeField] private List<float> toBeAnimatedNameImagesOpacities;
    [Space(10)]
    [SerializeField] private float animateNameInDuration;
    [SerializeField] private AnimationCurve animateNameInCurve;
    [Header("Animate Name Out")]
    public float animateNameOutDuration;
    public AnimationCurve animateNameOutCurve;
    private bool inTransition;

    private bool animateDefInInit = true;
    private bool animateDefIn = false;
    private float animateDefInTimeValue = 0;

    private bool animateDefOutInit = true;
    private bool animateDefOut = false;
    private float animateDefOutTimeValue = 0;

    private bool animateNameInInit = true;
    private bool animateNameIn = false;
    private float animateNameInTimeValue = 0;

    private bool animateNameOutInit = true;
    private bool animateNameOut = false;
    private float animateNameOutTimeValue = 0;


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
            toBeAnimatedGlobalTexts[i].color = new Color(toBeAnimatedGlobalTexts[i].color.r, toBeAnimatedGlobalTexts[i].color.g, toBeAnimatedGlobalTexts[i].color.b, 0);
        }
        for (int i = 0; i < toBeAnimatedGlobalImages.Length; i++)
        {
            toBeAnimatedGlobalImagesOpacities.Add(toBeAnimatedGlobalImages[i].color.a);
            toBeAnimatedGlobalImages[i].color = new Color(toBeAnimatedGlobalImages[i].color.r, toBeAnimatedGlobalImages[i].color.g, toBeAnimatedGlobalImages[i].color.b, 0);
        }

        for (int i = 0; i < toBeAnimatedDefTexts.Length; i++)
        {
            toBeAnimatedDefTextsOpacities.Add(toBeAnimatedDefTexts[i].color.a);
            toBeAnimatedDefTexts[i].color = new Color(toBeAnimatedDefTexts[i].color.r, toBeAnimatedDefTexts[i].color.g, toBeAnimatedDefTexts[i].color.b, 0);
        }
        for (int i = 0; i < toBeAnimatedDefImages.Length; i++)
        {
            toBeAnimatedDefImagesOpacities.Add(toBeAnimatedDefImages[i].color.a);
            toBeAnimatedDefImages[i].color = new Color(toBeAnimatedDefImages[i].color.r, toBeAnimatedDefImages[i].color.g, toBeAnimatedDefImages[i].color.b, 0);
        }

        for (int i = 0; i < toBeAnimatedNameTexts.Length; i++)
        {
            toBeAnimatedNameTextsOpacities.Add(toBeAnimatedNameTexts[i].color.a);
            toBeAnimatedNameTexts[i].color = new Color(toBeAnimatedNameTexts[i].color.r, toBeAnimatedNameTexts[i].color.g, toBeAnimatedNameTexts[i].color.b, 0);
        }
        for (int i = 0; i < toBeAnimatedNameImages.Length; i++)
        {
            toBeAnimatedNameImagesOpacities.Add(toBeAnimatedNameImages[i].color.a);
            toBeAnimatedNameImages[i].color = new Color(toBeAnimatedNameImages[i].color.r, toBeAnimatedNameImages[i].color.g, toBeAnimatedNameImages[i].color.b, 0);
        }

        //Animate In
        animateDefIn = true;
    }

    void Update()
    {
        AnimDefIn();
        AnimDefOut();
        AnimNameIn();
        AnimNameOut();

        if (ownDefinition != "" && ownRating != Vector2.one * -99 && atDefinition && !inTransition) 
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

    private void AnimDefIn()
	{
        if (!animateDefIn) { return; }

        if (animateDefInInit)
        {
            inTransition = true;
            onDefinitionActivated.Invoke();
            definitionInputField.gameObject.SetActive(true);
            definitionInputField.enabled = true;

            animateDefInTimeValue = 0;
            animateDefInInit = false;
        }

        if (animateDefInTimeValue > 0 && animateDefInTimeValue < animateDefInDuration)
        {
            float EvaluatedTimeValue = animateDefInCurve.Evaluate(animateDefInTimeValue / animateDefInDuration);

            for (int i = 0; i < toBeAnimatedGlobalTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedGlobalTextsOpacities[i], EvaluatedTimeValue);
                toBeAnimatedGlobalTexts[i].color = new Color(toBeAnimatedGlobalTexts[i].color.r, toBeAnimatedGlobalTexts[i].color.g, toBeAnimatedGlobalTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedDefTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedDefTextsOpacities[i], EvaluatedTimeValue);
                toBeAnimatedDefTexts[i].color = new Color(toBeAnimatedDefTexts[i].color.r, toBeAnimatedDefTexts[i].color.g, toBeAnimatedDefTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedGlobalImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedGlobalImagesOpacities[i], EvaluatedTimeValue);
                toBeAnimatedGlobalImages[i].color = new Color(toBeAnimatedGlobalImages[i].color.r, toBeAnimatedGlobalImages[i].color.g, toBeAnimatedGlobalImages[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedDefImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedDefImagesOpacities[i], EvaluatedTimeValue);
                toBeAnimatedDefImages[i].color = new Color(toBeAnimatedDefImages[i].color.r, toBeAnimatedDefImages[i].color.g, toBeAnimatedDefImages[i].color.b, _newOpacity);
            }
        }
        else if (animateDefInTimeValue > animateDefInDuration)
        {
            definitionInputField.GetComponent<Image>().raycastTarget = true;
            definitionInputField.Select();
            onDefinitionAnimatedIn.Invoke();
            inTransition = false;

            animateDefInInit = true;
            animateDefIn = false;
        }

        animateDefInTimeValue += Time.deltaTime;
        Mathf.Clamp(animateDefInTimeValue, 0, animateDefInDuration + 0.01f);
    }

    private void AnimDefOut()
	{
        if (!animateDefOut) { return; }

        if (animateDefOutInit)
        {
            inTransition = true;
            atDefinition = false;
            onDefinitionAnimateOut.Invoke();
            definitionInputField.enabled = false;
            definitionInputField.GetComponent<Image>().raycastTarget = false;

            animateDefOutTimeValue = 0;
            animateDefOutInit = false;
        }

        if (animateDefOutTimeValue > 0 && animateDefOutTimeValue < animateDefOutDuration)
        {
            float EvaluatedTimeValue = animateDefOutCurve.Evaluate(animateDefOutTimeValue / animateDefOutDuration);

            for (int i = 0; i < toBeAnimatedDefTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedDefTextsOpacities[i], 0, EvaluatedTimeValue);
                toBeAnimatedDefTexts[i].color = new Color(toBeAnimatedDefTexts[i].color.r, toBeAnimatedDefTexts[i].color.g, toBeAnimatedDefTexts[i].color.b, _newOpacity);
            }

            for (int i = 0; i < toBeAnimatedDefImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedDefImagesOpacities[i], 0, EvaluatedTimeValue);
                toBeAnimatedDefImages[i].color = new Color(toBeAnimatedDefImages[i].color.r, toBeAnimatedDefImages[i].color.g, toBeAnimatedDefImages[i].color.b, _newOpacity);
            }
        }
        else if (animateDefOutTimeValue > animateDefOutDuration)
        {

            definitionInputField.gameObject.SetActive(false);
            StartNameIn();
            inTransition = false;

            animateDefOutInit = true;
            animateDefOut = false;
        }

        animateDefOutTimeValue += Time.deltaTime;
        Mathf.Clamp(animateDefOutTimeValue, 0, animateDefOutDuration + 0.01f);

    }

    private void AnimNameIn()
	{
        if (!animateNameIn) { return; }

        if (animateNameInInit)
        {
            inTransition = true;
            onNameActivated.Invoke();
            nameInputField.gameObject.SetActive(true);
            nameInputField.enabled = true;

            animateNameInTimeValue = 0;
            animateNameInInit = false;
        }

        if (animateNameInTimeValue > 0 && animateNameInTimeValue < animateNameInDuration)
        {
            float EvaluatedTimeValue = animateNameInCurve.Evaluate(animateNameInTimeValue / animateNameInDuration);

            for (int i = 0; i < toBeAnimatedNameTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedNameTextsOpacities[i], EvaluatedTimeValue);
                toBeAnimatedNameTexts[i].color = new Color(toBeAnimatedNameTexts[i].color.r, toBeAnimatedNameTexts[i].color.g, toBeAnimatedNameTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedNameImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(0, toBeAnimatedNameImagesOpacities[i], EvaluatedTimeValue);
                toBeAnimatedNameImages[i].color = new Color(toBeAnimatedNameImages[i].color.r, toBeAnimatedNameImages[i].color.g, toBeAnimatedNameImages[i].color.b, _newOpacity);
            }
        }
        else if (animateNameInTimeValue > animateNameInDuration)
        {
            nameInputField.GetComponent<Image>().raycastTarget = true;
            nameInputField.Select();
            onNameAnimatedIn.Invoke();
            inTransition = false;

            animateNameInInit = true;
            animateNameIn = false;
        }

        animateNameInTimeValue += Time.deltaTime;
        Mathf.Clamp(animateNameInTimeValue, 0, animateNameInDuration + 0.01f);
    } 

    private void AnimNameOut()
	{
        if (!animateNameOut) { return; }

        if (animateNameOutInit)
        {
            inTransition = true;
            onNameAnimateOut.Invoke();
            nameInputField.enabled = false;
            nameInputField.GetComponent<Image>().raycastTarget = false;

            animateNameOutTimeValue = 0;
            animateNameOutInit = false;
        }

        if (animateNameOutTimeValue > 0 && animateNameOutTimeValue < animateNameOutDuration)
        {
            float EvaluatedTimeValue = animateNameOutCurve.Evaluate(animateNameOutTimeValue / animateNameOutDuration);

            for (int i = 0; i < toBeAnimatedGlobalTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedGlobalTextsOpacities[i], 0, EvaluatedTimeValue);
                toBeAnimatedGlobalTexts[i].color = new Color(toBeAnimatedGlobalTexts[i].color.r, toBeAnimatedGlobalTexts[i].color.g, toBeAnimatedGlobalTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedNameTexts.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedNameTextsOpacities[i], 0, EvaluatedTimeValue);
                toBeAnimatedNameTexts[i].color = new Color(toBeAnimatedNameTexts[i].color.r, toBeAnimatedNameTexts[i].color.g, toBeAnimatedNameTexts[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedGlobalImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedGlobalImagesOpacities[i], 0, EvaluatedTimeValue);
                toBeAnimatedGlobalImages[i].color = new Color(toBeAnimatedGlobalImages[i].color.r, toBeAnimatedGlobalImages[i].color.g, toBeAnimatedGlobalImages[i].color.b, _newOpacity);
            }
            for (int i = 0; i < toBeAnimatedNameImages.Length; i++)
            {
                float _newOpacity = Mathf.Lerp(toBeAnimatedNameImagesOpacities[i], 0, EvaluatedTimeValue);
                toBeAnimatedNameImages[i].color = new Color(toBeAnimatedNameImages[i].color.r, toBeAnimatedNameImages[i].color.g, toBeAnimatedNameImages[i].color.b, _newOpacity);
            }

        }
        else if (animateNameOutTimeValue > animateNameOutDuration)
        {
            nameInputField.gameObject.SetActive(false);
            inTransition = false;

            sequenceManager.AddToGameState();
            animateNameOutInit = true;
            animateNameOut = false;
        }

        animateNameOutTimeValue += Time.deltaTime;
        Mathf.Clamp(animateNameOutTimeValue, 0, animateNameOutDuration + 0.01f);

    }

    public void StartDefOut()
	{
        dataManager.currentSaveData.ownDefinition = ownDefinition;
        dataManager.UpdateSaveFile();
        animateDefOut = true;
    }

    public void StartNameIn()
    {
        animateNameIn = true;
    }

    public void CloseDown()
    {
        delivered = true;
        dataManager.currentSaveData.source = ownSource;
        dataManager.currentSaveData.ownRating = ownRating;
        dataManager.UpdateSaveFile();
        TrySendOwnDefinition();

        animateNameOut = true;
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
        if (_inputString == "") { definitionDisplay.text = definitionPlaceholder; ownDefinition = ""; definitionInputField.caretColor = new Color(definitionInputField.caretColor.r, definitionInputField.caretColor.g, definitionInputField.caretColor.b, 0); return; }

        definitionInputField.caretColor = new Color(definitionInputField.caretColor.r, definitionInputField.caretColor.g, definitionInputField.caretColor.b, 1);
        ownDefinition = _inputString;
        definitionDisplay.text = reformatter.ReformatInputDefinitionField(_inputString, defHighlightCols);
    }

    public void OnNewNameInput(string _inputString)
    {
        if (_inputString == "" || _inputString == null) { nameDisplay.text = namePlaceholder; ownSource = ""; new Color(definitionInputField.caretColor.r, definitionInputField.caretColor.g, definitionInputField.caretColor.b, 0); return; }

        nameInputField.caretColor = new Color(definitionInputField.caretColor.r, definitionInputField.caretColor.g, definitionInputField.caretColor.b, 1);
        ownSource = _inputString;
        nameDisplay.text = reformatter.ReformatInputSourceField(_inputString, sourceHighlightCol);
    }
}
