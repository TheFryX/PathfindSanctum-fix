using ExileCore2;
using ImGuiNET;

namespace PathfindSanctum;

public class PathfindSanctumPlugin : BaseSettingsPlugin<PathfindSanctumSettings>
{
    private readonly SanctumStateTracker stateTracker = new();
    private PathFinder pathFinder;
    private WeightCalculator weightCalculator;

    private string saveAsName = string.Empty;

    // ImGUI Flags
    private bool showSaveDialog;
    private bool showDeleteDialog;
    private bool showResetDialog;
    private bool showSaveAsNewProfileDialog;
    private bool showSaveAsInvalidNameError;

    public override bool Initialise()
    {
        weightCalculator = new WeightCalculator(GameController, Settings);
        pathFinder = new PathFinder(Graphics, Settings, stateTracker, weightCalculator);
        return base.Initialise();
    }

    public override void Render()
    {
        //hotfix - previous InGame state not updating in EC2
        //if (!GameController.Game.IngameState.InGame)
        if (!GameController.InGame)
            return;

        if (
            GameController.Area.CurrentArea.Area.RawName == "G2_13"
            || GameController.Area.CurrentArea.IsHideout
        )
            return;

        if (
            stateTracker.HasRoomData()
            && !stateTracker.IsSameSanctum(GameController.Area.CurrentArea.Hash)
        )
        {
            stateTracker.Reset(GameController.Area.CurrentArea.Hash);
            return;
        }

        var floorWindow = GameController.Game.IngameState.IngameUi.SanctumFloorWindow;
        if (floorWindow == null || !floorWindow.IsVisible)
            return;

        stateTracker.UpdateRoomStates(floorWindow);
        UpdateAndRenderPath();
    }

    public override void DrawSettings() {
        // Draw profile selection
        //Settings.ProfileManager.ProfileNames = Settings.Profiles.Keys.ToArray();
        ImGui.Text("Profile Selection");

        /*
        //Dropdown list
        if (ImGui.Combo("string.Empty", ref Settings.selectedProfileIndex, Settings.profileNames, Settings.profileNames.Length)) {
            Settings.LoadSelectedProfile();
        }
        */

        // ListBox - saves a click and doesn't obstruct buttons
        if (ImGui.ListBox(string.Empty, ref Settings.selectedProfileIndex, Settings.ProfileManager.ProfileNames, Settings.ProfileManager.ProfileNames.Length)) {
            Settings.ProfileManager.LoadSelectedProfile();
        }

        // Draw profile selection buttons
        if (ImGui.Button("Save")) {
            showSaveDialog = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Save configured custom weights to the currently selected profile");
        }
        ImGui.SameLine();
        if (ImGui.Button("Save as")) {
            showSaveAsNewProfileDialog = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Save configured custom weights as a new profile");
        }
        ImGui.SameLine();
        if (ImGui.Button("Reset")) {
            showResetDialog = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Reset currently selected profile to default values");
        }
        ImGui.SameLine();
        if (ImGui.Button("Delete")) {
            showDeleteDialog = true;
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Delete the currently selected profile");
        }

        // Draw dialogs 
        DrawSaveDialog();
        DrawSaveAsNewDialog();
        DrawResetProfileDialog();
        DrawDeleteProfileDialog();

        // Draw remaining settings from PathfindSanctumSettings class annotations
        base.DrawSettings();
    }

    private void DrawSaveDialog() {
        if (showSaveDialog) {
            int buttonWidth = 60;
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 0));
            // Check if saving profile is allowed
            if (Settings.ProfileManager.ProtectedProfiles.Contains(Settings.CurrentProfile)) {
                ImGui.OpenPopup("Error saving profile");
                if (ImGui.BeginPopupModal("Error saving profile", ref showSaveDialog, ImGuiWindowFlags.AlwaysAutoResize)) {
                    ImGui.TextWrapped("You can not override the built-in profiles 'Default' and 'No-Hit'!\n \nSave as new profile instead!\n ");
                    ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - buttonWidth) * 0.5f);
                    if (ImGui.Button("Ok", new System.Numerics.Vector2(buttonWidth, 0))) {
                        showSaveDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            } else {
                ImGui.OpenPopup("Confirm save");
                if (ImGui.BeginPopupModal("Confirm save", ref showSaveDialog, ImGuiWindowFlags.AlwaysAutoResize)) {

                    ImGui.TextWrapped($"Save your currently configured custom weights to profile '{Settings.CurrentProfile}'?\n ");
                    ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - (buttonWidth * 2) + 10) * 0.5f);
                    if (ImGui.Button("Yes", new System.Numerics.Vector2(buttonWidth, 0))) {
                        Settings.ProfileManager.SaveProfileAs(Settings.CurrentProfile);
                        showSaveDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No", new System.Numerics.Vector2(buttonWidth, 0))) {
                        showSaveDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            }
        }
    }

    private void DrawSaveAsNewDialog() {
        if (showSaveAsNewProfileDialog) {
            int buttonWidth = 100;
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 0));
            ImGui.OpenPopup("Save as new profile");
            if (ImGui.BeginPopupModal("Save as new profile", ref showSaveAsNewProfileDialog)) {
                ImGui.TextWrapped("This will save your currently configured custom weights to a new profile.\n ");
                ImGui.TextWrapped("Please enter a name and confirm to save");
                ImGui.InputText("##SaveAsProfileName", ref saveAsName, 42);
                ImGui.Text(" ");
                buttonWidth = 100;
                if (ImGui.Button("Save", new System.Numerics.Vector2(buttonWidth, 0))) {
                    if (Settings.ProfileManager.ProtectedProfiles.Contains(saveAsName) || string.IsNullOrWhiteSpace(saveAsName)) {
                        showSaveAsInvalidNameError = true;
                        //Settings.showSaveAsNewProfileDialog = false;
                        //ImGui.CloseCurrentPopup();
                    } else {
                        Settings.ProfileManager.SaveProfileAs(saveAsName);
                        saveAsName = string.Empty;
                        showSaveAsNewProfileDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new System.Numerics.Vector2(buttonWidth, 0))) {
                    showSaveAsNewProfileDialog = false;
                    ImGui.CloseCurrentPopup();
                }

                // Draw save as error popup 
                if (showSaveAsInvalidNameError) {
                    ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
                    ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 0));
                    ImGui.OpenPopup("Error saving profile");
                    if (ImGui.BeginPopupModal("Error saving profile", ref showSaveAsInvalidNameError, ImGuiWindowFlags.AlwaysAutoResize)) {
                        ImGui.TextWrapped("Invalid name!\nName cannot be empty and you can not override the built-in profiles 'Default' and 'No-Hit'\n ");
                        ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - buttonWidth) * 0.5f);
                        if (ImGui.Button("Ok", new System.Numerics.Vector2(buttonWidth, 0))) {
                            showSaveAsInvalidNameError = false;
                        }
                        ImGui.EndPopup();
                    }
                }

                ImGui.EndPopup();
            }
        }
    }

    private void DrawResetProfileDialog() {
        if (showResetDialog) {
            int buttonWidth = 60;
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 0));
            ImGui.OpenPopup("Confirm reset");
            if (ImGui.BeginPopupModal("Confirm reset", ref showResetDialog, ImGuiWindowFlags.AlwaysAutoResize)) {

                ImGui.TextWrapped($"Are you sure you want to reset the profile '{Settings.CurrentProfile}' to default values?\n ");
                ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - ((buttonWidth * 2) + 10)) * 0.5f);
                if (ImGui.Button("Yes", new System.Numerics.Vector2(buttonWidth, 0))) {
                    Settings.ProfileManager.ResetCurrentProfileToDefault();
                    showResetDialog = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("No", new System.Numerics.Vector2(buttonWidth, 0))) {
                    showResetDialog = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }
    }

    private void DrawDeleteProfileDialog() {
        if (showDeleteDialog) {
            int buttonWidth = 60;
            var center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 0));
            // Check if deleting profile is allowed
            if (Settings.ProfileManager.ProtectedProfiles.Contains(Settings.CurrentProfile)) {
                ImGui.OpenPopup("Error deleting profile");
                if (ImGui.BeginPopupModal("Error deleting profile", ref showDeleteDialog, ImGuiWindowFlags.AlwaysAutoResize)) {
                    ImGui.TextWrapped("You can not delete the built-in profiles 'Default' and 'No-Hit'!\n ");
                    ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - buttonWidth) * 0.5f);
                    if (ImGui.Button("Ok", new System.Numerics.Vector2(buttonWidth, 0))) {
                        showDeleteDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            } else {
                ImGui.OpenPopup("Confirm deletion");
                if (ImGui.BeginPopupModal("Confirm deletion", ref showDeleteDialog, ImGuiWindowFlags.AlwaysAutoResize)) {
                    ImGui.TextWrapped($"Are you sure you want to delete the profile '{Settings.CurrentProfile}'?\n ");
                    ImGui.SetCursorPosX((ImGui.GetContentRegionAvail().X - (buttonWidth * 2) + 10) * 0.5f);
                    if (ImGui.Button("Yes", new System.Numerics.Vector2(buttonWidth, 0))) {
                        Settings.ProfileManager.DeleteCurrentProfile();
                        showDeleteDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No", new System.Numerics.Vector2(buttonWidth, 0))) {
                        showDeleteDialog = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            }
        }
    }

    private void UpdateAndRenderPath()
    {
        // EC2 0.5 can briefly expose the Sanctum window before all child/FloorData
        // objects are populated. Avoid crashing the whole plugin during that frame.
        if (pathFinder == null
            || stateTracker.roomsByLayer == null
            || stateTracker.roomsByLayer.Count == 0)
            return;

        // TODO: Optimize this so it's not executed on every render (maybe only executed if we updated our known states)
        pathFinder.CreateRoomWeightMap();

        if (Settings.DebugSettings.DebugEnable)
        {
            pathFinder.DrawDebugInfo();
        }

        pathFinder.FindBestPath();
        pathFinder.DrawBestPath();
    }
}
