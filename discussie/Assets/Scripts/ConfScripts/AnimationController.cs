using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
	[SerializeField] private Material[] backgroundMats;
	[SerializeField] private float scrollSpeed;

	private void Update()
	{
		for (int i = 0; i < backgroundMats.Length; i++)
		{
			float _yOffset = backgroundMats[i].GetFloat("_NoiseOffsetY");
			_yOffset += Time.deltaTime * scrollSpeed;
			backgroundMats[i].SetFloat("_NoiseOffsetY", _yOffset);
		}
	}
}
