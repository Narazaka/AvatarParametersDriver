using System;
using UnityEngine;
using VRC.SDKBase;

namespace net.narazaka.vrchat.avatar_parameters_driver
{
    [Serializable]
    public class DriveSetting
    {
        [SerializeField]
        public bool LocalOnly;
        [SerializeField]
        public DriveCondition[] Contitions;
        [SerializeField]
        public VRC_AvatarParameterDriver.Parameter[] Parameters;
    }
}
