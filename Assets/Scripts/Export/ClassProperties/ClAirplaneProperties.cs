using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ClAirplaneProperties : ClProperties
	{
		public LabelID[] eventActions;
		public AudioSource horn;
		public AudioClip[] collisionClips;
		public AudioSource collisionSource;
		public Transform engineTransform;
		public GameObject lightsObject;
		public AudioSource engineSound;
		public int gears;
		public float gearRatio;
		public Rotor[] rotors;
	}
}
