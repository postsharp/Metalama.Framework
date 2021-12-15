using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

public class DesignTimeValidatorInstance
{
    public SymbolKey ValidatedDeclaration { get; }

    private readonly ReferenceKinds _referenceKinds;
    private readonly ValidatorSource _source;

    internal DesignTimeValidatorInstance( ISymbol validatedDeclaration, ReferenceKinds referenceKinds, ValidatorSource source )
    {
        this.ValidatedDeclaration = SymbolKey.Create( validatedDeclaration );
        this._referenceKinds = referenceKinds;
        this._source = source;
    }

    internal ReferenceValidatorInstance ToReferenceValidationInstance( CompilationModel compilation )
        => new ReferenceValidatorInstance( this._source, compilation.Factory.GetDeclarationFromId( this.ValidatedDeclaration.GetId().ToString() ).AssertNotNull(  ), this._referenceKinds );
}