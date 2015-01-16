// Copyright (c) Microsoft Corporation.  All rights reserved.

#include "stdafx.h"
#include "DXGIObject.h"

using namespace Microsoft::Data::Visualization::DirectX::Utilities;
using namespace Microsoft::Data::Visualization::DirectX::Graphics;

generic <typename T> where T : GraphicsObject
T GraphicsObject::GetParent(void) 
{
    void* tempParent = NULL;
    GUID guid = CommonUtils::GetGuid(T::typeid);
    
    Validate::VerifyResult(CastInterface<IDXGIObject>()->GetParent(guid, &tempParent));

    return Utilities::Convert::CreateIUnknownWrapper<T>(static_cast<IUnknown*>(tempParent));
}