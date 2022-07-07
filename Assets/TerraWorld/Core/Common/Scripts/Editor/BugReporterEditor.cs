#if TERRAWORLD_PRO
/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2019
    
    http://terraunity.com
    info@terraunity.com
    
    Report encountered bugs to TerraUnity Team
    
*/

using UnityEngine;
using UnityEditor;
using System.Net.Mail;

namespace TerraUnity.Edittime
{
    public class BugReporterEditor : EditorWindow
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private static Vector2 windowSize = new Vector2(700, 500);
        private static string emailAddress = "";

        static void Init()
        {
            BugReporterEditor window = (BugReporterEditor)GetWindow(typeof(BugReporterEditor));
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

        private void OnEnable()
        {
            Init();
        }

        void OnGUI ()
        {
            GUILayout.Space(20);

            GUIStyle style = new GUIStyle(EditorStyles.toolbarTextField);
            style.fixedHeight = 20;
            style.fontSize = 11;
            style.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("Email Address", MessageType.None);
            emailAddress = EditorGUILayout.TextField(emailAddress, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 100;
            style.fixedHeight = 20;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("ACCEPT", style))
            {
                if(!string.IsNullOrEmpty(emailAddress) && IsValidEmail(emailAddress))
                {
                    BugReporter.SendReport(emailAddress);
                    CloseWindow();
                }
                else
                {
                    EditorUtility.DisplayDialog("TERRAWORLD","INVALID EMAIL ADDRESS \n Plase type a valid email address!\n\nWe ask you to send us your email address in order to contact you and notify about the status of the reported bug.", "Ok");
                    return;
                }
            }

            if (GUILayout.Button("DECLINE", style))
            {
                CloseWindow();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(BugReporter.report);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void CloseWindow ()
        {
            this.Close();
        }
    }
}
#endif

