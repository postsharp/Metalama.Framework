// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking.Inlining;

internal record struct InliningId ( int Value );

internal sealed class InliningAnalysisContext
{
    private readonly PersistentContext _persistentContext;

    public bool UsingSimpleInlining { get; }

    public string? ReturnVariableIdentifier { get; }

    public InliningId Id { get; }

    public InliningId? ParentId { get; }

    public InliningAnalysisContext() : this( null, new PersistentContext(), true, null ) { }

    private InliningAnalysisContext( InliningId? parentId, PersistentContext identifierProvider, bool usingSimpleInlining, string? returnVariableIdentifier )
    {
        this.UsingSimpleInlining = usingSimpleInlining;
        this._persistentContext = identifierProvider;
        this.Id = this._persistentContext.GetNextId();
        this.ParentId = parentId;
        this.ReturnVariableIdentifier = returnVariableIdentifier;
    }

    public string AllocateReturnLabel() => this._persistentContext.AllocateReturnLabel();

    internal InliningAnalysisContext Recurse() => new( this.Id, this._persistentContext, this.UsingSimpleInlining, null );

    internal InliningAnalysisContext RecurseWithSimpleInlining() => new( this.Id, this._persistentContext, true, null );

    internal InliningAnalysisContext RecurseWithComplexInlining( string? returnVariableIdentifier )
        => new( this.Id, this._persistentContext, false, returnVariableIdentifier );

    private sealed class PersistentContext
    {
        private int _nextOrdinal = 1;
        private int _nextReturnLabelIdentifier = 1;

        public InliningId GetNextId() => new InliningId( this._nextOrdinal++ );

        public string AllocateReturnLabel()
        {
            var id = this._nextReturnLabelIdentifier++;

            return $"__aspect_return_{id}";
        }
    }
}