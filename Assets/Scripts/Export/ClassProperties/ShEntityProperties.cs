using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ShEntityProperties : ShProperties
	{
		public Transform mainT;
		public AppIndex[] availableApps;
		public bool hasInventory;
		public bool shop;
		public int value;
		public InventoryStruct[] collectedItems;
	}
}
