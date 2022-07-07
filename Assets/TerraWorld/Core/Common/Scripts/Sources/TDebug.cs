#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TerraUnity.Edittime
{
    public static class TDebug
    {
        public static bool DebugMode = false;
        public static string FileName = TAddresses.corePath + "terraworldDebuglogfile.log";
        public static string currentProcessLog;
        public static List<Exception> exceptions = new List<Exception>();
        private static bool ErrorLog { get => TProjectSettings.ErrorLog; }

        //public static bool ErrorLog = TProjectSettings.ErrorLog;

        public static void Reset()
        {
            exceptions = new List<Exception>();
        }

        public static void Initialize()
        {
            try
            {
                // Check if file exists with its full path    
                if (File.Exists(FileName))
                {
                    // If file found, delete it    
                    File.Delete(FileName);
                }
                exceptions.Clear();
            }
            catch (Exception)
            {
            }
        }

        public static void LogErrorToUnityUI(Exception exception)
        {
            bool newError = true;
#if !TERRAWORLD_DEBUG
            for (int i = 0; i < exceptions.Count; i++)
            {
                if (exceptions[i].Message == exception.Message)
                {
                    newError = false;
                    break;
                }
            }
#endif

            if (newError)
            {
                exceptions.Add(exception);
                string additionalInfo = "";

                foreach (DictionaryEntry de in exception.Data)
                    if (de.Key.ToString() == "TW")
                        additionalInfo = " \n\n " + de.Value.ToString();

                Log2File(exception.Message + "-" + exception.StackTrace);

                if (newError)
                {
                    EditorUtility.DisplayDialog("TERRAWORLD", "ERROR : " + exception.Message + additionalInfo, "Ok");

                    if (!ErrorLog)
                        UnityEngine.Debug.Log(exception);
                    else
                        UnityEngine.Debug.LogException(exception);
                }
            }

            TTerrainGenerator.SetStatusToError();
           // EditorUtility.DisplayDialog("TerraWorld", "Error : " + exception.Message, "OK");
        }

        public static void LogWarningToUnityUI(string message)
        {
            Log2File("Warning : " + message);
            //UnityEngine.Debug.LogWarning(message);
        }

        public static void LogInfoToUnityUI(string message)
        {
            Log2File(message);
            UnityEngine.Debug.Log(message);
        }

        public static void TraceMessage
        (
            string message = "",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            //System.Diagnostics.Trace.WriteLine("message: " + message);
            //System.Diagnostics.Trace.WriteLine("member name: " + memberName);
            //System.Diagnostics.Trace.WriteLine("source file path: " + sourceFilePath);
            //System.Diagnostics.Trace.WriteLine("source line number: " + sourceLineNumber);
            //UnityEngine.Debug.Log("message: " + message + "member name: " + memberName + "source file path: " + sourceFilePath + "source line number: " + sourceLineNumber);
            //Debug.Log(DateTime.Now.ToString("HH:mm:ss") + " - " + memberName + " - " + message);

            if (!string.IsNullOrEmpty(message))
            {
                currentProcessLog = message;
                if (DebugMode) UnityEngine.Debug.Log("TerraWorld (" + DateTime.Now.ToString("HH:mm:ss") + ") - " + memberName + " - " + message);
            }

            Log2File(message + " - " + memberName + "-" + Path.GetFileName(sourceFilePath) + "-" + sourceLineNumber);
        }

        private static void Log2File(string logMessage)
        {
            try
            {
                using (StreamWriter w = File.AppendText(FileName))
                {
                    w.Write("\r\n");
                    w.Write($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
                    w.Write(" - " + logMessage);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void DisplayMessage(string message, string title)
        {
            EditorUtility.DisplayDialog(title, message, "OK");
        }

        public static void OpenWebPage(string message, string title, string webaddress)
        {
            if (EditorUtility.DisplayDialog(title, message, "OK", "Cancel"))
            {
                try
                {
                    Process.Start(webaddress);
                }
#if TERRAWORLD_DEBUG
                catch (Exception e)
                {
                    throw e;
                }
#else
                catch { }
#endif
            }
        }
    }
}
#endif
#endif

