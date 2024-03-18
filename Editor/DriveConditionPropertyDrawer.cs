using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using Narazaka.VRChat.AvatarParametersUtil.Editor;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    [CustomPropertyDrawer(typeof(DriveCondition))]
    class DriveConditionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty element, GUIContent label)
        {
            var parameterUtil = AvatarParametersUtilEditor.Get(element.serializedObject);

            var parameter = element.FindPropertyRelative(nameof(DriveCondition.Parameter));
            var mode = element.FindPropertyRelative(nameof(DriveCondition.Mode));
            var threshold = element.FindPropertyRelative(nameof(DriveCondition.Threshold));
            var valueType = parameterUtil.GetParameter(parameter.stringValue)?.ParameterType;
            var width = rect.width;
            rect.width = 65;
            EditorGUI.LabelField(rect, label);
            rect.x += rect.width;
            rect.width = width - 195;
            parameterUtil.ShowParameterNameField(rect, parameter, GUIContent.none);
            if (mode.enumValueIndex == -1) mode.enumValueIndex = 0;
            if (valueType is AnimatorControllerParameterType type)
            {
                if (!DriveCondition.IsValidMode(type, DriveCondition.ModeByEnumValueIndex(mode.enumValueIndex)))
                {
                    switch (type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            mode.enumValueIndex = 0; // If
                            break;
                        case AnimatorControllerParameterType.Int:
                        case AnimatorControllerParameterType.Float:
                            mode.enumValueIndex = 2; // Greater
                            break;
                    }
                }

                if (type == AnimatorControllerParameterType.Bool)
                {
                    rect.x += rect.width;
                    rect.width = 130;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        var isIf = EditorGUI.Toggle(rect, mode.enumValueIndex == 0);
                        if (check.changed)
                        {
                            mode.enumValueIndex = isIf ? 0 : 1;
                        }
                    }
                }
                else
                {
                    rect.x += rect.width;
                    rect.width = 85;
                    var enums = type == AnimatorControllerParameterType.Int ? DriveCondition.IntEnums : DriveCondition.FloatEnums;
                    var enumLabels = type == AnimatorControllerParameterType.Int ? DriveCondition.IntEnumLabels : DriveCondition.FloatEnumLabels;
                    var partialEnumValueIndex = EditorGUI.Popup(rect, System.Array.IndexOf(enums, DriveCondition.ModeByEnumValueIndex(mode.enumValueIndex)), enumLabels);
                    mode.enumValueIndex = DriveCondition.EnumValueIndexByMode(enums[partialEnumValueIndex]);
                    rect.x += rect.width;
                    rect.width = 45;
                    EditorGUI.PropertyField(rect, threshold, GUIContent.none);
                }
            }
            else
            {
                var modeIsBool = mode.enumValueIndex < 2;
                rect.x += rect.width;
                rect.width = modeIsBool ? 130 : 85;
                EditorGUI.PropertyField(rect, mode, GUIContent.none);
                if (!modeIsBool)
                {
                    rect.x += rect.width;
                    rect.width = 45;
                    EditorGUI.PropertyField(rect, threshold, GUIContent.none);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
