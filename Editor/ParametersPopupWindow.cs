using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public class ParametersPopupWindow : PopupWindowContent
    {
        public Action Redraw;
        VRCAvatarDescriptor Avatar;
        VRCExpressionParameters.Parameter[] Parameters;
        SerializedProperty Property;
        SearchField SearchField;
        string SearchQuery;
        bool IncludeAnimators;
        ParametersTreeView TreeView;

        public ParametersPopupWindow(VRCAvatarDescriptor avatar, SerializedProperty property)
        {
            Avatar = avatar;
            Property = property;
        }

        public override void OnGUI(Rect rect)
        {

            if (SearchField == null) SearchField = new SearchField();
            SearchQuery = SearchField.OnGUI(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), SearchQuery);

            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height -= EditorGUIUtility.singleLineHeight;
            var newIncludeAnimators = EditorGUI.Toggle(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Include Animators", IncludeAnimators);
            if (newIncludeAnimators != IncludeAnimators || Parameters == null)
            {
                IncludeAnimators = newIncludeAnimators;
                Parameters = Util.GetParameters(Avatar, IncludeAnimators);
                TreeView = null;
            }
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.height -= EditorGUIUtility.singleLineHeight;
            if (TreeView == null)
            {
                TreeView = new ParametersTreeView(new TreeViewState(), Parameters)
                {
                    OnSelect = (parameter) =>
                    {
                        Property.stringValue = parameter.name;
                        Property.serializedObject.ApplyModifiedProperties();
                        if (Redraw != null) Redraw();
                    },
                    OnCommit = (parameter) =>
                    {
                        Property.stringValue = parameter.name;
                        Property.serializedObject.ApplyModifiedProperties();
                        if (Redraw != null) Redraw();
                        editorWindow.Close();
                    }
                };
                TreeView.Reload();
            }
            TreeView.searchString = SearchQuery;
            TreeView.OnGUI(rect);
        }
    }
}
