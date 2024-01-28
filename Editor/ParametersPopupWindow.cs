using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public class ParametersPopupWindow : PopupWindowContent
    {
        public Action Redraw;
        VRCExpressionParameters.Parameter[] Parameters;
        SerializedProperty Property;
        float Width;
        SearchField SearchField;
        string SearchQuery;
        ParametersTreeView TreeView;

        public ParametersPopupWindow(VRCExpressionParameters.Parameter[] parameters, SerializedProperty property, float width)
        {
            Parameters = parameters;
            Property = property;
            Width = width;
        }

        public override void OnGUI(Rect rect)
        {

            if (SearchField == null) SearchField = new SearchField();
            SearchQuery = SearchField.OnGUI(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), SearchQuery);

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
