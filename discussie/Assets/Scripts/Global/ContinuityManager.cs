using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuityManager : MonoBehaviour
{
    #region Singleton
    public static ContinuityManager instance;
    [HideInInspector] public bool hasBeenToMenu;
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
        DontDestroyOnLoad(this);
        hasBeenToMenu = false;
    }
    #endregion

}
