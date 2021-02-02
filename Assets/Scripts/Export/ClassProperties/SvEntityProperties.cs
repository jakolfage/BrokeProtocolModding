using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class SvEntityProperties : SvProperties
	{
		public float destroyAfter;
		public bool destroyEmpty;
		public bool lockable;
		public ItemOption[] itemOptions;
	}
}