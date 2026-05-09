// PromptOrchestrationService.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Prompts;

/// <summary>
/// Implements the authoritative prompt orchestration flow as defined in the design.
/// </summary>
public sealed class PromptOrchestrationService : IPromptOrchestrationService
{
    private readonly IPromptRegistry _registry;
    private readonly IPromptSuppressionService _suppressionService;
    private readonly IPromptPresenter _presenter;

    public PromptOrchestrationService(
        IPromptRegistry registry,
        IPromptSuppressionService suppressionService,
        IPromptPresenter presenter)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(suppressionService);
        ArgumentNullException.ThrowIfNull(presenter);

        _registry = registry;
        _suppressionService = suppressionService;
        _presenter = presenter;
    }

    /// <summary>
    /// Orchestrates prompt execution per design §9:
    /// <list type="number">
    ///   <item>Resolve the definition from the registry.</item>
    ///   <item>If suppression is allowed, consult the suppression service.</item>
    ///   <item>If a stored response exists, bypass the presenter and return it.</item>
    ///   <item>Otherwise, delegate to the presenter.</item>
    ///   <item>If the presenter's response is eligible, persist it.</item>
    ///   <item>Return the response to the caller.</item>
    /// </list>
    /// </summary>
    public PromptResponse Execute(PromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var definition = _registry.GetDefinition(request.PromptId);

        // Step 2-3: suppression bypass
        if (definition.AllowSuppression)
        {
            var stored = _suppressionService.TryGetSuppressedResponse(request.PromptId);
            if (stored is not null)
                return stored;
        }

        // Step 4: delegate to presenter
        var bound = new BoundPromptRequest(request, definition);
        var response = _presenter.Present(bound);

        // Enforce presenter contract: None is not a valid interactive result.
        if (response.Result == PromptResult.None)
            throw new InvalidOperationException(
                $"Presenter returned {nameof(PromptResult.None)} for prompt '{request.PromptId}'. " +
                $"{nameof(PromptResult.None)} is reserved for non-interactive bypass paths and must never be returned by a presenter.");

        // Step 5: conditionally persist
        if (PromptSuppressionEligibility.IsEligible(response))
            _suppressionService.PersistSuppression(request.PromptId, response);

        return response;
    }
}
