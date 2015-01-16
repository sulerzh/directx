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

int BitmapFrameDecode::GetSize()
{
	return 0;
}

Guid BitmapFrameDecode::GetPixelFormat()
{
	return PixelFormats::Alpha8Bpp;
}
