using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Bestan.Common.Config;
using System.IO;
using Google.Protobuf.Collections;
using System.Collections;

namespace bestan.config
{
    enum CONFIG_TYPE
    {
        TEST_PYTHON_CONFIG,

    }
    
    class ConfigManager
    {
        const string PROTO_MD5 = "43fb77f624a0e43d142a149b66eda32";

        static Dictionary<CONFIG_TYPE, Dictionary<int, IMessage>> allConfigs = new Dictionary<CONFIG_TYPE, Dictionary<int, IMessage>>();

        public static bool LoadConfig(string rootPath)
        {
            var proto = ReadProto(rootPath + "all_config.proto.dat");
            if (proto == null)
            {
                return false;
            }
            if (proto.Md5 != PROTO_MD5)
            {
                Console.WriteLine("proto md5 校验错误，需要重新完整生成配置");
                return false;
            }
            foreach (var it in proto.AllProto)
            {
                if (!LoadOneConfig(rootPath, it.Key))
                {
                    return false;
                }
            }
            
            return true;
        }

        static bool LoadOneConfig(string rootPath, string proto_name)
        {
            string dataPath = rootPath + proto_name + ".dat";
            FileStream file;
            try
            {
                file = new FileStream(dataPath, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件 " + dataPath + " 失败:error=" + e.Message);
                return false;
            }
            var objType = Type.GetType("Bestan.Config." + proto_name + "_all");
            if (null == objType)
            {
                Console.WriteLine("找不到config类型：proto_msg=" + proto_name);
                return false;
            }
            var obj = (IMessage)Activator.CreateInstance(objType);
            obj.MergeFrom(file);
            var srcData = (IDictionary)objType.GetMethod("get_Configs").Invoke(obj, null);
            var outData = new Dictionary<int, IMessage>();
            foreach (var it in srcData)
            {
                var entry = (DictionaryEntry)it;
                outData.Add((int)entry.Key, (IMessage)entry.Value);
            }
            var dataType = Enum.Parse<CONFIG_TYPE>(proto_name.ToUpper());
            allConfigs.Add(dataType, outData);
            return true;
        }

        static excel_proto ReadProto(string path)
        {
            excel_proto proto_data = new excel_proto();
            FileStream file;
            try
            {
                file = new FileStream(path, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件 " + path + " 失败:error=" + e.Message);
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
                    Console.WriteLine("解析文件 " + path + " 失败:error=" + e.Message);
                    file.Close();
                    return null;
                }
            }
            file.Close();
            return proto_data;
        }

        public static T GetConfig<T>(CONFIG_TYPE configType, int index) where T : class, IMessage
        {
            if (!allConfigs.TryGetValue(configType, out Dictionary<int, IMessage> configs))
            {
                return null;
            }
            if (!configs.TryGetValue(index, out IMessage config))
            {
                return null;
            }

            return (T)config;
        }

        public static Dictionary<int, IMessage> GetConfigs(CONFIG_TYPE configType)
        {
            if (!allConfigs.TryGetValue(configType, out Dictionary<int, IMessage> configs))
            {
                return null;
            }

            return configs;
        }
    }
}
