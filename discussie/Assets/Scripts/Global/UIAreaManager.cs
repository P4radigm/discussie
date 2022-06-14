using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIAreaManager : MonoBehaviour
{
	//works for portrait mode only rn

	[SerializeField] private bool adjustForSafeArea = false;
	[SerializeField] private bool adjustForKeyboardArea = false;
	//[SerializeField] private bool continouslyAdjust = false;
	private RectTransform ownRecTransform;

	private void Start()
	{
		ownRecTransform = GetComponent<RectTransform>();

		Vector4 _anchorOffset = Vector4.zero;
		if (adjustForSafeArea) { _anchorOffset = AddSafeAreaOffset(_anchorOffset); }
		UpdateAnchors(_anchorOffset);
	}

	private void Update()
	{
		if (!adjustForKeyboardArea) { return; }

		Vector4 _anchorOffset = Vector4.zero;
		if (adjustForSafeArea) { _anchorOffset = AddSafeAreaOffset(_anchorOffset); }
		_anchorOffset = AddKeyboardAreaOffset(_anchorOffset);
		UpdateAnchors(_anchorOffset);
	}

	private void UpdateAnchors(Vector4 anchorOffset)
	{
		ownRecTransform.offsetMin = new Vector2(anchorOffset.x, anchorOffset.y);
		ownRecTransform.offsetMax = new Vector2(-anchorOffset.z, -anchorOffset.w);
	}

	private Vector4 AddSafeAreaOffset(Vector4 inputOffset)
	{
		float _offsetMinX = Screen.safeArea.x;
		float _offsetMinY = Screen.safeArea.y;
		float _offsetMaxX = Screen.width - (Screen.safeArea.x + Screen.safeArea.width);
		float _offsetMaxY = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
		//Debug.Log($"_offsetMaxY = {_offsetMaxY}");
		inputOffset = new Vector4(inputOffset.x + _offsetMinX, inputOffset.y + _offsetMinY, inputOffset.z + _offsetMaxX, inputOffset.w + _offsetMaxY);
		return inputOffset;
	}

	private Vector4 AddKeyboardAreaOffset(Vector4 inputOffset)
	{
		if (!TouchScreenKeyboard.visible){ return inputOffset; }		

		float _offsetMinY = TouchScreenKeyboard.area.height + TouchScreenKeyboard.area.y;
		inputOffset = new Vector4(inputOffset.x, inputOffset.y + _offsetMinY, inputOffset.z, inputOffset.w);
		return inputOffset;
	}

}
