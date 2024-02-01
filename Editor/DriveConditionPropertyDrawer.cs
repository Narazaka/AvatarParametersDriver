using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    [CustomPropertyDrawer(typeof(DriveCondition))]
    class DriveConditionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty element, GUIContent label)
        {
            var parameterUtil = ParameterUtil.Get(element.serializedObject);

            var parameter = element.FindPropertyRelative(nameof(DriveCondition.Parameter));
            var mode = element.FindPropertyRelative(nameof(DriveCondition.Mode));
            var threshold = element.FindPropertyRelative(nameof(DriveCondition.Threshold));
            var valueType = parameterUtil.GetParameter(parameter.stringValue)?.valueType;
            var width = rect.width;
            rect.width = 65;
            EditorGUI.LabelField(rect, label);
            rect.x += rect.width;
            rect.width = width - 195;
            parameterUtil.ShowParameterField(rect, parameter);
            if (mode.enumValueIndex == -1) mode.enumValueIndex = 0;
            if (valueType is VRCExpressionParameters.ValueType type)
            {
                if (!DriveCondition.IsValidMode(type, DriveCondition.ModeByEnumValueIndex(mode.enumValueIndex)))
                {
                    switch (type)
                    {
                        case VRCExpressionParameters.ValueType.Bool:
                            mode.enumValueIndex = 0; // If
                            break;
                        case VRCExpressionParameters.ValueType.Int:
                        case VRCExpressionParameters.ValueType.Float:
                            mode.enumValueIndex = 2; // Greater
                            break;
                    }
                }

                if (type == VRCExpressionParameters.ValueType.Bool)
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
                    var enums = type == VRCExpressionParameters.ValueType.Int ? DriveCondition.IntEnums : DriveCondition.FloatEnums;
                    var enumLabels = type == VRCExpressionParameters.ValueType.Int ? DriveCondition.IntEnumLabels : DriveCondition.FloatEnumLabels;
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
