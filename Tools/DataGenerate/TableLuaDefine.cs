﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bestan.Common;

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

        //通用table定义map
        private Dictionary<string, TableUnitConfig> commonTableMap = new Dictionary<string, TableUnitConfig>();
        //所有table定义map
        private Dictionary<string, TableUnitWrap> allTableUnitMap = new Dictionary<string, TableUnitWrap>();
        //所有config定义map
        private Dictionary<string, TableConfig> allTableConfigMap = new Dictionary<string, TableConfig>();

        public static readonly string LINE_SEP = "\n";
        private static readonly string TAB_SEP = "	";

        public void Init()
        {
            //Console.Write(Directory.GetCurrentDirectory());
            //载入基础配置
            LuaConfigs.LoadSingleConfig<TableGlobalConfig>("run_config\\global.lua");
            //载入数据配置
            LuaConfigs.LoadSingleConfig<TableDataConfig>(TableGlobalConfig.Instance.dataConfigPath);
            //载入excel属性配置
            LuaConfigs.LoadSingleConfig<ExcelConfig>(TableGlobalConfig.Instance.excelPropConfigPath);

            //通用表格bean、enum配置
            var commonTable = LuaConfigs.LoadSingleConfig<TableConfig>(TableGlobalConfig.Instance.GetCommonConfigPath());
            AddCommonTableConfig(commonTable);
            //自定义表格配置
            foreach (var customTableName in TableGlobalConfig.Instance.customConfigs)
            {
                var customTable = LuaConfigs.LoadSingleConfig<TableConfig>(TableGlobalConfig.Instance.tableLuaDefinePath + customTableName + ".lua");
                AddTableConfig(customTable);
            }
        }

        /// <summary>
        /// 生成excel
        /// </summary>
        public void ToolGenerateExcel()
        {
            foreach (var it in allTableConfigMap)
            {
                string dir = TableGlobalConfig.Instance.tableExcelRootPath + "\\" + it.Value.configAlias;
                
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                foreach (var table in it.Value.tables)
                {
                    if (table.type != TAB_TYPE.TABLE)
                        continue;
                    string path = dir + "\\" + table.alias + ".xlsx";
                    ExcelMan.Instance.InitExcel(table, path);
                }
            }
        }

        /// <summary>
        /// 生成proto文件
        /// </summary>
        public void ToolGenerateProto()
        {
            foreach (var it in allTableConfigMap)
            {
                GenerateOneProto(it.Value);
            }
        }

        public static bool IsBaseType(string typeName)
        {
            return TableGlobalConfig.Instance.GetBaseTypeDesc(typeName) != null;
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
            if (allTableConfigMap.ContainsKey(tableConfig.configName))
            {
                //重复的配置
                throw new Exception(string.Format("CheckTableConfig:duplicat config name({0}) with common config", tableConfig.configName));
            }
            //添加配置
            allTableConfigMap.Add(tableConfig.configName, tableConfig);

            var tempMap = new Dictionary<string, TableUnitConfig>();
            foreach (var it in tableConfig.tables)
            {
                if (allTableUnitMap.TryGetValue(it.name, out TableUnitWrap wrapConfig))
                {
                    //重复的表格名称
                    throw new Exception(string.Format("CheckTableConfig:duplicate table name({0});configName1={1},configName2={2}", it.name, tableConfig.configName, wrapConfig.configName));
                }
                //字段名
                var tempSectionItemSet = new HashSet<string>();
                //字段别名
                var tempSectionAliasItemSet = new HashSet<string>();
                foreach (TableSection sectionItem in it.items)
                {
                    if (tempSectionItemSet.Contains(sectionItem.name))
                    {
                        //重复的字段名称
                        throw new Exception(string.Format("CheckTableConfig:duplicate section name: section={0};table={1},tableConfig={2}", sectionItem.name, it.name, tableConfig.configName));
                    }
                    if (tempSectionAliasItemSet.Contains(sectionItem.alias))
                    {
                        //重复的字段alias
                        throw new Exception(string.Format("CheckTableConfig:duplicate section alias: section={0};aliase={1};table={2},tableConfig={3}", sectionItem.name, sectionItem.alias, it.name, tableConfig.configName));
                    }
                    tempSectionItemSet.Add(sectionItem.name);
                    tempSectionAliasItemSet.Add(sectionItem.alias);
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

                it.Init();
            }
        }

        public TableUnitConfig GetItemTableUnit(string name)
        {
            if (allTableUnitMap.TryGetValue(name, out TableUnitWrap value))
            {
                return value.tableUnit;
            }
            return null;
        }

        private void GenerateOneProto(TableConfig config)
        {
            string path = TableGlobalConfig.Instance.tableProtoPath;
            using (var file = new FileStream(path + config.configName + ".proto", FileMode.Create))
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
                    .Append(";").Append(TAB_SEP).Append("//").Append(it.alias).Append(LINE_SEP);
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
                    .Append(";").Append(TAB_SEP).Append("//").Append(it.alias).Append(LINE_SEP);
            }
            ss.Append("}").Append(LINE_SEP);
        }

        public void ToolGenerateTemplData()
        {
            var tableGenerate = new TableGenerateConfig();
            tableGenerate.tableList = new List<string>();
            foreach (var it in allTableConfigMap)
            {
                string dir = TableGlobalConfig.Instance.tableExcelRootPath + "\\" + it.Value.configAlias;
                foreach (var table in it.Value.tables)
                {
                    if (table.type != TAB_TYPE.TABLE)
                        continue;

                    string path = dir + "\\" + table.alias + ".xlsx";
                    ExcelMan.Instance.ReadExcel2Templ(table, path, it.Value.configAlias);
                    tableGenerate.tableList.Add(table.name);
                }
            }
            tableGenerate.WriteToFile(TableGlobalConfig.Instance.tableTemplDataPath + TableGlobalConfig.Instance.tableGenerateConfigName);
        }
    }
}
