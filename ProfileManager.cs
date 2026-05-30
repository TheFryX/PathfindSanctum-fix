using ExileCore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PathfindSanctum {
    internal class ProfileManager {
        private PathfindSanctumSettings Settings { get; set; }
        
        internal string[] ProfileNames { 
            get { return [.. Settings.Profiles.Keys]; } 
        }

        internal List<string> ProtectedProfiles { get; private set; } = new List<string> { Constants.Profiles.Default, Constants.Profiles.NoHit};

        internal ProfileManager(PathfindSanctumSettings settings) {
            Settings = settings;
        }

        internal void LoadSelectedProfile() {
            Settings.Profiles[ProfileNames[Settings.selectedProfileIndex]].LoadProfile(Settings);
        }

        internal void SaveProfileAs(string name) {
            var profiles = Settings.Profiles;
            if (ProtectedProfiles.Contains(name) || string.IsNullOrWhiteSpace(name)) {
                Logger.Log.Warning($"Saving profile failed! '{name}' is not a valid profile name.");
                return;
            }
            if (Settings.Profiles.ContainsKey(name)) {
                profiles[name] = ProfileContent.CreateNewProfileFromSettings(Settings);
            } else {
                profiles.Add(name, ProfileContent.CreateNewProfileFromSettings(Settings));
                Settings.selectedProfileIndex = profiles.Count - 1;
            }
            LoadSelectedProfile();
        }

        internal void ResetCurrentProfileToDefault() {
            var currentProfileName = Settings.CurrentProfile;
            Settings.Profiles[currentProfileName] = currentProfileName switch {
                Constants.Profiles.Default => ProfileContent.CreateDefaultProfile(),
                Constants.Profiles.NoHit => ProfileContent.CreateNoHitProfile(),
                _ => ProfileContent.CreateDefaultProfile()
            };
            LoadSelectedProfile();
        }

        internal void DeleteCurrentProfile() {
            var currentProfile = Settings.CurrentProfile;
            if (ProtectedProfiles.Contains(currentProfile)) {
                Logger.Log.Warning($"Deleting profile failed! '{currentProfile}' is protected and can not be deleted.");
                return;
            }
            Settings.Profiles.Remove(currentProfile);
            if (Settings.selectedProfileIndex >= ProfileNames.Length) {
                Settings.selectedProfileIndex = ProfileNames.Length - 1;
            }
            LoadSelectedProfile();
        }
    }
}
