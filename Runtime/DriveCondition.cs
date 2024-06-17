using System;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace net.narazaka.vrchat.avatar_parameters_driver
{
    [Serializable]
    public class DriveCondition
    {
        [SerializeField]
        public string Parameter;
        [SerializeField]
        public ConditionMode Mode;
        [SerializeField]
        public float Threshold;

        public enum ConditionMode
        {
            If = 1,
            IfNot = 2,
            Greater = 3,
            Less = 4,
            Equals = 6,
            NotEqual = 7,
        }

#if UNITY_EDITOR
        public static bool IsValidMode(VRCExpressionParameters.ValueType valueType, AnimatorConditionMode mode)
        {
            switch (valueType)
            {
                case VRCExpressionParameters.ValueType.Bool:
                    return mode == AnimatorConditionMode.If || mode == AnimatorConditionMode.IfNot;
                case VRCExpressionParameters.ValueType.Int:
                    return mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less || mode == AnimatorConditionMode.Equals || mode == AnimatorConditionMode.NotEqual;
                case VRCExpressionParameters.ValueType.Float:
                    return mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less;
                default:
                    return false;
            }
        }

        public static bool IsValidMode(AnimatorControllerParameterType valueType, AnimatorConditionMode mode)
        {
            switch (valueType)
            {
                case AnimatorControllerParameterType.Bool:
                    return mode == AnimatorConditionMode.If || mode == AnimatorConditionMode.IfNot;
                case AnimatorControllerParameterType.Int:
                    return mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less || mode == AnimatorConditionMode.Equals || mode == AnimatorConditionMode.NotEqual;
                case AnimatorControllerParameterType.Float:
                    return mode == AnimatorConditionMode.Greater || mode == AnimatorConditionMode.Less;
                default:
                    return false;
            }
        }

        public static AnimatorConditionMode ModeByEnumValueIndex(int index)
        {
            if (index == -1) throw new System.InvalidCastException();
            return (AnimatorConditionMode)Enum.GetValues(typeof(AnimatorConditionMode)).GetValue(index);
        }

        public static int EnumValueIndexByMode(AnimatorConditionMode mode)
        {
            return System.Array.IndexOf(Enum.GetValues(typeof(AnimatorConditionMode)), mode);
        }

        public static AnimatorConditionMode[] IntEnums = new[]
        {
            AnimatorConditionMode.Greater,
            AnimatorConditionMode.Less,
            AnimatorConditionMode.Equals,
            AnimatorConditionMode.NotEqual,
        };

        public static AnimatorConditionMode[] FloatEnums = new[]
        {
            AnimatorConditionMode.Greater,
            AnimatorConditionMode.Less,
        };

        public static string[] IntEnumLabels = new[]
        {
            ">Greater",
            "<Less",
            "==Equals",
            "!=NotEqual",
        };

        public static string[] FloatEnumLabels = new[]
        {
            ">Greater",
            "<Less",
        };
#endif
    }
}
