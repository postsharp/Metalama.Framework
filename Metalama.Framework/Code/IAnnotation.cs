// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Serialization;

namespace Metalama.Framework.Code;

public interface IAnnotation : ICompileTimeSerializable { }

public interface IAnnotation<in T> : IAnnotation
    where T : class, IDeclaration { }