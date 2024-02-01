using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDKBase;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    [CustomEditor(typeof(AvatarParametersDriver))]
    public class AvatarParametersDriverEditor : Editor
    {
        SerializedProperty DriveSettings;
        ReorderableList DriveSettingsList;
        Dictionary<int, ReorderableList> ContitionsListCache = new Dictionary<int, ReorderableList>();
        Dictionary<int, ReorderableList> ParametersListCache = new Dictionary<int, ReorderableList>();
        ParameterUtil ParameterUtil;

        void OnEnable()
        {
            ParameterUtil = new ParameterUtil(serializedObject);
            DriveSettings = serializedObject.FindProperty(nameof(AvatarParametersDriver.DriveSettings));
            DriveSettingsList = new ReorderableList(serializedObject, DriveSettings);
            DriveSettingsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Drive Settings");
            };
            DriveSettingsList.elementHeightCallback = (int index) =>
            {
                var element = DriveSettings.GetArrayElementAtIndex(index);
                var conditionsList = GetConditionsList(index, element);
                var conditionsListHeight = conditionsList.GetHeight();
                var parametersList = GetParametersList(index, element);
                var parametersListHeight = parametersList.GetHeight();
                return conditionsListHeight + parametersListHeight + EditorGUIUtility.standardVerticalSpacing * 4 + EditorGUIUtility.singleLineHeight;
            };
            DriveSettingsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = DriveSettings.GetArrayElementAtIndex(index);
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative(nameof(DriveSetting.LocalOnly)));
                rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                var conditionsList = GetConditionsList(index, element);
                var conditionsListHeight = conditionsList.GetHeight();
                conditionsList.DoList(new Rect(rect.x, rect.y, rect.width, conditionsListHeight));
                rect.y += EditorGUIUtility.standardVerticalSpacing + conditionsListHeight;
                var parametersList = GetParametersList(index, element);
                var parametersListHeight = parametersList.GetHeight();
                parametersList.DoList(new Rect(rect.x, rect.y, rect.width, parametersListHeight));
            };
            DriveSettingsList.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
            {
                ContitionsListCache.Remove(oldIndex);
                ContitionsListCache.Remove(newIndex);
                ParametersListCache.Remove(oldIndex);
                ParametersListCache.Remove(newIndex);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DriveSettingsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        ReorderableList GetConditionsList(int index, SerializedProperty driveSetting = null)
        {
            if (!ContitionsListCache.TryGetValue(index, out var conditionsList))
            {
                conditionsList = ContitionsListCache[index] = SetupConditionsList((driveSetting ?? DriveSettings.GetArrayElementAtIndex(index)).FindPropertyRelative(nameof(DriveSetting.Contitions)));
            }
            return conditionsList;
        }

        ReorderableList SetupConditionsList(SerializedProperty conditionsElement)
        {
            var conditionsList = new ReorderableList(serializedObject, conditionsElement);
            conditionsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Conditions");
            };
            conditionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = conditionsList.serializedProperty.GetArrayElementAtIndex(index);
                var parameter = element.FindPropertyRelative(nameof(DriveCondition.Parameter));
                var mode = element.FindPropertyRelative(nameof(DriveCondition.Mode));
                var threshold = element.FindPropertyRelative(nameof(DriveCondition.Threshold));
                var valueType = ParameterUtil.GetParameter(parameter.stringValue)?.valueType;
                var width = rect.width;
                rect.width = 65;
                EditorGUI.LabelField(rect, "Parameter");
                rect.x += rect.width;
                rect.width = width - 195;
                ParameterUtil.ShowParameterField(rect, parameter);
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
            };
            conditionsList.elementHeight = EditorGUIUtility.singleLineHeight;
            return conditionsList;
        }

        ReorderableList GetParametersList(int index, SerializedProperty driveSetting = null)
        {
            if (!ParametersListCache.TryGetValue(index, out var parametersList))
            {
                parametersList = ParametersListCache[index] = SetupParametersList((driveSetting ?? DriveSettings.GetArrayElementAtIndex(index)).FindPropertyRelative(nameof(DriveSetting.Parameters)));
            }
            return parametersList;
        }

        ReorderableList SetupParametersList(SerializedProperty parametersElement)
        {
            var parametersList = new ReorderableList(serializedObject, parametersElement);
            parametersList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Drive Parameters");
            };
            parametersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var width = rect.width;
                var element = parametersList.serializedProperty.GetArrayElementAtIndex(index);
                var type = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.type));
                var name = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.name));
                var value = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.value));
                var parameter = ParameterUtil.GetParameter(name.stringValue);
                var parameterIsBool = parameter != null && parameter.valueType == VRCExpressionParameters.ValueType.Bool;
                switch (type.enumValueIndex)
                {
                    case 2: // random
                        rect.width = 70;
                        EditorGUI.PropertyField(rect, type, GUIContent.none);
                        rect.x += rect.width;
                        rect.width = width - 180;
                        ParameterUtil.ShowParameterField(rect, name);
                        if (parameterIsBool)
                        {
                            var chance = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.chance));
                            rect.x += rect.width;
                            rect.width = 110;
                            EditorGUI.Slider(rect, chance, 0, 1, GUIContent.none);
                        }
                        else
                        {
                            rect.x += rect.width;
                            rect.width = 45;
                            EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.valueMin)), GUIContent.none);
                            rect.x += rect.width;
                            rect.width = 20;
                            EditorGUI.LabelField(rect, "～");
                            rect.x += rect.width;
                            rect.width = 45;
                            EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.valueMax)), GUIContent.none);
                        }
                        break;
                    case 3: // copy
                        var x = rect.x;
                        rect.height = EditorGUIUtility.singleLineHeight;
                        var convertRange = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.convertRange));

                        rect.width = 70;
                        EditorGUI.PropertyField(rect, type, GUIContent.none);
                        rect.x += rect.width;
                        rect.width = width - 180;
                        ParameterUtil.ShowParameterField(rect, name);
                        if (convertRange.boolValue)
                        {
                            rect.x += rect.width;
                            rect.width = 45;
                            EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.sourceMin)), GUIContent.none);
                            rect.x += rect.width;
                            rect.width = 20;
                            EditorGUI.LabelField(rect, "～");
                            rect.x += rect.width;
                            rect.width = 45;
                            EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.sourceMax)), GUIContent.none);
                        }

                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                        rect.x = x + 70 + (width - 180 - 30) / 2;
                        rect.width = 30;
                        EditorGUI.LabelField(rect, "↓");
                        rect.x = x + width - 110;
                        rect.width = 110;
                        EditorGUIUtility.labelWidth = 90;
                        EditorGUI.PropertyField(rect, convertRange);
                        EditorGUIUtility.labelWidth = 0;

                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                        rect.x = x + 70;
                        rect.width = width - 180;
                        ParameterUtil.ShowParameterField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.source)));
                        if (convertRange.boolValue)
                        {
                            rect.x += rect.width;
                            rect.width = 45;
                            EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.destMin)), GUIContent.none);
                            rect.x += rect.width;
                            rect.width = 20;
                            EditorGUI.LabelField(rect, "～");
                            rect.x += rect.width;
                            rect.width = 45;
                            EditorGUI.PropertyField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.destMax)), GUIContent.none);
                        }

                        break;
                    default:
                        rect.width = 70;
                        EditorGUI.PropertyField(rect, type, GUIContent.none);
                        rect.x += rect.width;
                        rect.width = width - 115;
                        ParameterUtil.ShowParameterField(rect, name);
                        rect.x += rect.width;
                        rect.width = 45;
                        if (parameterIsBool)
                        {
                            if (type.enumValueIndex == 1) // add
                            {
                                EditorGUI.LabelField(rect, EditorGUIUtility.IconContent("Warning"));
                            }
                            else
                            {
                                using (var check = new EditorGUI.ChangeCheckScope())
                                {
                                    var valueBool = EditorGUI.Toggle(rect, value.floatValue != 0);
                                    if (check.changed) value.floatValue = valueBool ? 1 : 0;
                                }
                            }
                        }
                        else
                        {
                            EditorGUI.PropertyField(rect, value, GUIContent.none);
                        }
                        break;
                }
            };
            parametersList.elementHeightCallback = (int index) =>
            {
                var element = parametersList.serializedProperty.GetArrayElementAtIndex(index);
                var type = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.type));
                switch (type.enumValueIndex)
                {
                    case 3: return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
                    default: return EditorGUIUtility.singleLineHeight;
                }
            };
            return parametersList;
        }
    }
}
