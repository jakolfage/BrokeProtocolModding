using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ShUnderbarrelProperties : ShProperties
	{
		public Transform mainT;
		public AppIndex[] availableApps;
		public bool hasInventory;
		public bool shop;
		public int value;
		public InventoryStruct[] collectedItems;
		public Seat[] seats;
		public GameObject hideInterior;
		public Transform exitTransform;
		public float viewAngleLimit;
		public Transform turretT;
		public Transform barrelT;
		public ThrownEntity[] thrownEntities;
		public float thrownDelay;
		public AudioSource thrownSound;
		public int clipSize;
		public string itemName;
		public bool needItem;
		public bool illegal;
		public ShReference license;
		public float weight;
		public ShReference ammoItem;
		public Transform specialT;
		public Grip[] usableGrips;
		public float accuracyBuff;
		public float recoilBuff;
		public float recoveryBuff;
		public LineRenderer laserRenderer;
	}
}
