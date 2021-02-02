using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ShEffectProperties : ShProperties
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
		public float maxStat;
		public Rigidbody positionRB;
		public float waterDrag;
		public float spawnVelocity;
	}
}
