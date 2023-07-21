// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using K4os.Hash.xxHash;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline;

internal sealed class DesignTimeReferenceValidatorInstance : IReferenceValidatorProperties
{
    private readonly ValidatorDriver _driver;
    private readonly string _description;

    public SymbolDictionaryKey ValidatedDeclaration { get; }

    public ReferenceKinds ReferenceKinds { get; }

    public bool IncludeDerivedTypes { get; }

    public DeclarationKind ValidatedDeclarationKind { get; }

    internal ValidatorImplementation Implementation { get; }

    internal DesignTimeReferenceValidatorInstance(
        ISymbol validatedDeclaration,
        ReferenceKinds referenceReferenceKinds,
        bool includeDerivedTypes,
        ValidatorDriver driver,
        ValidatorImplementation implementation,
        string description )
    {
        this.ValidatedDeclaration = SymbolDictionaryKey.CreatePersistentKey( validatedDeclaration );
        this.ValidatedDeclarationKind = validatedDeclaration.GetDeclarationKind();
        this.ReferenceKinds = referenceReferenceKinds;
        this.IncludeDerivedTypes = includeDerivedTypes;
        this._driver = driver;
        this._description = description;
        this.Implementation = implementation;
    }

    internal ReferenceValidatorInstance ToReferenceValidationInstance( CompilationModel compilation )
        => new(
            compilation.Factory.GetDeclaration( this.ValidatedDeclaration ).AssertNotNull(),
            this._driver,
            this.Implementation,
            this.ReferenceKinds,
            this.IncludeDerivedTypes,
            this._description );

    public TransitiveValidatorInstance ToTransitiveValidatorInstance()
        => new(
            this.ValidatedDeclaration.ToRef(),
            this.ReferenceKinds,
            this.IncludeDerivedTypes,
            this.Implementation.Implementation,
            this.Implementation.State,
            this._driver.MethodName,
            this._description );

    public override string ToString() => this._description;
}