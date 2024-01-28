using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine.Animations;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;

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
                var parameters = Util.GetParameters(ctx.AvatarDescriptor, true);
                var parameterByName = parameters.ToDictionary(p => p.name);

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
                    var toActive = idleState.AddTransition(activeState);
                    toActive.hasExitTime = false;
                    toActive.hasFixedDuration = true;
                    toActive.duration = 0f;
                    toActive.exitTime = 0f;
                    foreach (var condition in driveSetting.Contitions)
                    {
                        toActive.AddCondition((AnimatorConditionMode)condition.Mode, condition.Threshold, condition.Parameter);
                    }
                    var toIdle = activeState.AddTransition(idleState);
                    toIdle.hasExitTime = false;
                    toIdle.hasFixedDuration = true;
                    toIdle.duration = 0f;
                    toIdle.exitTime = 0f;
                    foreach (var condition in driveSetting.Contitions)
                    {
                        toIdle.AddCondition(((AnimatorConditionMode)condition.Mode).Reverse(), condition.Threshold, condition.Parameter);
                    }
                }
                var mergeAnimator = ctx.AvatarRootObject.AddComponent<ModularAvatarMergeAnimator>();
                mergeAnimator.animator = animator;
                mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                mergeAnimator.matchAvatarWriteDefaults = true;
            });
        }

        AnimationClip MakeEmptyAnimationClip()
        {
            var clip = new AnimationClip();
            clip.SetCurve("__AvatarParametersDriver_EMPTY__", typeof(GameObject), "localPosition.x", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1f / 60f, value = 0 } } });
            return clip;
        }
    }
}
