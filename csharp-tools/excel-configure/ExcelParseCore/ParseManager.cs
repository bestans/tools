using System;
using System.Collections.Generic;
using Google.Protobuf;
using Bestan.Common.Config;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Aspose.Cells;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace bestan.common.excelparse
{
    public enum DATA_TYPE
    {
        INVALID,
        INT,
        LONG,
        STRING,
        STRUCT,
        FLOAT,
        DOUBLE,
    };

    class DataTypeKey
    {
        public string typeName;
        public int index = 0;
        public bool isArray = false;
        public DATA_TYPE dataType = DATA_TYPE.INVALID;

        public DataTypeKey(string typeName, int indexP, bool isArrayArg, DATA_TYPE dataTypeArg)
        {
            this.typeName = typeName;
            index = indexP;
            isArray = isArrayArg;
            dataType = dataTypeArg;
        }

        public override int GetHashCode()
        {
            return typeName.GetHashCode() + index.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = (DataTypeKey)obj;
            return typeName == other.typeName && index == other.index;
        }

        public string GetRealSection()
        {
            string s = typeName;
            if (dataType == DATA_TYPE.STRUCT)
            {
                s += "_info";
            }
            if (isArray)
            {
                s += "_list";
            }
            return s;
        }
    };

    class DataTypeDesc
    {
        public List<string> path = new List<string>();
        public bool isArray = false;
        public int arrayIndex = -1;
        public DATA_TYPE dataType = DATA_TYPE.INVALID;

        public Dictionary<DataTypeKey, DataTypeDesc> dataMap = new Dictionary<DataTypeKey, DataTypeDesc>();
    };

    class DataStruct
    {
        public DATA_TYPE dataType = DATA_TYPE.INVALID;
        public bool isArray = false;
        public Dictionary<string, DataStruct> subStructMap = new Dictionary<string, DataStruct>();
        public string desc;
        public int excelLine = -1;
        public string section;

        public DataStruct(DATA_TYPE dataTypeArg)
        {
            dataType = dataTypeArg;
        }
        public DataStruct(DATA_TYPE dataTypeArg, bool isArrayArg)
        {
            dataType = dataTypeArg;
            isArray = isArrayArg;
        }


        public string GetRealSection()
        {
            string s = section;
            if (dataType == DATA_TYPE.STRUCT)
            {
                s += "_info";
            }
            if (isArray)
            {
                s += "_list";
            }
            return s;
        }
    };

    public class Utils
    {
        public static string[] Split(string str, string pattern)
        {
            return str.Split(new string[] { pattern }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string RemoveNumber(string key)
        {
            return Regex.Replace(key, @"\d", "");
        }
    }

    public class PythonScript
    {
        protected ScriptEngine engine;
        protected ScriptScope scope;
        protected ScriptSource script;
        protected string scriptName;

        public PythonScript(string scriptName)
        {
            this.scriptName = scriptName;
        }

        public virtual bool Init()
        {
            try
            {
                var cfg = ParseManager.GetInstance().config;
                engine = Python.CreateEngine();
                var paths = engine.GetSearchPaths();
                //paths.Add(@"D:\Program Files (x86)\IronPython 2.7\Lib");
                string pythonPath = ParseManager.GetInstance().GetPythonPath();
                paths.Add(pythonPath + "Lib");
                paths.Add(pythonPath);
                engine.SetSearchPaths(paths);
                scope = engine.CreateScope();
                script = engine.CreateScriptSourceFromFile(pythonPath + scriptName);
                script.Execute(scope);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("PythonScript error:message=" + e.Message);
            }
            return true;
        }
    }

    public class ExcelScript : PythonScript
    {
        Func<object, object, object> importPathFunc;
        Func<object, object, object> addLineDataFunc;
        Func<object> outputFunc;
        Func<object> testFunc;

        bool init = false;

        public ExcelScript(string scriptName) : base(scriptName)
        {
        }

        public override bool Init()
        {
            if (init)
            {
                return true;
            }
            init = true;
            if (!base.Init())
                return false;

            importPathFunc = scope.GetVariable<Func<object, object, object>>("ImportPathData");
            addLineDataFunc = scope.GetVariable<Func<object, object, object>>("MakeLineData");
            outputFunc = scope.GetVariable<Func<object>>("OutputData");
            testFunc = scope.GetVariable<Func<object>>("Test");
            return true;
        }

        public bool ImportPath(IMessage pathData, string msgNameAll)
        {
            try
            {
                importPathFunc(pathData.ToByteString().ToString(Encoding.ASCII), msgNameAll);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ImportPath error:message=" + e.Message);
            }
            return false;
        }

        public bool AddLineData(List<string> descList)
        {
            return AddLineData(descList.ToArray());
        }
        public bool AddLineData(string[] descList)
        {
            try
            {
                if (descList.Length <= 0)
                {
                    Console.WriteLine("AddLineData  failed:desc is null");
                    return false;
                }
                int id = int.Parse(descList[0]);
                addLineDataFunc(id, descList);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("AddLineData failed:message=" + e.Message);
            }
            return false;
        }

        public byte[] OutputData()
        {
            try
            {
                var ret = (IronPython.Runtime.Bytes)outputFunc();
                return ret.ToByteArray();
            }
            catch (Exception e)
            {
                Console.WriteLine("OutputData failed:message=" + e.Message);
            }
            return null;
        }
        public bool Test()
        {
            try
            {
                testFunc();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Test error:message=" + e.Message);
            }
            return false;
        }
    }

    public class ParseManager
    {
        const string NAME_PATTERN = "#";
        const string TYPE_PATTERN = "_ARRAY";
        const string LINE_SIGN = "\n";
        const string PROTO_FILE = "all_config.proto";
        const string PROTO_DATA_FILE = PROTO_FILE + ".dat";

        static Dictionary<DATA_TYPE, excel_section.Types.DATA_TYPE> type2PBEnumType = new Dictionary<DATA_TYPE, excel_section.Types.DATA_TYPE> {
            { DATA_TYPE.INT, excel_section.Types.DATA_TYPE.Int32 },
            { DATA_TYPE.LONG, excel_section.Types.DATA_TYPE.Int64 },
            { DATA_TYPE.STRING, excel_section.Types.DATA_TYPE.String },
            //{ DATA_TYPE.FLOAT, excel_section.Types.DATA_TYPE.Float },
            { DATA_TYPE.DOUBLE, excel_section.Types.DATA_TYPE.Double },
            { DATA_TYPE.STRUCT, excel_section.Types.DATA_TYPE.Int32 },
        };
        static Dictionary<DATA_TYPE, string> type2PBType = new Dictionary<DATA_TYPE, string> {
            { DATA_TYPE.INT, "int32" },
            { DATA_TYPE.LONG, "int64" },
            { DATA_TYPE.STRING, "string" },
            { DATA_TYPE.FLOAT, "float" },
            { DATA_TYPE.DOUBLE, "double" },
        };
        static Dictionary<string, DATA_TYPE> typeMap = new Dictionary<string, DATA_TYPE> {
            { "INT", DATA_TYPE.INT },
            { "LONG", DATA_TYPE.LONG },
            { "STRING", DATA_TYPE.STRING},
            { "ST", DATA_TYPE.STRUCT },
            { "FLOAT", DATA_TYPE.FLOAT },
            { "DOUBLE", DATA_TYPE.DOUBLE },
        };
        
        static ParseManager instance = new ParseManager();

        //数据
        string filePath;
        string msgName;
        Dictionary<string, DataStruct> dataStructMap = new Dictionary<string, DataStruct>();
        Dictionary<DataTypeKey, DataTypeDesc> structMap = new Dictionary<DataTypeKey, DataTypeDesc>();
        List<List<DataTypeKey>> indexPathAll = new List<List<DataTypeKey>>();
        Dictionary<int, DATA_TYPE> index2Type = new Dictionary<int, DATA_TYPE>();
        excel_table table_data = new excel_table();
        ExcelScript excelScript = new ExcelScript(@"PythonParse.py");
        Dictionary<string, excel_path_full> allPaths = new Dictionary<string, excel_path_full>();
        //配置
        public ParseConfig config = new ParseConfig();

        static void Main(string[] args)
        {
            ParseExcel(args);
            //test9();
            //Console.ReadKey();
        }

        public static void test9()
        {
            var t = Type.GetType("Bestan.Common.Config.excel_proto");
            foreach (var field in t.GetMethods())
            {
                Console.WriteLine(field.Name);
            }
            var obj = (excel_proto)Activator.CreateInstance(t);
            t.GetMethod("set_Md5").Invoke(obj, new string[] { "aaa"});
            Console.WriteLine(t.GetMethod("get_Md5").Invoke(obj, null));

            var dataType = Enum.Parse<DATA_TYPE>("INT");
            Console.WriteLine(dataType);
            test_python_all va = new test_python_all();
            foreach (var it in va.Configs)
            {
            }
        }
        public static void test8()
        {
            Console.WriteLine(GetMD5("aaaa"));
        }
        public static void ParseExcel(string[] args)
        {
            if (args.Length >= 1)
            {
                var path = args[0];
                ParseConfig tempConfig = null;
                try
                {

                    tempConfig = ReadConfig(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("读取配置发生错误:path=" + path + ",error=" + e.Message);
                    return;
                }
                if (tempConfig == null)
                {
                    Console.WriteLine("读取配置" + path + "失败");
                    return;
                }
                GetInstance().config = tempConfig;
            }
            var config = GetInstance().config;
            List<string> excels = new List<string>();
            if (args.Length >= 2)
            {
                //指定了excel
                excels.Add(args[1]);
            }
            else if (args.Length >= 1)
            {
                var dir = new DirectoryInfo(config.excelPath);
                foreach (var file in dir.GetFiles())
                {
                    if (file.Extension == ".xlsx" && !config.ignoreFiles.Contains(file.Name))
                    {
                        excels.Add(file.FullName);
                    }
                }
            }
            else
            {
                excels.Add("test_python_config.xlsx");
            }
            //第一步:生成proto数据
            foreach (var fileName in excels)
            {
                if (!ExcelParseManager.ParseExcel(fileName, true))
                {
                    Console.WriteLine("parse excel proto failed: " + fileName);
                    return;
                }
                else
                {
                    Console.WriteLine("parse excel proto sucess: " + fileName);
                }
            }
            //第二步：生成proto文件
            if (!ParseManager.GetInstance().GenerateProto())
            {
                Console.WriteLine("生成proto文件失败");
                return;
            }
            //第三步:解析数据
            foreach (var fileName in excels)
            {
                if (!ExcelParseManager.ParseExcel(fileName, false))
                {
                    Console.WriteLine("parse excel data failed: " + fileName);
                    return;
                }
                else
                {
                    Console.WriteLine("parse excel data sucess: " + fileName);
                }
            }
        }

        public static void Test6()
        {
            var config = ReadConfig("config.json");
            Console.WriteLine("x2=" + config.ignoreFiles);
        }

        public static void Test5()
        {
            string path = "test_python.dat";
            FileStream file;
            try
            {
                file = new FileStream(path, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("GenerateProto failed:打开文件" + path + "失败,error=" + e.Message);
                return;
            }
            if (file.Length <= 0)
            {
                Console.WriteLine("GenerateProto failed:文件" + path + "为空");
                file.Close();
                return;
            }
            test_python_all all = new test_python_all();
            all.MergeFrom(file);
            Console.WriteLine("all=" + all);
        }
        public static void Test4()
        {
            ExcelParseManager.ParseExcel("test_python.xlsx", false);
        }
        public static void Test3()
        {
            var workbook = ExcelParseManager.ReadExcel("test.xlsx");

            var sh = workbook.Worksheets["数据表"];
            for (int row = 0; row <= sh.Cells.MaxDataRow; ++row)
            {
                var ret = ExcelParseManager.GetLineData(sh, row);
                foreach (var it in ret)
                {
                    Console.Write(" " + it);
                }
                Console.WriteLine();
            }
        }

        public static void Test2()
        {
            excel_path_full pathInfo = new excel_path_full();
            excel_path_cell tempCellPath;
            excel_section tempSection;

            //id
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "id";
            tempSection.DataType = excel_section.Types.DATA_TYPE.Int32;
            //count
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "count";
            tempSection.DataType = excel_section.Types.DATA_TYPE.String;
            //value1
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "value";
            tempSection.Index = 1;
            tempSection.DataType = excel_section.Types.DATA_TYPE.Int32;
            //value2
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "value";
            tempSection.Index = 2;
            tempSection.DataType = excel_section.Types.DATA_TYPE.Int32;
            //skill-id
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "skill";
            tempSection.Index = 1;
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "id";
            tempSection.Index = 0;
            tempSection.DataType = excel_section.Types.DATA_TYPE.Double;
            //skill-rate
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "skill";
            tempSection.Index = 1;
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "rate";
            tempSection.Index = 0;
            tempSection.DataType = excel_section.Types.DATA_TYPE.Int32;
            //skill2-id
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "skill";
            tempSection.Index = 2;
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "id";
            tempSection.Index = 0;
            tempSection.DataType = excel_section.Types.DATA_TYPE.Double;
            //skill2-rate
            tempCellPath = new excel_path_cell();
            pathInfo.Paths.Add(tempCellPath);
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "skill";
            tempSection.Index = 2;
            tempSection = new excel_section();
            tempCellPath.CellPath.Add(tempSection);
            tempSection.Section = "rate";
            tempSection.Index = 0;
            tempSection.DataType = excel_section.Types.DATA_TYPE.Int32;

            string[] descList =
            {
                "7","aa6","5","4","3.111","2","10.22","11"
            };

            GetInstance().excelScript.ImportPath(pathInfo, "test_python_all");
            GetInstance().excelScript.AddLineData(descList);

            test_python_all all = new test_python_all();
            all.MergeFrom(GetInstance().excelScript.OutputData());
            Console.WriteLine("all=" + all.ToString());
        }

        public static void Test1()
        {
            GetInstance().excelScript.Test();
        }

        public static ParseManager GetInstance()
        {
            return instance;
        }

        public bool InitPython()
        {
            if (!excelScript.Init())
                return false;
            
            return true;
        }

        public static void ConsoleError(string pre, int column, string error)
        {
            if (column < 0)
            {
                Console.WriteLine(pre + ":" + error);
            }
            else
            {
                string s = ((char)('A' + column)).ToString();
                Console.WriteLine(pre + ":列[" + s + "]:" + error);
            }
        }

        DATA_TYPE GetDataType(string section)
        {
            if (!typeMap.TryGetValue(section, out DATA_TYPE value))
                return DATA_TYPE.INVALID;

            return value;
        }

        public bool BeginMessage(string inputMsgName, string path, List<string> descList, List<string> nameList, List<string> typeList)
        {
            filePath = path;
            msgName = inputMsgName.ToLower();
            dataStructMap.Clear();
            structMap.Clear();
            indexPathAll.Clear();
            index2Type.Clear();
            table_data = new excel_table();

            if (nameList.Count <= 0 || nameList.Count != typeList.Count || nameList.Count != descList.Count)
            {
                ConsoleError(inputMsgName, -1, "类型解析内容为空");
                return false;
            }

            for (int j = 0; j < nameList.Count; ++j)
            {
                var indexPath = new List<DataTypeKey>();
                indexPathAll.Add(indexPath);
                var sectionDesc = descList[j];
                var section = nameList[j];
                var sectionType = typeList[j];
                if (sectionDesc.Length <= 0)
                {
                    ConsoleError(inputMsgName, j, "注释为空");
                    return false;
                }
                if (section.Length <= 0)
                {
                    ConsoleError(inputMsgName, j, "字段名称为空");
                    return false;
                }
                if (sectionType.Length <= 0)
                {
                    ConsoleError(inputMsgName, j, "字段类型为空");
                    return false;
                }

                var curMap = structMap;
                var pDataStructMap = dataStructMap;

                var sectionTypeSplit = Utils.Split(sectionType, "$");
                var indexVec = new List<int>();
                if (sectionTypeSplit.Length == 2)
                {
                    int index = int.Parse(sectionTypeSplit[1]);
                    while (index > 0)
                    {
                        int tempIndex = index % 100;
                        if (tempIndex <= 0)
                        {
                            ConsoleError(inputMsgName, j, "索引错误");
                            return false;
                        }
                        indexVec.Insert(0, tempIndex);
                        index = index / 100;
                    }
                    if (indexVec.Count <= 0)
                    {
                        //索引是空的
                        ConsoleError(inputMsgName, j, "索引是空的");
                        return false;
                    }

                    sectionTypeSplit = Utils.Split(sectionTypeSplit[0], "#");
                }
                else if (sectionTypeSplit.Length != 1)
                {
                    ConsoleError(inputMsgName, j, "字段类型分隔符$数量不正确");
                    return false;
                }
                else
                {
                    sectionTypeSplit = Utils.Split(sectionTypeSplit[0], "#");
                }

                var sectionSplit = Utils.Split(section, "#");
                if (sectionTypeSplit.Length <= 0 || sectionSplit.Length != sectionTypeSplit.Length)
                {
                    //类型同字段数量不匹配
                    ConsoleError(inputMsgName, j, "字段类型与字段名字个数不符");
                    return false;
                }

                int usedIndexNum = 0;
                for (int k = 0; k < sectionTypeSplit.Length; ++k)
                {
                    var tempSection = sectionSplit[k].ToLower();

                    var tempSectionType = sectionTypeSplit[k];
                    var tempSectionTypeSplit = Utils.Split(tempSectionType, "_ARRAY");
                    if (tempSectionTypeSplit.Length != 1)
                    {
                        //类型错误
                        ConsoleError(inputMsgName, j, "字段类型可能包含不正确的ARRAY");
                        return false;
                    }
                    bool isArray = (tempSectionTypeSplit[0].Length != tempSectionType.Length);
                    tempSectionType = tempSectionTypeSplit[0];
                    int tempIndex = 0;
                    if (isArray)
                    {
                        if (usedIndexNum >= indexVec.Count)
                        {
                            //索引数量不正确
                            ConsoleError(inputMsgName, j, "字段类型索引错误，不匹配");
                            return false;
                        }
                        tempIndex = indexVec[usedIndexNum++];
                    }

                    //记下类型
                    var dataType = GetDataType(tempSectionType);
                    if (dataType == DATA_TYPE.INVALID)
                    {
                        //错误的数据类型
                        ConsoleError(inputMsgName, j, "错误的数据类型" + tempSectionType);
                        return false;
                    }
                    if (!pDataStructMap.TryGetValue(tempSection, out DataStruct tempStru))
                    {
                        tempStru = new DataStruct(dataType, isArray);
                        if (k >= sectionTypeSplit.Length - 1)
                        {
                            tempStru.desc = Utils.RemoveNumber(sectionDesc);
                        }
                        tempStru.excelLine = j;
                        tempStru.section = tempSection;
                        pDataStructMap[tempSection] = tempStru;
                        pDataStructMap = tempStru.subStructMap;
                    }
                    else
                    {
                        pDataStructMap = tempStru.subStructMap;
                    }

                    //记下索引
                    var tempDataMap = curMap;
                    var tempKey = new DataTypeKey(tempSection, tempIndex, isArray, dataType);
                    foreach (var tempCheck in tempDataMap)
                    {
                        if (tempCheck.Key.typeName != tempSection)
                        {
                            continue;
                        }
                        if (tempCheck.Key.isArray != isArray)
                        {
                            ConsoleError(inputMsgName, j, "字段相同，但一个是数组另一个不是数组");
                            return false;
                        }
                    }
                    if (tempDataMap.TryGetValue(tempKey, out DataTypeDesc tempDataDesc))
                    {
                        if (k >= sectionTypeSplit.Length - 1)
                        {
                            //已经有重复类型了
                            ConsoleError(inputMsgName, j, "字段类型索引错误，有重复的");
                            return false;
                        }
                    }
                    else
                    {
                        tempDataDesc = new DataTypeDesc();
                        tempDataDesc.dataType = dataType;
                        tempDataDesc.isArray = isArray;
                        tempDataMap.Add(tempKey, tempDataDesc);
                    }
                    curMap = tempDataDesc.dataMap;

                    if (k == sectionTypeSplit.Length - 1)
                    {
                        //最后一个元素
                        if (dataType == DATA_TYPE.STRUCT)
                        {
                            ConsoleError(inputMsgName, j, "没有指定数据类型");
                            return false;
                        }
                        index2Type[j] = dataType;
                    }
                    //记下路径
                    indexPath.Add(tempKey);
                }
            }

            StringBuilder ss = new StringBuilder();
            if (!ParseDataStruct(ss, msgName, dataStructMap, 0))
            {
                return false;
            }
            //写proto文件
            if (!WriteProto(ss.ToString()))
            {
                return false;
            }

            //设置msgName
            table_data.Path = new excel_path_full();
            if (!ParsePath(table_data.Path))
            {
                return false;
            }
            allPaths.Add(msgName, table_data.Path);
            return true;
        }
        
        public bool ImportPath(string msgName)
        {
            this.msgName = msgName;
            if (!allPaths.TryGetValue(msgName, out excel_path_full value))
            {
                ConsoleError(msgName, -1, "找不到对应的excel_path_full数据");
                return false;
            }
            if (!excelScript.ImportPath(value, msgName + "_all"))
            {
                ConsoleError(msgName, -1, "python解析数据路径和配置名称错误");
                return false;
            }
            return true;
        }

        public bool AddLineData(int excelLine, List<string> dataList)
        {
            if (!excelScript.AddLineData(dataList))
            {
                ConsoleExcelLine(msgName, excelLine, -1, "数据解析错误");
                return false;
            }
            return true;
        }

        public bool EndMessage()
        {
            var data = excelScript.OutputData();
            if (data == null)
            {
                ConsoleError(msgName, -1, "dat数据为空");
                return false;
            }
            string fileName = config.excelDataPath + msgName + ".dat";
            FileStream file;
            try
            {
                file = new FileStream(fileName, FileMode.Create);
            }
            catch (Exception e)
            {
                ConsoleError(msgName, -1, "打开文件" + fileName + "失败:error=" + e.Message);
                return false;
            }
            file.Write(data, 0, data.Length);
            file.Close();
            return true;
        }

        //生成proto文件
        public bool GenerateProto()
        {
            string path = config.protoDataPath + PROTO_DATA_FILE;
            FileStream file;
            try
            {
                file = new FileStream(path, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("GenerateProto failed:打开文件" + path + "失败,error=" + e.Message);
                return false;
            }
            if (file.Length <= 0)
            {
                Console.WriteLine("GenerateProto failed:文件" + path + "为空");
                file.Close();
                return false;
            }

            excel_proto proto = new excel_proto();

            var data = new byte[file.Length];
            file.Read(data, 0, data.Length);
            file.Close();

            try
            {
                proto.MergeFrom(data);
            }
            catch (Exception e)
            {
                Console.WriteLine("GenerateProto failed:解析文件" + path + "失败，error=", e.Message);
                return false;
            }

            StringBuilder ss = new StringBuilder();
            ss.Append("syntax = \"proto3\";").Append(LINE_SIGN)
                .Append("package bestan.config;").Append(LINE_SIGN);
            foreach (var it in proto.AllProto)
            {
                ss.Append(it.Value.ToStringUtf8());

                ss.Append(LINE_SIGN).Append("message " + it.Key + "_all").Append(LINE_SIGN)
                    .Append("{").Append(LINE_SIGN)
                    .Append("\tmap<int32, ").Append(it.Key).Append("> configs = 1;").Append(LINE_SIGN)
                    .Append("}").Append(LINE_SIGN);
            }

            var proto_data = ReadProto();
            if (proto_data == null)
            {
                return false;
            }
            proto_data.Md5 = GetMD5(ss.ToString());
            ss.Append(LINE_SIGN).Append("//MD5=").Append(proto_data.Md5).Append(LINE_SIGN);
            if (!WriteProto(proto_data))
            {
                return false;
            }
            string protoFile = config.protoPath + PROTO_FILE;
            try
            {
                file = new FileStream(protoFile, FileMode.Create);
            }
            catch (Exception e)
            {
                Console.WriteLine("GenerateProto failed:打开文件" + protoFile + "失败，error=" + e.Message);
                return false;
            }
            var outData = System.Text.Encoding.Default.GetBytes(ss.ToString());
            file.Write(outData, 0, outData.Length);
            file.Close();

            if (!RunProcess())
            {
                Console.WriteLine("使用protoc生成python用的pb文件失败:proto=" + config.protoPath + PROTO_FILE);
                return false;
            }
            if (!InitPython())
            {
                Console.WriteLine("InitPython 执行失败");
                return false;
            }
            return true;
        }
        void ConsoleExcelLine(string excelName, int row, int col, string error)
        {
            Console.WriteLine(excelName + "中单元格(" + GetExcelPos(row, col) + "):" + error);
        }

        string GetExcelPos(int row, int col)
        {
            string ret = "";
            if (col >= 0)
            {
                ret += "第" + ((char)('A' + col)).ToString() + "列 ";
            }
            if (row >= 0)
            {
                ret += "第" + row + "行";
            }
            return ret;
        }
        static string GetExcelColumn(int col)
        {
            return ((char)('A' + col)).ToString();
        }
        bool ParsePath(excel_path_full path)
        {
            int column = 0;
            foreach (var it in indexPathAll)
            {
                var cell_path = new excel_path_cell();
                path.Paths.Add(cell_path);
                foreach (var sectionIt in it)
                {
                    excel_section section_path = new excel_section();
                    cell_path.CellPath.Add(section_path);
                    section_path.Index = sectionIt.index;
                    section_path.Section = sectionIt.GetRealSection();

                    if (!type2PBEnumType.TryGetValue(sectionIt.dataType, out excel_section.Types.DATA_TYPE pbType)) {
                        ConsoleError(msgName, -1, "不支持的数据类型,列[" + GetExcelColumn(column) + "],dataType=" + sectionIt.dataType.ToString());
                        return false;
                    }
                    section_path.DataType = pbType;
                }
                column++;
            }
            return true;
        }

        void WriteTab(StringBuilder ss, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                ss.Append("\t");
            }
        }

        bool ParseDataStruct(StringBuilder ss, string msgName, Dictionary<string, DataStruct> struMap, int level)
        {
            if (level == 0)
            {
                ss.Append(LINE_SIGN);
            }
            WriteTab(ss, level);
            ss.Append("message " + msgName + LINE_SIGN);

            WriteTab(ss, level);
            ss.Append("{").Append(LINE_SIGN);

            foreach (var it in struMap)
            {
                var stru = it.Value;
                if (stru.dataType != DATA_TYPE.STRUCT)
                {
                    if (!type2PBType.TryGetValue(stru.dataType, out string tempValue))
                    {
                        ConsoleError(msgName, stru.excelLine, "存在错误的数据类型");
                        return false;
                    }
                    continue;
                }

                //解析结构体
                ParseDataStruct(ss, it.Key, stru.subStructMap, level + 1);
            }

            int pb_index = 0;
            foreach (var it in struMap)
            {
                var stru = it.Value;
                WriteTab(ss, level);
                ss.Append("\t");
                string sectionName = stru.GetRealSection();
                string sectionType = it.Key;
                if (stru.dataType != DATA_TYPE.STRUCT)
                {
                    if (!type2PBType.TryGetValue(stru.dataType, out string tempValue))
                    {
                        ConsoleError(msgName, stru.excelLine, "存在错误的数据类型type=" + stru.dataType.ToString());
                        return false;
                    }

                    sectionType = tempValue;
                }

                if (stru.isArray)
                {
                    ss.Append("repeated ");
                }
                ss.Append(sectionType).Append(" ").Append(sectionName).Append(" = ").Append(++pb_index).Append(";");
                if (stru.desc != null && stru.desc.Length > 0)
                {
                    ss.Append(" //").Append(stru.desc);
                }
                ss.Append(LINE_SIGN);
            }

            WriteTab(ss, level);
            ss.Append("}").Append(LINE_SIGN);
            return true;
        }

        excel_proto ReadProto()
        {
            excel_proto proto_data = new excel_proto();
            string protoDataPath = config.protoDataPath + PROTO_DATA_FILE;
            FileStream file;
            try
            {
                file = new FileStream(protoDataPath, FileMode.OpenOrCreate);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件 " + protoDataPath + " 失败:error=" + e.Message);
                return null;
            }

            if (file.Length > 0)
            {
                try
                {
                    proto_data.MergeFrom(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine("解析文件 " + protoDataPath + " 失败:error=" + e.Message);
                    file.Close();
                    return null;
                }
            }
            file.Close();
            return proto_data;
        }

        bool WriteProto(excel_proto proto_data)
        {
            string protoDataPath = config.protoDataPath + PROTO_DATA_FILE;
            FileStream file;
            try
            {
                file = new FileStream(protoDataPath, FileMode.Create);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件 " + protoDataPath + " 失败:error=" + e.Message);
                return false;
            }

            var data = proto_data.ToByteArray();
            file.Write(data, 0, data.Length);
            file.Close();
            return true;
        }

        bool WriteProto(string proto)
        {
            string protoDataPath = config.protoDataPath + PROTO_DATA_FILE;
            if (proto.Length <= 0)
            {
                ConsoleError(msgName, -1, "生成的proto为空");
                return false;
            }
            var proto_data = ReadProto();
            if (proto_data == null)
            {
                return false;
            }

            if (proto_data.AllProto.TryGetValue(msgName, out ByteString tempValue))
            {
                proto_data.AllProto[msgName] = ByteString.CopyFromUtf8(proto);
            }
            else
            {
                proto_data.AllProto.Add(msgName, ByteString.CopyFromUtf8(proto));
            }
            
            return WriteProto(proto_data);
        }

        public string GetPythonPath()
        {
            return config.rootDir + "python/";
        }
        public bool RunProcess()
        {
            var dir = new DirectoryInfo(config.rootDir);
            if (dir == null)
            {
                Console.WriteLine("rootDir配置错误:rootDir=" + config.rootDir);
                return false;
            }
            var protoPath = new DirectoryInfo(config.protoPath);
            if (protoPath == null)
            {
                Console.WriteLine("protoPath配置错误:protoPath=" + config.protoPath);
                return false;
            }

            string processName = dir.FullName + "gen_proto.bat";
            string protocPath = dir.FullName + "protoc.exe";
            try
            {
                var exep = Process.Start(processName, protocPath + " " + protoPath.FullName + " " + GetPythonPath());
                exep.WaitForExit();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("RunProcess:"+ processName + " failed:message=" + e.Message);
            }

            return false;
        }

        public static ParseConfig ReadConfig(string path)
        {
            FileStream file;
            try
            {
                file = new FileStream(path, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件" + path + "失败,error=" + e.Message);
                return null;
            }

            var dataBytes = new byte[file.Length];
            file.Read(dataBytes, 0, dataBytes.Length);
            var data = System.Text.Encoding.Default.GetString(dataBytes);
            return JsonConvert.DeserializeObject<ParseConfig>(data);
        }

        public static string GetMD5(string myString)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = System.Text.Encoding.UTF8.GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);
            string byte2String = "";

            for (int i = 0; i < targetData.Length; i++)
            {
                byte2String += targetData[i].ToString("x");
            }

            return byte2String;
        }
    }
}
