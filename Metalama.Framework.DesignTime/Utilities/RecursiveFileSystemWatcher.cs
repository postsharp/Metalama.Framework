// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Utilities;

/// <summary>
/// Variation of <see cref="FileSystemWatcher"/> that can handle missing parent directories.
/// </summary>
internal class RecursiveFileSystemWatcher : IDisposable
{
    // Ensures that _watcher and EnableRaisingEvents are synchronized.
    private readonly object _lock = new();

    private FileSystemWatcher? _watcher;

    private RecursiveFileSystemWatcher? _parentDirectoryWatcher;

    private bool _enableRaisingEvents;

    public string Path { get; }

    public string Filter { get; }

    public bool EnableRaisingEvents
    {
        get => this._enableRaisingEvents;
        set
        {
            lock ( this._lock )
            {
                this._enableRaisingEvents = value;

                if ( this._watcher != null )
                {
                    this._watcher.EnableRaisingEvents = value;
                }
            }
        }
    }

    public event FileSystemEventHandler? Changed;

    public event FileSystemEventHandler? Created;

    public RecursiveFileSystemWatcher( string path, string filter )
    {
        this.Path = path ?? throw new ArgumentNullException( nameof(path) );
        this.Filter = filter;

        if ( Directory.Exists( path ) )
        {
            this.CreateWatcher();
        }
        else
        {
            var parentDirectory = System.IO.Path.GetDirectoryName( path )
                                  ?? throw new ArgumentException(
                                      $"Couldn't start watching for the parent directory to be created: it seems the directory '{path}' doesn't exist, but it's a root directory.",
                                      nameof(path) );

            this._parentDirectoryWatcher = new RecursiveFileSystemWatcher( parentDirectory, System.IO.Path.GetFileName( path ) );

            void OnParentDirectoryCreated( object? s, FileSystemEventArgs? e )
            {
                if ( Directory.Exists( path ) )
                {
                    this.CreateWatcher();
                    this._parentDirectoryWatcher?.Dispose();
                    this._parentDirectoryWatcher = null;

                    if ( this.EnableRaisingEvents )
                    {
                        // If files were created in the directory before the watcher was created, we need to raise the events for them.
                        foreach ( var file in Directory.EnumerateFileSystemEntries( path, filter ) )
                        {
                            this.Created?.Invoke( this, new FileSystemEventArgs( WatcherChangeTypes.Created, path, file ) );
                            this.Changed?.Invoke( this, new FileSystemEventArgs( WatcherChangeTypes.Changed, path, file ) );
                        }
                    }
                }
            }

            this._parentDirectoryWatcher.Created += OnParentDirectoryCreated;
            this._parentDirectoryWatcher.EnableRaisingEvents = true;

            // If the parent directory was created after we checked whether it exists and before we started watching it, we need to check it again.
            OnParentDirectoryCreated( null, null );
        }
    }

    private void CreateWatcher()
    {
        var watcher = new FileSystemWatcher( this.Path, this.Filter );

        watcher.Changed += ( _, e ) => this.Changed?.Invoke( this, e );
        watcher.Created += ( _, e ) => this.Created?.Invoke( this, e );

        lock ( this._lock )
        {
            watcher.EnableRaisingEvents = this.EnableRaisingEvents;

            this._watcher = watcher;
        }
    }

    public void Dispose()
    {
        this._watcher?.Dispose();
        this._parentDirectoryWatcher?.Dispose();

        this.Changed = null;
        this.Created = null;
    }
}