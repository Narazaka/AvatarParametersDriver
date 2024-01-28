using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace net.narazaka.vrchat.avatar_parameters_driver.editor
{
    public class ParametersTreeView : TreeView
    {
        class Item : TreeViewItem
        {
            public VRCExpressionParameters.Parameter source;
        }

        public Action<VRCExpressionParameters.Parameter> OnSelect;
        public Action<VRCExpressionParameters.Parameter> OnCommit;

        VRCExpressionParameters.Parameter[] Parameters;

        public ParametersTreeView(TreeViewState state, VRCExpressionParameters.Parameter[] parameters) : base(state)
        {
            Parameters = parameters;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            SetupParentsAndChildrenFromDepths(root, Parameters.Select((p, index) => new Item() { id = index, depth = 0, displayName = p.name, source = p } as TreeViewItem).ToList());
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item is Item source)
            {
                var rect = args.rowRect;
                rect.xMin += GetContentIndent(args.item) + extraSpaceBeforeIconAndLabel;
                EditorGUI.LabelField(rect, source.source.name);
            }
            else
            {
                base.RowGUI(args);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var item = Parameters[selectedIds[0]];
            if (item != null && OnSelect != null) OnSelect(item);
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = Parameters[id];
            if (item != null && OnCommit != null) OnCommit(item);
        }
    }
}
