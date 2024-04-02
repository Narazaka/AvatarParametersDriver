using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEngine;
using UnityEditor.Animations;
using VRC.SDK3.Dynamics.Contact.Components;
using System;
using Narazaka.VRChat.AvatarParametersUtil;
using nadena.dev.ndmf;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public static class Util
    {
        [Obsolete("Use AvatarParametersUtil.GetParameters instead.")]
        public static VRCExpressionParameters.Parameter[] GetParameters(VRCAvatarDescriptor avatar, bool includeAnimators = false)
        {
            return ParameterInfo.ForUI.GetParametersForObject(avatar.gameObject).ToDistinctSubParameters().Select(param => new VRCExpressionParameters.Parameter
            {
                name = param.EffectiveName,
                valueType = param.ParameterType == null ? VRCExpressionParameters.ValueType.Float : ((AnimatorControllerParameterType)param.ParameterType).ToVRCExpressionParametersValueType(),
                networkSynced = param.WantSynced,
            }).ToArray();
        }

#if AvatarParametersDriver_HAS_AvatarParametersExclusiveGroup_OLD
        [Obsolete("Use AvatarParametersUtil.ToAnimatorControllerParameter instead.")]
        public static AnimatorControllerParameter ToAnimatorControllerParameter(this VRCExpressionParameters.Parameter parameter)
        {
            return AvatarParametersUtil.ToAnimatorControllerParameter(parameter);
        }
#endif

        public static AnimatorControllerLayer AddLastLayer(this AnimatorController controller, string name)
        {
            controller.AddLayer(name);
            var layer = controller.layers[controller.layers.Length - 1];
            layer.defaultWeight = 1f;
            return layer;
        }

        public static AnimatorState AddConfiguredState(this AnimatorStateMachine stateMachine, string name, Motion motion)
        {
            var state = stateMachine.AddState(name);
            state.writeDefaultValues = false;
            state.motion = motion;
            return state;
        }

        public static AnimatorConditionMode Reverse(this AnimatorConditionMode mode)
        {
            switch (mode)
            {
                case AnimatorConditionMode.If:
                    return AnimatorConditionMode.IfNot;
                case AnimatorConditionMode.IfNot:
                    return AnimatorConditionMode.If;
                case AnimatorConditionMode.Greater:
                    return AnimatorConditionMode.Less;
                case AnimatorConditionMode.Less:
                    return AnimatorConditionMode.Greater;
                case AnimatorConditionMode.Equals:
                    return AnimatorConditionMode.NotEqual;
                case AnimatorConditionMode.NotEqual:
                    return AnimatorConditionMode.Equals;
                default:
                    throw new System.InvalidOperationException();
            }
        }
    }
}
