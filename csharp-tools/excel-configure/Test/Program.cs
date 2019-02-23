using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;

namespace Test
{
    class Program
    {
        //         [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "test", CallingConvention = CallingConvention.Cdecl)]
        //         public static extern int test();
        //         [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "test2", CallingConvention = CallingConvention.Cdecl)]
        //         public static extern int test2(int size, ref string[] arr);

        enum DATA_TYPE
        {
               INVALID,
        }
        static void Main(string[] args)
        {
            Console.WriteLine(DATA_TYPE.INVALID.GetType());
            Console.ReadKey();
        }

        public static void test2()
        {
            try
            {
                using (var file = new FileStream("testaaa.txt", FileMode.Open))
                {
                    Console.WriteLine("aaaa");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("bbb");
        }

        public static void test1()
        {
            //Utils.test1();
            Console.WriteLine("Hello World!");
            //             Console.WriteLine(test());
            //             string[] arr =
            //             {
            //                 "aaaa",
            //                 "bbbb",
            //             };
            //             Console.WriteLine(test2(arr.Length, ref arr));
            Console.WriteLine("aaa_ARRRAY".Split(new string[] { "_ARRRAY" }, StringSplitOptions.RemoveEmptyEntries).Length);
            Console.WriteLine(((char)('A' + 2)).ToString());
            string key = "发达1123水电费";
            Console.WriteLine(Regex.Replace(key, @"\d", ""));
            Console.WriteLine(DATA_TYPE.INVALID.ToString());
        }
    }
}
