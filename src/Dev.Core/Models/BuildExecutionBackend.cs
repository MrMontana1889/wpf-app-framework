// BuildExecutionBackend.cs
// Copyright (c) 2026 MrMontana1889.  See LICENSE

namespace Dev.Core.Models;

/// <summary>
/// Controls how BentleyBuild is invoked during execution.
/// </summary>
public enum BuildExecutionBackend
{
    /// <summary>
    /// Phase 2.6a: Behaves identically to <see cref="DirectUv"/>.
    /// Reserved for future policy logic (Phase 2.6b).
    /// </summary>
    Auto,

    /// <summary>
    /// Invokes BentleyBuild directly via <c>uv.exe</c> using <c>ArgumentList</c>.
    /// Uses Hostx86 MSVC behavior (BentleyBuild internal defaults).
    /// Safest path regarding Windows command-line length limits.
    /// </summary>
    DirectUv,

    /// <summary>
    /// Invokes BentleyBuild via <c>cmd.exe</c> with a <c>VsDevCmd.bat</c> wrapper.
    /// Forces Hostx64 MSVC. Required for large C++ / PCH-heavy builds.
    /// Cannot use <c>ArgumentList</c> due to Windows <c>cmd.exe /s /c</c> semantics.
    /// </summary>
    CmdWrapper,
}
