using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ShWeaponProperties : ShProperties
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
		public Grip grip;
		public float moveSpeed;
		public byte burstSize;
		public float useDelay;
		public bool fireAnimation;
		public AudioSource useSound;
		public DamageIndex damageIndex;
		public float range;
		public FireEffect fireEffect;
		public float damage;
	}
}
