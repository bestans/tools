using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

namespace DataGenerate
{
    public enum BASE_TYPE
    {
        FLOAT,
        DOUBLE,
        INT,
        LONG,
        SHORT,
        BOOL,
        STRING,
    }

    /// <summary>
    /// 表格类型
    /// </summary>
    public enum TAB_TYPE
    { 
        /// <summary>
        /// 表格
        /// </summary>
        TABLE,
        /// <summary>
        /// 枚举
        /// </summary>
        ENUM,
        /// <summary>
        /// 自定义结构
        /// </summary>
        BEAN,
    }
    public abstract class TableSection : BaseLuaConfig
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string name;
    }

    public class TableUnitConfig : BaseLuaConfig
    {
        [LuaParam(isFirstLoad = true)]
        public TAB_TYPE type;
        public string name;
        public IList items;
        public string alias;
        [LuaParam(policy = LuaParamPolicy.OPTIONAL)]
        public string idType = "normal";

        protected override Type AfterFirstFieldLoadModify(Type srcFieldType, string section)
        {
            if (section == "items")
            {
                switch (type)
                {
                    case TAB_TYPE.ENUM:
                        return typeof(List<EnumItemConfig>);
                    default:
                        return typeof(List<BeanItemConfig>);
                }
            }
            return base.AfterFirstFieldLoadModify(srcFieldType, section);
        }
        protected override void AfterLoad()
        {
            if (type == TAB_TYPE.ENUM)
            {
                return;
            }

            if (!TableDataConfig.Instance.tableIndexMap.TryGetValue(idType, out int value))
            {
                throw new Exception("invalid idType=" + idType);
            }
        }
    }

    public class TableConfig : BaseLuaConfig
    {
        public List<TableUnitConfig> tables;
        public string configName;
        public string configDirName;
    }
}
