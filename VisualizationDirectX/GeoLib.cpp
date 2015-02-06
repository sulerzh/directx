#include "stdafx.h"
#include "GeoLib.h"

using namespace Microsoft::Data::Visualization::Engine::Graphics::Internal;

void GeoLib::DeleteArray(IntPtr arr)
{
	delete[] arr.ToPointer();
}

void GeoLib::Tessellate(IntPtr polygonCoordinates, int polygonCoordinateCount, IntPtr polygonRings, int ringCount, double maxEdgeLength, IntPtr* triangleCoordinates, int* triangleCoordinateCount, IntPtr* triangleIndices, int* triangleIndexCount)
{
	//::GeodeticTessellate();
	/*double* pTriangleCoordinates = nullptr;
	int* pTriangleIndex = nullptr;
	if (GeodeticTessellate((double*)polygonCoordinates.ToPointer(), polygonCoordinateCount, (int *)polygonRings.ToPointer(), ringCount, maxEdgeLength, &pTriangleCoordinates, triangleCoordinateCount, &pTriangleIndex, triangleIndexCount) >= 0)
	{
		IntPtr ptr2 = new IntPtr((void*)pTriangleCoordinates);
		triangleCoordinates[0] = ptr2;
		IntPtr ptr = new IntPtr((void*)pTriangleIndex);
		triangleIndices[0] = ptr;
	}
	else
	{
		delete[]((void*)pTriangleCoordinates);
		delete[]((void*)pTriangleIndex);
		triangleIndexCount[0] = 0;
		triangleCoordinateCount[0] = 0;
	}*/
}
