using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity.Attributes;
using Leap.Unity;

namespace CustomLeapGestures
{
	public class PalmFlip : Detector
	{
		[Tooltip("The interval in seconds at which to check this detector's conditions.")]
		public float period = .1f;

		[Tooltip("The maximum amount of time in seconds allowed to be used to flip palm. ")]
		public float maximumFlipTime = .1f;

		[AutoFind(AutoFindLocations.Parents)]
		[Tooltip("The hand model to watch. Set automatically if detector is on a hand.")]
		public IHandModel handModel = null;

		[Tooltip("The angle in degrees from the palm down direction at which to turn on.")]
		[Range(0, 360)]
		public float onAngleDown = 45;

		[Tooltip("The angle in degrees from the palm down direction at which to turn off.")]
		[Range(0, 360)]
		public float offAngleDown = 65;

		[Tooltip("The angle in degrees from the palm up direction at which to turn on.")]
		[Range(0, 360)]
		public float onAngleUp = 45;

		[Tooltip("The angle in degrees from the palm up direction at which to turn off.")]
		[Range(0, 360)]
		public float offAngleUp = 65;

		bool isPalmFlipping = false;
		bool isPalmDown = false;

		private IEnumerator watcherCoroutine;

		private void OnValidate()
		{
			if (offAngleUp < onAngleUp)
			{
				offAngleUp = onAngleUp;
			}
			if (offAngleDown < onAngleDown)
			{
				offAngleDown = onAngleDown;
			}
		}

		private void Awake()
		{
			watcherCoroutine = palmWatcher();
		}

		private void OnEnable()
		{
			StartCoroutine(watcherCoroutine);
		}

		private void OnDisable()
		{
			StopCoroutine(watcherCoroutine);
			Deactivate();
		}

		private IEnumerator palmWatcher()
		{
			Hand hand;
			Vector3 normal;
			bool isPalmUp = false;
			float angleToDown;
			float angleToUp;
			float flipTime = 0;
			while (true)
			{
				if (handModel != null)
				{
					hand = handModel.GetLeapHand();
					if (hand != null)
					{
						normal = hand.PalmNormal.ToVector3();
						angleToDown = Vector3.Angle(normal, Vector3.down);
						angleToUp = Vector3.Angle(normal, Vector3.up);
						if (angleToDown <= onAngleDown)
						{
							isPalmDown = true;
						}
						else if (angleToDown > offAngleDown)
						{
							if (isPalmDown)
							{
								isPalmFlipping = true;
							}
							isPalmDown = false;
						}

						if (isPalmFlipping)
						{
							flipTime += 1 * Time.deltaTime;
							normal = hand.PalmNormal.ToVector3();
							if (angleToUp <= onAngleUp && flipTime <= maximumFlipTime)
							{
								Activate();
								flipTime = 0;
								isPalmFlipping = false;
								isPalmUp = true;
								Debug.Log("I just flipped palm");
							}
						}
						if (angleToUp > offAngleUp && isPalmUp)
						{
							Deactivate();
							isPalmUp = false;
							Debug.Log("I removed palm from up-position");
						}
						if (flipTime >= maximumFlipTime)
						{
							flipTime = 0;
							isPalmFlipping = false;
						}
					}
				}
				yield return new WaitForSeconds(period);
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (ShowGizmos && handModel != null)
			{
				Color centerColor;
				if (IsActive)
				{
					centerColor = Color.green;
				}
				else if (isPalmDown)
				{
					centerColor = Color.green;
				}
				else {
					centerColor = Color.red;
				}
				Hand hand = handModel.GetLeapHand();
				if (!isPalmFlipping)
				{
					Utils.DrawCone(hand.PalmPosition.ToVector3(), hand.PalmNormal.ToVector3(), onAngleDown, hand.PalmWidth, centerColor, 8);
					Utils.DrawCone(hand.PalmPosition.ToVector3(), hand.PalmNormal.ToVector3(), offAngleDown, hand.PalmWidth, Color.blue, 8);
				}
				else {
					Utils.DrawCone(hand.PalmPosition.ToVector3(), hand.PalmNormal.ToVector3(), onAngleUp, hand.PalmWidth, centerColor, 8);
					Utils.DrawCone(hand.PalmPosition.ToVector3(), hand.PalmNormal.ToVector3(), offAngleUp, hand.PalmWidth, Color.blue, 8);
				}
			}
		}
#endif
	}
}
