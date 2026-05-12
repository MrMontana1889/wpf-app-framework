// IDialogService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

/// <summary>
/// Result of a save changes prompt.
/// </summary>
public enum SaveChangesResult
{
    Yes,
    No,
    Cancel
}

/// <summary>
/// Service for displaying dialogs in the ModelFinder application.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a generic informational message dialog.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="message">Dialog body message.</param>
    void ShowMessage(string title, string message);

    /// <summary>
    /// Shows the About dialog.
    /// </summary>
    void ShowAboutDialog();

    /// <summary>
    /// Shows the Application Settings dialog.
    /// </summary>
    void ShowAppSettingsDialog();

    /// <summary>
    /// Shows a save changes prompt dialog.
    /// </summary>
    /// <returns>User's choice: Yes, No, or Cancel.</returns>
    SaveChangesResult ShowSaveChangesPrompt();

    /// <summary>
    /// Shows an Open File dialog for .mfjson files.
    /// </summary>
    /// <param name="filePath">The selected file path, or null if cancelled.</param>
    /// <returns>True if a file was selected, false if cancelled.</returns>
    bool ShowOpenFileDialog(out string? filePath);

    /// <summary>
    /// Shows a Save File dialog for .mfjson files.
    /// </summary>
    /// <param name="filePath">The selected file path, or null if cancelled.</param>
    /// <returns>True if a file was selected, false if cancelled.</returns>
    bool ShowSaveFileDialog(out string? filePath);

    /// <summary>
    /// Shows a folder browser dialog for selecting a directory.
    /// </summary>
    /// <param name="folderPath">The selected folder path, or null if cancelled.</param>
    /// <returns>True if a folder was selected, false if cancelled.</returns>
    bool ShowFolderBrowserDialog(out string? folderPath);
}
