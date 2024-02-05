using System.Linq;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Narazaka.VRChat.AvatarParametersUtil.Editor;
using System;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    [Obsolete("Use AvatarParametersUtilEditor instead.")]
    public class ParameterUtil
    {
        [Obsolete("Use AvatarParametersUtilEditor.Get instead.")]
        public static ParameterUtil Get(SerializedObject serializedObject, bool forceUpdate = false)
        {
            AvatarParametersUtilEditor.Get(serializedObject, forceUpdate);
            return new ParameterUtil(serializedObject);
        }

        public SerializedObject SerializedObject;

        public ParameterUtil(SerializedObject serializedObject)
        {
            SerializedObject = serializedObject;
            AvatarParametersUtilEditor.Get(serializedObject, true);
        }

        [Obsolete("Use AvatarParametersUtilEditor instead.")]
        public void ShowParameterField(Rect rect, SerializedProperty property)
        {
            AvatarParametersUtilEditor.Get(SerializedObject).ShowParameterNameField(rect, property, GUIContent.none);
        }

        [Obsolete("Use AvatarParametersUtilEditor instead.")]
        public VRCExpressionParameters.Parameter GetParameter(string name)
        {
            return AvatarParametersUtilEditor.Get(SerializedObject).GetParameter(name);
        }
    }
}
