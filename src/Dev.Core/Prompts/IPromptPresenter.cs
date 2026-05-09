// IPromptPresenter.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Prompts;

/// <summary>
/// Abstraction for prompt presentation. Implementations are responsible for
/// rendering the prompt and returning the user's decision.
/// </summary>
/// <remarks>
/// Contract invariants that implementations must honour:
/// <list type="bullet">
///   <item><description><see cref="PromptResult.None"/> must never be returned — it is reserved for non-interactive bypass paths.</description></item>
///   <item><description>The returned <see cref="PromptResponse"/> must always be created via <see cref="PromptResponse.FromUserInteraction"/>.</description></item>
/// </list>
/// </remarks>
public interface IPromptPresenter
{
    /// <summary>
    /// Displays the prompt described by <paramref name="request"/> and returns the
    /// user's decision including their optional suppression intent.
    /// </summary>
    PromptResponse Present(BoundPromptRequest request);
}
