using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ShPlayerProperties : ShProperties
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
		public float maxSpeed;
		public Transform rotationT;
		public WearableOptions[] wearableOptions;
		public CapsuleCollider capsule;
		public bool boss;
		public Transform weaponBone;
		public Transform originT;
		public float jumpVelocity;
		public float force;
		public float drag;
		public PhysicMaterial physicMaterial;
	}
}
