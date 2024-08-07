// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Concurrent;

namespace Metalama.Framework.DesignTime.SourceGeneration;

// Backup for when the touch file is not watched for changes by the IDE; only works inside the same process.
// This exists for two reasons:
// 1. Roslyn in VS has a bug (https://github.com/dotnet/roslyn/issues/74716) where the touch file is not correctly watched for changes.
// 2. Rider does not currently watch the touch file for changes (https://youtrack.jetbrains.com/issue/RIDER-75959).
// Note that this approach can't trigger running the generator, which means that changes will become visible only once the user types something in some C# file in the project.
internal static class TouchFileWatcher
{
    private static ConcurrentDictionary<string, bool> UpdatedTouchFiles { get; } = new();

    public static void MarkAsUpdated( string touchFilePath ) => UpdatedTouchFiles[touchFilePath] = true;

    public static bool GetIsUpdated( string touchFilePath ) => UpdatedTouchFiles.ContainsKey( touchFilePath );

    public static bool GetIsUpdatedAndReset( string touchFilePath ) => UpdatedTouchFiles.TryRemove( touchFilePath, out _ );
}