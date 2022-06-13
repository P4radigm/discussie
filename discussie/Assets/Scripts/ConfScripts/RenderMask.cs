using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderMask : MonoBehaviour
{
    [SerializeField] private RawImage mask;
    [SerializeField] private float renderRes;  

    void Awake()
    {
        Camera cameraComponent = GetComponent<Camera>();

        if (cameraComponent.targetTexture != null)
		{
            cameraComponent.targetTexture.Release();
		}

        cameraComponent.targetTexture = new RenderTexture(Mathf.FloorToInt(Screen.width * renderRes), Mathf.FloorToInt(Screen.height * renderRes), 0);
        mask.texture = cameraComponent.targetTexture;
    }
}
