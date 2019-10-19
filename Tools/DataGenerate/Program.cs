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
            try
            {
                Run(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed. ErrorInfo=" + e.Message);
            }
            Console.ReadKey();
        }

        static void Run(string[] args)
        {
            TableLuaDefine.Instance.Init();
            TableLuaDefine.Instance.ToolGenerateExcel();
            TableLuaDefine.Instance.ToolGenerateProto();
        }
    }
}
