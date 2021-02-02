using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ClMineAPProperties : ClProperties
	{
		public LabelID[] eventActions;
		public AudioSource horn;
		public AudioClip[] collisionClips;
		public AudioSource collisionSource;
		public EffectPrefab effectOnDestroy;
	}
}
