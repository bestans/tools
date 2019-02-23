using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using System.IO;

namespace Bestan.Common.Config
{

    enum CONFIG_TYPE
    {
        TEST,
    };
    class ConfigManager
    {
        static Dictionary<CONFIG_TYPE, Dictionary<int, IMessage>> allConfigs = new Dictionary<CONFIG_TYPE, Dictionary<int, IMessage>>();

        public static bool LoadData(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open);

            excel_table table = new excel_table();

            if (file.Length <= 0)
            {
                Console.WriteLine("ConfigManager:LoadData:文件" + path + "为空");
                return false;
            }
            var data = new byte[file.Length];
            file.Read(data, 0, data.Length);
            table.MergeFrom(data);
            
            //var configType = CONFIG_TYPE.Parse(table.ProtoMsgName.ToStringUtf8().ToUpper());
            var tablePath = table.Path;
            foreach (var lineData in table.Table)
            {
                int index = lineData.Key;
                foreach (var cell in lineData.Value.CellData)
                {

                } 
            }
            return true;
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

        public static T GetConfigs<T>(CONFIG_TYPE configType, int index) where T : Dictionary<int, IMessage>
        {
            if (!allConfigs.TryGetValue(configType, out Dictionary<int, IMessage> configs))
            {
                return null;
            }

            return (T)configs;
        }
    }
}
