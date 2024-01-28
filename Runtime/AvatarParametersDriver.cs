using UnityEngine;
using VRC.SDKBase;

namespace net.narazaka.vrchat.avatar_parameters_driver
{
    public class AvatarParametersDriver : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        public DriveSetting[] DriveSettings;
    }
}
