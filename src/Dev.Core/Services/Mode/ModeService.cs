// ModeService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Services.Mode;

/// <summary>
/// Default implementation of <see cref="IModeService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safety: this class is not thread-safe. All calls are expected on the
/// application's main thread.
/// </para>
/// </remarks>
public sealed class ModeService : IModeService
{
    private readonly IToolbarRegistryService? _toolbarRegistry;
    private readonly List<IInteractionOverlay> _activeOverlays = new();
    private BaselineMode _activeBaselineMode = BaselineMode.Simple;
    private IFeatureMode? _activeFeatureMode;

    /// <summary>
    /// Creates a new <see cref="ModeService"/>.
    /// </summary>
    /// <param name="toolbarRegistry">
    /// Optional toolbar registry. When provided, <see cref="IFeatureMode.PrimaryToolbarId"/>
    /// visibility is automatically set on enter/exit.
    /// </param>
    public ModeService(IToolbarRegistryService? toolbarRegistry = null)
    {
        _toolbarRegistry = toolbarRegistry;
    }

    /// <inheritdoc/>
    public BaselineMode ActiveBaselineMode => _activeBaselineMode;

    /// <inheritdoc/>
    public IFeatureMode? ActiveFeatureMode => _activeFeatureMode;

    /// <inheritdoc/>
    public IReadOnlyList<IInteractionOverlay> ActiveOverlays => _activeOverlays;

    /// <inheritdoc/>
    public bool IsFeatureModeActive => _activeFeatureMode is not null;

    /// <inheritdoc/>
    public bool IsShutdownBlocked => IsFeatureModeActive;

    /// <inheritdoc/>
    public event EventHandler<BaselineModeChangedEventArgs>? BaselineModeChanged;

    /// <inheritdoc/>
    public event EventHandler<FeatureModeChangedEventArgs>? FeatureModeChanged;

    /// <inheritdoc/>
    public void SetBaselineMode(BaselineMode mode)
    {
        if (_activeBaselineMode == mode)
            return;

        var previous = _activeBaselineMode;
        _activeBaselineMode = mode;
        BaselineModeChanged?.Invoke(this, new BaselineModeChangedEventArgs(previous, mode));
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a feature mode is already active.
    /// </exception>
    public void EnterFeatureMode(IFeatureMode mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        if (_activeFeatureMode is not null)
            throw new InvalidOperationException(
                $"Feature mode '{_activeFeatureMode.ModeId}' is already active. " +
                "Exit the current feature mode before entering a new one.");

        _activeFeatureMode = mode;

        if (mode.PrimaryToolbarId.HasValue)
            _toolbarRegistry?.SetVisibility(mode.PrimaryToolbarId.Value, true);

        mode.OnEnter();

        FeatureModeChanged?.Invoke(this, new FeatureModeChangedEventArgs(mode, isEntering: true));
    }

    /// <inheritdoc/>
    public void ExitFeatureMode()
    {
        if (_activeFeatureMode is null)
            return;

        var exiting = _activeFeatureMode;
        _activeFeatureMode = null;

        if (exiting.PrimaryToolbarId.HasValue)
            _toolbarRegistry?.SetVisibility(exiting.PrimaryToolbarId.Value, false);

        exiting.OnExit();

        FeatureModeChanged?.Invoke(this, new FeatureModeChangedEventArgs(exiting, isEntering: false));
    }

    /// <inheritdoc/>
    public async Task<bool> TryApplyAndExitFeatureModeAsync()
    {
        if (_activeFeatureMode is null)
            return false;

        if (!await _activeFeatureMode.TryApplyAsync())
            return false;

        ExitFeatureMode();
        return true;
    }

    /// <inheritdoc/>
    public void CancelAndExitFeatureMode()
    {
        if (_activeFeatureMode is null)
            return;

        _activeFeatureMode.Cancel();
        ExitFeatureMode();
    }

    /// <inheritdoc/>
    public void ShowOverlay<TResult>(IInteractionOverlay<TResult> overlay, Action<TResult> onResult)
    {
        ArgumentNullException.ThrowIfNull(overlay);
        ArgumentNullException.ThrowIfNull(onResult);

        overlay.SetResultCallback(result =>
        {
            if (!IsTopOverlay(overlay))
                return;

            onResult(result);
            CloseOverlay(overlay);
        });
        _activeOverlays.Add(overlay);
        overlay.OnEnter();
    }

    /// <inheritdoc/>
    public void CloseOverlay(IInteractionOverlay overlay)
    {
        ArgumentNullException.ThrowIfNull(overlay);

        if (!_activeOverlays.Remove(overlay))
            return;

        overlay.OnExit();
    }

    private bool IsTopOverlay(IInteractionOverlay overlay)
    {
        if (_activeOverlays.Count == 0)
            return false;

        return ReferenceEquals(_activeOverlays[^1], overlay);
    }
}
