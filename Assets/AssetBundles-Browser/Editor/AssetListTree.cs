using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using AssetBundleBrowser.AssetBundleModel;


namespace AssetBundleBrowser
{
    internal class AssetListTree : TreeView
    {
        List<BundleInfo> m_SourceBundles = new List<BundleInfo>();
        AssetBundleManageTab m_Controller;
        Object[] m_EmptyObjectList = new Object[0];

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState => new MultiColumnHeaderState(GetColumns());

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };

            retVal[0].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 100;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Bundle", "Bundle name. 'auto' means asset was pulled in due to dependency");
            retVal[1].minWidth = 50;
            retVal[1].width = 100;
            retVal[1].maxWidth = 300;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Size", "Size on disk");
            retVal[2].minWidth = 30;
            retVal[2].width = 75;
            retVal[2].maxWidth = 100;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("!", "Errors, Warnings, or Info");
            retVal[3].minWidth = 16;
            retVal[3].width = 16;
            retVal[3].maxWidth = 16;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = false;
            retVal[3].autoResize = false;

            return retVal;
        }

        internal AssetListTree(TreeViewState state, MultiColumnHeaderState mchs, AssetBundleManageTab ctrl ) : base(state, new MultiColumnHeader(mchs))
        {
            m_Controller = ctrl;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }

        internal void Update()
        {
            bool dirty = false;
            foreach (var bundle in m_SourceBundles)
            {
                dirty |= bundle.Dirty;
            }
            if (dirty)
                Reload();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            return rows;
        }

        internal void SetSelectedBundles(IEnumerable<BundleInfo> bundles)
        {
            m_Controller.SetSelectedItems(null);
            m_SourceBundles = bundles.ToList();
            SetSelection(new List<int>());
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            return Model.CreateAssetListTreeView(m_SourceBundles);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as AssetTreeItem, args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, AssetTreeItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            if(column != 3)
               GUI.color = item.ItemColor;

            switch (column)
            {
                case 0:
                    {
                        var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if(item.icon != null)
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height), 
                            item.displayName, 
                            args.selected, 
                            args.focused);
                    }
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.Asset.BundleName, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.Asset.GetSizeString(), args.selected, args.focused);
                    break;
                case 3:
                    var icon = item.MessageIcon();
                    if (icon != null)
                    {
                        var iconRect = new Rect(cellRect.x, cellRect.y, cellRect.height, cellRect.height);
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }
                    break;
            }
            GUI.color = oldColor;
        }

        protected override void DoubleClickedItem(int id)
        {
            var assetItem = FindItem(id, rootItem) as AssetTreeItem;
            if (assetItem != null)
            {
                Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.Asset.FullAssetName);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        public void SetSelection( List<string> paths )
        {
            List<int> selected = new List<int>();
            AddIfInPaths( paths, selected, rootItem );
            SetSelection( selected );
        }

        void AddIfInPaths( List<string> paths, List<int> selected, TreeViewItem me )
        {
            var assetItem = me as AssetTreeItem;
            if( assetItem != null && assetItem.Asset != null )
            {
                if( paths.Contains( assetItem.Asset.FullAssetName ) )
                {
                    if( selected.Contains( me.id ) == false )
                        selected.Add( me.id );
                }
            }

            if( me.hasChildren )
            {
                foreach( TreeViewItem item in me.children )
                {
                    AddIfInPaths( paths, selected, item );
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null)
                return;

            List<Object> selectedObjects = new List<Object>();
            List<AssetInfo> selectedAssets = new List<AssetInfo>();
            foreach (var id in selectedIds)
            {
                var assetItem = FindItem(id, rootItem) as AssetTreeItem;
                if (assetItem != null)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.Asset.FullAssetName);
                    selectedObjects.Add(o);
                    Selection.activeObject = o;
                    selectedAssets.Add(assetItem.Asset);
                }
            }
            m_Controller.SetSelectedItems(selectedAssets);
            Selection.objects = selectedObjects.ToArray();
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            args.draggedItemIDs = GetSelection();
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = m_EmptyObjectList;
            List<AssetTreeItem> items = 
                new List<AssetTreeItem>(args.draggedItemIDs.Select(id => FindItem(id, rootItem) as AssetTreeItem));
            DragAndDrop.paths = items.Select(a => a.Asset.FullAssetName).ToArray();
            DragAndDrop.SetGenericData("AssetListTreeSource", this);
            DragAndDrop.StartDrag("AssetListTree");
        }
        
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if(IsValidDragDrop())
            {
                if (args.performDrop)
                {
                    Model.MoveAssetToBundle(DragAndDrop.paths, m_SourceBundles[0].m_Name.BundleName, m_SourceBundles[0].m_Name.Variant);
                    Model.ExecuteAssetMove();
                    foreach (var bundle in m_SourceBundles)
                    {
                        bundle.RefreshAssetList();
                    }
                    m_Controller.UpdateSelectedBundles(m_SourceBundles);
                }
                return DragAndDropVisualMode.Copy;//Move;
            }

            return DragAndDropVisualMode.Rejected;
        }

        protected bool IsValidDragDrop()
        {
            //can't drag onto none or >1 bundles
            if (m_SourceBundles.Count == 0 || m_SourceBundles.Count > 1)
                return false;
            
            //can't drag nothing
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
                return false;

            //can't drag into a folder
            var folder = m_SourceBundles[0] as BundleFolderInfo;
            if (folder != null)
                return false;

            var data = m_SourceBundles[0] as BundleDataInfo;
            if(data == null)
                return false; // this should never happen.

            var thing = DragAndDrop.GetGenericData("AssetListTreeSource") as AssetListTree;
            if (thing != null)
                return false;
            
            if(data.IsEmpty)
                return true;


            if (data.IsSceneBundle)
            {
                foreach (var assetPath in DragAndDrop.paths)
                {
                    if ((AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(SceneAsset)) && (!AssetDatabase.IsValidFolder(assetPath)))
                        return false;
                }
            }
            else
            {
                foreach (var assetPath in DragAndDrop.paths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(SceneAsset))
                        return false;
                }
            }

            return true;
        }

        protected override void ContextClickedItem(int id)
        {
            List<AssetTreeItem> selectedNodes = new List<AssetTreeItem>();
            foreach(var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem) as AssetTreeItem);
            }

            if(selectedNodes.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove asset(s) from bundle."), false, RemoveAssets, selectedNodes);
                menu.ShowAsContext();
            }
        }

        void RemoveAssets(object obj)
        {
            var selectedNodes = obj as List<AssetTreeItem>;
            var assets = new List<AssetInfo>();
            //var bundles = new List<AssetBundleModel.BundleInfo>();
            foreach (var node in selectedNodes)
            {
                if (!System.String.IsNullOrEmpty(node.Asset.BundleName))
                    assets.Add(node.Asset);
            }
            Model.MoveAssetToBundle(assets, string.Empty, string.Empty);
            Model.ExecuteAssetMove();
            foreach (var bundle in m_SourceBundles)
            {
                bundle.RefreshAssetList();
            }
            m_Controller.UpdateSelectedBundles(m_SourceBundles);
            //ReloadAndSelect(new List<int>());
        }

        protected override void KeyEvent()
        {
            if (m_SourceBundles.Count > 0 && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<AssetTreeItem> selectedNodes = new List<AssetTreeItem>();
                foreach (var nodeID in GetSelection())
                {
                    selectedNodes.Add(FindItem(nodeID, rootItem) as AssetTreeItem);
                }

                RemoveAssets(selectedNodes);
            }
        }
    }
}
