using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColorUpdate : MonoBehaviour
{
    [SerializeField] private Camera renderTexCam;
    [SerializeField] private RenderTexture renderTex;
    [SerializeField] public SpriteRenderer targetRenderer;
    [SerializeField] private float changeSpeed;
    [Space(20)]
    private Color baseColor;
    [SerializeField] private float disappearDuration;
    [SerializeField] private AnimationCurve disappearCurve;
    private Coroutine disappearRoutine;
    [SerializeField] private float appearDuration;
    [SerializeField] private AnimationCurve appearCurve;
    private Coroutine appearRoutine;
    [SerializeField] private float positionSmoothSpeed;
    [SerializeField] private float snapDistance;
    [SerializeField] private float rotationSmoothSpeed;
    [SerializeField] private float snapAngle;
    [SerializeField] private float colResetDuration;
    [SerializeField] private AnimationCurve colResetCurve;
    private Coroutine colResetRoutine;
    [Space(20)]
    [SerializeField] private Image floodPanel;
    [SerializeField] private TextMeshProUGUI[] endTexts;
    //[SerializeField] private TextMeshProUGUI timer;
    private float currentMiddlePixelValue;
    //private Color startingCol;
    private Vector3 startHSVcol;
    [HideInInspector] public Vector3 updateHSVcol;
    private Texture2D tex;

    [HideInInspector] public bool animsOnly = true;
    [HideInInspector] public bool followSpawnPosition = false;

    [SerializeField] private GameManager gM;

    void Start()
    {
        baseColor = targetRenderer.color;
        animsOnly = true;
        Color.RGBToHSV(targetRenderer.color, out startHSVcol.x, out startHSVcol.y, out startHSVcol.z);
        updateHSVcol = startHSVcol;

        tex = new Texture2D(16, 16, TextureFormat.RGB24, false);
    }

    void Update()
    {
        //Get middle of screen pixel value
        renderTexCam.targetTexture = renderTex;
        renderTexCam.Render();
        RenderTexture.active = renderTex;

        tex.ReadPixels(new Rect(8, 8, 2, 2), 8, 8);
        tex.Apply();
        currentMiddlePixelValue = tex.GetPixel(8, 8).r;

		if (animsOnly) { return; }
        //update player visuals
        if(currentMiddlePixelValue == 1)
		{
            if (updateHSVcol.z < 1)
            {
                updateHSVcol.z += changeSpeed * Time.deltaTime;
            }
			else if(updateHSVcol.y > 0)
			{
                updateHSVcol.y -= changeSpeed * Time.deltaTime;
            }
        }
		else if(currentMiddlePixelValue == 0)
		{
            if (updateHSVcol.y < 1)
            {
                updateHSVcol.y += changeSpeed * Time.deltaTime;
            }
            else if(updateHSVcol.z > 0)
            {
                updateHSVcol.z -= changeSpeed * Time.deltaTime;
            }
        }

        //Clamp S & V Values
        updateHSVcol.y = Mathf.Clamp(updateHSVcol.y, 0, 1);
        updateHSVcol.z = Mathf.Clamp(updateHSVcol.z, 0, 1);

        targetRenderer.color = Color.HSVToRGB(updateHSVcol.x, updateHSVcol.y, updateHSVcol.z);
    }

	private void FixedUpdate()
	{
        if (!followSpawnPosition) { return; }
        gM = GameManager.instance;
        Vector3 curPos = transform.position;
        Quaternion curRot = transform.rotation;

        if (Quaternion.Angle(transform.rotation, gM.playerStartPos.rotation) < snapAngle) { transform.rotation = gM.playerStartPos.rotation; }

        if (Vector3.Distance(curPos, gM.playerStartPos.position) < snapDistance) { transform.position = gM.playerStartPos.position; return; }

        Vector3 targetPos = new Vector3(gM.playerStartPos.position.x, gM.playerStartPos.position.y, curPos.z);
        Vector3 smoothedPosition = Vector3.Lerp(curPos, targetPos, positionSmoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        Quaternion targetRot = gM.playerStartPos.rotation;
        Quaternion smoothedRot = Quaternion.Lerp(curRot, targetRot, rotationSmoothSpeed * Time.deltaTime);
        transform.rotation = smoothedRot;
    }

    public void StartColorReset()
	{
        if (colResetRoutine != null) { return; }
        colResetRoutine = StartCoroutine(ColorReset());
    }

    private IEnumerator ColorReset()
	{
        float _timeValue = 0;
        Color _currentPlayerCol = targetRenderer.color;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / colResetDuration;
            float _evaluatedTimeValue = colResetCurve.Evaluate(_timeValue);
            Color _newColor = Color.Lerp(_currentPlayerCol, baseColor, _evaluatedTimeValue);

            //dit update de material
            targetRenderer.color = _newColor;

            yield return null;
        }

        updateHSVcol = startHSVcol;
        colResetRoutine = null;
    }

	public void StartDisappearAnim()
	{
        if(disappearRoutine != null) { return; }
        disappearRoutine = StartCoroutine(DisappearIE());
	}

    private IEnumerator DisappearIE()
	{
        float _timeValue = 0;
        Color _currentPlayerCol = targetRenderer.color;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / disappearDuration;
            float _evaluatedTimeValue = disappearCurve.Evaluate(_timeValue);
            float _newOpacity = Mathf.Lerp(1f, 0f, _evaluatedTimeValue);

            //dit update de material
            targetRenderer.color = new Color(_currentPlayerCol.r, _currentPlayerCol.g, _currentPlayerCol.b, _newOpacity);

            yield return null;
        }

        disappearRoutine = null;
        Destroy(this.gameObject);
    }

    public void StartAppearAnim()
    {
        gM = GameManager.instance;
        gM.cU = this;
        gM.pC = GetComponent<PlayerControls>();
        Camera.main.GetComponent<SmoothFollow>().targetTransform = transform;
        if (appearRoutine != null) { return; }
        appearRoutine = StartCoroutine(AppearIE());
    }

    private IEnumerator AppearIE()
    {
        float _timeValue = 0;
        Color _currentPlayerCol = targetRenderer.color;

        while (_timeValue < 1)
        {
            _timeValue += Time.deltaTime / appearDuration;
            float _evaluatedTimeValue = appearCurve.Evaluate(_timeValue);
            float _newOpacity = Mathf.Lerp(0f, 1f, _evaluatedTimeValue);

            //dit update de material
            targetRenderer.color = new Color(_currentPlayerCol.r, _currentPlayerCol.g, _currentPlayerCol.b, _newOpacity);

            transform.position = new Vector3(gM.playerStartPos.position.x, gM.playerStartPos.position.y, -1);

            yield return null;
        }

        appearRoutine = null;
    }
}
