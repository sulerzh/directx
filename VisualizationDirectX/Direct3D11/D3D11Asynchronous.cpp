// Copyright (c) Microsoft Corporation.  All rights reserved.

#include "stdafx.h"
#include "D3D11Asynchronous.h"

using namespace Microsoft::Data::Visualization::DirectX::Direct3D11;

UInt32 Asynchronous::DataSize::get()
{
    return CastInterface<ID3D11Asynchronous>()->GetDataSize();
}

