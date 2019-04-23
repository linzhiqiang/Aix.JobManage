using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Exceptions
{
    public class BizException : Exception
    {
        public int Code { get; set; }

        public BizException(int code, string message) : base(message)
        {
            this.Code = code;
        }
    }
}
