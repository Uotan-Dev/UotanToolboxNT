using System;
using System.Collections.Generic;
using System.Text;

namespace WuXingLibrary.code.module
{
    public enum ResponseErrorType
    {
        None,               // 无错误
        AuthenticationError, // 需要验证
        OtherError          // 其他错误
    }
}