#pragma once

#ifdef _WINRT_DLL
using namespace Platform;
#else
using namespace System;
#endif

namespace ChakraBridge {

/// <summary>
/// A compound result containing the string result and a <see cref="JsErrorCode" /> for the error.
/// </summary>
public value struct ChakraStringResult
{
    /// <summary>The <see cref="JsErrorCode" /> for the operation, JsNoError if no error has occurred.</summary>
    int ErrorCode;
    /// <summary>The string result for the operation.</summary>
    String^ Result;
};

};
