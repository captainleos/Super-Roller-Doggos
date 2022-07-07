#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Linq;

namespace TerraUnity.UI
{
    public class TProgressBar
    {
        public enum ProgressOptionsList
        {
            Indefinite,
            Managed,
            None,
            Sticky,
            Synchronous,
            Unmanaged
        }

#if UNITY_2020_1_OR_NEWER
        private static Progress.Options ProgressOptions (ProgressOptionsList options)
        {
            if      (options.Equals(ProgressOptionsList.Indefinite))  return Progress.Options.Indefinite;
            else if (options.Equals(ProgressOptionsList.Managed))     return Progress.Options.Managed;
            else if (options.Equals(ProgressOptionsList.None))        return Progress.Options.None;
            else if (options.Equals(ProgressOptionsList.Sticky))      return Progress.Options.Sticky;
            else if (options.Equals(ProgressOptionsList.Synchronous)) return Progress.Options.Synchronous;
            else if (options.Equals(ProgressOptionsList.Unmanaged))   return Progress.Options.Unmanaged;
            else return Progress.Options.Managed;
        }
#endif

        public static int StartProgressBar(string title, string info, ProgressOptionsList options, bool maximizeWindow)
        {
#if UNITY_2020_1_OR_NEWER
            int ID = Progress.Start(title, info, ProgressOptions(options));
            if (maximizeWindow) Progress.ShowDetails();
            return ID;
#else
            return -1;
#endif
        }

        public static int StartCancelableProgressBar(string title, string info, ProgressOptionsList options, bool maximizeWindow, Func<bool> action)
        {
#if UNITY_2020_1_OR_NEWER
            int ID = Progress.Start(title, info, ProgressOptions(options));
            Progress.RegisterCancelCallback(ID, action);
            if (maximizeWindow) Progress.ShowDetails();

            return ID;
#else
            return -1;
#endif
        }

        public static void DisplayProgressBar(string title, string info, float progress, int ID)
        {
#if UNITY_2020_1_OR_NEWER
            if (Progress.Exists(ID)) Progress.Report(ID, progress, info);
#else
            EditorUtility.DisplayProgressBar(title, info, progress);
#endif
        }

        public static void DisplayCancelableProgressBar(string title, string info, float progress, int ID, Func<bool> action)
        {
#if UNITY_2020_1_OR_NEWER
            if (Progress.Exists(ID)) Progress.Report(ID, progress, info);
#else
            if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
                action.Invoke();
#endif
        }

        public static void RemoveProgressBar(int ID)
        {
#if UNITY_2020_1_OR_NEWER
            if (Progress.Exists(ID)) Progress.Remove(ID);
#else
            EditorUtility.ClearProgressBar();
#endif
        }

        public static void RemoveAllProgressBarsWithTitle(string title)
        {
#if UNITY_2020_1_OR_NEWER
            Progress.Item[] progressItems = Progress.EnumerateItems().ToArray();

            for (int i = 0; i < progressItems.Length; i++)
                if (progressItems[i].name.Equals(title))
                    progressItems[i].Remove();
#endif
        }

        public static void CancelProgressBar(int ID)
        {
#if UNITY_2020_1_OR_NEWER
            if (Progress.Exists(ID) && Progress.IsCancellable(ID)) Progress.Cancel(ID);
#endif
        }
    }
}
#endif

