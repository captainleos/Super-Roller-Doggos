#if UNITY_EDITOR
#if TERRAWORLD_PRO
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public enum ServerSyncStatus
    {
        FirstTime,
        Initializing,
        Initialized,
        Downloading,
        Idle,
        Error
    }

    public class ServerSync
    {
        private static float packageDownloadProgress = 0;
        private static float templatesSyncProgress = 0f;
        //private static bool artworkDownloadInProgress = false;
        //private static bool packageDownloadInProgress = false;
        private static ServerSyncStatus status = ServerSyncStatus.FirstTime;

        private static WebClient webClient;
        private static Exception errorException;
        private static Action<Exception, string> SyncTemplatesFromServerCompleted;
        private static Action<Exception, string> DownloadPackageCompleted;
        private static int templatesSynced = 0;
        private static string pathURL;
        private static List<string> retrieveURLList;
        private static List<string> retrieveURLListDesc;
        private static int[] fileHash;
        private static int[] fileHashDesc;
        private static string packageURL;
        private static string localPath;
        private static string assetPath;

        public static float PackageDownloadProgress { get => packageDownloadProgress; }
        public static float TemplatesSyncProgress { get => templatesSyncProgress; }
        public static ServerSyncStatus Status { get => status; }

        public static async Task ASyncTemplatesFromServer(string URL, Action<Exception, string> onCompleted)
        {
            await Task.Run(() => SyncTemplatesFromServer(URL, onCompleted));
        }

        public static void SyncTemplatesFromServer(string URL, Action<Exception, string> onCompleted)
        {
            SyncTemplatesFromServerCompleted = onCompleted;
            errorException = null;
            status = ServerSyncStatus.Initializing;
            pathURL = URL;
            templatesSyncProgress = 0.01f;

            // Get template categories
            List<string> templateCategories = RetrieveAllLinks(pathURL);
            if (templateCategories == null) { onCompleted(errorException, ""); return; }

            templatesSyncProgress = 0.25f;

            // Get template directories
            List<string> templateDirectories = new List<string>();
            for (int i = 0; i < templateCategories.Count; i++)
            {
                List<string> templateDirectory = RetrieveAllLinks(templateCategories[i]);
                if (templateDirectory == null) { onCompleted(errorException, ""); return; }
                templateDirectories.AddRange(templateDirectory);
            }

            templatesSyncProgress = 0.5f;

            // Check if template directories are valid and contain defined file formats in them
            string[] filters = new string[3] { ".png", ".txt", ".link" };
            List<string> validLinks = new List<string>();
            for (int i = 0; i < templateDirectories.Count; i++)
            {
                string validLink = CheckValidLink(templateDirectories[i], filters);
                if (validLink == null) { onCompleted(errorException, ""); return; }
                if (!string.IsNullOrEmpty(validLink)) validLinks.Add(validLink);
            }

            // Sync project folders with server directories
            List<string> validPaths = new List<string>();
            for (int i = 0; i < validLinks.Count; i++)
            {
                string projectPath = Path.GetFullPath(TAddresses.templatesPath_Pro + validLinks[i].Replace(pathURL, ""));
                validPaths.Add(projectPath);
            }

            string[] topDirs = Directory.GetDirectories(TAddresses.templatesPath_Pro, "*.*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < topDirs.Length; i++)
            {
                bool CategoryDirectoryFoundOnServer = false;
                for (int j = 0; j < validPaths.Count; j++)
                {
                    string existingDirName = Path.GetFileName(topDirs[i]);
                    string validDirName = Path.GetFileName(Path.GetDirectoryName(validPaths[j]));

                    if (existingDirName == validDirName) CategoryDirectoryFoundOnServer = true;

                }

                if (!CategoryDirectoryFoundOnServer)
                    Directory.Delete(topDirs[i], true);
                else
                {
                    string[] templateDirs = Directory.GetDirectories(topDirs[i], "*.*", SearchOption.TopDirectoryOnly);
                    for (int k = 0; k < templateDirs.Length; k++)
                    {
                        bool TemplateDirectoryFoundOnServer = false;
                        for (int j = 0; j < validPaths.Count; j++)
                        {
                            string existingDirName = Path.GetFileName(templateDirs[k]);
                            string validDirName = Path.GetFileName(validPaths[j]);

                            if (existingDirName == validDirName) TemplateDirectoryFoundOnServer = true;
                        }
                        if (!TemplateDirectoryFoundOnServer) Directory.Delete(templateDirs[k], true);
                    }
                }
            }

            for (int i = 0; i < validPaths.Count; i++)
                if (!Directory.Exists(validPaths[i])) Directory.CreateDirectory(validPaths[i]);

            status = ServerSyncStatus.Initialized;

            // templatesSyncProgress = 1f;

            // Retrieve template artworks from server and import in project for UI rendering
            DownloadTemplatesArtwork(validLinks);

            // Retrieve template description text from server and import in project for UI rendering
            DownloadTemplatesDescription(validLinks);
        }

        private static string GetDirectoryListingRegexForUrl()
        {
            return @"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))";
        }

        private static List<string> RetrieveAllLinks(string url)
        {
            try
            {
                url = url.TrimEnd('/');
                List<string> retrieveURLs = new List<string>();
                Uri uriResult;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) return null;

                Random random = new Random(DateTime.Now.Millisecond);
                int randNum = random.Next();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriResult + "?" + randNum);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(GetDirectoryListingRegexForUrl());
                        MatchCollection matches = regex.Matches(html);

                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    string dir = match.Value;
                                    string templateDir = dir.Replace("href=\"", "").TrimEnd('"').TrimEnd('/');
                                    if (templateDir.StartsWith("?C=")) continue;
                                    if (templateDir.StartsWith("/")) continue; // Parent Directory
                                    if (string.IsNullOrEmpty(templateDir)) continue;

                                    string templateURI = uriResult + "/" + templateDir;
                                    retrieveURLs.Add(templateURI);
                                }
                            }
                        }
                    }
                }

                request.Abort();
                return retrieveURLs;
            }
            catch (Exception e)
            {
                status = ServerSyncStatus.Error;
                errorException = e;
                return null;
            }
        }

        private static string CheckValidLink(string url, string[] filters)
        {
            try
            {
                url = url.TrimEnd('/');
                string retrieveURL = "";
                List<string> fileNames = new List<string>();
                Uri uriResult;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) return null;

                Random random = new Random(DateTime.Now.Millisecond);
                int randNum = random.Next();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriResult + "?" + randNum);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(GetDirectoryListingRegexForUrl());
                        MatchCollection matches = regex.Matches(html);

                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    string dir = match.Value;
                                    string fileName = dir.Replace("href=\"", "").TrimEnd('"').TrimEnd('/');
                                    if (fileName.StartsWith("?C=")) continue;
                                    if (fileName.StartsWith("/")) continue; // Parent Directory
                                    if (string.IsNullOrEmpty(fileName)) continue;

                                    for (int i = 0; i < filters.Length; i++)
                                        if (Path.GetExtension(fileName).Equals(filters[i]))
                                            fileNames.Add(fileName);
                                }
                            }
                        }
                    }
                }

                request.Abort();

                if (fileNames.Count == filters.Length)
                    retrieveURL = uriResult.ToString();

                return retrieveURL;
            }
            catch (Exception e)
            {
                status = ServerSyncStatus.Error;
                errorException = e;
                return null;
            }
        }

        private static string GetRetrieveLink(string url, string retrieveFormat)
        {
            try
            {
                if (string.IsNullOrEmpty(retrieveFormat)) return null;
                url = url.TrimEnd('/');
                string retrieveURL = "";
                Uri uriResult;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) return null;

                Random random = new Random(DateTime.Now.Millisecond);
                int randNum = random.Next();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriResult + "?" + randNum);
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(GetDirectoryListingRegexForUrl());
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    string dir = match.Value;
                                    string filePath = dir.Replace("href=\"", "").TrimEnd('"').TrimEnd('/');
                                    if (filePath.StartsWith("?C=")) continue;
                                    if (filePath.StartsWith("/")) continue; // Parent Directory
                                    if (string.IsNullOrEmpty(filePath)) continue;

                                    if (filePath.EndsWith(retrieveFormat))
                                    {
                                        retrieveURL = url + "/" + filePath;
                                        return retrieveURL;
                                    }
                                }
                            }
                        }
                    }
                }

                request.Abort();
                return retrieveURL;
            }
            catch (Exception e)
            {
                status = ServerSyncStatus.Error;
                errorException = e;
                return null;
            }
        }

        private static string ReadTextFromURL(string url)
        {
            try
            {
                string retrieveURL = "";
                Uri uriResult;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) return null;

                using (WebClient wc = new WebClient())
                {
                    retrieveURL = wc.DownloadString(uriResult);
                }

                return retrieveURL;
            }
            catch (Exception e)
            {
                status = ServerSyncStatus.Error;
                errorException = e;
                return null;
            }
        }

        private static string GetAssetPath(string url, string retrieveFormat, string path)
        {
            try
            {
                string retrieveURL = "";
                string localDir = Path.GetDirectoryName(url.Replace(pathURL, ""));
                string[] assetNames = new string[0];
                string fullPath = Path.GetFullPath(path + localDir);
                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
                assetNames = Directory.GetFiles(fullPath, "*" + retrieveFormat, SearchOption.AllDirectories);

                //if (isInCache)
                //{
                //    string currentDirCache = Path.GetFullPath(TAddresses.templatesPath_Pro_Cache + templateDirLocal);
                //    if (!Directory.Exists(currentDirCache)) Directory.CreateDirectory(currentDirCache);
                //    assetNames = Directory.GetFiles(currentDirCache, "*" + retrieveFormat, SearchOption.AllDirectories);
                //}
                //else
                //{
                //    string currentDir = Path.GetFullPath(TAddresses.templatesPath_Pro + Path.DirectorySeparatorChar + templateDirLocal);
                //    if (!Directory.Exists(currentDir)) Directory.CreateDirectory(currentDir);
                //    assetNames = Directory.GetFiles(currentDir, "*" + retrieveFormat, SearchOption.AllDirectories);
                //}

                if (url.EndsWith(retrieveFormat))
                {
                    if (assetNames.Length == 0)
                        retrieveURL = url;
                    else
                    {
                        string fileName = Path.GetFileName(assetNames[0]);

                        if (!fileName.Equals(Path.GetFileName(url)))
                        {
                            File.Delete(Path.GetFullPath(assetNames[0]));
                            retrieveURL = url;
                        }
                    }
                }

                return retrieveURL;
            }
            catch (Exception e)
            {
                errorException = e;
                return null;
            }
        }

        private static void DownloadTemplatesArtwork(List<string> validLinks)
        {
            //artworkDownloadInProgress = true;
            templatesSynced = 0;
            retrieveURLList = new List<string>();

            for (int i = 0; i < validLinks.Count; i++)
            {
                string artworkURL = GetRetrieveLink(validLinks[i], ".png");
                if (artworkURL == null) { SyncTemplatesFromServerCompleted(errorException, ""); return; }
                artworkURL = GetAssetPath(artworkURL, ".png", TAddresses.templatesPath_Pro);
                if (artworkURL == null) { SyncTemplatesFromServerCompleted(errorException, ""); return; }
                if (!string.IsNullOrEmpty(artworkURL)) retrieveURLList.Add(artworkURL);
            }

            templatesSyncProgress = 0.6f;

            if (retrieveURLList.Count == 0)
            {
                status = ServerSyncStatus.Idle;
                //artworkDownloadInProgress = false;
                templatesSyncProgress = 0.9f;
                SyncTemplatesFromServerCompleted(errorException, "");
            }
            else
            {
                // templatesSyncProgress = 0.01f;
                fileHash = new int[retrieveURLList.Count];

                for (int i = 0; i < retrieveURLList.Count; i++)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        Uri URL = new Uri(retrieveURLList[i]);
                        fileHash[i] = webClient.GetHashCode();
                        webClient.DownloadDataCompleted += DownloadDataCompletedArtworks;
                        webClient.DownloadDataAsync(URL);
                    }
                }
            }

            //templatesSyncProgress = 0.7f;
        }

        private static void DownloadDataCompletedArtworks(object sender, DownloadDataCompletedEventArgs e)
        {
            string retrievePath = "";
            WebClient client = (WebClient)sender;
            int hash = client.GetHashCode();

            if (e.Error != null && e.Error.Message.Contains("aborted"))
            {
                status = ServerSyncStatus.Error;
                errorException = e.Error;
            }
            else if (e.Error != null)
            {
                status = ServerSyncStatus.Error;
                errorException = e.Error;
            }
            else
            {
                try
                {
                    byte[] bytes = e.Result;

                    for (int i = 0; i < retrieveURLList.Count; i++)
                    {
                        if (hash.Equals(fileHash[i]))
                        {
                            string templateDirPath = retrieveURLList[i].Replace(pathURL, "");
                            retrievePath = TAddresses.templatesPath_Pro + templateDirPath;
                            File.WriteAllBytes(Path.GetFullPath(retrievePath), bytes);
                        }
                    }
                }
                catch (Exception e1)
                {
                    status = ServerSyncStatus.Error;
                    errorException = e1;
                }
            }

            templatesSyncProgress = 0.6f + 0.3f * ((float)templatesSynced++ / retrieveURLList.Count) / 10f;

            if (templatesSynced == retrieveURLList.Count)
            {
                status = ServerSyncStatus.Idle;
                //artworkDownloadInProgress = false;
            }

            SyncTemplatesFromServerCompleted(errorException, retrievePath);
        }

        private static void DownloadTemplatesDescription(List<string> validLinks)
        {
            retrieveURLListDesc = new List<string>();

            for (int i = 0; i < validLinks.Count; i++)
            {
                string descriptionURL = GetRetrieveLink(validLinks[i], ".txt");
                if (descriptionURL == null) { SyncTemplatesFromServerCompleted(errorException, ""); return; }
                descriptionURL = GetAssetPath(descriptionURL, ".txt", TAddresses.templatesPath_Pro);
                if (descriptionURL == null) { SyncTemplatesFromServerCompleted(errorException, ""); return; }
                if (!string.IsNullOrEmpty(descriptionURL)) retrieveURLListDesc.Add(descriptionURL);
            }

            if (retrieveURLListDesc.Count == 0)
                SyncTemplatesFromServerCompleted(errorException, "");
            else
            {
                fileHashDesc = new int[retrieveURLListDesc.Count];

                for (int i = 0; i < retrieveURLListDesc.Count; i++)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        Uri URL = new Uri(retrieveURLListDesc[i]);
                        fileHashDesc[i] = webClient.GetHashCode();
                        webClient.DownloadDataCompleted += DownloadDataCompletedDescriptions;
                        webClient.DownloadDataAsync(URL);
                    }
                }
            }
        }

        private static void DownloadDataCompletedDescriptions(object sender, DownloadDataCompletedEventArgs e)
        {
            string retrievePath = "";
            WebClient client = (WebClient)sender;
            int hash = client.GetHashCode();

            if (e.Error != null && e.Error.Message.Contains("aborted"))
            {
                status = ServerSyncStatus.Error;
                errorException = e.Error;
            }
            else if (e.Error != null)
            {
                status = ServerSyncStatus.Error;
                errorException = e.Error;
            }
            else
            {
                try
                {
                    byte[] bytes = e.Result;

                    for (int i = 0; i < retrieveURLListDesc.Count; i++)
                    {
                        if (hash.Equals(fileHashDesc[i]))
                        {
                            string templateDirPath = retrieveURLListDesc[i].Replace(pathURL, "");
                            retrievePath = TAddresses.templatesPath_Pro + templateDirPath;
                            File.WriteAllBytes(Path.GetFullPath(retrievePath), bytes);
                        }
                    }
                }
                catch (Exception e1)
                {
                    status = ServerSyncStatus.Error;
                    errorException = e1;
                }
            }

            SyncTemplatesFromServerCompleted(errorException, retrievePath);
        }

        public static void DownloadPackage(string ScenesURL, string localDirPath, string savePath, Action<Exception, string> onCompleted)
        {
            //if (string.IsNullOrEmpty(localDirPath)) return;
            if (localDirPath == null) return;

           // pathURL = baseURL;
            localPath = localDirPath;
            DownloadPackageCompleted = onCompleted;
            assetPath = savePath;

            status = ServerSyncStatus.Initializing;
            errorException = null;
            //packageDownloadInProgress = true;

            string DemoSceneFileName = "";
            List<string> linkFileURLS = new List<string>();
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    Uri URL = new Uri(ScenesURL);
                    byte[] bytes = webClient.DownloadData(URL);

                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        XmlDocument DemoScenesLinks = new XmlDocument();
                        DemoScenesLinks.Load(ms);
                        XmlNodeList elemList = DemoScenesLinks.GetElementsByTagName("FileName");
                        foreach (XmlNode attr in elemList)
                        {
                            DemoSceneFileName = attr.Attributes["Text"].InnerText;
                        }
                        if (string.IsNullOrEmpty(DemoSceneFileName)) throw new Exception("File name filed is correpted.");
                        elemList = DemoScenesLinks.GetElementsByTagName("URL");
                        foreach (XmlNode attr in elemList)
                        {
                            string DemoSceneURL = attr.Attributes["Text"].InnerText;
                            if (string.IsNullOrEmpty(DemoSceneFileName)) throw new Exception("URL filed is correpted.");
                            linkFileURLS.Add(DemoSceneURL);
                        }
                    }
                }
                if (linkFileURLS.Count == 0) throw new Exception("No Demo Scene Found!");
            }
            catch (Exception e)
            {
                errorException = e;
                DownloadPackageCompleted(errorException, localPath);
                return;
            }


            Random rand = new Random(DateTime.Now.Millisecond);
            int selectedIndex = rand.Next(linkFileURLS.Count);

            // packageURL = pathURL + localPath;
            // string linkFileURL = GetRetrieveLink(packageURL, ".link");
            // if (linkFileURL == null) { DownloadPackageCompleted(errorException, localPath); return; }

            // string packageURLDropBox = ReadTextFromURL(linkFileURL);

           // packageURL = linkFileURL.Replace(".link", ".unitypackage");
            string packageLocalFullPath = assetPath + DemoSceneFileName;
            if (File.Exists(packageLocalFullPath))
            {
                localPath = packageLocalFullPath;
                DownloadPackageCompleted(errorException, localPath);
            }
            else
            {
                status = ServerSyncStatus.Downloading;
                using (webClient = new WebClient())
                {
                    Uri URL = new Uri(linkFileURLS[selectedIndex]);
                    webClient.DownloadDataCompleted += DownloadDataCompletedUnitypackage;
                    webClient.QueryString.Add("SavePath", packageLocalFullPath);
                    webClient.DownloadProgressChanged += DownloadProgressChangedUnitypackage;
                    webClient.DownloadDataAsync(URL);
                }
            }

            //return;
            //if (packageURL == null) { DownloadPackageCompleted(errorException, localPath); return; }
            //
            //string packageLocalAddress = GetAssetPath(packageURL, ".unitypackage", assetPath);
            //if (packageLocalAddress == null) { DownloadPackageCompleted(errorException, localPath); return; }
            //
            //if (!string.IsNullOrEmpty(packageLocalAddress)) // Package is not available on PC, continue downloading it
            //{
            //    status = ServerSyncStatus.Downloading;
            //    using (webClient = new WebClient())
            //    {
            //        Uri URL = new Uri(packageURLDropBox);
            //        webClient.DownloadDataCompleted += DownloadDataCompletedUnitypackage;
            //        webClient.QueryString.Add("SavePath", packageLocalAddress);
            //        webClient.DownloadProgressChanged += DownloadProgressChangedUnitypackage;
            //        webClient.DownloadDataAsync(URL);
            //    }
            //}
            //else // Package is available on PC, continue importing it
            //{
            //    status = ServerSyncStatus.Initialized;
            //    string templateDirPath = packageURL.Replace(pathURL, "");
            //    string retrievePath = assetPath + templateDirPath;
            //    DownloadPackageCompleted(errorException, retrievePath);
            //}
        }

        private static void DownloadProgressChangedUnitypackage(object sender, DownloadProgressChangedEventArgs e)
        {
            packageDownloadProgress = (float)TUtils.Clamp(0.01d, 1d, (float)e.ProgressPercentage / 100f);
        }

        private static void DownloadDataCompletedUnitypackage(object sender, DownloadDataCompletedEventArgs e)
        {
            string retrievePath = "";

            //packageDownloadInProgress = false;
            packageDownloadProgress = 1f;

            if (e.Error != null && e.Error.Message.Contains("aborted"))
            {
                status = ServerSyncStatus.Idle;
                DownloadPackageCompleted(errorException, "");
            }
            else if (e.Error != null)
            {
                status = ServerSyncStatus.Error;
                errorException = e.Error;
                DownloadPackageCompleted(errorException, localPath);
            }
            else
            {
                try
                {
                    byte[] bytes = e.Result;
                    //string templateDirPath = packageURL.Replace(pathURL, "");
                    retrievePath = ((System.Net.WebClient)(sender)).QueryString["SavePath"];
                    File.WriteAllBytes(retrievePath, bytes);
                    DownloadPackageCompleted(errorException, retrievePath);
                }
                catch (Exception e1)
                {
                    status = ServerSyncStatus.Error;
                    errorException = e1;
                    DownloadPackageCompleted(errorException, localPath);
                }
            }

        }

        public static void CancelDownload()
        {
            errorException = null;
            if (webClient != null) webClient.CancelAsync();
        }
    }
}
#endif
#endif

