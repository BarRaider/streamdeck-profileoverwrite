using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ProfileOverwrite
{
     internal class ProfilesExplorer
    {
        private const string MANINFEST_FILE = "manifest.json";
        private const string WIN_PROFILE_LOCATION = @"Elgato\StreamDeck\ProfilesV2";
        private const string MAC_PROFILE_LOCATION = @"~/Library/Application Support/com.elgato.StreamDeck/ProfilesV2";
        private const string FOLDER_UUID = "com.elgato.streamdeck.profile.openchild";
        private const string FOLDER_PLUGIN_SUFFIX = ".sdProfile";
        private const string FOLDER_IMAGES = "CustomImages";

        public List<ProfileInfo> GetProfiles()
        {
            string directory = GetProfileDir();

            List<ProfileInfo> profiles = new List<ProfileInfo>();
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Directory not found: " + directory);
                throw new DirectoryNotFoundException("Directory not found: " + directory);
            }

            foreach (var folder in Directory.EnumerateDirectories(directory))
            {
                try
                {
                    ProfileInfo profile = FindProfile(folder);
                    if (profile != null)
                    {
                        profile.FullPath = folder;
                        profiles.Add(profile);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Profile Error: {ex}\r\nFolder: {folder}");
                }
            }

            return profiles;
        }

        private ProfileInfo FindProfile(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            string manifestFile = Path.Combine(directory, MANINFEST_FILE);
            if (!File.Exists(manifestFile))
            {
                return null;
            }

            JObject manifest = JObject.Parse(File.ReadAllText(manifestFile));
            return manifest.ToObject<ProfileInfo>();
        }

        public List<string> FindProfileFolderActions(ProfileInfo profileInfo)
        {
            List<string> folders = new List<string>();
            foreach (string key in profileInfo.Actions.Keys)
            {
                if (profileInfo.Actions[key].UUID == FOLDER_UUID)
                {
                    folders.Add(key);
                }
            }

            return folders;
        }

        public void MoveFolderBackLocation(ProfileInfo profileInfo, string folderLocation, string newLocation)
        {
            string folderGuid = GetFolderGuid(profileInfo, folderLocation);
            if (String.IsNullOrWhiteSpace(folderGuid))
            {
                Console.WriteLine("Invalid profile folder found. Cannot continue");
                return;
            }

            string manifestFile = Path.Combine(profileInfo.FullPath, "Profiles", folderGuid , MANINFEST_FILE);
            if (!File.Exists(manifestFile))
            {
                throw new FileNotFoundException(manifestFile);
            }

            JObject manifest = JObject.Parse(File.ReadAllText(manifestFile));
            if (manifest["Actions"][newLocation] == null) // New location doesn't have an existing key
            {
                manifest["Actions"][newLocation] = manifest["Actions"]["0,0"];
                manifest["Actions"]["0,0"].Parent.Remove();
            }
            else // Does have an existing key, replace them
            {
                var currPos = manifest["Actions"][newLocation];
                manifest["Actions"][newLocation] = manifest["Actions"]["0,0"];
                manifest["Actions"]["0,0"] = currPos;
            }

            File.WriteAllText(manifestFile, manifest.ToString());
            Console.WriteLine("Done! Please completely shut down the Stream Deck app and restart it to see the change");
        }

        public void OverwriteColumn(ProfileInfo originalProfile, ProfileInfo overwriteProfile, int column)
        {
            int maxRows = SDUtil.GetRowsForStreamDeckType(SDUtil.GetStreamDeckTypeFromProfile(originalProfile));
            Dictionary<string, List<string>> dicImageCopy = new Dictionary<string, List<string>>();
            for (int idx = 0; idx < maxRows; idx++)
            {
                string strKey = $"{column},{idx}";
                if (originalProfile.Actions.ContainsKey(strKey))
                {
                    overwriteProfile.Actions[strKey] = originalProfile.Actions[strKey];
                    
                    foreach (var state in originalProfile.Actions[strKey].States)
                    {
                        if (!(String.IsNullOrWhiteSpace(state.Image)))
                        {
                            if (!dicImageCopy.ContainsKey(strKey) || dicImageCopy[strKey] == null)
                            {
                                dicImageCopy[strKey] = new List<string>();
                            }
                            dicImageCopy[strKey].Add(state.Image);
                        }
                    }
                }
            }

            File.WriteAllText(Path.Combine(overwriteProfile.FullPath, MANINFEST_FILE), JObject.FromObject(overwriteProfile).ToString());
            CopyImageFiles(originalProfile, overwriteProfile, dicImageCopy);
        }

        private void CopyImageFiles(ProfileInfo originalProfile, ProfileInfo overwriteProfile, Dictionary<string, List<string>> dicImageCopy)
        {
            string originPath = originalProfile.FullPath;
            string destinationPath = overwriteProfile.FullPath;
            foreach (var key in dicImageCopy.Keys)
            {
                foreach (var item in dicImageCopy[key])
                {
                    string destinationFolder = Path.Combine(destinationPath, key, FOLDER_IMAGES);
                    if (!Directory.Exists(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }
                    File.Copy(Path.Combine(originPath, key, FOLDER_IMAGES, item), Path.Combine(destinationFolder, item), true);
                }
            }
        }

        private string GetFolderGuid(ProfileInfo profileInfo, string folderLocation)
        {
            try
            {
                return profileInfo.Actions[folderLocation].Settings["ProfileUUID"].Value<string>() + FOLDER_PLUGIN_SUFFIX;
            }
            catch { return null; }
        }

        private string GetProfileDir()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), WIN_PROFILE_LOCATION);
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return MAC_PROFILE_LOCATION; // Not tested
            }
            else
            {
                return null;
            }
        }
    }
}
