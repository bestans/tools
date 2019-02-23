using System;
using System.Collections.Generic;
using Google.Protobuf;
using Bestan.Common.Config;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using IronPython.Hosting;

namespace bestan.common.excelparse
{
    public enum DATA_TYPE {
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

    public class ParseManager
    {
        const string NAME_PATTERN = "#";
        const string TYPE_PATTERN = "_ARRAY";
        const string LINE_SIGN = "\n";
        const string PROTO_DATA_FILE = "all.proto.dat";

        static Dictionary<DATA_TYPE, string> type2PBType = new Dictionary<DATA_TYPE, string> {
            { DATA_TYPE.INT, "int32" },
            { DATA_TYPE.LONG, "int64" },
            { DATA_TYPE.STRING, "bytes" },
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
        string msgName;
        Dictionary<string, DataStruct> dataStructMap = new Dictionary<string, DataStruct>();
        Dictionary<DataTypeKey, DataTypeDesc> structMap = new Dictionary<DataTypeKey, DataTypeDesc>();
        List<List<DataTypeKey>> indexPathAll = new List<List<DataTypeKey>>();
        Dictionary<int, DATA_TYPE> index2Type = new Dictionary<int, DATA_TYPE>();
        excel_table table_data = new excel_table();

        public static ParseManager GetInstance()
        {
            return instance;
        }

        public static void ConsoleError(string pre, int column, string error)
        {
            if (column >= 0)
            {
                Console.WriteLine(pre + ":" + error);
            }
            else
            {
                string s = ((char)('A' + column)).ToString();
                Console.WriteLine(pre + "column=" + s + ":" + error);
            }
        }

        DATA_TYPE GetDataType(string section)
        {
            if (!typeMap.TryGetValue(section, out DATA_TYPE value))
                return DATA_TYPE.INVALID;

            return value;
        }
        
        public bool BeginMessage(string inputMsgName, List<string> descList, List<string> nameList, List<string> typeList)
        {
            msgName = inputMsgName.ToLower();
            Console.WriteLine("msgName=" + msgName);
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
                    ConsoleError(inputMsgName, j, "字段类型分隔符$有多个");
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
                        ConsoleError(inputMsgName, j, "错误的数据类型");
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
            table_data.ProtoMsgName = ByteString.CopyFromUtf8(msgName);
            table_data.Path = new excel_path_full();
            ParsePath(table_data.Path);

            return true;
        }
       
        public bool AddLineData(int excelLine, List<string> dataList)
	    {
            if (dataList.Count <= 0)
            {
                Console.WriteLine(msgName + "数据列为空");
                return false;
            }
            if (!int.TryParse(dataList[0], out int index))
            {
                ConsoleExcelLine(msgName, excelLine, 0, "索引解析错误");
                return false;
            }
            if (table_data.Table.TryGetValue(index, out excel_line_data line_data))
            {
                ConsoleExcelLine(msgName, excelLine, -1, "存在重复的索引:index=" + index);
                return false;
            }

            for (int i = 0; i < dataList.Count; ++i)
            {
                if (!index2Type.TryGetValue(i, out DATA_TYPE dataType))
                {
                    ConsoleExcelLine(msgName, excelLine, i, "找不到数据类型:index=" + index);
                    return false;
                }
                var cell_data = new excel_cell_data();
                line_data.CellData.Add(cell_data);
                var dataDesc = dataList[i];

                try
                {
                    switch (dataType)
                    {
                        case DATA_TYPE.INT:
                            {
                                cell_data.DataType = excel_cell_data.Types.DATA_TYPE.Int32;
                                cell_data.Int32Value = int.Parse(dataDesc);
                            }
                            break;
                        case DATA_TYPE.LONG:
                            {
                                cell_data.DataType = excel_cell_data.Types.DATA_TYPE.Int64;
                                cell_data.Int64Value = long.Parse(dataDesc);
                            }
                            break;
                        case DATA_TYPE.STRING:
                            {
                                cell_data.DataType = excel_cell_data.Types.DATA_TYPE.String;
                                cell_data.BytesValue = ByteString.CopyFromUtf8(dataDesc);
                            }
                            break;
                        case DATA_TYPE.FLOAT:
                            {
                                cell_data.DataType = excel_cell_data.Types.DATA_TYPE.Float;
                                cell_data.FloatValue = float.Parse(dataDesc);
                            }
                            break;
                        case DATA_TYPE.DOUBLE:
                            {
                                cell_data.DataType = excel_cell_data.Types.DATA_TYPE.Double;
                                cell_data.DoubleValue = double.Parse(dataDesc);
                            }
                            break;

                        default:
                            ConsoleExcelLine(msgName, excelLine, i, "错误的数据类型:type=" + dataType);
                            return false;
                    }
                }
                catch (Exception e)
                {
                    ConsoleExcelLine(msgName, excelLine, i, "解析数据出错:error=" + e.Message);
                    return false;
                }
            }
            
            return true;
	    }

        public bool EndMessage()
        {
            var data = table_data.ToByteArray();
            if (data.Length <= 0)
            {
                ConsoleError(msgName, -1, "dat数据为空");
                return false;
            }
            string fileName = msgName + ".dat";
            FileStream file;
            try
            {
                file = new FileStream(fileName, FileMode.Truncate);
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
        public static bool GenerateProto(string dir)
        {
            string path = dir + PROTO_DATA_FILE;
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
                .Append("package bestan.common.config;").Append(LINE_SIGN);
            foreach (var it in proto.AllProto)
            {
                ss.Append(it.Value.ToStringUtf8());
            }
            string protoFile = dir + "all.proto";
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
                ret += ((char)('A' + col)).ToString();
            }
            if (row >= 0)
            {
                ret += row;
            }
            return ret;
        }
        bool ParsePath(excel_path_full path)
        {
            foreach (var it in indexPathAll)
            {
                var cell_path = new excel_path_cell();
                path.Paths.Add(cell_path);
                foreach (var sectionIt in it)
                {
                    excel_section section_path = new excel_section();
                    cell_path.CellPath.Add(section_path);
                    section_path.Index = sectionIt.index;
                    section_path.Section = ByteString.CopyFromUtf8(sectionIt.GetRealSection());
                }
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

        bool WriteProto(string proto)
        {
            if (proto.Length <= 0)
            {
                ConsoleError(msgName, -1, "生成的proto为空");
                return false;
            }
            FileStream file;
            try
            {
                file = new FileStream(PROTO_DATA_FILE, FileMode.OpenOrCreate);
            } catch (Exception e)
            {
                ConsoleError(msgName, -1, "打开文件" + PROTO_DATA_FILE + "失败:error=" + e.ToString());
                return false;
            }
            
            excel_proto proto_data = new excel_proto();
            Console.WriteLine("read proto size=" + file.Length);
            if (file.Length > 0)
            {
                try
                {
                    proto_data.MergeFrom(file);
                }
                catch (Exception e)
                {
                    ConsoleError(msgName, -1, "解析文件" + PROTO_DATA_FILE + "失败:error=" + e.Message);
                    file.Close();
                    return false;
                }
            }
            file.Close();

            file = new FileStream(PROTO_DATA_FILE, FileMode.Truncate);

            if (proto_data.AllProto.TryGetValue(msgName, out ByteString tempValue))
            {
                proto_data.AllProto[msgName] = ByteString.CopyFromUtf8(proto);
            }
            else
            {
                proto_data.AllProto.Add(msgName, ByteString.CopyFromUtf8(proto));
            }
            
            var data = proto_data.ToByteArray();
            file.Write(data, 0, data.Length);
            file.Close();
            return true;
        }
    }
}
