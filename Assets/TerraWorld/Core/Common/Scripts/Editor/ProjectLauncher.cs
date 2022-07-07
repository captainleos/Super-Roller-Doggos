#if TERRAWORLD_PRO
#if UNITY_EDITOR
/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2020
    
    http://terraunity.com
    info@terraunity.com
    
    TerraWorld's Project Launcher
    
*/


using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
//using UnityEngine.Video;
using System.Diagnostics;
using System.IO;

namespace TerraUnity.Edittime
{
    [InitializeOnLoad]
    public class ProjectLauncher : EditorWindow
    {
        private void ShowOSX()
        {
            string filename = TAddresses.macInstructionsPath;
            Process.Start(Path.GetFullPath(filename));
            filename = TAddresses.macInstructionsPath + "ReadMe (IOS).txt";
            Process.Start(Path.GetFullPath(filename));
        }

        static ProjectLauncher window;
        //private static VideoPlayer videoPlayer;
        //private static GameObject videoPlayerGO;
        private static bool isPlayed = false;
        private static Vector2 windowSize = new Vector2(540, 680);
        private static Vector2 scrollPosition = Vector2.zero;
        private static string changeLogText;
        private static bool openGuidePage = true;

        enum ShowTabs
        {
            showEULA,
            showCHANGELOG,
            showMacInstructions
        }
        private static ShowTabs activeTab = ShowTabs.showEULA;

        private static string license =
            @"
End-User License Agreement (EULA) of TerraWorld Plugin


IMPORTANT – PLEASE READ THIS END USER LICENSE AGREEMENT (THE “AGREEMENT”) CAREFULLY
BEFORE ATTEMPTING TO DOWNLOAD OR USE ANY SOFTWARE, DOCUMENTATION, OR OTHER MATERIALS
MADE AVAILABLE THROUGH THIS PACKAGE (TERRAWORLD).  THIS AGREEMENT CONSTITUTES A 
LEGALLY BINDING AGREEMENT BETWEEN YOU OR THE COMPANY WHICH YOU REPRESENT AND ARE 
AUTHORIZED TO BIND (the “Licensee” or “You”), AND TERRAUNITY SOFTWARE CORPORATION 
(“Terraworld” or “Licensor”).  PLEASE CLICK ON ""ACCEPT"" BUTTON IF YOU AGREE TO 
BE BOUND BY THE TERMS AND CONDITIONS OF THIS AGREEMENT.  BY CHECKING ""ACCEPT"" 
BUTTON AND/OR BY PURCHASING, DOWNLOADING, INSTALLING OR OTHERWISE USING THE 
SOFTWARE AND/OR PACKAGE, YOU ACKNOWLEDGE (1) THAT YOU HAVE READ THIS AGREEMENT, 
(2) THAT YOU UNDERSTAND IT, (3) THAT YOU AGREE TO BE BOUND BY ITS TERMS AND 
CONDITIONS, AND (4) TO THE EXTENT YOU ARE ENTERING INTO THIS AGREEMENT ON BEHALF
OF A COMPANY, YOU HAVE THE POWER AND AUTHORITY TO BIND THAT COMPANY.  

This End-User License Agreement (""EULA"") is a legal agreement between you and
TerraUnity.

This EULA agreement governs your acquisition and use of our TerraWorld Plugin
software(""Software"") directly from TerraUnity or indirectly through a TerraUnity
authorized reseller or distributor(a ""Reseller"").
Please read this EULA agreement carefully before completing the installation
process and using the TerraWorld Plugin software.It provides a license to use
the TerraWorld Plugin software and contains warranty information and liability
disclaimers.

If you register for a free trial of the TerraWorld Plugin software, this EULA
agreement will also govern that trial.By clicking ""accept"" or installing
and/or using the TerraWorld Plugin software, you are confirming your acceptance
of the Software and agreeing to become bound by the terms of this EULA agreement.
If you are entering into this EULA agreement on behalf of a company or other
legal entity, you represent that you have the authority to bind such entity and
its affiliates to these terms and conditions.If you do not have such authority
or if you do not agree with the terms and conditions of this EULA agreement,
do not install or use the Software, and you must not accept this EULA agreement.
This EULA agreement shall apply only to the Software supplied by TerraUnity
herewith regardless of whether other software is referred to or described herein.
The terms also apply to any TerraUnity updates, supplements, Internet-based
services, and support services for the Software, unless other terms accompany
those items on delivery. If so, those terms apply.This EULA was created by EULA
Template for TerraWorld Plugin.


License Grant

TerraWorld package requires one license per seat.

TerraUnity hereby grants you a personal, non-transferable, non-exclusive licence
to use the TerraWorld Plugin software on your devices in accordance with the
terms of this EULA agreement.
You are permitted to load the TerraWorld Plugin software (for example a PC,
laptop, mobile or tablet) under your control. You are responsible for ensuring
your device meets the minimum requirements of the TerraWorld Plugin software.

Collection and Use of Data.

TERRAWORLD uses tools to deliver certain Software features and extensions, 
identify trends and bugs, collect activation information, usage statistics 
and track other data related to Your use of the Software as a refrence just
to better improvements in software in later versions. By Your acceptance of 
the terms of this Agreement and/or use of the Software, You authorize the 
collection, use and disclosure of this data for the purposes provided 
for in this Agreement.(This feature can be disabled any time later in settings section)


You are not permitted to:

• Edit, alter, modify, adapt, translate or otherwise change the whole or any
part of the Software nor permit the whole or any part of the Software to be
combined with or become incorporated in any other software, nor decompile,
disassemble or reverse engineer the Software or attempt to do any such things

• Reproduce, copy, distribute, resell or otherwise use the Software for any
commercial purpose

• Allow any third party to use the Software on behalf of or for the benefit
of any third party

• Use the Software in any way which breaches any applicable local, national
or international law

• Use the Software for any purpose that TerraUnity considers is a breach of
this EULA agreement


Intellectual Property and Ownership

TerraUnity shall at all times retain ownership of the Software as originally
downloaded by you and all subsequent downloads of the Software by you.
The Software (and the copyright, and other intellectual property rights of
whatever nature in the Software, including any modifications made thereto) are
and shall remain the property of TerraUnity.
TerraUnity reserves the right to grant licences to use the Software to third
parties.


Termination

This EULA agreement is effective from the date you first use the Software and
shall continue until terminated. You may terminate it at any time upon written
notice to TerraUnity.
It will also terminate immediately if you fail to comply with any term of this
EULA agreement. Upon such termination, the licenses granted by this EULA
agreement will immediately terminate and you agree to stop all access and use
of the Software. The provisions that by their nature continue and survive will
survive any termination of this EULA agreement.


Governing Law

This EULA agreement, and any dispute arising out of or in connection with this
EULA agreement, shall be governed by and construed in accordance with the laws
of international.
";

        static ProjectLauncher()
        {
            TResourcesManager.LoadAllResources();
            EditorApplication.update += Update;
        }

        static void Initialize()
        {
            //RemoveVideoPlayer();
            TResourcesManager.LoadProjectLauncherResources();
            changeLogText = ChangeLogText();

            //videoPlayerGO = new GameObject("TerraWorld Video");
            //videoPlayerGO.hideFlags = HideFlags.HideAndDontSave;
            //videoPlayer = videoPlayerGO.AddComponent<VideoPlayer>();
            //videoPlayer.playOnAwake = true;
            //videoPlayer.url = Path.GetFullPath(AssetDatabase.GetAssetPath(TResourcesManager.videoObject));
            //videoPlayer.isLooping = true;
            //videoPlayer.Play();

            isPlayed = true;
        }

        static void Update ()
        {
            if (!TProjectSettings.IsInstallationCompleted()) return;
            if (!TProjectSettings.IsIntroWindowShowed())
            {
                TProjectSettings.SetIntroWindowShowed();

                if (!isPlayed)
                    Initialize();

                window = GetWindow<ProjectLauncher>(true, "Welcome To TerraWorld", true);
                window.position = new Rect
                    (
                        (Screen.currentResolution.width / 2) - (windowSize.x / 2),
                        (Screen.currentResolution.height / 2) - (windowSize.y / 2),
                        windowSize.x,
                        windowSize.y
                    );
                
                window.minSize = new Vector2(windowSize.x, windowSize.y);
                window.maxSize = new Vector2(windowSize.x, windowSize.y);
            }
            else
                EditorApplication.update -= Update;
        }

        public void OnGUI()
        {
            Repaint();

            //if (videoPlayer != null) GUI.DrawTexture(new Rect(0, 0, 540, 304), videoPlayer.texture, ScaleMode.ScaleToFit);
            if (TResourcesManager.introImage != null) GUI.DrawTexture(new Rect(0, 0, 540, 304), TResourcesManager.introImage, ScaleMode.ScaleToFit);

            GUILayout.Space(304);

            TabsGUI();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (activeTab == ShowTabs.showEULA)
                GUILayout.Label(license);
            else if(activeTab == ShowTabs.showCHANGELOG)
                GUILayout.Label(changeLogText);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            //GUILayout.BeginHorizontal();
            //loadDemoScene = EditorGUILayout.Toggle(loadDemoScene);
            //GUILayout.Space(-150);
            //GUI.backgroundColor = Color.clear;
            //EditorGUILayout.HelpBox(new GUIContent("LOAD DEMO SCENE", "Load TerraWorld's demo scene after closing this window"), true);
            //GUI.backgroundColor = Color.white;
            //GUILayout.FlexibleSpace();
            //GUILayout.EndHorizontal();
            //
            //GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            openGuidePage = EditorGUILayout.Toggle(openGuidePage);
            GUILayout.Space(-150);
            GUI.backgroundColor = Color.clear;
            EditorGUILayout.HelpBox(new GUIContent("SHOW ME HOW TO USE TERRAWORLD FOR THE FIRST TIME!", "Open TerraWorld's hello world webpage"), true);
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("CHECK OUT SCENES FOLDER FOR DEMO & LEARNING CONTENT", MessageType.Warning, true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedHeight = 25;
            style.alignment = TextAnchor.MiddleCenter;

            style.fixedWidth = 70;

            if (GUILayout.Button("ACCEPT", style))
            {
                CloseWindow();
            }
//#endif

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private static void TabsGUI()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fontSize = 8;
            style.fixedWidth = 70;
            style.fixedHeight = 20;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);

            // EULA Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showEULA))
                GUI.color = Color.white;
            else
                GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.75f);

            if (GUILayout.Button("EULA", style))
            {
                activeTab = ShowTabs.showEULA;
            }

            GUI.color = Color.white;

            // CHANGELOG Tab
            //---------------------------------------------------------------------------------------------------------------------------------------------------

            if (activeTab.Equals(ShowTabs.showCHANGELOG))
                GUI.color = Color.white;
            else
                GUI.color = new Color(0.85f, 0.85f, 0.85f, 0.75f);

            if (GUILayout.Button("CHANGELOG", style))
            {
                activeTab = ShowTabs.showCHANGELOG;
            }

            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private static string ChangeLogText ()
        {
            if (File.Exists(TAddresses.helpPath + "CHANGELOG.md"))
                return File.ReadAllText(TAddresses.helpPath + "CHANGELOG.md");
            else
                return "No CHANGELOG file found!";
        }

        private void OpenHelloWorldPage()
        {
            Help.BrowseURL("https://terraunity.com/how-to-generate-your-first-world-in-terraworld/");
        }

        //private static void RemoveVideoPlayer ()
        //{
        //    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        //        if (go.hideFlags == HideFlags.HideAndDontSave && go.name == "TerraWorld Video")
        //            DestroyImmediate(go);
        //}

        //private void LoadDemoScene ()
        //{
        //    if (loadDemoScene)
        //    {
        //        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //        string demoScenePath = "Assets/TerraWorld/Scenes/Pro/Demo Scene Download.unity";
        //
        //        if (File.Exists(demoScenePath))
        //            EditorSceneManager.OpenScene(demoScenePath);
        //        else
        //            UnityEngine.Debug.Log("Demo Scene Downloader Not Found!");
        //    }
        //
        //    if (openGuidePage)
        //        OpenHelloWorldPage();
        //}

        private void CloseWindow()
        {
            //if (Application.platform == RuntimePlatform.OSXEditor)
            //{
            //    string macInstructionsPath = TAddresses.macInstructionsPath + "Mac_Instructions.txt";
            //    Process.Start(Path.GetFullPath(macInstructionsPath));
            //}

            this.Close();
        }

        private void OnDestroy ()
        {
#if UNITY_EDITOR_OSX
            if (EditorUtility.DisplayDialog("TERRAWORLD", "IMPORTANT\n\nMAC USERS SHOULD APPLY SOME CHANGES MANUALLY\n\nPlease follow the instructions described in \"ReadMe (IOS)\" file to enable TERRAWORLD.", "OK"))
            { ShowOSX(); }
#endif

            if (openGuidePage)
                OpenHelloWorldPage();

            //LoadDemoScene();
            //RemoveVideoPlayer();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

    }
}
#endif
#endif

