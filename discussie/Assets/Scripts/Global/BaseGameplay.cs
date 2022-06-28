using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseGameplay : MonoBehaviour
{
    public bool active;

    [SerializeField] private UnityEvent onActivated;
    [Space(20)]
    public float endingGraceTime;
    [SerializeField] private UnityEvent onDeActivated;

    private Coroutine closeRoutine;
    bool closeDown = false;
    float closeDownTimer = 0;

    private SequenceManager sM;

    public void StartUp()
	{
        sM = SequenceManager.instance;

        onActivated.Invoke();
    }

	private void Update()
	{
        CloseDown();
    }

    private void CloseDown()
	{
		if (!closeDown) { return; }

        if(closeDownTimer >= endingGraceTime)
		{
            sM.AddToGameState();
        }

        closeDownTimer += Time.deltaTime;

    }

	public void StartCloseDown()
	{
        onDeActivated.Invoke();
        closeDown = true;
    }

}
