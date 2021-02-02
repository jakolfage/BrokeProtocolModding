using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ClVaultProperties : ClProperties
	{
		public LabelID[] eventActions;
		public MeshRenderer bomb;
		public AudioSource alarmSound;
		public AudioSource beepSound;
		public AudioSource bombSound;
		public ParticleSystem explosionParticles;
	}
}
