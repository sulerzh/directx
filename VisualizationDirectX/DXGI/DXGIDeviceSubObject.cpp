// Copyright (c) Microsoft Corporation.  All rights reserved.

#include "stdafx.h"
#include "DXGIDeviceSubObject.h"

#include "DXGIDevice.h"
#include "Direct3D11/D3D11Device.h"

using namespace Microsoft::Data::Visualization::DirectX::Utilities;
using namespace Microsoft::Data::Visualization::DirectX::Graphics;

Microsoft::Data::Visualization::DirectX::Direct3D11::D3DDevice^ DeviceSubObject::AsDirect3D11Device::get(void)
{
    ID3D11Device* tempoutDevice = NULL;

    Validate::VerifyResult(CastInterface<IDXGIDeviceSubObject>()->GetDevice(__uuidof(tempoutDevice), (void**)&tempoutDevice));

    return tempoutDevice == NULL ? nullptr : gcnew Microsoft::Data::Visualization::DirectX::Direct3D11::D3DDevice(tempoutDevice);
}

Graphics::Device^ DeviceSubObject::AsGraphicsDevice::get(void)
{
    IDXGIDevice* tempoutDevice = NULL;

    Validate::VerifyResult(CastInterface<IDXGIDeviceSubObject>()->GetDevice(__uuidof(tempoutDevice), (void**)&tempoutDevice));

    return tempoutDevice == NULL ? nullptr : gcnew Graphics::Device(tempoutDevice);
}

