using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.localization;
using UnityEngine.Animations;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using Narazaka.VRChat.AvatarParametersUtil;
using Narazaka.VRChat.AvatarParametersUtil.Editor;

[assembly: ExportsPlugin(typeof(net.narazaka.vrchat.avatar_parameters_driver.editor.AvatarParametersDriverPlugin))]

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public class AvatarParametersDriverPlugin : Plugin<AvatarParametersDriverPlugin>
    {
        public override string QualifiedName => "net.narazaka.vrchat.avatar_parameters_driver";

        public override string DisplayName => "Avatar Parameters Driver";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar").Run("AvatarParametersDriver", Pass);
        }

        AnimationClip clip = null;

        void Pass(BuildContext ctx)
        {
            var avatarParametersDrivers = ctx.AvatarRootObject.GetComponentsInChildren<AvatarParametersDriver>();
            if (avatarParametersDrivers.Length == 0) return;

            clip = MakeEmptyAnimationClip();

            var nonHierarchicalDrivers = avatarParametersDrivers.Where(d => !d.PreserveHierarchy).ToArray();
            BuildAvatarParametersDriver(ctx, nonHierarchicalDrivers, ctx.AvatarRootObject);

            var hierarchicalDriversByObject = avatarParametersDrivers.Where(d => d.PreserveHierarchy).GroupBy(d => d.gameObject);
            foreach (var group in hierarchicalDriversByObject)
            {
                BuildAvatarParametersDriver(ctx, group.ToArray(), group.Key);
            }
        }

        void BuildAvatarParametersDriver(BuildContext ctx, IList<AvatarParametersDriver> avatarParametersDrivers, GameObject obj)
        {
            var info = ParameterInfo.ForContext(ctx);
            var rawParameters = ReferenceEquals(obj, ctx.AvatarRootObject)
                ? info.GetParametersForObject(obj)
                : AvatarParametersHierarchy.GetParametersAtHierarchy(info, ctx.AvatarRootObject, obj);
            var parameters = rawParameters.ToDistinctSubParameters();
            var parameterByName = parameters.ToDictionary(p => p.EffectiveName);

            var driveSettings = avatarParametersDrivers.SelectMany(d => d.DriveSettings).ToList();
            // レイヤー名にGameObject名を出すため、driveSettings と同じ順序で持ち主のGameObjectを並べておく
            var driveSettingOwners = avatarParametersDrivers.SelectMany(d => d.DriveSettings.Select(_ => d.gameObject)).ToList();
            var parameterNames = driveSettings.SelectMany(d => d.Contitions).Select(d => d.Parameter)
                .Concat(driveSettings.SelectMany(d => d.Parameters).Select(d => d.name))
                .Concat(driveSettings.SelectMany(d => d.Parameters).Where(p => p.type == VRC_AvatarParameterDriver.ChangeType.Copy).Select(d => d.source))
                .Distinct();
            var invalidParameters = parameterNames.Where(p => !parameterByName.ContainsKey(p)).ToArray();
            if (invalidParameters.Length > 0)
            {
                var invalidParameterSet = new HashSet<string>(invalidParameters);
                // 複数コンポーネントがマージされている場合があるため、どのコンポーネントが無効なパラメーターを参照しているか個別に報告する
                foreach (var driver in avatarParametersDrivers)
                {
                    var missingInDriver = ParameterNamesOf(driver).Where(invalidParameterSet.Contains).ToArray();
                    if (missingInDriver.Length == 0) continue;
                    using (ErrorReport.WithContextObject(driver.gameObject))
                    {
                        ErrorReport.ReportError(new ParametersNotFoundError(missingInDriver));
                    }
                }
                return;
            }
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
                var layer = animator.AddLastLayer($"Avatar Parameters Driver [{driveSettingOwners[i].name}]");
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
                    if (driveSetting.HasReturnConditions)
                    {
                        MakeBackTransition(activeState, idleState, driveSetting.Contitions, parameterByName);
                    }
                    else
                    {
                        MakeFreeBackTransition(activeState, idleState);
                    }
                }
                else
                {
                    MakeForwardTransition(idleState, activeState, driveSetting.Contitions);
                    if (driveSetting.HasReturnConditions)
                    {
                        MakeBackTransition(activeState, idleState, driveSetting.Contitions, parameterByName);
                    }
                    else
                    {
                        MakeFreeBackTransition(activeState, idleState);
                    }
                }
            }
            var mergeAnimator = obj.AddComponent<ModularAvatarMergeAnimator>();
            mergeAnimator.animator = animator;
            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            mergeAnimator.matchAvatarWriteDefaults = true;

            foreach (var avatarParametersDriver in avatarParametersDrivers)
            {
                Object.DestroyImmediate(avatarParametersDriver);
            }
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

        void MakeFreeBackTransition(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.hasFixedDuration = true;
            transition.duration = 0f;
            transition.exitTime = 0f;
        }

        string[] ParameterNamesOf(AvatarParametersDriver driver)
        {
            return driver.DriveSettings.SelectMany(d => d.Contitions).Select(d => d.Parameter)
                .Concat(driver.DriveSettings.SelectMany(d => d.Parameters).Select(d => d.name))
                .Concat(driver.DriveSettings.SelectMany(d => d.Parameters).Where(p => p.type == VRC_AvatarParameterDriver.ChangeType.Copy).Select(d => d.source))
                .Distinct().ToArray();
        }

        AnimationClip MakeEmptyAnimationClip()
        {
            var clip = new AnimationClip();
            clip.SetCurve("__AvatarParametersDriver_EMPTY__", typeof(GameObject), "localPosition.x", new AnimationCurve { keys = new Keyframe[] { new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1f / 60f, value = 0 } } });
            return clip;
        }

        static readonly istring Title = new istring("Parameters not found", "パラメーターが見付かりませんでした");

        // NDMF標準の SimpleError を使うと、Error Report ウィンドウで他のエラーと同じ
        // アイコン + タイトル + 内容のレイアウトになる。ローカライズ機構は使わないので、
        // キーを素通しして常に渡された文字列（istring が現在の言語で解決した文字列）を
        // そのまま表示する Localizer を充てる
        static readonly Localizer PassthroughLocalizer = new Localizer("en-US", () => new List<(string, System.Func<string, string>)>
        {
            ("en-US", key => "{0}"),
        });

        class ParametersNotFoundError : SimpleError
        {
            readonly string detail;

            public ParametersNotFoundError(string[] parameters)
            {
                detail = string.Join(", ", parameters);
            }

            public override Localizer Localizer => PassthroughLocalizer;
            public override string TitleKey => Title;
            public override string[] TitleSubst => new string[] { Title };
            public override string DetailsKey => detail;
            public override string[] DetailsSubst => new[] { detail };
            public override string FormatHint() => null;
            public override ErrorSeverity Severity => ErrorSeverity.Error;
        }
    }
}
