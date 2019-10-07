using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataGenerate
{
    public class TableUnitWrap
    {
        public TableUnitConfig tableUnit;
        public string configName;

        public TableUnitWrap(TableUnitConfig tableUnit, string configName)
        {
            this.tableUnit = tableUnit;
            this.configName = configName;
        }
    }
    public class TableLuaDefine
    {
        public static TableLuaDefine Instance = new TableLuaDefine();

        private TableConfig common;
        private Dictionary<string, TableUnitConfig> commonTableMap = new Dictionary<string, TableUnitConfig>();
        private Dictionary<string, TableUnitWrap> allTableUnitMap = new Dictionary<string, TableUnitWrap>();
        private Dictionary<string, TableConfig> allTableConfigMap = new Dictionary<string, TableConfig>();

        private static readonly string LINE_SEP = "\n";
        private static readonly string TAB_SEP = "	";
        public void Init()
        {

        }

        private void LoadCommon()
        {

        }

        public static bool IsBaseType(string typeName)
        {
            return Enum.TryParse<BASE_TYPE>(typeName, out BASE_TYPE ret);
        }

        /// <summary>
        /// 添加公共配置
        /// </summary>
        /// <param name="commonTable"></param>
        private void AddCommonTableConfig(TableConfig commonTable)
        {
            foreach (var it in commonTable.tables)
            {
                commonTableMap.Add(it.name, it);
            }
            AddTableConfig(commonTable);
        }
        private void AddTableConfig(TableConfig tableConfig)
        {
            allTableConfigMap.Add(tableConfig.configName, tableConfig);
        }

        private void CheckTableConfig(TableConfig tableConfig)
        {
            if (allTableConfigMap.ContainsKey(tableConfig.configName))
            {
                //重复的表格配置
                throw new Exception(string.Format("CheckTableConfig:duplicat config name({0}) with common config", tableConfig.configName));
            }
            var tempMap = new Dictionary<string, TableUnitConfig>();
            foreach (var it in tableConfig.tables)
            {
                if (allTableUnitMap.TryGetValue(it.name, out TableUnitWrap wrapConfig))
                {
                    //重复的表格名称
                    throw new Exception(string.Format("CheckTableConfig:duplicate table name({0});configName1={1},configName2={2}", it.name, tableConfig.configName, wrapConfig.configName));
                }
                var tempSectionItemSet = new HashSet<string>();
                foreach (TableSection sectionItem in it.items)
                {
                    if (tempSectionItemSet.Contains(sectionItem.name))
                    {
                        //重复的字段名称
                        throw new Exception(string.Format("CheckTableConfig:duplicate section name({0});table={1},tableConfig={2}", sectionItem.name, it.name, tableConfig.configName));
                    }
                    tempSectionItemSet.Add(sectionItem.name);
                    if (it.type == TAB_TYPE.ENUM)
                    {
                        continue;
                    }
                    var sectionBean = (BeanItemConfig)sectionItem;
                    if (IsBaseType(sectionBean.type))
                    {
                        continue;
                    }
                    if (!tempMap.ContainsKey(sectionBean.type) && !commonTableMap.ContainsKey(sectionBean.type))
                    {
                        //字段类型未定义或者没有按照顺序定义
                        throw new Exception(string.Format("CheckTableConfig:section type undefine or invalid define sort ;sectionType=({0}),section={1},table={2},tableConfig={3}", sectionBean.type,  sectionItem.name, it.name, tableConfig.configName));
                    }
                }
                allTableUnitMap.Add(it.name, new TableUnitWrap(it, tableConfig.configName));
                tempMap.Add(it.name, it);
            }
        }

        private void GenerateAllConfigProto()
        {

        }

        private void GenerateOneProto(TableConfig config)
        {
            string path = TableGlobalConfig.Instance.tableProtoPath;
            using (var file = new FileStream(path, FileMode.Create))
            {
                StringBuilder ss = new StringBuilder();
                foreach (var it in config.tables)
                {
                    if (it.type == TAB_TYPE.ENUM)
                    {
                        GenerateEnum(it, ss);
                    } else
                    {
                        GenerateBean(it, ss);
                    }
                }
                var data = Encoding.UTF8.GetBytes(ss.ToString());
                file.Write(data, 0, data.Length);
            }
        }
        private void GenerateEnum(TableUnitConfig config, StringBuilder ss)
        {
            ss.Append("enum ").Append(config.name).Append(" {").Append(LINE_SEP);
            int index = 0;
            foreach (EnumItemConfig it in config.items)
            {
                ss.Append(TAB_SEP).Append(config.name + "_" + it.name).Append(" = ").Append(index++)
                    .Append(";").Append(TAB_SEP).Append("//").Append(it.alias).Append(TAB_SEP);
            }
            ss.Append("}").Append(LINE_SEP);
        }
        private void GenerateBean(TableUnitConfig config, StringBuilder ss)
        {
            ss.Append("message ").Append(config.name).Append(" {").Append(LINE_SEP);
            int index = 1;
            foreach (BeanItemConfig it in config.items)
            {
                ss.Append(TAB_SEP);
                if (it.isList)
                {
                    ss.Append("repeated ");
                }
                ss.Append(it.type).Append(" ").Append(it.name).Append(" = ").Append(index++)
                    .Append(";").Append(TAB_SEP).Append("//").Append(it.alias).Append(TAB_SEP);
            }
            ss.Append("}").Append(LINE_SEP);
        }
    }
}
