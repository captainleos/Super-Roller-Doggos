#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;

namespace TerraUnity.Edittime
{
    public class BugReporter
    {
        public static string report;
        public static string FileName = TAddresses.corePath + "feedback.log";

        public static void SendReport(string email)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("mail.terraunity.com");

                mail.From = new MailAddress("bugreporter@terraunity.com");
                mail.ReplyToList.Add(email);
                mail.To.Add("bugreporter@terraunity.com");
                mail.Subject = "Terraworld Bug Report from " + email;
                mail.Body = report;

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("bugreporter@terraunity.com", "bugreporter");
                SmtpServer.Timeout = 20000;
                SmtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                string userState = "bugreporter";
                SmtpServer.SendAsync(mail, userState);

            }
            catch (Exception ex)
            {
                TDebug.LogErrorToUnityUI(ex);
            }
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            string token = (string)e.UserState;

            if (e.Error != null)
                TDebug.LogErrorToUnityUI(e.Error);
            else
                TDebug.LogInfoToUnityUI("Thank you, log file has been sent to TerraUnity team for debugging");
        }

        private static string GetSysInfo()
        {
            string sysInfo = "";
            sysInfo += "\n\r" + "Operating System :" + SystemInfo.operatingSystem;
            sysInfo += "\n\r" + "Device Type :" + SystemInfo.deviceType;
            sysInfo += "\n\r" + "System Memory Size :" + SystemInfo.systemMemorySize;
            sysInfo += "\n\r" + "Processor Type :" + SystemInfo.processorType;
            sysInfo += "\n\r" + "Processor count :" + SystemInfo.processorCount;
            sysInfo += "\n\r" + "Graphic Device Name :" + SystemInfo.graphicsDeviceName;
            sysInfo += "\n\r" + "Graphic Memory :" + SystemInfo.graphicsMemorySize;
            sysInfo += "\n\r" + "Graphic Shader level :" + SystemInfo.graphicsShaderLevel;
            sysInfo += "\n\r" + "Max Texture Size :" + SystemInfo.maxTextureSize;
            sysInfo += "\n\r" + "Unity Version :" + Application.unityVersion;
            sysInfo += "\n\r" + "Target Platform :" + Application.platform;
            return sysInfo;
        }

        private static string GetActiveGraph()
        {
            string graphPath = TTerraWorld.GraphPath;
            string activegraph = File.ReadAllText(graphPath);
            return activegraph;
        }

        private static string GetLog()
        {
            string log = "";
            log = File.ReadAllText(TDebug.FileName);
            return log;
        }

        public static bool GenerateFeedbackLog()
        {
            try
            {
                report = "\n\r------------- System INFO --------------\n\r" + GetSysInfo() + "\n\r------------- ACTIVE GRAPH --------------\n\r" + GetActiveGraph() + "\n\r-------------LOG FILE-------------\n\r" + GetLog();
                return true;
            }

            catch { return false; }
        }
    }
}
#endif
#endif

