using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;

namespace CustomLeapGestures
{
	public class Wave : Detector
	{
		[Tooltip("The interval in seconds at which to check this detector's conditions.")]
		public float period = .1f;

		[Tooltip("The angle in degrees of the fingers' pointing direction from the up direction in which to begin wave")]
		[Range(0, 360)]
		public float OnAngle = 15f;

		[Tooltip("The angle in degrees of the fingers' point direction from the up direction in which to start tracking wave")]
		[Range(0, 360)]
		public float OffAngle = 25f;

		[AutoFind(AutoFindLocations.Parents)]
		[Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
		public IHandModel handModel = null;

		private bool isFingerPointingUp = false;
		private bool areFingersExtended = false;

		/** The required thumb state. */
		private PointingState thumb = PointingState.Extended;
		/** The required index finger state. */
		private PointingState index = PointingState.Extended;
		/** The required middle finger state. */
		private PointingState middle = PointingState.Extended;
		/** The required ring finger state. */
		private PointingState ring = PointingState.Extended;
		/** The required pinky finger state. */
		private PointingState pinky = PointingState.Extended;

		private IEnumerator extendedWatcherCoroutine;

		private IEnumerator fingerWatcherCoroutine;

		void OnValidate()
		{
			if (OffAngle < OnAngle)
			{
				OffAngle = OnAngle;
			}
		}

		void Awake()
		{
			extendedWatcherCoroutine = ExtendedFingerWatcher();
			fingerWatcherCoroutine = FingerPointingWatcher();
		}

		void OnEnable()
		{
			StartCoroutine(extendedWatcherCoroutine);
			StartCoroutine(fingerWatcherCoroutine);
		}

		void OnDisable()
		{
			StopCoroutine(extendedWatcherCoroutine);
			StopCoroutine(fingerWatcherCoroutine);
			Deactivate();
		}

		IEnumerator FingerPointingWatcher()
		{
			Hand hand;
			Vector3 fingerDirection;
			Vector3 targetDirection;
			while (true)
			{
				if (handModel != null)
				{
					hand = handModel.GetLeapHand();
					if (hand != null)
					{
						targetDirection = Vector3.up;
						fingerDirection = hand.Fingers[2].Bone(Bone.BoneType.TYPE_DISTAL).Direction.ToVector3();
						float angleTo = Vector3.Angle(fingerDirection, targetDirection);
						if (handModel.IsTracked && angleTo <= OnAngle)
						{
							isFingerPointingUp = true;
						}
						else if (!handModel.IsTracked || angleTo >= OffAngle)
						{
							isFingerPointingUp = false;
						}
					}
				}
				yield return new WaitForSeconds(period);
			}
		}

		IEnumerator ExtendedFingerWatcher()
		{
			Hand hand;
			while (true)
			{
				bool fingerState = false;
				if (handModel != null && handModel.IsTracked)
				{
					hand = handModel.GetLeapHand();
					if (hand != null)
					{
						fingerState = matchFingerState(hand.Fingers[0], thumb)
						  && matchFingerState(hand.Fingers[1], index)
						  && matchFingerState(hand.Fingers[2], middle)
						  && matchFingerState(hand.Fingers[3], ring)
						  && matchFingerState(hand.Fingers[4], pinky);

						if (handModel.IsTracked && fingerState)
						{
							areFingersExtended = true;
						}
						else if (!handModel.IsTracked || !fingerState)
						{
							areFingersExtended = false;
						}
					}
				}
				else if (IsActive)
				{
					Deactivate();
				}
				yield return new WaitForSeconds(period);
			}
		}

		private bool matchFingerState(Finger finger, PointingState requiredState)
		{
			return (requiredState == PointingState.Either) ||
				   (requiredState == PointingState.Extended && finger.IsExtended) ||
				   (requiredState == PointingState.NotExtended && !finger.IsExtended);
		}

		private enum PointingState { Extended, NotExtended, Either }
	}
}
