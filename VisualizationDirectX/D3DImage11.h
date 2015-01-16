#pragma once
#include <windows.h>

#include <stdlib.h>
#include <stdio.h>

#include <d3d9.h>
//#include <d3dx9.h>
#include <D3D11.h>

using namespace System;
using namespace System::Windows::Interop;
using namespace System::Windows;

namespace Microsoft { namespace Data { namespace Visualization { namespace Engine { namespace Graphics { namespace Internal {
	
public ref class D3DImage11 : D3DImage
	{
		static D3DImage11()
		{
			InitD3D9(GetDesktopWindow());
		}
		~D3DImage11();
	public:
		void SetBackBuffer11(IntPtr pResource);
	private:
		static IDirect3D9Ex* mD3D9;
		static IDirect3DDevice9Ex* mD3D9Device;
	private:
		static D3DFORMAT ConvertDXGIToD3D9Format(DXGI_FORMAT format);
		
		static HRESULT GetSharedSurface(HANDLE hSharedHandle, void** ppUnknown, UINT width, UINT height, DXGI_FORMAT format);
		
		static HRESULT GetSharedHandle(IUnknown *pUnknown, HANDLE * pHandle);
		
		static HRESULT InitD3D9(HWND hWnd);
	};

} } } } } }
