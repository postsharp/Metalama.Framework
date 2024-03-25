// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking.Inlining;

internal sealed class InliningAnalysisContext
{
    private readonly PersistentContext _persistentContext;

    public bool UsingSimpleInlining { get; }

    public string? ReturnVariableIdentifier { get; }

    public int Ordinal { get; }

    public int? ParentOrdinal { get; }

    public InliningAnalysisContext() : this( null, new PersistentContext(), true, null ) { }

    private InliningAnalysisContext( int? parentOrdinal, PersistentContext identifierProvider, bool usingSimpleInlining, string? returnVariableIdentifier )
    {
        this.UsingSimpleInlining = usingSimpleInlining;
        this._persistentContext = identifierProvider;
        this.Ordinal = this._persistentContext.GetNextOrdinal();
        this.ParentOrdinal = parentOrdinal;
        this.ReturnVariableIdentifier = returnVariableIdentifier;
    }

    public string AllocateReturnLabel() => this._persistentContext.AllocateReturnLabel();

    internal InliningAnalysisContext Recurse() => new( this.Ordinal, this._persistentContext, this.UsingSimpleInlining, null );

    internal InliningAnalysisContext RecurseWithSimpleInlining() => new( this.Ordinal, this._persistentContext, true, null );

    internal InliningAnalysisContext RecurseWithComplexInlining( string? returnVariableIdentifier )
        => new( this.Ordinal, this._persistentContext, false, returnVariableIdentifier );

    private sealed class PersistentContext
    {
        private int _nextOrdinal;
        private int _nextReturnLabelIdentifier = 1;

        public int GetNextOrdinal() => this._nextOrdinal++;

        public string AllocateReturnLabel()
        {
            var id = this._nextReturnLabelIdentifier++;

            return $"__aspect_return_{id}";
        }
    }
}