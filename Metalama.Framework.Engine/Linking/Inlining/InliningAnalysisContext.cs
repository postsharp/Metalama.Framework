// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal sealed class InliningAnalysisContext
    {
        private readonly PersitentContext _persistentContext;

        public bool UsingSimpleInlining { get; }

        public bool DeclaredReturnVariable { get; set; }

        public bool DeclaredReturnLabel { get; set; }

        public int Ordinal { get; }

        public int? ParentOrdinal { get; }

        public InliningAnalysisContext() : this( null, new PersitentContext(), true )
        {
        }

        private InliningAnalysisContext( int? parentOrdinal, PersitentContext identifierProvider, bool usingSimpleInlining )
        {
            this.UsingSimpleInlining = usingSimpleInlining;
            this._persistentContext = identifierProvider;
            this.Ordinal = this._persistentContext.GetNextOrdinal();
            this.ParentOrdinal = parentOrdinal;
        }

        public string AllocateReturnVariable()
        {
            this.DeclaredReturnVariable = true;
            return this._persistentContext.AllocateReturnVariable();
        }

        public string AllocateReturnLabel()
        {
            this.DeclaredReturnLabel = true;
            return this._persistentContext.AllocateReturnLabel();
        }

        internal InliningAnalysisContext Recurse()
        {
            return new InliningAnalysisContext( this.Ordinal, this._persistentContext, this.UsingSimpleInlining );
        }

        internal InliningAnalysisContext RecurseWithComplexInlining()
        {
            return new InliningAnalysisContext( this.Ordinal, this._persistentContext, false );
        }

        private class PersitentContext
        {
            private int _nextOrdinal;
            private int _nextReturnLabelIdentifier = 1;
            private int _nextReturnVariableIdentifier = 1;

            public int GetNextOrdinal()
            {
                return this._nextOrdinal++;
            }

            public string AllocateReturnLabel()
            {
                var id = this._nextReturnLabelIdentifier++;

                return $"__aspect_return_{id}";
            }

            public string AllocateReturnVariable()
            {
                var id = this._nextReturnVariableIdentifier++;

                return $"__aspect_return_value_{id}";
            }
        }
    }
}