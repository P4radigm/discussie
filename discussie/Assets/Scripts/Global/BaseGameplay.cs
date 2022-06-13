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

    private SequenceManager sM;

    public void StartUp()
	{
        sM = SequenceManager.instance;

        onActivated.Invoke();
    }

    public void StartCloseDown()
	{
        if(closeRoutine != null) { return; }
        closeRoutine = StartCoroutine(CloseDown());
    }

    private IEnumerator CloseDown()
	{
        onDeActivated.Invoke();
        yield return new WaitForSeconds(endingGraceTime);
        sM.AddToGameState();
        //gameObject.SetActive(false);
    }

}
