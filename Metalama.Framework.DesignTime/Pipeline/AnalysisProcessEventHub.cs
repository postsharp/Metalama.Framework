// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Rpc.Notifications;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.Pipeline;

public class AnalysisProcessEventHub : IService
{
    private readonly ILogger _logger;
    private bool _isEditingCompileTimeCode;

    public AnalysisProcessEventHub( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "EventHub" );
    }

    public bool IsEditingCompileTimeCode
    {
        get => this._isEditingCompileTimeCode;
        set
        {
            if ( value != this._isEditingCompileTimeCode )
            {
                this._isEditingCompileTimeCode = value;
                this.IsEditingCompileTimeCodeChanged?.Invoke( value );
            }
        }
    }

    public event Action<bool>? IsEditingCompileTimeCodeChanged;

    public event Action? EditingCompileTimeCodeCompleted;

    public event Action<CompilationResultChangedEventArgs>? CompilationResultChanged;

    public void PublishCompileTimeCodeCompletedEditing()
    {
        this.EditingCompileTimeCodeCompleted?.Invoke();
    }

    public bool IsUserInterfaceAttached { get; private set; }

    public void PublishUserInterfaceAttached()
    {
        this.IsUserInterfaceAttached = true;
    }

    public void PublishCompilationResultChangedNotification( CompilationResultChangedEventArgs notification )
    {
        if ( this.CompilationResultChanged == null )
        {
            this._logger.Warning?.Log( "Do not publish a change notification because there is no listener." );
        }

        this.CompilationResultChanged?.Invoke( notification );
    }
}