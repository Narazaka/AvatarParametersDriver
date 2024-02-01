using System.Linq;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public class ParameterUtil
    {
        static Dictionary<SerializedObject, ParameterUtil> Cache = new Dictionary<SerializedObject, ParameterUtil>();

        public static ParameterUtil Get(SerializedObject serializedObject, bool forceUpdate = false)
        {
            if (Cache.TryGetValue(serializedObject, out var parameterUtil) && parameterUtil != null)
            {
                if (forceUpdate)
                {
                    parameterUtil.UpdateParametersCache();
                }
            }
            else
            {
                parameterUtil = new ParameterUtil(serializedObject);
                Cache.Add(serializedObject, parameterUtil);
            }
            return parameterUtil;
        }

        public SerializedObject SerializedObject;
        VRCExpressionParameters.Parameter[] ParametersCache;
        Dictionary<string, int> ParameterNameToIndexCache = new Dictionary<string, int>();

        public ParameterUtil(SerializedObject serializedObject)
        {
            SerializedObject = serializedObject;
            UpdateParametersCache();
        }

        public void ShowParameterField(Rect rect, SerializedProperty property)
        {
            rect.width -= EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, property, GUIContent.none);
            rect.x += rect.width;
            rect.width = EditorGUIUtility.singleLineHeight;
            GUIStyle style = "IN DropDown";
            if (EditorGUI.DropdownButton(rect, GUIContent.none, FocusType.Keyboard, style))
            {
                PopupWindow.Show(rect, new ParametersPopupWindow(GetParentAvatar())
                {
                    UpdateProperty = (name) =>
                    {
                        property.stringValue = name;
                        SerializedObject.ApplyModifiedProperties();
                        UpdateParametersCache();
                    }
                });
            }
            var parameter = GetParameter(property.stringValue);
            rect.x -= 30;
            rect.width = 30;
            EditorGUI.LabelField(rect, parameter == null ? "?" : parameter.valueType.ToString(), EditorStyles.centeredGreyMiniLabel);
        }

        public VRCExpressionParameters.Parameter GetParameter(string name)
        {
            if (ParameterNameToIndexCache.TryGetValue(name, out var index))
            {
                return ParametersCache[index];
            }
            return null;
        }

        void UpdateParametersCache()
        {
            var avatar = GetParentAvatar();
            ParametersCache = Util.GetParameters(avatar, true);
            ParameterNameToIndexCache = ParametersCache.Select((p, index) => new { p.name, index }).ToDictionary(p => p.name, p => p.index);
        }

        VRCAvatarDescriptor GetParentAvatar()
        {
            return (SerializedObject.targetObject as Component).GetComponentInParent<VRCAvatarDescriptor>();
        }
    }
}
