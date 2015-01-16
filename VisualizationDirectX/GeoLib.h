#pragma once

namespace Microsoft { namespace Data { namespace Visualization { namespace Engine { namespace Graphics { namespace Internal {

public ref class GeoLib
{
public:
	static void DeleteArray(IntPtr arr);
	static void Tessellate(IntPtr polygonCoordinates, int polygonCoordinateCount, IntPtr polygonRings, int ringCount, double maxEdgeLength, IntPtr* triangleCoordinates, int* triangleCoordinateCount, IntPtr* triangleIndices, int* triangleIndexCount);
};

} } } } } }



