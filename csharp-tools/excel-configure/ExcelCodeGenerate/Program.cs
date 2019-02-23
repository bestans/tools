using System;
using Google.Protobuf;
using Bestan.Common.Config;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using bestan.config;
using Bestan.Config;

namespace ExcelCodeGenerate
{
    class Program
    {
        static void Main(string[] args)
        {
            Test1();
            Console.ReadKey();
        }
        public static void Test1()
        {
            if (!ConfigManager.LoadConfig("./"))
            {
                return;
            }

            var configs = ConfigManager.GetConfigs(CONFIG_TYPE.TEST_PYTHON_CONFIG);
            foreach (var it in configs)
            {
                Console.WriteLine("key=" + it.Key);
                Console.WriteLine("Value=" + it.Value);
            }
            var data = ConfigManager.GetConfig<test_python_config>(CONFIG_TYPE.TEST_PYTHON_CONFIG, 1);
            Console.WriteLine("data=" + data);
        }
        public static bool GenerateCode()
        {
            string configPath = "generate_code_config.json";
            var config = ReadJsonConfig(configPath);
            if (null == config)
            {
                Console.WriteLine("读取配置失败，config=" + configPath);
                return false;
            }
            FileStream file;
            try
            {
                file = new FileStream(config.protoDataPath, FileMode.Open);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件 " + config.protoDataPath + " 失败:error=" + e.Message);
                return false;
            }
            excel_proto proto = new excel_proto();
            proto.MergeFrom(file);

            var dotnetTemplateStr = ReadCodeTemplate(config.dotnetCodeTemplatePath);
            if (null == dotnetTemplateStr)
            {
                Console.WriteLine("读取模板配置失败，config=" + config.dotnetCodeTemplatePath);
                return false;
            }
            GenerateConfigType(proto, out StringBuilder dotnetCode, out StringBuilder javaCode);
            dotnetTemplateStr = dotnetTemplateStr.Replace("CONFIG_TYPE_REPLACE_STRING,", dotnetCode.ToString());
            dotnetTemplateStr = dotnetTemplateStr.Replace("MD5_REPLACE_STRING", proto.Md5);
            if (!WriteGenerateCode(config.dotnetCodePath, dotnetTemplateStr))
            {
                return false;
            }
            return true;
        }

        public static void GenerateConfigType(excel_proto proto, out StringBuilder dotnetCode, out StringBuilder javaCode)
        {   
            dotnetCode = new StringBuilder();
            javaCode = new StringBuilder();
            foreach (var it in proto.AllProto)
            {
                dotnetCode.Append(it.Key.ToUpper()).Append(",\r\n");
            }
        }

        public static bool WriteGenerateCode(string path, string content)
        {
            FileStream file;
            try
            {
                file = new FileStream(path, FileMode.Create);
            }
            catch (Exception e)
            {
                Console.WriteLine("打开文件 " + path + " 失败:error=" + e.Message);
                return false;
            }
            var data = System.Text.Encoding.UTF8.GetBytes(content);
            file.Write(data, 0, data.Length);
            file.Close();
            return true;
        }

        public static string ReadCodeTemplate(string path)
        {
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
            var data = new byte[file.Length];
            file.Read(data, 0, data.Length);
            file.Close();
            return System.Text.Encoding.UTF8.GetString(data);
        }
        public static GenerateCodeConfig ReadJsonConfig(string path)
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
            return JsonConvert.DeserializeObject<GenerateCodeConfig>(data);
        }
    }
}
