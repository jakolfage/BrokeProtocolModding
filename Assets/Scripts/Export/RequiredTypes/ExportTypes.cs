using System;
using UnityEngine;

namespace BrokeProtocol.Required
{
    [Serializable]
    public class Reference { public string entityName; }

    [Serializable]
    public class ShReference : Reference {}

    [Serializable]
    public class ClReference : Reference {}

    [Serializable]
    public class SvReference : Reference {}

    public enum EntityState
    {
        Unlocked,
        Locked,
        ForSale,
    }

    [Serializable]
    public struct ItemOption
    {
        public string itemName;
        public float chance;
        public int minCount;
        public int maxCount;
    }

    [Serializable]
    public sealed class ProcessOption
    {
        public InventoryStruct[] input;
        public InventoryStruct[] output;
    }

    public enum StanceIndex
    {
        Stand,
        Crouch,
        SitMovable,
        SitFixed,
        SitMotorcycle,
        SitKart,
        Parachute,
        Gunner,
        Sleep,
        KnockedDown,
        Recovering,
        KnockedOut,
        Dead
    }

    public enum AppIndex
    {
        Apps,
        Contacts,
        Blocked,
        Calls,
        Inbox,
        Services,
        Deposit,
        Withdraw
    }

    [Serializable]
    public struct Seat
    {
        public Transform seatPositionT;
        public StanceIndex stanceIndex;
    }

    [Serializable]
    public struct ThrownEntity
    {
        public string thrownName;
        public Transform startPositionT;
        public FireEffect fireEffect;
    }

    [Serializable]
    public struct Rotor
    {
        public Transform rotorHubT;
        public Vector3 axis;
    }

    [Serializable]
    public struct WearableOptions
    {
        public string[] wearableNames;
    }

    [Serializable]
    public struct InventoryStruct
    {
        public string itemName;
        public int count;

        public InventoryStruct(string itemName, int count)
        {
            this.itemName = itemName;
            this.count = count;
        }
    }

    [Serializable]
    public struct LabelID
    {
        public string label;
        public string ID;

        public LabelID(string label, string ID)
        {
            this.label = label;
            this.ID = ID;
        }
    }

    [Serializable]
    public struct TypeLabelID
    {
        public string type;
        public string label;
        public string ID;

        public TypeLabelID(string type, string label, string ID)
        {
            this.type = type;
            this.label = label;
            this.ID = ID;
        }
    }

    public enum Grip
    {
        None,
        Surrender,
        Restrained,
        Pistol,
        Rifle,
        Shotgun,
        TwoHand,
        Device
    }

    public enum DamageIndex
    {
        Null,
        Melee,
        Gun,
        Random,
        Collision,
        Stun
    }

    public enum ConsumableType
    {
        Food,
        Drink,
        Drug,
        Medical
    }

    public enum WearableType
    {
        Head,
        Face,
        Body,
        Back,
        Armor,
        Legs,
        Feet,
        Hands
    }

    public enum BodyPart
    {
        Null = -1,
        Head, // Vision / hearing loss
        Chest, // Energy loss
        Abdomen, // Dehydration & hunger
        Arms, // Aim loss
        Legs, // Speed loss
        Count
    }

    public enum BodyEffect
    {
        Null = -1,
        Temporary, // Heals by itself (quickly)
        Drugged, // Heals by itself (criminal)
        Pain, // Heal by drugs or morphine (fast)
        Bloodloss, // Heal by bandage (medium)
        Fracture, // Heal by splint (very slow)
    }

    public enum EffectPrefab
    {
        None,
        Blocked,
        Blood,
        BoomBig,
        BoomSmall,
        Debris,
        FireEffect,
        Flash,
        LightParticle,
        SmokeEffect,
        Sparks,
        TireGrass,
        TireSmoke,
        Trail,
        Tracer,
        MuzzleFlash,
        TaserFlash,
        Exinguisher,
        WaterStream,
        Laser
    }

    public enum FireEffect
    {
        None,
        MuzzleFlash,
        TaserFlash,
        Exinguisher,
        WaterStream
    }

    public static class ObjectTag
    {
        public const string untagged = "Untagged";
        public const string nonStaticTag = "NonStatic";
        public const string grassTag = "Grass";
        public const string climbableTag = "Climbable";
        public const string junctionTag = "Junction";
        public const string interSectionTag = "Intersection";
        public const string trafficLightTag = "TrafficLight";
        public const string folderTag = "Folder";
    }

    public static class Animations
    {
        public static int stance = Animator.StringToHash("stance");
        public static int fire = Animator.StringToHash("fire");
        public static int viewVertical = Animator.StringToHash("viewVertical");
        public static int viewHorizontal = Animator.StringToHash("viewHorizontal");
        public static int velocityForward = Animator.StringToHash("velocityForward");
        public static int velocityRight = Animator.StringToHash("velocityRight");
        public static int grip = Animator.StringToHash("grip");
        public static int startSwitch = Animator.StringToHash("startSwitch");
        public static int reload = Animator.StringToHash("reload");
        public static int jump = Animator.StringToHash("jump");
        public static int changeStance = Animator.StringToHash("changeStance");
        public static int sprint = Animator.StringToHash("sprint");
        public static int getUpFront = Animator.StringToHash("getUpFront");
        public static int getUpBack = Animator.StringToHash("getUpBack");
        public static int swimming = Animator.StringToHash("swimming");
        public static int consume = Animator.StringToHash("consume");
        public static int fixedForward = Animator.StringToHash("fixedForward");
        public static int gesture = Animator.StringToHash("gesture");
        public static int zoom = Animator.StringToHash("zoom");
        public static int point = Animator.StringToHash("point");
        public static int load = Animator.StringToHash("load");
    }

    public static class Consts
    {
        public const int fixedLayerCount = 8;
    }
}