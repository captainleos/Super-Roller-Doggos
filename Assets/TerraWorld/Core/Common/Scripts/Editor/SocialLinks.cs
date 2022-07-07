#if TERRAWORLD_PRO
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

namespace TerraUnity.Edittime
{
    public class SocialLinkes : EditorWindow
    {
        private static Vector2 windowSize = new Vector2(464, 80);

        [MenuItem("Tools/TerraUnity/Help Center/Help File", false, 2)]
        static void ShowHelp()
        {
            TTerraWorld.FeedbackEvent(EventCategory.UX , EventAction.Click, "HelpFile");
            string filename = TAddresses.helpPath + "TerraWorld Quick Guide.pdf";

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            filename = TAddresses.helpPath;
#endif

            Process.Start(Path.GetFullPath(filename));
        }

        [MenuItem("Tools/TerraUnity/Help Center/HOWTO Tweaks", false, 2)]
        static void ShowHelpHowTo()
        {
            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "HOWTO");
            Help.BrowseURL("https://terraunity.com/how-to/");
        }

        [MenuItem("Tools/TerraUnity/Contact Us", false, 3)]
        static void Init()
        {
            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Contact");
            SocialLinkes window = (SocialLinkes)GetWindow(typeof(SocialLinkes));
            window.position = new Rect
                (
                    (Screen.currentResolution.width / 2) - (windowSize.x / 2),
                    (Screen.currentResolution.height / 2) - (windowSize.y / 2),
                    windowSize.x,
                    windowSize.y
                );

            window.minSize = new Vector2(windowSize.x, windowSize.y);
            window.maxSize = new Vector2(windowSize.x, windowSize.y);

            window.titleContent = new GUIContent("Contact Us", "TerraUnity Contact Links");
        }

        void OnGUI ()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 64;
            style.fixedHeight = 64;

            GUILayout.Space(14);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(TResourcesManager.websiteIcon, style))
            {
                Help.BrowseURL("http://terraunity.com");
            }

            if (GUILayout.Button(TResourcesManager.youtubeIcon, style))
            {
                Help.BrowseURL("https://www.youtube.com/user/TerraUnity");
            }

            if (GUILayout.Button(TResourcesManager.twitterIcon, style))
            {
                Help.BrowseURL("https://twitter.com/TerraUnity");
            }

            if (GUILayout.Button(TResourcesManager.linkedinIcon, style))
            {
                Help.BrowseURL("https://www.linkedin.com/company/TerraUnity");
            }

            if (GUILayout.Button(TResourcesManager.redditIcon, style))
            {
                Help.BrowseURL("https://www.reddit.com/r/TerraUnity");
            }

            if (GUILayout.Button(TResourcesManager.facebookIcon, style))
            {
                Help.BrowseURL("https://www.facebook.com/TerraUnity");
            }

            if (GUILayout.Button(TResourcesManager.discordIcon, style))
            {
                Help.BrowseURL("https://discord.gg/9J6Jk7B");
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
#endif
