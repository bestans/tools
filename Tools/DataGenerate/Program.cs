using System;
using System.Collections.Generic;
using System.Text;

namespace DataGenerate
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            TableLuaDefine.Instance.Init();
            TableLuaDefine.Instance.ToolGenerateExcel();
        }
    }
}
