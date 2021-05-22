using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    public interface IAspectMarker { }
    
     public interface IAspectMarker<in TMarkedDeclaration, TAspectTarget, TAspectClass> : IAspectMarker, IEligible<TMarkedDeclaration>
            where TMarkedDeclaration : class, IDeclaration
            where TAspectTarget : class, IAspectTarget 
            where TAspectClass : IAspect<TAspectTarget>, new()
        {
            
        }
    
      
}