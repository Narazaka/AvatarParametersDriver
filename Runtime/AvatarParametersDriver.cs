using UnityEngine;
using VRC.SDKBase;

namespace net.narazaka.vrchat.avatar_parameters_driver
{
    public class AvatarParametersDriver : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        public DriveSetting[] DriveSettings = new DriveSetting[0];
        [SerializeField]
        public bool PreserveHierarchy;

        void Reset()
        {
            PreserveHierarchy = true;
        }
    }
}
