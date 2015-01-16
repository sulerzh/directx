// Copyright (c) Microsoft Corporation.  All rights reserved.

#include "stdafx.h"
#include "DXGIDevice1.h"

using namespace Microsoft::Data::Visualization::DirectX::Utilities;
using namespace Microsoft::Data::Visualization::DirectX::Graphics;

UInt32 Device1::MaximumFrameLatency::get()
{
    UINT tempoutMaxLatency;
    Validate::VerifyResult(CastInterface<IDXGIDevice1>()->GetMaximumFrameLatency(&tempoutMaxLatency));
    return tempoutMaxLatency;
}

void Device1::MaximumFrameLatency::set(UInt32 value)
{
    Validate::VerifyResult(CastInterface<IDXGIDevice1>()->SetMaximumFrameLatency(static_cast<UINT>(value)));
}

