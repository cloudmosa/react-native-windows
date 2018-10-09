#include "pch.h"
#include "NativeJavaScriptExecutor.h"

#ifndef _WINRT_DLL
#include <vcclr.h>
#endif

const wchar_t* BATCH_BRIDGE = L"__fbBatchedBridge";

using namespace ChakraBridge;

int NativeJavaScriptExecutor::InitializeHost()
{
    this->host = new ChakraHost();
    return this->host->Init();
}

int NativeJavaScriptExecutor::DisposeHost()
{
    int ret = this->host->Destroy();
    delete this->host;
    this->host = nullptr;
    return ret;
}

int NativeJavaScriptExecutor::SetGlobalVariable(String^ variableName, String^ stringifiedText)
{
    JsValueRef valueStringified;
    IfFailRet(JsPointerToString(StringChars(stringifiedText), StringLength(stringifiedText), &valueStringified));

    JsValueRef valueJson;
    IfFailRet(this->host->JsonParse(valueStringified, &valueJson));
    IfFailRet(this->host->SetGlobalVariable(StringChars(variableName), valueJson));

    return JsNoError;
}

ChakraStringResult NativeJavaScriptExecutor::GetGlobalVariable(String^ variableName)
{
    JsValueRef globalVariable;

    IfFailRetNullPtr(this->host->GetGlobalVariable(StringChars(variableName), &globalVariable));

    JsValueRef globalVariableJson;
    IfFailRetNullPtr(this->host->JsonStringify(globalVariable, &globalVariableJson));

    const wchar_t* szBuf;
    size_t bufLen;
    IfFailRetNullPtr(JsStringToPointer(globalVariableJson, &szBuf, &bufLen));

    ChakraStringResult finalResult = { JsNoError, CreateString(szBuf, bufLen) };
    return finalResult;
}

int NativeJavaScriptExecutor::RunScript(String^ source, String^ sourceUri)
{
    JsValueRef result;
    IfFailRet(this->host->RunScript(StringChars(source), StringChars(sourceUri), &result));
    return JsNoError;
}

int NativeJavaScriptExecutor::SerializeScript(String^ source, String^ serialized)
{
    IfFailRet(this->host->SerializeScript(StringChars(source), StringChars(serialized)));
    return JsNoError;
}

int NativeJavaScriptExecutor::RunSerializedScript(String^ source, String^ serialized, String^ sourceUri)
{
    JsValueRef result;
    IfFailRet(this->host->RunSerializedScript(StringChars(source), StringChars(serialized), StringChars(sourceUri), &result));
    return JsNoError;
}

ChakraStringResult NativeJavaScriptExecutor::CallFunctionAndReturnFlushedQueue(String^ moduleName, String^ methodName, String^ args)
{
    JsPropertyIdRef fbBridgeId;
    IfFailRetNullPtr(JsGetPropertyIdFromName(BATCH_BRIDGE, &fbBridgeId));

    JsValueRef fbBridgeObj;
    IfFailRetNullPtr(JsGetProperty(host->globalObject, fbBridgeId, &fbBridgeObj));

    JsPropertyIdRef methodId;
    IfFailRetNullPtr(JsGetPropertyIdFromName(L"callFunctionReturnFlushedQueue", &methodId));

    JsValueRef method;
    IfFailRetNullPtr(JsGetProperty(fbBridgeObj, methodId, &method));

    JsValueRef moduleNameRef, methodNameRef;
    IfFailRetNullPtr(JsPointerToString(StringChars(moduleName), StringLength(moduleName), &moduleNameRef));
    IfFailRetNullPtr(JsPointerToString(StringChars(methodName), StringLength(methodName), &methodNameRef));

    JsValueRef argObj;
    IfFailRetNullPtr(JsPointerToString(StringChars(args), StringLength(args), &argObj));

    JsValueRef jsonObj;
    IfFailRetNullPtr(host->JsonParse(argObj, &jsonObj));

    JsValueRef result;
    JsValueRef newArgs[4] = { host->globalObject, moduleNameRef, methodNameRef, jsonObj };
    IfFailRetNullPtr(JsCallFunction(method, newArgs, 4, &result));

    JsValueRef stringifiedResult;
    IfFailRetNullPtr(host->JsonStringify(result, &stringifiedResult));

    const wchar_t* szBuf;
    size_t bufLen;
    IfFailRetNullPtr(JsStringToPointer(stringifiedResult, &szBuf, &bufLen));

    ChakraStringResult finalResult = { JsNoError, CreateString(szBuf, bufLen) };
    return finalResult;
}

ChakraStringResult NativeJavaScriptExecutor::InvokeCallbackAndReturnFlushedQueue(int callbackId, String^ args)
{
    JsPropertyIdRef fbBridgeId;
    IfFailRetNullPtr(JsGetPropertyIdFromName(BATCH_BRIDGE, &fbBridgeId));

    JsValueRef fbBridgeObj;
    IfFailRetNullPtr(JsGetProperty(host->globalObject, fbBridgeId, &fbBridgeObj));

    JsPropertyIdRef methodId;
    IfFailRetNullPtr(JsGetPropertyIdFromName(L"invokeCallbackAndReturnFlushedQueue", &methodId));

    JsValueRef method;
    IfFailRetNullPtr(JsGetProperty(fbBridgeObj, methodId, &method));

    JsValueRef callbackIdRef;
    IfFailRetNullPtr(JsIntToNumber(callbackId, &callbackIdRef));

    JsValueRef argsObj;
    IfFailRetNullPtr(JsPointerToString(StringChars(args), StringLength(args), &argsObj));

    JsValueRef argsJson;
    IfFailRetNullPtr(host->JsonParse(argsObj, &argsJson));

    JsValueRef result;
    JsValueRef newArgs[3] = { host->globalObject, callbackIdRef, argsJson };
    IfFailRetNullPtr(JsCallFunction(method, newArgs, 3, &result));

    JsValueRef stringifiedResult;
    IfFailRetNullPtr(host->JsonStringify(result, &stringifiedResult));

    const wchar_t* szBuf;
    size_t bufLen;
    IfFailRetNullPtr(JsStringToPointer(stringifiedResult, &szBuf, &bufLen));

    ChakraStringResult finalResult = { JsNoError, CreateString(szBuf, bufLen) };
    return finalResult;
}

ChakraStringResult NativeJavaScriptExecutor::FlushedQueue()
{
    JsPropertyIdRef fbBridgeId;
    IfFailRetNullPtr(JsGetPropertyIdFromName(BATCH_BRIDGE, &fbBridgeId));

    JsValueRef fbBridgeObj;
    IfFailRetNullPtr(JsGetProperty(host->globalObject, fbBridgeId, &fbBridgeObj));

    JsPropertyIdRef methodId;
    IfFailRetNullPtr(JsGetPropertyIdFromName(L"flushedQueue", &methodId));

    JsValueRef method;
    IfFailRetNullPtr(JsGetProperty(fbBridgeObj, methodId, &method));

    JsValueRef result;
    JsValueRef newArgs[1] = { host->globalObject };
    IfFailRetNullPtr(JsCallFunction(method, newArgs, 1, &result));

    JsValueRef stringifiedResult;
    IfFailRetNullPtr(host->JsonStringify(result, &stringifiedResult));

    const wchar_t* szBuf;
    size_t bufLen;
    IfFailRetNullPtr(JsStringToPointer(stringifiedResult, &szBuf, &bufLen));

    ChakraStringResult finalResult = { JsNoError, CreateString(szBuf, bufLen) };
    return finalResult;
}

// static inline
size_t NativeJavaScriptExecutor::StringLength(String^ string)
{
#ifdef _WINRT_DLL
    return string->Length();
#else
    return string->Length;
#endif
}

// static inline
const wchar_t* NativeJavaScriptExecutor::StringChars(String^ string)
{
#ifdef _WINRT_DLL
    return string->Data();
#else
    pin_ptr<const wchar_t> wch = PtrToStringChars(string);
    return wch;
#endif
}

// static inline
String^ NativeJavaScriptExecutor::CreateString(const wchar_t* szBuf, size_t length)
{
#ifdef _WINRT_DLL
    return ref new String(szBuf, length);
#else
    return gcnew String(szBuf, 0, length);
#endif
}
