// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation
{
    public interface IReferenceIndexerOptions
    {
        bool MustDescendIntoMembers();

        bool MustDescendIntoImplementation();

        bool MustDescendIntoReferencedBaseTypes( ReferenceKinds referenceKinds );

        bool MustDescendIntoReferencedDeclaringType( ReferenceKinds referenceKinds );

        bool MustDescendIntoReferencedNamespace( ReferenceKinds referenceKinds );

        bool MustDescendIntoReferencedAssembly( ReferenceKinds referenceKinds );
    }
}