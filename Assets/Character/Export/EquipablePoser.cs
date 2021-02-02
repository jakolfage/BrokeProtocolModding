using UnityEngine;
using UnityEngine.UI;
using BrokeProtocol.Required;
using System;
using System.Linq;

public class EquipablePoser : MonoBehaviour
{
    [SerializeField]
    private Dropdown gripDropdown = null;

    [SerializeField]
    private Animator poserAnimator = null;

    private void Start()
    {
        gripDropdown.AddOptions(Enum.GetNames(typeof(Grip)).ToList());
    }

    public void UpdateGripAnimation()
    {
        poserAnimator.SetInteger(Animations.grip, gripDropdown.value);
        poserAnimator.SetTrigger(Animations.startSwitch);
    }
}
