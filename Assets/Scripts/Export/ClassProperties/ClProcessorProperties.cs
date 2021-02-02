using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ClProcessorProperties : ClProperties
	{
		public LabelID[] eventActions;
		public AudioSource horn;
		public float zoomFactor;
		public Transform zoomT;
	}
}