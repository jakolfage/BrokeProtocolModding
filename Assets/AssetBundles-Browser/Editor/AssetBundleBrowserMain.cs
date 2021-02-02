using UnityEditor;
using UnityEngine;

namespace AssetBundleBrowser
{
    public class AssetBundleBrowserMain : EditorWindow
    {

        private static AssetBundleBrowserMain s_instance = null;

        internal static AssetBundleBrowserMain Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = GetWindow<AssetBundleBrowserMain>();
                return s_instance;
            }
        }

        enum Mode
        {
            Browser,
            Builder,
        }

        [SerializeField]
        Mode m_Mode;

        [SerializeField]
        internal AssetBundleManageTab m_ManageTab;

        [SerializeField]
        internal AssetBundleBuildTab m_BuildTab;

        private Texture2D m_RefreshTexture;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 32;

        [MenuItem("Window/Broke Protocol Asset Export", priority = 2050)]
        static void ShowWindow()
        {
            s_instance = null;
            Instance.titleContent = new GUIContent("Broke Protocol Asset Export");
            Instance.Show();
        }

        private void OnEnable()
        {
            Rect subPos = GetSubWindowArea();
            if(m_ManageTab == null)
                m_ManageTab = new AssetBundleManageTab();
            m_ManageTab.OnEnable(subPos, this);
            if(m_BuildTab == null)
                m_BuildTab = new AssetBundleBuildTab();
            m_BuildTab.OnEnable(this);

            m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");
        } 

        private void OnDisable()
        {
            if (m_BuildTab != null)
                m_BuildTab.OnDisable();
        }

        private Rect GetSubWindowArea()
        {
            float padding = k_MenubarPadding;

            Rect subPos = new Rect(0, padding, position.width, position.height - padding);
            return subPos;
        }

        private void Update()
        {
            switch (m_Mode)
            {
                case Mode.Builder:
                    break;
                case Mode.Browser:
                    m_ManageTab.Update();
                    break;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(k_ToolbarPadding);
            bool clicked = false;
            switch (m_Mode)
            {
                case Mode.Browser:
                    clicked = GUILayout.Button(m_RefreshTexture);
                    if (clicked)
                        m_ManageTab.ForceReloadData();
                    break;
                case Mode.Builder:
                    GUILayout.Space(m_RefreshTexture.width + k_ToolbarPadding);
                    break;
            }

            float toolbarWidth = position.width - k_ToolbarPadding * 4 - m_RefreshTexture.width;

            string[] labels = new string[] { "Configure", "Build" };
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, labels, "LargeButton", GUILayout.Width(toolbarWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            switch (m_Mode)
            {
                case Mode.Builder:
                    m_BuildTab.OnGUI();
                    break;
                case Mode.Browser:
                    m_ManageTab.OnGUI(GetSubWindowArea());
                    break;
            }
        }
    }
}
