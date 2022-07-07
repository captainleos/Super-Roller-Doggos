#ifndef TERRASTANDARD_CORE_FORWARD_CGINC_INCLUDED
#define TERRASTANDARD_CORE_FORWARD_CGINC_INCLUDED

#include "UnityStandardConfig.cginc"

#include "TerraStandard_Core_Tessellation.cginc"
VertexOutputForwardBase vertBase (VertexInput v) { return vertForwardBase(v); }
VertexOutputForwardAdd vertAdd (VertexInput v) { return vertForwardAdd(v); }
half4 fragBase (VertexOutputForwardBase i) : SV_Target { return fragForwardBaseInternal(i); }
half4 fragAdd (VertexOutputForwardAdd i) : SV_Target { return fragForwardAddInternal(i); }


#endif // TERRASTANDARD_CORE_FORWARD_INCLUDED
