using BrokeProtocol.Required;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BrokeProtocol.Properties
{
	public class ClPlayerProperties : ClProperties
	{
		public LabelID[] eventActions;
		public AudioSource horn;
		public AudioClip[] collisionClips;
		public AudioSource collisionSource;
		public SkinnedMeshRenderer skinnedMeshRenderer;
		public Transform headBone;
		public RectTransform textTransform;
		public GameObject nameObject;
		public GameObject messageObject;
		public Text nameText;
		public Text messageText;
		public AudioSource hurtSound;
		public AudioSource voice;
		public AudioSource footstepSource;
		public Animator animator;
	}
}
