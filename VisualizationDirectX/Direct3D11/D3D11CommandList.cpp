// Copyright (c) Microsoft Corporation.  All rights reserved.

#include "stdafx.h"
#include "D3D11CommandList.h"

using namespace Microsoft::Data::Visualization::DirectX::Direct3D11;

UInt32 CommandList::ContextOptions::get()
{
    return CastInterface<ID3D11CommandList>()->GetContextFlags();
}

