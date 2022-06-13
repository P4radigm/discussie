using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UZKgameplayManager))]
[RequireComponent(typeof(UZKgameplayInput))]
public class UZKgameplaySettings : MonoBehaviour
{
	[System.Serializable]
	public class CounterColours
	{
		public Color GroupOneCol;
		public Color GroupTwoCol;
	}

	[System.Serializable]
	public class ElementGroups
	{
		public GameObject elementPrefab;
		public float spawnDistributionWeight;
	}

	[Header("Gameplay Values")]
	public int maxFingers;
	public int spawnAmount;
	[Tooltip("This is in world space!")]
	public float extraWorldSpawnBorderAdjustment;
	
	[Space(20)]
	[Header("Element Spawn Pools")]
	[NonReorderable]
	public ElementGroups[] stockElementPool;
	[NonReorderable]
	public ElementGroups[] randomElementPool;

	[Space(20)]
	[Header("Element Options")]
	public float ElementSize;
	[Range(0, 1)] public float ElementColliderRadius;
	public float minGroupRadius;
	public bool springCollisionEnable;
	public float springLength;
	public float springDampingRatio;
	public float springFrequency;
	public float springBreakForce;
	public float springGracePeriod;

	[Space(20)]
	[Header("Global color settings")]
	[NonReorderable][SerializeField] public CounterColours[] counterColours;

}
