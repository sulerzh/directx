// Copyright (c) Microsoft Corporation.  All rights reserved.

#include "stdafx.h"

#include "WICBitmapFrameDecode.h"

using namespace Microsoft::Data::Visualization::DirectX::Utilities;
using namespace Microsoft::Data::Visualization::DirectX::WindowsImagingComponent;


BitmapSource^ BitmapFrameDecode::ToBitmapSource()
{
    IWICBitmapSource* pBitmapSource = NULL;
    Validate::VerifyResult(
        CastInterface<IUnknown>()->QueryInterface(__uuidof(IWICBitmapSource), (void **)&pBitmapSource));
    return gcnew BitmapSource( pBitmapSource );
}

BitmapSize^ BitmapFrameDecode::GetSize()
{
	UINT width = 0;
	UINT height = 0;
	Validate::VerifyResult(
		CastInterface<IWICBitmapFrameDecode>()->GetSize(&width, &height));

	return gcnew BitmapSize(width, height);
}

Guid BitmapFrameDecode::GetPixelFormat()
{
	GUID guid;
	Validate::VerifyResult(
		CastInterface<IWICBitmapFrameDecode>()->GetPixelFormat(&guid));

	return Utilities::Convert::SystemGuidFromGUID(guid);
}
