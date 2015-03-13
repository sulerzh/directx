#pragma once

namespace Microsoft { namespace Data { namespace Visualization { namespace Engine { namespace Graphics { namespace Internal {

using namespace System::Runtime::InteropServices;

[DllImport("SqlServerSpatial110.dll")]
extern int GeodeticTessellate(const double* polygonCoordinates, int polygonCoordinateCount, const int* polygonRings, int ringCount, double maxEdgeLength, double** triangleCoordinates, int* triangleCoordinateCount, int** triangleIndices, int* triangleIndexCount);

public ref class GeoLib
{
public:
	static void DeleteArray(IntPtr arr);
	static void Tessellate(IntPtr polygonCoordinates, int polygonCoordinateCount, IntPtr polygonRings, int ringCount, double maxEdgeLength, IntPtr* triangleCoordinates, int* triangleCoordinateCount, IntPtr* triangleIndices, int* triangleIndexCount);
};

} } } } } }



