using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine.Animations;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using Narazaka.VRChat.AvatarParametersUtil;

[assembly: ExportsPlugin(typeof(net.narazaka.vrchat.avatar_parameters_driver.editor.AvatarParametersDriverPlugin))]

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public class AvatarParametersDriverPlugin : Plugin<AvatarParametersDriverPlugin>
    {
        public override string QualifiedName => "net.narazaka.vrchat.avatar_parameters_driver";

        public override string DisplayName => "Avatar Parameters Driver";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("AvatarParametersDriver", ctx =>
            {
                var avatarParametersDrivers = ctx.AvatarRootObject.GetComponentsInChildren<AvatarParametersDriver>();
                if (avatarParametersDrivers.Length == 0) return;
                var parameters = ParameterInfo.ForContext(ctx).GetParametersForObject(ctx.AvatarRootObject).ToDistinctSubParameters();
                var parameterByName = parameters.ToDictionary(p => p.EffectiveName);

                var driveSettings = avatarParametersDrivers.SelectMany(d => d.DriveSettings).ToList();
                var parameterNames = driveSettings.SelectMany(d => d.Contitions).Select(d => d.Parameter)
                    .Concat(driveSettings.SelectMany(d => d.Parameters).Select(d => d.name))
                    .Concat(driveSettings.SelectMany(d => d.Parameters).Where(p => p.type == VRC_AvatarParameterDriver.ChangeType.Copy).Select(d => d.source))
                    .Distinct();
                var invalidParameters = parameterNames.Where(p => !parameterByName.ContainsKey(p)).ToArray();
                if (invalidParameters.Length > 0)
                {
                    throw new System.InvalidOperationException($"Parameters {string.Join(", ", invalidParameters)} not found");
                }
                var clip = MakeEmptyAnimationClip();
                var animator = new AnimatorController();
                foreach (var parameterName in parameterNames)
                {
                    if (parameterByName.TryGetValue(parameterName, out var parameter))
                    {
                        animator.AddParameter(parameter.ToAnimatorControllerParameter());
                    }
                }
                for (var i = 0; i < driveSettings.Count; ++i)
                {
                    var driveSetting = driveSettings[i];
                    var layer = animator.AddLastLayer($"Avatar Parameters Driver {i}");
                    var idleState = layer.stateMachine.AddConfiguredState("idle", clip);
                    layer.stateMachine.defaultState = idleState;
                    var activeState = layer.stateMachine.AddConfiguredState("active", clip);
                    activeState.behaviours = new StateMachineBehaviour[]
                    {
                        new VRCAvatarParameterDriver
                        {
                            parameters = driveSetting.Parameters.ToList(),
                            localOnly = driveSetting.LocalOnly,
                        },
                    };
                    if (driveSetting.UsePreContitions)
                    {
                        var preActiveState = layer.stateMachine.AddConfiguredState("pre_active", clip);
                        MakeForwardTransition(idleState, preActiveState, driveSetting.PreContitions);
                        MakeForwardTransition(preActiveState, activeState, driveSetting.Contitions);
                        MakeBackTransition(activeState, idleState, driveSetting.Contitions, parameterByName);
                    }
                    else
                    {
                        MakeForwardTransition(idleState, activeState, driveSetting.Contitions);
                        MakeBackTransition(activeState, idleState, driveSetting.Contitions, parameterByName);
                    }
                }
                var mergeAnimator = ctx.AvatarRootObject.AddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.animator = animator;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.matchAvatarWriteDefaults = true;

                foreach (var avatarParametersDriver in avatarParametersDrivers)
                {
                    Object.DestroyImmediate(avatarParametersDriver);
                }
            });
        }

        void MakeForwardTransition(AnimatorState from, AnimatorState to, DriveCondition[] conditions)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            transition.exitTime = 0f;
            foreach (var condition in conditions)
            {
                transition.AddCondition((AnimatorConditionMode)condition.Mode, condition.Threshold, condition.Parameter);
            }
        }

        void MakeBackTransition(AnimatorState from, AnimatorState to, DriveCondition[] reverseConditions, Dictionary<string, ProvidedParameter> parameterByName)
        {
            foreach (var condition in reverseConditions)
            {
                var reverse = from.AddTransition(to);
                reverse.hasExitTime = false;
                reverse.hasFixedDuration = true;
                reverse.duration = 0f;
                reverse.exitTime = 0f;
                var type = parameterByName.TryGetValue(condition.Parameter, out var pp) ? pp.ParameterType : null;
                var reversedCondition = condition.Reverse(type);
                reverse.AddCondition((AnimatorConditionMode)reversedCondition.Mode, reversedCondition.Threshold, reversedCondition.Parameter);
            }
        }

        AnimationClip MakeEmptyAnimationClip()
        {
            var clip = new AnimationClip();
            clip.SetCurve("__AvatarParametersDriver_EMPTY__", typeof(GameObject), "localPosition.x", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1f / 60f, value = 0 } } });
            return clip;
        }
    }
}
