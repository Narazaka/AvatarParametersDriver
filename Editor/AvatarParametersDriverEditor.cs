using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using VRC.SDKBase;
using VRC.SDK3.Avatars.ScriptableObjects;
using Narazaka.VRChat.AvatarParametersUtil.Editor;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    [CustomEditor(typeof(AvatarParametersDriver))]
    public class AvatarParametersDriverEditor : Editor
    {
        SerializedProperty DriveSettings;
        ReorderableList DriveSettingsList;
        Dictionary<int, ReorderableList> ContitionsListCache = new Dictionary<int, ReorderableList>();
        Dictionary<int, ReorderableList> PreContitionsListCache = new Dictionary<int, ReorderableList>();
        Dictionary<int, ReorderableList> ParametersListCache = new Dictionary<int, ReorderableList>();
        AvatarParametersUtilEditor ParameterUtil;

        void OnEnable()
        {
            ParameterUtil = AvatarParametersUtilEditor.Get(serializedObject, true);
            DriveSettings = serializedObject.FindProperty(nameof(AvatarParametersDriver.DriveSettings));
            DriveSettingsList = new ReorderableList(serializedObject, DriveSettings);
            DriveSettingsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, T.DriveSettings);
            };
            DriveSettingsList.elementHeightCallback = (int index) =>
            {
                var element = DriveSettings.GetArrayElementAtIndex(index);
                var usePreContitions = element.FindPropertyRelative(nameof(DriveSetting.UsePreContitions)).boolValue;
                var noReturnConditions = element.FindPropertyRelative(nameof(DriveSetting.NoReturnConditions));
                var preConditionsListHeight = 0f;
                if (usePreContitions)
                {
                    var preConditionsList = GetPreConditionsList(index, element);
                    preConditionsListHeight = preConditionsList.GetHeight();
                }
                var conditionsList = GetConditionsList(index, element);
                var conditionsListHeight = conditionsList.GetHeight();
                var parametersList = GetParametersList(index, element);
                var parametersListHeight = parametersList.GetHeight();
                return conditionsListHeight + preConditionsListHeight + parametersListHeight + EditorGUIUtility.standardVerticalSpacing * (preConditionsListHeight == 0f ? 5 : 6) + EditorGUIUtility.singleLineHeight * 3 + (noReturnConditions.boolValue ? EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight : 0);
            };
            DriveSettingsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = DriveSettings.GetArrayElementAtIndex(index);
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative(nameof(DriveSetting.LocalOnly)), T.LocalOnly.GUIContent);
                rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                var usePreContitions = element.FindPropertyRelative(nameof(DriveSetting.UsePreContitions));
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), usePreContitions, T.UsePreContitions.GUIContent);
                rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                if (usePreContitions.boolValue)
                {
                    var preConditionsList = GetPreConditionsList(index, element);
                    var preConditionsListHeight = preConditionsList.GetHeight();
                    preConditionsList.DoList(new Rect(rect.x, rect.y, rect.width, preConditionsListHeight));
                    rect.y += EditorGUIUtility.standardVerticalSpacing + preConditionsListHeight;
                }
                var conditionsList = GetConditionsList(index, element);
                var conditionsListHeight = conditionsList.GetHeight();
                conditionsList.DoList(new Rect(rect.x, rect.y, rect.width, conditionsListHeight));
                rect.y += EditorGUIUtility.standardVerticalSpacing + conditionsListHeight;
                var parametersList = GetParametersList(index, element);
                var parametersListHeight = parametersList.GetHeight();
                parametersList.DoList(new Rect(rect.x, rect.y, rect.width, parametersListHeight));
                rect.y += EditorGUIUtility.standardVerticalSpacing + parametersListHeight;
                var noReturnConditions = element.FindPropertyRelative(nameof(DriveSetting.NoReturnConditions));
                var noReturnConditionsRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginProperty(noReturnConditionsRect, T.NoReturnConditions.GUIContent, noReturnConditions);
                noReturnConditions.boolValue = EditorGUI.ToggleLeft(noReturnConditionsRect, T.NoReturnConditions.GUIContent, noReturnConditions.boolValue);
                EditorGUI.EndProperty();
                rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                if (noReturnConditions.boolValue)
                {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), T.NoReturnConditionsDescription, MessageType.Info);
                    rect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                }
            };
            DriveSettingsList.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
            {
                ContitionsListCache.Remove(oldIndex);
                ContitionsListCache.Remove(newIndex);
                PreContitionsListCache.Remove(oldIndex);
                PreContitionsListCache.Remove(newIndex);
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
                conditionsList = ContitionsListCache[index] = SetupConditionsList((driveSetting ?? DriveSettings.GetArrayElementAtIndex(index)).FindPropertyRelative(nameof(DriveSetting.Contitions)), T.Contitions.GUIContent);
            }
            return conditionsList;
        }

        ReorderableList GetPreConditionsList(int index, SerializedProperty driveSetting = null)
        {
            if (!PreContitionsListCache.TryGetValue(index, out var conditionsList))
            {
                conditionsList = PreContitionsListCache[index] = SetupConditionsList((driveSetting ?? DriveSettings.GetArrayElementAtIndex(index)).FindPropertyRelative(nameof(DriveSetting.PreContitions)), T.PreContitions.GUIContent);
            }
            return conditionsList;
        }

        ReorderableList SetupConditionsList(SerializedProperty conditionsElement, GUIContent header)
        {
            var parameterLabelContent = T.Parameters.GUIContent;
            var conditionsList = new ReorderableList(serializedObject, conditionsElement);
            conditionsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, header);
            };
            conditionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                EditorGUI.PropertyField(rect, conditionsList.serializedProperty.GetArrayElementAtIndex(index), parameterLabelContent);
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
                EditorGUI.LabelField(rect, T.DriveParameters);
            };
            parametersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var width = rect.width;
                var element = parametersList.serializedProperty.GetArrayElementAtIndex(index);
                var type = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.type));
                var name = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.name));
                var value = element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.value));
                var parameter = ParameterUtil.GetParameter(name.stringValue);
                var parameterIsBool = parameter != null && parameter.ParameterType == UnityEngine.AnimatorControllerParameterType.Bool;
                switch (type.enumValueIndex)
                {
                    case 2: // random
                        rect.width = 70;
                        EditorGUI.PropertyField(rect, type, GUIContent.none);
                        rect.x += rect.width;
                        rect.width = width - 180;
                        ParameterUtil.ShowParameterNameField(rect, name, GUIContent.none);
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
                        ParameterUtil.ShowParameterNameField(rect, element.FindPropertyRelative(nameof(VRC_AvatarParameterDriver.Parameter.source)), GUIContent.none);
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
                        EditorGUI.PropertyField(rect, convertRange, T.ConvertRange.GUIContent);
                        EditorGUIUtility.labelWidth = 0;

                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                        rect.x = x + 70;
                        rect.width = width - 180;
                        ParameterUtil.ShowParameterNameField(rect, name, GUIContent.none);
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
                        ParameterUtil.ShowParameterNameField(rect, name, GUIContent.none);
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

        static class T
        {
            public static istring DriveSettings = new istring("Drive Settings", "パラメータードライバー設定");
            public static istring LocalOnly = new istring("Local Only", "ローカルのみ");
            public static istring UsePreContitions = new istring("Use Pre Contitions", "事前条件を使用");
            public static istring Contitions = new istring("Contitions", "条件");
            public static istring PreContitions = new istring("Pre Contitions", "事前条件");
            public static istring Parameters = new istring("Parameters", "パラメーター");
            public static istring DriveParameters = new istring("Drive Parameters", "うごかすパラメーター");
            public static istring ConvertRange = new istring("Convert Range", "値の範囲を変換");
            public static istring NoReturnConditions = new istring("No Return Conditions (Experimental)", "戻り条件を設定しない（実験的）");
            public static istring NoReturnConditionsDescription = new istring(
                "Parameters will continue to be set as long as the condition is satisfied.",
                "条件が成立している間パラメーターが設定され続けます"
                );
        }
    }
}
