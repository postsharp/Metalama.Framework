// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.DesignTime.Pipeline;

public class DesignTimeValidatorInstance
{
    public SymbolDictionaryKey ValidatedDeclaration { get; }

    private readonly ReferenceKinds _referenceKinds;
    private readonly ValidatorDriver _driver;

    internal ValidatorImplementation Implementation { get; }

    internal DesignTimeValidatorInstance(
        ISymbol validatedDeclaration,
        ReferenceKinds referenceKinds,
        ValidatorDriver driver,
        ValidatorImplementation implementation )
    {
        this.ValidatedDeclaration = SymbolDictionaryKey.CreatePersistentKey( validatedDeclaration );
        this._referenceKinds = referenceKinds;
        this._driver = driver;
        this.Implementation = implementation;
    }

    internal ReferenceValidatorInstance ToReferenceValidationInstance( CompilationModel compilation )
        => new(
            compilation.Factory.GetDeclarationFromId( this.ValidatedDeclaration.GetId().ToString() ).AssertNotNull(),
            this._driver,
            this.Implementation,
            this._referenceKinds );
}