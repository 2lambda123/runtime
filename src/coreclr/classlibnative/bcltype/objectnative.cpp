// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// File: ObjectNative.cpp
//

//
// Purpose: Native methods on System.Object
//
//

#include "common.h"

#include "objectnative.h"
#include "excep.h"
#include "vars.hpp"
#include "field.h"
#include "object.h"
#include "comsynchronizable.h"
#include "eeconfig.h"


NOINLINE static INT32 GetHashCodeHelper(OBJECTREF objRef)
{
    DWORD idx = 0;

    FC_INNER_PROLOG(ObjectNative::GetHashCode);

    HELPER_METHOD_FRAME_BEGIN_RET_ATTRIB_1(Frame::FRAME_ATTR_EXACT_DEPTH|Frame::FRAME_ATTR_CAPTURE_DEPTH_2, objRef);

    idx = objRef->GetHashCodeEx();

    HELPER_METHOD_FRAME_END();
    FC_INNER_EPILOG();
    return idx;
}

// Note that we obtain a sync block index without actually building a sync block.
// That's because a lot of objects are hashed, without requiring support for
FCIMPL1(INT32, ObjectNative::GetHashCode, Object* obj) {

    CONTRACTL
    {
        FCALL_CHECK;
        INJECT_FAULT(FCThrow(kOutOfMemoryException););
    }
    CONTRACTL_END;

    VALIDATEOBJECT(obj);

    if (obj == 0)
        return 0;

    OBJECTREF objRef(obj);

    {
        DWORD bits = objRef->GetHeader()->GetBits();

        if (bits & BIT_SBLK_IS_HASH_OR_SYNCBLKINDEX)
        {
            if (bits & BIT_SBLK_IS_HASHCODE)
            {
                // Common case: the object already has a hash code
                return  bits & MASK_HASHCODE;
            }
            else
            {
                // We have a sync block index. This means if we already have a hash code,
                // it is in the sync block, otherwise we generate a new one and store it there
                SyncBlock *psb = objRef->PassiveGetSyncBlock();
                if (psb != NULL)
                {
                    DWORD hashCode = psb->GetHashCode();
                    if (hashCode != 0)
                        return  hashCode;
                }
            }
        }
    }

    FC_INNER_RETURN(INT32, GetHashCodeHelper(objRef));
}
FCIMPLEND

FCIMPL1(INT32, ObjectNative::TryGetHashCode, Object* obj) {

    CONTRACTL
    {
        FCALL_CHECK;
    }
    CONTRACTL_END;

    VALIDATEOBJECT(obj);

    if (obj == 0)
        return 0;

    OBJECTREF objRef(obj);

    {
        DWORD bits = objRef->GetHeader()->GetBits();

        if (bits & BIT_SBLK_IS_HASH_OR_SYNCBLKINDEX)
        {
            if (bits & BIT_SBLK_IS_HASHCODE)
            {
                // Common case: the object already has a hash code
                return  bits & MASK_HASHCODE;
            }
            else
            {
                // We have a sync block index. There may be a hash code stored within the sync block.
                SyncBlock *psb = objRef->PassiveGetSyncBlock();
                if (psb != NULL)
                {
                    return psb->GetHashCode();
                }
            }
        }
    }

    return 0;
}
FCIMPLEND

//
// Compare by ref for normal classes, by value for value types.
//
// <TODO>@todo: it would be nice to customize this method based on the
// defining class rather than doing a runtime check whether it is
// a value type.</TODO>
//

FCIMPL2(FC_BOOL_RET, ObjectNative::Equals, Object *pThisRef, Object *pCompareRef)
{
    CONTRACTL
    {
        FCALL_CHECK;
        INJECT_FAULT(FCThrow(kOutOfMemoryException););
    }
    CONTRACTL_END;

    if (pThisRef == pCompareRef)
        FC_RETURN_BOOL(TRUE);

    // Since we are in FCALL, we must handle NULL specially.
    if (pThisRef == NULL || pCompareRef == NULL)
        FC_RETURN_BOOL(FALSE);

    MethodTable *pThisMT = pThisRef->GetMethodTable();

    // If it's not a value class, don't compare by value
    if (!pThisMT->IsValueType())
        FC_RETURN_BOOL(FALSE);

    // Make sure they are the same type.
    if (pThisMT != pCompareRef->GetMethodTable())
        FC_RETURN_BOOL(FALSE);

    // Compare the contents (size - vtable - sync block index).
    DWORD dwBaseSize = pThisMT->GetBaseSize();
    if(pThisMT == g_pStringClass)
        dwBaseSize -= sizeof(WCHAR);
    BOOL ret = memcmp(
        (void *) (pThisRef+1),
        (void *) (pCompareRef+1),
        dwBaseSize - sizeof(Object) - sizeof(int)) == 0;

    FC_GC_POLL_RET();

    FC_RETURN_BOOL(ret);
}
FCIMPLEND

NOINLINE static Object* GetClassHelper(OBJECTREF objRef)
{
    FC_INNER_PROLOG(ObjectNative::GetClass);
    _ASSERTE(objRef != NULL);
    TypeHandle typeHandle = objRef->GetTypeHandle();
    OBJECTREF refType = NULL;

    HELPER_METHOD_FRAME_BEGIN_RET_ATTRIB_1(Frame::FRAME_ATTR_EXACT_DEPTH|Frame::FRAME_ATTR_CAPTURE_DEPTH_2, refType);

        refType = typeHandle.GetManagedClassObject();

    HELPER_METHOD_FRAME_END();
    FC_INNER_EPILOG();
    return OBJECTREFToObject(refType);
}

// This routine is called by the Object.GetType() routine.   It is a major way to get the System.Type
FCIMPL1(Object*, ObjectNative::GetClass, Object* pThis)
{
    CONTRACTL
    {
        FCALL_CHECK;
        INJECT_FAULT(FCThrow(kOutOfMemoryException););
    }
    CONTRACTL_END;

    OBJECTREF objRef = ObjectToOBJECTREF(pThis);
    if (objRef != NULL)
    {
        MethodTable* pMT = objRef->GetMethodTable();
        OBJECTREF typePtr = pMT->GetManagedClassObjectIfExists();
        if (typePtr != NULL)
        {
            return OBJECTREFToObject(typePtr);
        }
    }
    else
        FCThrow(kNullReferenceException);

    FC_INNER_RETURN(Object*, GetClassHelper(objRef));
}
FCIMPLEND

FCIMPL1(Object*, ObjectNative::AllocateUninitializedClone, Object* pObjUNSAFE)
{
    FCALL_CONTRACT;

    // Delegate error handling to managed side (it will throw NullReferenceException)
    if (pObjUNSAFE == NULL)
        return NULL;

    OBJECTREF refClone  = ObjectToOBJECTREF(pObjUNSAFE);

    HELPER_METHOD_FRAME_BEGIN_RET_1(refClone);

    MethodTable* pMT = refClone->GetMethodTable();

    // assert that String has overloaded the Clone() method
    _ASSERTE(pMT != g_pStringClass);

    if (pMT->IsArray()) {
        refClone = DupArrayForCloning((BASEARRAYREF)refClone);
    } else {
        // We don't need to call the <cinit> because we know
        //  that it has been called....(It was called before this was created)
        refClone = AllocateObject(pMT);
    }

    HELPER_METHOD_FRAME_END();

    return OBJECTREFToObject(refClone);
}
FCIMPLEND

extern "C" BOOL QCALLTYPE Monitor_Wait(QCall::ObjectHandleOnStack pThis, INT32 Timeout)
{
    QCALL_CONTRACT;

    BOOL retVal = FALSE;

    BEGIN_QCALL;

    GCX_COOP();

     // Arguments validated on managed side
    _ASSERTE(pThis.Get() != NULL);
    _ASSERTE(Timeout >= INFINITE_TIMEOUT);

    retVal = pThis.Get()->Wait(Timeout);

    END_QCALL;

    return retVal;
}

extern "C" void QCALLTYPE Monitor_Pulse(QCall::ObjectHandleOnStack pThis)
{
    QCALL_CONTRACT;

    BEGIN_QCALL;

    GCX_COOP();

    _ASSERTE(pThis.Get() != NULL);
    pThis.Get()->Pulse();

    END_QCALL;
}

extern "C" void QCALLTYPE Monitor_PulseAll(QCall::ObjectHandleOnStack pThis)
{
    QCALL_CONTRACT;

    BEGIN_QCALL;

    GCX_COOP();

    _ASSERTE(pThis.Get() != NULL);
    pThis.Get()->PulseAll();

    END_QCALL;
}

FCIMPL1(FC_BOOL_RET, ObjectNative::IsLockHeld, Object* pThisUNSAFE)
{
    FCALL_CONTRACT;

    BOOL retVal;
    DWORD owningThreadId;
    DWORD acquisitionCount;

    //
    // If the lock is held, check if it's held by the current thread.
    //
    retVal = pThisUNSAFE->GetThreadOwningMonitorLock(&owningThreadId, &acquisitionCount);
    if (retVal)
        retVal = GetThread()->GetThreadId() == owningThreadId;

    FC_RETURN_BOOL(retVal);
}
FCIMPLEND

extern "C" INT64 QCALLTYPE Monitor_GetLockContentionCount()
{
    QCALL_CONTRACT;

    INT64 result = 0;

    BEGIN_QCALL;

    result = (INT64)Thread::GetTotalMonitorLockContentionCount();

    END_QCALL;
    return result;
}
