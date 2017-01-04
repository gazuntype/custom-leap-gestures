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
		public float handOnAngle = 30f;

		[Tooltip("The angle in degrees of the fingers' point direction from the up direction in which to start tracking wave")]
		[Range(0, 360)]
		public float handOffAngle = 45f;

		[Tooltip("The minimum angle in degrees the wrist has to rotate to be considered a wave")]
		[Range(0, 180)]
		public float wristAngle = 60f;

		[Tooltip("The maximum amount of time in seconds allowed to be used to complete a wave")]
		public float maximumWaveTime = .5f;

		[AutoFind(AutoFindLocations.Parents)]
		[Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
		public IHandModel handModel = null;

		[Tooltip("Wrist mode checks for waves that are done with only the wrist. Arm mode checks for whole arm waves.")]
		public WaveType waveType;

		[Tooltip("This is if it's a single wave or a double wave")]
		public WaveNumber waveNumber;

		float waveTime = 0;

		private bool isFingerPointingUp = false;
		private bool areFingersExtended = false;
		private bool hasBegunWave = false;
		private bool isWaving = false;
		private bool isReturningWave = false;
		private bool doneSingleWave = false;

		public enum WaveNumber { Single, Double }
		public enum WaveType { Wrist, Arm}

		private PointingState thumb = PointingState.Extended;
		private PointingState index = PointingState.Extended;
		private PointingState middle = PointingState.Extended;
		private PointingState ring = PointingState.Extended;
		private PointingState pinky = PointingState.Extended;

		private IEnumerator extendedWatcherCoroutine;
		private IEnumerator fingerWatcherCoroutine;


		void OnValidate()
		{
			if (handOffAngle < handOnAngle)
			{
				handOffAngle = handOnAngle;
			}
		}

		void Awake()
		{
			extendedWatcherCoroutine = ExtendedFingerWatcher();
			fingerWatcherCoroutine = FingerPointingWatcher();
		}

		void Update()
		{
			SingleWristWaveWatcher();
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
						if (handModel.IsTracked && angleTo <= handOnAngle)
						{
							isFingerPointingUp = true;
						}
						else if (!handModel.IsTracked || angleTo >= handOffAngle)
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

		void SingleWristWaveWatcher()
		{
			Hand hand;
			Vector3 fingerDirection;
			Vector3 targetDirection;
			float angleTo;

			if (handModel != null)
			{
				hand = handModel.GetLeapHand();
				if (hand != null)
				{
					targetDirection = Vector3.up;
					fingerDirection = hand.Fingers[2].Bone(Bone.BoneType.TYPE_DISTAL).Direction.ToVector3();
					angleTo = Vector3.Angle(fingerDirection, targetDirection);
					if (areFingersExtended && isFingerPointingUp && !hasBegunWave)
					{
						hasBegunWave = true;
						Debug.Log("hasBegunWave = " + hasBegunWave);
					}
					if (hasBegunWave && !isFingerPointingUp)
					{
						hasBegunWave = false;
						isWaving = true;
						Debug.Log("isWaving = " + isWaving);
					}
					if (isWaving && angleTo < wristAngle)
					{
						waveTime += Time.deltaTime;
					}
					else if (isWaving && angleTo >= wristAngle && waveTime <= maximumWaveTime)
					{
						waveTime = 0;
						isWaving = false;
						Activate();
					}
				}
			}
			if (waveTime >= maximumWaveTime)
			{
				waveTime = 0;
				isWaving = false;
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
