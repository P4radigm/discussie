using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class DISelementParticleBehaviour : MonoBehaviour
{
	private VisualEffect vfx;

	[HideInInspector] public int spawnAmount;
	[HideInInspector] public DISelementBehaviour parentElement;
	[HideInInspector] public TextMeshProUGUI target;
	[HideInInspector] public int topBot;
	[HideInInspector] [ColorUsage(true, true)] public Color color;

	[SerializeField] private Vector2 initialRandomYVelocityMinMax;
	[SerializeField] private float distanceScaleValue;
	[SerializeField] private Vector3 spawnAreaSize;
	[SerializeField] private AnimationCurve sizeOverLifetime;
	[SerializeField] private float targetSphereRadius;
	[SerializeField] private float attractionSpeedAtDistance1Local;
	[SerializeField] private float attractionForceAtDistance1Local;
	[SerializeField] private float stickDistanceAtDistance1Local;
	[SerializeField] private float stickForceAtDistance1Local;

	public void InitVFX()
	{
		vfx = GetComponent<VisualEffect>();

		Vector3 targetWorldPoint = target.transform.position;

		transform.SetParent(parentElement.ownConnectionPivot);
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		vfx.SetInt("Amount", spawnAmount);
		vfx.SetVector3("SpawnAreaSize", spawnAreaSize);
		vfx.SetAnimationCurve("Size", sizeOverLifetime);
		vfx.SetVector4("Color", color);
		vfx.SetVector3("TargetSpherePosition", transform.InverseTransformPoint(targetWorldPoint));
		vfx.SetFloat("TargetSphereRadius", targetSphereRadius);
		float DistanceLocal = Vector3.Distance(transform.position, transform.InverseTransformPoint(targetWorldPoint));
		vfx.SetFloat("Attraction Speed", attractionSpeedAtDistance1Local + ((DistanceLocal - 1f) * distanceScaleValue) * attractionSpeedAtDistance1Local);
		vfx.SetFloat("Attraction Force", attractionForceAtDistance1Local + ((DistanceLocal - 1f) * distanceScaleValue) * attractionForceAtDistance1Local);
		vfx.SetFloat("Stick Distance", stickDistanceAtDistance1Local + ((DistanceLocal - 1f) * distanceScaleValue) * stickDistanceAtDistance1Local);
		vfx.SetFloat("Stick Force", stickForceAtDistance1Local + ((DistanceLocal - 1f) * distanceScaleValue) * stickForceAtDistance1Local);
		vfx.SetVector2("AddRandomYVelocity", initialRandomYVelocityMinMax * (float)topBot);
	}

	public void FireVFX()
	{
		vfx.Reinit();
	}
}
