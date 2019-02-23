using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Bestan.Common.Config;
using Google.Protobuf;
using bestan.common.excelparse;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace test_frame
{
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct data
    { 
        /// char[10]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 10)]
        public string str;
    }

    class PIStringArray
    {
        public int dataSize = 0;
        public int[] dataUnit = null;
        public byte[] data = null;
    };
    
    class Program
    {
        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "test", CallingConvention = CallingConvention.Cdecl)]
        public static extern int test();
        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "test2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int test2(int size, ref int[] sizeList, ref string[] arr);

        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "test3", CallingConvention = CallingConvention.Cdecl)]
        public static extern int test3(int size, ref data d);
        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "test4", CallingConvention = CallingConvention.Cdecl)]
        public static extern int test4(int size, int[] unitSize, byte[] d);
        
        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "BeginMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool BeginMessage(int msgSize, byte[] msgName, int dataSize, int[] dataUnit, byte[] data);
        
        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "AddLineData", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool AddLineData(int line, int dataSize, int[] dataUnit, byte[] data);

        [DllImport(@"E:\tools\cpp-tools\Debug\protobuf-desc.dll", EntryPoint = "EndMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EndMessage();

        static void Main(string[] args)
        {
            test6();
            Console.ReadKey();
        }


        public static void test9()
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                paths.Add(@"D:\Program Files (x86)\IronPython 2.7\Lib");
                engine.SetSearchPaths(paths);
                ScriptScope scope = engine.CreateScope();

                ScriptSource script = engine.CreateScriptSourceFromFile(@"PythonParse.py");
                var result = script.Execute(scope);

                var func = scope.GetVariable<Func<object>>("Test");
                func();
            }
            catch (Exception e)
            {
                Console.WriteLine("aaaa");
                Console.WriteLine(e.Message);
            }
        }

        public static void test8()
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                //paths.Add(@"D:\Program Files (x86)\IronPython 2.7\Lib");
                engine.SetSearchPaths(paths);
                ScriptScope scope = engine.CreateScope();

                ScriptSource script = engine.CreateScriptSourceFromFile(@"main5.py");
                var result = script.Execute(scope);

//                 excel_cell_data cell_Data = new excel_cell_data();
//                 cell_Data.Int32Value = 200;
//                 cell_Data.Int64Value = 300;
// 
//                 List<string> list = new List<string>();
//                 list.Add("1111");
//                 list.Add("2222");
//                 string[] list2 = { "aaaa", "bbbb" };
//                 var func = scope.GetVariable<Func<object, object>>("Test1");
//                 func(list2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void test7()
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                paths.Add(@"D:\Program Files (x86)\IronPython 2.7\Lib");
                engine.SetSearchPaths(paths);
                ScriptScope scope = engine.CreateScope();

                ScriptSource script = engine.CreateScriptSourceFromFile(@"main3.py");
                var result = script.Execute(scope);

                excel_cell_data cell_Data = new excel_cell_data();

                List<string> list = new List<string>();
                list.Add("1111");
                list.Add("2222");
                string[] list2 = { "aaaa", "bbbb" };
                var func = scope.GetVariable<Func<object>>("Test3");
                var retFunc = func();
                var ttt = (IronPython.Runtime.Bytes)retFunc;
                var retData = ttt.ToByteArray();
                cell_Data.MergeFrom(retData);
                Console.WriteLine(cell_Data.Int32Value);
                Console.WriteLine(cell_Data.Int64Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void test6()
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                paths.Add(@"D:\Program Files (x86)\IronPython 2.7\Lib");
                engine.SetSearchPaths(paths);
                ScriptScope scope = engine.CreateScope();

                ScriptSource script = engine.CreateScriptSourceFromFile(@"main3.py");
                var result = script.Execute(scope);

                excel_cell_data cell_Data = new excel_cell_data();
                cell_Data.Int32Value = 200;
                cell_Data.Int64Value = 300;

                List<string> list = new List<string>();
                list.Add("1111");
                list.Add("2222");
                string[] list2 = { "aaaa", "bbbb" };
                var func = scope.GetVariable<Func<object, object>>("Test2");
                IronPython.Runtime.Bytes data = new IronPython.Runtime.Bytes(cell_Data.ToByteArray());
                func(data);


                var func2 = scope.GetVariable<Func<object>>("Test3");
                func2();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void test5()
        {
            try
            {
                ScriptEngine engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                paths.Add(@"D:\Program Files (x86)\IronPython 2.7\Lib");
                engine.SetSearchPaths(paths);
                ScriptScope scope = engine.CreateScope();

                ScriptSource script = engine.CreateScriptSourceFromFile(@"main.py");
                var result = script.Execute(scope);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void test4()
        {
            string[] descList = {
                "索引",
                "名字",
                "技能1id",
                "技能1概率",
                "技能2id",
                "技能2概率",
                "声望概率",
                "声望id",
            };
            string[] nameList =
            {
                "id",
                "name",
                "skill#id",
                "skill#rate",
                "skill#id",
                "skill#rate",
                "repu#rate",
                "repu#id",
            };
            string[] typeList = {
                "INT",
                "STRING",
                "ST_ARRAY#INT$1",
                "ST_ARRAY#INT$1",
                "ST_ARRAY#INT$2",
                "ST_ARRAY#INT$2",
                "ST#FLOAT",
                "ST#INT",
            };
            ParseManager.GetInstance().BeginMessage("test", descList.ToList(), nameList.ToList(), typeList.ToList());
            ParseManager.GenerateProto("");
        }

        public static void test3()
        {
            FileStream file = new FileStream("all.proto.dat", FileMode.Open);
            Console.WriteLine(file.Length);
            excel_proto proto = new excel_proto();
            proto.MergeFrom(file);
            Console.WriteLine(proto.ToString());
            foreach(var it in proto.AllProto)
            {
                Console.WriteLine(it.Key);
                Console.WriteLine(it.Value.ToStringUtf8());
            }

            var dataFile = new FileStream("test.dat", FileMode.Open);
            excel_table table = new excel_table();
            table.MergeFrom(dataFile);
            Console.WriteLine(table.ProtoMsgName);
            foreach(var it in table.Table)
            {
                foreach(var cell in it.Value.CellData)
                {
                    Console.WriteLine("int="+cell.Int32Value +",float=" + cell.FloatValue);
                }
            }
            Console.ReadKey();
        }

        static PIStringArray GetPIStringArrayData(string[] input)
        {
            if (input == null || input.Length <= 0)
                return null;

            var ret = new PIStringArray();
            ret.dataSize = (int)input.Length;
            ret.dataUnit = new int[input.Length];

            MemoryStream m = new MemoryStream();
            for (int i = 0; i < input.Length; ++i)
            {
                var tempBytes = System.Text.Encoding.Default.GetBytes(input[i]);
                ret.dataUnit[i] = tempBytes.Length;
                m.Write(tempBytes, 0, tempBytes.Length);
            }
            ret.data = new byte[m.Length];
            m.Seek(0, SeekOrigin.Begin);
            m.Read(ret.data, 0, (int)m.Length);
            m.Close();
            return ret;
        }

        static void test2()
        {
            string fileName = "test.proto";
            var fileNameBytes = System.Text.Encoding.Default.GetBytes(fileName);
            //BeginParse(fileNameBytes.Length, fileNameBytes);
            string[] strDataList = {
                "索引",
                "名字",
                "技能1id",
                "技能1概率",
                "技能2id",
                "技能2概率",
                "声望概率",
                "声望id",
                "id",
                "name",
                "skill#id",
                "skill#rate",
                "skill#id",
                "skill#rate",
                "repu#rate",
                "repu#id",
                "INT",
                "STRING",
                "ST_ARRAY#INT$1",
                "ST_ARRAY#INT$1",
                "ST_ARRAY#INT$2",
                "ST_ARRAY#INT$2",
                "ST#FLOAT",
                "ST#INT",
            };
            int[] dataSizeList = new int[strDataList.Length];
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < strDataList.Length; ++i)
            {
                var tempBytes = System.Text.Encoding.Default.GetBytes(strDataList[i]);
                dataSizeList[i] = tempBytes.Length;
                m.Write(tempBytes, 0, tempBytes.Length);
            }
            byte[] data = new byte[m.Length];
            m.Seek(0, SeekOrigin.Begin);
            m.Read(data, 0, data.Length);

            string strMsgName = "test";
            var msgName = System.Text.Encoding.Default.GetBytes(strMsgName);
            Console.WriteLine("start:" + data.Length);
            BeginMessage(msgName.Length, msgName, strDataList.Length, dataSizeList, data);
            
            string[] dataList = {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7.234",
                    "8",
            };

            var dataStru = GetPIStringArrayData(dataList);
            AddLineData(1, dataStru.dataSize, dataStru.dataUnit, dataStru.data);
            EndMessage();
            Console.ReadKey();
        }

        static void test1()
        { 
            Console.WriteLine("test");
            Console.WriteLine(test());
            //             string[] arr =
            //             {
            //                 "aaaa",
            //                 "bbbb",
            //             };
            //             int[] sizeList = new int[arr.Length];
            //             for (int i = 0; i < sizeList.Length; ++i)
            //             {
            //                 sizeList[i] = arr[i].Length;
            //             }
            //             
            //             Console.WriteLine(test2(arr.Length, ref sizeList, ref arr));
            data d;
            d.str = "技能1id";
            //             Console.WriteLine(System.Text.Encoding.Default.GetBytes(d.str).Length);
            //             test3(4, ref d);
            var strBytes = System.Text.Encoding.Default.GetBytes(d.str);
            int[] unitSize = new int[strBytes.Length];
            int ii = 10;
            for (int i = 0; i < unitSize.Length; ++i)
            {
                unitSize[i] = ++ii;
            }
            test4(strBytes.Length, unitSize, strBytes);
            Console.ReadKey();
        }
    }
}
