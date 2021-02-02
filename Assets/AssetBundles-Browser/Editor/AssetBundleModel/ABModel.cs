﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleBrowser.AssetBundleModel
{
    /// <summary>
    /// Static class holding model data for Asset Bundle Browser tool. Data in Model is read from DataSource, but is not pushed.  
    /// 
    /// If not using a custom DataSource, then the data comes from the AssetDatabase.  If you wish to alter the data from code, 
    ///  you should just push changes to the AssetDatabase then tell the Model to Rebuild(). If needed, you can also loop over
    ///  Update() until it returns true to force all sub-items to refresh.
    ///  
    /// </summary>
    public static class Model
    {
        const string k_NewBundleBaseName = "newbundle";
        const string k_NewVariantBaseName = "newvariant";
        internal static Color k_LightGrey = Color.grey * 1.5f;

        private static BundleFolderConcreteInfo s_RootLevelBundles = new BundleFolderConcreteInfo("", null);
        private static List<ABMoveData> s_MoveData = new List<ABMoveData>();
        private static List<BundleInfo> s_BundlesToUpdate = new List<BundleInfo>();
        private static Dictionary<string, AssetInfo> s_GlobalAssetList = new Dictionary<string, AssetInfo>();
        private static Dictionary<string, HashSet<string>> s_DependencyTracker = new Dictionary<string, HashSet<string>>();

        private static bool s_InErrorState = false;
        const string k_DefaultEmptyMessage = "Drag assets here or right-click to begin creating bundles.";
        const string k_ProblemEmptyMessage = "There was a problem parsing the list of bundles. See console.";
        static private Texture2D s_folderIcon = null;
        static private Texture2D s_bundleIcon = null;
        static private Texture2D s_sceneIcon = null;

        /// <summary>
        /// Update will loop over bundles that need updating and update them. It will only update one bundle
        ///  per frame and will continue on the same bundle next frame until that bundle is marked as doneUpdating.
        ///  By default, this will cause a very slow collection of dependency data as it will only update one bundle per
        /// </summary>
        public static bool Update()
        {
            bool shouldRepaint = false;
            ExecuteAssetMove(false);     //this should never do anything. just a safety check.

            //TODO - look into EditorApplication callback functions.
            
            int size = s_BundlesToUpdate.Count;
            if (size > 0)
            {
                s_BundlesToUpdate[size - 1].Update();
                s_BundlesToUpdate.RemoveAll(item => item.DoneUpdating == true);
                if (s_BundlesToUpdate.Count == 0)
                {
                    shouldRepaint = true;
                    foreach(var bundle in s_RootLevelBundles.GetChildList())
                    {
                        bundle.RefreshDupeAssetWarning();
                    }
                }
            }
            return shouldRepaint;
        }

        internal static void ForceReloadData(TreeView tree)
        {
            s_InErrorState = false;
            Rebuild();
            tree.Reload();
            bool doneUpdating = s_BundlesToUpdate.Count == 0;

            EditorUtility.DisplayProgressBar("Updating Bundles", "", 0);
            int fullBundleCount = s_BundlesToUpdate.Count;
            while (!doneUpdating && !s_InErrorState)
            {
                int currCount = s_BundlesToUpdate.Count;
                EditorUtility.DisplayProgressBar("Updating Bundles", s_BundlesToUpdate[currCount-1].DisplayName, (float)(fullBundleCount- currCount) / (float)fullBundleCount);
                doneUpdating = Update();
            }
            EditorUtility.ClearProgressBar();
        }
        
        /// <summary>
        /// Clears and rebuilds model data.  
        /// </summary>
        public static void Rebuild()
        {
            s_RootLevelBundles = new BundleFolderConcreteInfo("", null);
            s_MoveData = new List<ABMoveData>();
            s_BundlesToUpdate = new List<BundleInfo>();
            s_GlobalAssetList = new Dictionary<string, AssetInfo>();
            Refresh();
        }

        internal static void AddBundlesToUpdate(IEnumerable<BundleInfo> bundles)
        {
            foreach(var bundle in bundles)
            {
                bundle.ForceNeedUpdate();
                s_BundlesToUpdate.Add(bundle);
            }
        }

        internal static void Refresh()
        {
            EmptyMessage = k_ProblemEmptyMessage;
            if (s_InErrorState)
                return;

            var bundleList = ValidateBundleList();
            if(bundleList != null)
            {
                EmptyMessage = k_DefaultEmptyMessage;
                foreach (var bundleName in bundleList)
                {
                    AddBundleToModel(bundleName);
                }
                AddBundlesToUpdate(s_RootLevelBundles.GetChildList());
            }

            if(s_InErrorState)
            {
                s_RootLevelBundles = new BundleFolderConcreteInfo("", null);
                EmptyMessage = k_ProblemEmptyMessage;
            }
        }

        internal static string[] ValidateBundleList()
        {
            var bundleList = AssetDatabase.GetAllAssetBundleNames();
            bool valid = true;
            HashSet<string> bundleSet = new HashSet<string>();
            int index = 0;
            bool attemptedBundleReset = false;
            while(index < bundleList.Length)
            {
                var name = bundleList[index];
                if (!bundleSet.Add(name))
                {
                    LogError("Two bundles share the same name: " + name);
                    valid = false;
                }

                int lastDot = name.LastIndexOf('.');
                if (lastDot > -1)
                {
                    var bunName = name.Substring(0, lastDot);
                    var extraDot = bunName.LastIndexOf('.');
                    if(extraDot > -1)
                    {
                        if(attemptedBundleReset)
                        {
                            var message = "Bundle name '" + bunName + "' contains a period.";
                            message += "  Internally Unity keeps 'bundleName' and 'variantName' separate, but externally treat them as 'bundleName.variantName'.";
                            message += "  If a bundleName contains a period, the build will (probably) succeed, but this tool cannot tell which portion is bundle and which portion is variant.";
                            LogError(message);
                            valid = false;
                        }
                        else
                        {
                            AssetDatabase.RemoveUnusedAssetBundleNames();
                            index = 0;
                            bundleSet.Clear();
                            bundleList = AssetDatabase.GetAllAssetBundleNames();
                            attemptedBundleReset = true;
                            continue;
                        }
                    }

                    if (bundleList.Contains(bunName))
                    {
                        //there is a bundle.none and a bundle.variant coexisting.  Need to fix that or return an error.
                        if (attemptedBundleReset)
                        {
                            valid = false;
                            var message = "Bundle name '" + bunName + "' exists without a variant as well as with variant '" + name.Substring(lastDot+1) + "'.";
                            message += " That is an illegal state that will not build and must be cleaned up.";
                            LogError(message);
                        }
                        else
                        {
                            AssetDatabase.RemoveUnusedAssetBundleNames();
                            index = 0;
                            bundleSet.Clear();
                            bundleList = AssetDatabase.GetAllAssetBundleNames();
                            attemptedBundleReset = true;
                            continue;
                        }
                    }
                }

                index++;
            }

            if (valid)
                return bundleList;
            else
                return null;
        }

        internal static bool BundleListIsEmpty => s_RootLevelBundles.GetChildList().Count() == 0;

        internal static string EmptyMessage { get; private set; }

        internal static BundleInfo CreateEmptyBundle(BundleFolderInfo folder = null, string newName = null)
        {
            if ((folder as BundleVariantFolderInfo) != null)
                return CreateEmptyVariant(folder as BundleVariantFolderInfo);

            folder = (folder == null) ? s_RootLevelBundles : folder;
            string name = GetUniqueName(folder, newName);
            BundleNameData nameData;
            nameData = new BundleNameData(folder.m_Name.BundleName, name);
            return AddBundleToFolder(folder, nameData);
        }

        internal static BundleInfo CreateEmptyVariant(BundleVariantFolderInfo folder)
        {
            string name = GetUniqueName(folder, k_NewVariantBaseName);
            string variantName = folder.m_Name.BundleName + "." + name;
            BundleNameData nameData = new BundleNameData(variantName);
            return AddBundleToFolder(folder.Parent, nameData);
        }

        internal static BundleFolderInfo CreateEmptyBundleFolder(BundleFolderConcreteInfo folder = null)
        {
            folder = (folder == null) ? s_RootLevelBundles : folder;
            string name = GetUniqueName(folder) + "/dummy";
            BundleNameData nameData = new BundleNameData(folder.m_Name.BundleName, name);
            return AddFoldersToBundle(s_RootLevelBundles, nameData);
        }

        private static BundleInfo AddBundleToModel(string name)
        {
            if (name == null)
                return null;
            
            BundleNameData nameData = new BundleNameData(name);

            BundleFolderInfo folder = AddFoldersToBundle(s_RootLevelBundles, nameData);
            BundleInfo currInfo = AddBundleToFolder(folder, nameData);

            return currInfo;
        }

        private static BundleFolderConcreteInfo AddFoldersToBundle(BundleFolderInfo root, BundleNameData nameData)
        {
            BundleInfo currInfo = root;
            var folder = currInfo as BundleFolderConcreteInfo;
            var size = nameData.PathTokens.Count;
            for (var index = 0; index < size; index++)
            {
                if (folder != null)
                {
                    currInfo = folder.GetChild(nameData.PathTokens[index]);
                    if (currInfo == null)
                    {
                        currInfo = new BundleFolderConcreteInfo(nameData.PathTokens, index + 1, folder);
                        folder.AddChild(currInfo);
                    }

                    folder = currInfo as BundleFolderConcreteInfo;
                    if (folder == null)
                    {
                        s_InErrorState = true;
                        LogFolderAndBundleNameConflict(currInfo.m_Name.FullNativeName);
                        break;
                    }
                }
            }
            return currInfo as BundleFolderConcreteInfo;
        }

        private static void LogFolderAndBundleNameConflict(string name)
        {
            var message = "Bundle '";
            message += name;
            message += "' has a name conflict with a bundle-folder.";
            message += "Display of bundle data and building of bundles will not work.";
            message += "\nDetails: If you name a bundle 'x/y', then the result of your build will be a bundle named 'y' in a folder named 'x'.  You thus cannot also have a bundle named 'x' at the same level as the folder named 'x'.";
            LogError(message);
        }

        private static BundleInfo AddBundleToFolder(BundleFolderInfo root, BundleNameData nameData)
        {
            BundleInfo currInfo = root.GetChild(nameData.ShortName);
            if (!String.IsNullOrEmpty(nameData.Variant))
            {
                if(currInfo == null)
                {
                    currInfo = new BundleVariantFolderInfo(nameData.BundleName, root);
                    root.AddChild(currInfo);
                }
                var folder = currInfo as BundleVariantFolderInfo;
                if (folder == null)
                {
                    var message = "Bundle named " + nameData.ShortName;
                    message += " exists both as a standard bundle, and a bundle with variants.  ";
                    message += "This message is not supported for display or actual bundle building.  ";
                    message += "You must manually fix bundle naming in the inspector.";
                    
                    LogError(message);
                    return null;
                }
                
                
                currInfo = folder.GetChild(nameData.Variant);
                if (currInfo == null)
                {
                    currInfo = new BundleVariantDataInfo(nameData.FullNativeName, folder);
                    folder.AddChild(currInfo);
                }
                
            }
            else
            {
                if (currInfo == null)
                {
                    currInfo = new BundleDataInfo(nameData.FullNativeName, root);
                    root.AddChild(currInfo);
                }
                else
                {
                    var dataInfo = currInfo as BundleDataInfo;
                    if (dataInfo == null)
                    {
                        s_InErrorState = true;
                        LogFolderAndBundleNameConflict(nameData.FullNativeName);
                    }
                }
            }
            return currInfo;
        }

        private static string GetUniqueName(BundleFolderInfo folder, string suggestedName = null)
        {
            suggestedName = (suggestedName == null) ? k_NewBundleBaseName : suggestedName;
            string name = suggestedName;
            int index = 1;
            bool foundExisting = (folder.GetChild(name) != null);
            while (foundExisting)
            {
                name = suggestedName + index;
                index++;
                foundExisting = (folder.GetChild(name) != null);
            }
            return name;
        }

        internal static BundleTreeItem CreateBundleTreeView()
        {
            return s_RootLevelBundles.CreateTreeView(-1);
        }

        internal static AssetTreeItem CreateAssetListTreeView(IEnumerable<AssetBundleModel.BundleInfo> selectedBundles)
        {
            var root = new AssetTreeItem();
            if (selectedBundles != null)
            {
                foreach (var bundle in selectedBundles)
                {
                    bundle.AddAssetsToNode(root);
                }
            }
            return root;
        }

        internal static bool HandleBundleRename(BundleTreeItem item, string newName)
        {
            var originalName = new BundleNameData(item.Bundle.m_Name.FullNativeName);

            var findDot = newName.LastIndexOf('.');
            var findSlash = newName.LastIndexOf('/');
            var findBSlash = newName.LastIndexOf('\\');
            if (findDot == 0 || findSlash == 0 || findBSlash == 0)
                return false; //can't start a bundle with a / or .

            bool result = item.Bundle.HandleRename(newName, 0);

            if (findDot > 0 || findSlash > 0 || findBSlash > 0)
            {
                item.Bundle.Parent.HandleChildRename(newName, string.Empty);
            }

            ExecuteAssetMove();

            var node = FindBundle(originalName);
            if (node != null)
            {
                var message = "Failed to rename bundle named: ";
                message += originalName.FullNativeName;
                message += ".  Most likely this is due to the bundle being assigned to a folder in your Assets directory, AND that folder is either empty or only contains assets that are explicitly assigned elsewhere.";
                Debug.LogError(message);
            }

            return result;  
        }

        internal static void HandleBundleReparent(IEnumerable<BundleInfo> bundles, BundleFolderInfo parent)
        {
            parent = (parent == null) ? s_RootLevelBundles : parent;
            foreach (var bundle in bundles)
            {
                bundle.HandleReparent(parent.m_Name.BundleName, parent);
            }
            ExecuteAssetMove();
        }

        internal static void HandleBundleMerge(IEnumerable<BundleInfo> bundles, BundleDataInfo target)
        {
            foreach (var bundle in bundles)
            {
                bundle.HandleDelete(true, target.m_Name.BundleName, target.m_Name.Variant);
            }
            ExecuteAssetMove();
        }

        internal static void HandleBundleDelete(IEnumerable<BundleInfo> bundles)
        {
            var nameList = new List<BundleNameData>();
            foreach (var bundle in bundles)
            {
                nameList.Add(bundle.m_Name);
                bundle.HandleDelete(true);
            }
            ExecuteAssetMove();

            //check to see if any bundles are still there...
            foreach(var name in nameList)
            {
                var node = FindBundle(name);
                if(node != null)
                {
                    var message = "Failed to delete bundle named: ";
                    message += name.FullNativeName;
                    message += ".  Most likely this is due to the bundle being assigned to a folder in your Assets directory, AND that folder is either empty or only contains assets that are explicitly assigned elsewhere.";
                    Debug.LogError(message);
                }
            }
        }

        internal static BundleInfo FindBundle(BundleNameData name)
        {
            BundleInfo currNode = s_RootLevelBundles;
            foreach (var token in name.PathTokens)
            {
                if(currNode is BundleFolderInfo)
                {
                    currNode = (currNode as BundleFolderInfo).GetChild(token);
                    if (currNode == null)
                        return null;
                }
                else
                {
                    return null;
                }
            }

            if(currNode is BundleFolderInfo)
            {
                currNode = (currNode as BundleFolderInfo).GetChild(name.ShortName);
                if(currNode is BundleVariantFolderInfo)
                {
                    currNode = (currNode as BundleVariantFolderInfo).GetChild(name.Variant);
                }
                return currNode;
            }
            else
            {
                return null;
            }
        }

        internal static BundleInfo HandleDedupeBundles(IEnumerable<BundleInfo> bundles, bool onlyOverlappedAssets)
        {
            var newBundle = CreateEmptyBundle();
            HashSet<string> dupeAssets = new HashSet<string>();
            HashSet<string> fullAssetList = new HashSet<string>();

            //if they were just selected, then they may still be updating.
            bool doneUpdating = s_BundlesToUpdate.Count == 0;
            while (!doneUpdating)
                doneUpdating = Update();

            foreach (var bundle in bundles)
            {
                foreach (var asset in bundle.GetDependencies())
                {
                    if (onlyOverlappedAssets)
                    {
                        if (!fullAssetList.Add(asset.FullAssetName))
                            dupeAssets.Add(asset.FullAssetName);
                    }
                    else
                    {
                        if (asset.IsMessageSet(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles))
                            dupeAssets.Add(asset.FullAssetName);
                    }
                }
            }

            if (dupeAssets.Count == 0)
                return null;
            
            MoveAssetToBundle(dupeAssets, newBundle.m_Name.BundleName, string.Empty);
            ExecuteAssetMove();
            return newBundle;
        }

        internal static BundleInfo HandleConvertToVariant(BundleDataInfo bundle)
        {
            bundle.HandleDelete(true, bundle.m_Name.BundleName, k_NewVariantBaseName);
            ExecuteAssetMove();
            var root = bundle.Parent.GetChild(bundle.m_Name.ShortName) as BundleVariantFolderInfo;

            if (root != null)
                return root.GetChild(k_NewVariantBaseName);
            else
            {
                //we got here because the converted bundle was empty.
                var vfolder = new BundleVariantFolderInfo(bundle.m_Name.BundleName, bundle.Parent);
                var vdata = new BundleVariantDataInfo(bundle.m_Name.BundleName + "." + k_NewVariantBaseName, vfolder);
                bundle.Parent.AddChild(vfolder);
                vfolder.AddChild(vdata);
                return vdata;
            }
        }

        internal class ABMoveData
        {
            internal string assetName;
            internal string bundleName;
            internal string variantName;
            internal ABMoveData(string asset, string bundle, string variant)
            {
                assetName = asset;
                bundleName = bundle;
                variantName = variant;
            }
            internal void Apply()
            {
                AssetImporter.GetAtPath(assetName).SetAssetBundleNameAndVariant(bundleName, variantName);
            }
        }

        internal static void MoveAssetToBundle(AssetInfo asset, string bundleName, string variant)
        {
            s_MoveData.Add(new ABMoveData(asset.FullAssetName, bundleName, variant));
        }

        internal static void MoveAssetToBundle(string assetName, string bundleName, string variant)
        {
            s_MoveData.Add(new ABMoveData(assetName, bundleName, variant));
        }

        internal static void MoveAssetToBundle(IEnumerable<AssetInfo> assets, string bundleName, string variant)
        {
            foreach (var asset in assets)
                MoveAssetToBundle(asset, bundleName, variant);
        }

        internal static void MoveAssetToBundle(IEnumerable<string> assetNames, string bundleName, string variant)
        {
            foreach (var assetName in assetNames)
                MoveAssetToBundle(assetName, bundleName, variant);
        }

        internal static void ExecuteAssetMove(bool forceAct=true)
        {
            var size = s_MoveData.Count;
            if(forceAct)
            {
                if (size > 0)
                {
                    bool autoRefresh = EditorPrefs.GetBool("kAutoRefresh");
                    EditorPrefs.SetBool("kAutoRefresh", false);
                    AssetDatabase.StartAssetEditing();
                    EditorUtility.DisplayProgressBar("Moving assets to bundles", "", 0);
                    for (int i = 0; i < size; i++)
                    {
                        EditorUtility.DisplayProgressBar("Moving assets to bundle " + s_MoveData[i].bundleName, System.IO.Path.GetFileNameWithoutExtension(s_MoveData[i].assetName), (float)i / (float)size);
                        s_MoveData[i].Apply();
                    }
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                    EditorPrefs.SetBool("kAutoRefresh", autoRefresh);
                    s_MoveData.Clear();
                }
                AssetDatabase.RemoveUnusedAssetBundleNames();
                Refresh();
            }
        }
        
        // This version of CreateAsset is only used for dependent assets.
        internal static AssetInfo CreateAsset(string name, AssetInfo parent)
        {
            if (ValidateAsset(name))
            {
                var bundleName = GetBundleName(name); 
                return CreateAsset(name, bundleName, parent);
            }
            return null;
        }

        internal static AssetInfo CreateAsset(string name, string bundleName)
        {
            if(ValidateAsset(name))
            {
                return CreateAsset(name, bundleName, null);
            }
            return null;
        }

        private static AssetInfo CreateAsset(string name, string bundleName, AssetInfo parent)
        {
            if(!String.IsNullOrEmpty(bundleName))
            {
                return new AssetInfo(name, bundleName);
            }
            else
            {
                AssetInfo info = null;
                if(!s_GlobalAssetList.TryGetValue(name, out info))
                {
                    info = new AssetInfo(name, string.Empty);
                    s_GlobalAssetList.Add(name, info);
                }
                info.AddParent(parent.DisplayName);
                return info;
            }

        }

        internal static bool ValidateAsset(string name)
        {
            if (!name.StartsWith("Assets/"))
                return false;
            string ext = System.IO.Path.GetExtension(name);
            if (ext == ".dll" || ext == ".cs" || ext == ".meta" || ext == ".js" || ext == ".boo")
                return false;

            return true;
        }

        internal static string GetBundleName(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return string.Empty;
            }
            var bundleName = importer.assetBundleName;
            if (importer.assetBundleVariant.Length > 0)
            {
                bundleName = bundleName + "." + importer.assetBundleVariant;
            }
            return bundleName;
        }

        internal static int RegisterAsset(AssetInfo asset, string bundle)
        {
            if(s_DependencyTracker.ContainsKey(asset.FullAssetName))
            {
                s_DependencyTracker[asset.FullAssetName].Add(bundle);
                int count = s_DependencyTracker[asset.FullAssetName].Count;
                if (count > 1)
                    asset.SetMessageFlag(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles, true);
                return count;
            }

            var bundles = new HashSet<string>();
            bundles.Add(bundle);
            s_DependencyTracker.Add(asset.FullAssetName, bundles);
            return 1;            
        }

        internal static void UnRegisterAsset(AssetInfo asset, string bundle)
        {
            if (s_DependencyTracker == null || asset == null)
                return;

            if (s_DependencyTracker.ContainsKey(asset.FullAssetName))
            {
                s_DependencyTracker[asset.FullAssetName].Remove(bundle);
                int count = s_DependencyTracker[asset.FullAssetName].Count;
                switch (count)
                {
                    case 0:
                        s_DependencyTracker.Remove(asset.FullAssetName);
                        break;
                    case 1:
                        asset.SetMessageFlag(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles, false);
                        break;
                    default:
                        break;
                }
            }
        }

        internal static IEnumerable<string> CheckDependencyTracker(AssetInfo asset)
        {
            if (s_DependencyTracker.ContainsKey(asset.FullAssetName))
            {
                return s_DependencyTracker[asset.FullAssetName];
            }
            return new HashSet<string>();
        }

        static internal void LogError(string message)
        {
            Debug.LogError("AssetBundleBrowser: " + message);
        }
        static internal void LogWarning(string message)
        {
            Debug.LogWarning("AssetBundleBrowser: " + message);
        }

        static internal Texture2D GetFolderIcon()
        {
            if (s_folderIcon == null)
                FindBundleIcons();
            return s_folderIcon;
        }
        static internal Texture2D GetBundleIcon()
        {
            if (s_bundleIcon == null)
                FindBundleIcons();
            return s_bundleIcon;
        }
        static internal Texture2D GetSceneIcon()
        {
            if (s_sceneIcon == null)
                FindBundleIcons();
            return s_sceneIcon;
        }
        static private void FindBundleIcons()
        {
            s_folderIcon = EditorGUIUtility.FindTexture("Folder Icon");

            var packagePath = System.IO.Path.GetFullPath("Packages/com.unity.assetbundlebrowser");
            if (System.IO.Directory.Exists(packagePath))
            {
                s_bundleIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.unity.assetbundlebrowser/Editor/Icons/ABundleBrowserIconY1756Basic.png", typeof(Texture2D));
                s_sceneIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.unity.assetbundlebrowser/Editor/Icons/ABundleBrowserIconY1756Scene.png", typeof(Texture2D));
            }
        }
    }
}
