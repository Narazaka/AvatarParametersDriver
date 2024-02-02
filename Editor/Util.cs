using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEngine;
using UnityEditor.Animations;
using VRC.SDK3.Dynamics.Contact.Components;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public static class Util
    {
        public static VRCExpressionParameters.Parameter[] GetParameters(VRCAvatarDescriptor avatar, bool includeAnimators = false)
        {
            var parameters = new List<VRCExpressionParameters.Parameter>();
            if (avatar == null)
            {
                return parameters.ToArray();
            }
            if (avatar.expressionParameters != null) parameters.AddRange(avatar.expressionParameters.parameters);
            var maParameters = avatar.GetComponentsInChildren<ModularAvatarParameters>();
            foreach (var maParameter in maParameters.Where(map => map.parameters != null))
            {
                parameters.AddRange(maParameter.parameters.Where(p => p.syncType != ParameterSyncType.NotSynced).Select(p => new VRCExpressionParameters.Parameter
                {
                    name = p.nameOrPrefix,
                    valueType = p.syncType == ParameterSyncType.Bool ? VRCExpressionParameters.ValueType.Bool : p.syncType == ParameterSyncType.Int ? VRCExpressionParameters.ValueType.Int : VRCExpressionParameters.ValueType.Float,
                    saved = p.saved,
                    defaultValue = p.defaultValue,
                    networkSynced = !p.localOnly,
                }));
            }

            var receivers = avatar.GetComponentsInChildren<VRCContactReceiver>();
            foreach (var receiver in receivers)
            {
                parameters.Add(new VRCExpressionParameters.Parameter
                {
                    name = receiver.parameter,
                    valueType = receiver.receiverType == VRC.Dynamics.ContactReceiver.ReceiverType.Proximity ? VRCExpressionParameters.ValueType.Float : VRCExpressionParameters.ValueType.Bool,
                    saved = false,
                    defaultValue = 0f,
                    networkSynced = false,
                });
            }

            var providers = avatar.GetComponentsInChildren<IParameterNameAndTypesProvider>();
            foreach (var provider in providers)
            {
                parameters.AddRange(provider.GetParameterNameAndTypes());
            }

            if (includeAnimators)
            {
                var animatorControllers =
                    avatar.customizeAnimationLayers
                    ? avatar.baseAnimationLayers.Where(l => !l.isDefault).Select(l => l.animatorController).Concat(avatar.specialAnimationLayers.Where(l => !l.isDefault).Select(l => l.animatorController)).Where(ac => ac != null)
                    : new RuntimeAnimatorController[0];
                var maMergeAnimators = avatar.GetComponentsInChildren<ModularAvatarMergeAnimator>();
                animatorControllers = animatorControllers.Concat(maMergeAnimators.Select(ma => ma.animator).Where(ac => ac != null));
                foreach (var animatorController in animatorControllers)
                {
                    var controller = animatorController as AnimatorController;
                    if (controller == null) continue;
                    parameters.AddRange(controller.parameters.Select(p => p.ToVRCExpressionParametersParameter()));
                }
            }

            return parameters.Where(p => !string.IsNullOrEmpty(p.name)).Distinct(new ParameterNameComparer()).ToArray();
        }

        class ParameterNameComparer : IEqualityComparer<VRCExpressionParameters.Parameter>
        {
            public bool Equals(VRCExpressionParameters.Parameter x, VRCExpressionParameters.Parameter y)
            {
                return x.name == y.name;
            }

            public int GetHashCode(VRCExpressionParameters.Parameter obj)
            {
                return obj.name.GetHashCode();
            }
        }

        public static AnimatorControllerParameterType ToAnimatorControllerParameterType(this VRCExpressionParameters.ValueType valueType)
        {
            switch (valueType)
            {
                case VRCExpressionParameters.ValueType.Bool:
                    return AnimatorControllerParameterType.Bool;
                case VRCExpressionParameters.ValueType.Int:
                    return AnimatorControllerParameterType.Int;
                case VRCExpressionParameters.ValueType.Float:
                    return AnimatorControllerParameterType.Float;
                default:
                    throw new System.InvalidCastException();
            }
        }

        public static VRCExpressionParameters.ValueType ToVRCExpressionParametersValueType(this AnimatorControllerParameterType valueType)
        {
            switch (valueType)
            {
                case AnimatorControllerParameterType.Bool:
                    return VRCExpressionParameters.ValueType.Bool;
                case AnimatorControllerParameterType.Int:
                    return VRCExpressionParameters.ValueType.Int;
                case AnimatorControllerParameterType.Float:
                    return VRCExpressionParameters.ValueType.Float;
                case AnimatorControllerParameterType.Trigger:
                    return VRCExpressionParameters.ValueType.Bool;
                default:
                    throw new System.InvalidCastException();
            }
        }

        public static AnimatorControllerParameter ToAnimatorControllerParameter(this VRCExpressionParameters.Parameter parameter)
        {
            return new AnimatorControllerParameter
            {
                name = parameter.name,
                type = parameter.valueType.ToAnimatorControllerParameterType(),
                defaultBool = parameter.valueType == VRCExpressionParameters.ValueType.Bool ? parameter.defaultValue > 0.5f : false,
                defaultInt = parameter.valueType == VRCExpressionParameters.ValueType.Int ? (int)parameter.defaultValue : 0,
                defaultFloat = parameter.valueType == VRCExpressionParameters.ValueType.Float ? parameter.defaultValue : 0f,
            };
        }

        public static VRCExpressionParameters.Parameter ToVRCExpressionParametersParameter(this AnimatorControllerParameter parameter)
        {
            return new VRCExpressionParameters.Parameter
            {
                name = parameter.name,
                valueType = parameter.type.ToVRCExpressionParametersValueType(),
                saved = false,
                defaultValue = parameter.type == AnimatorControllerParameterType.Bool ? (parameter.defaultBool ? 1f : 0f) : parameter.type == AnimatorControllerParameterType.Int ? parameter.defaultInt : parameter.defaultFloat,
                networkSynced = false,
            };
        }

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
