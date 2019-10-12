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

    public class ExcelTitle
    {
        public string title;
        public string titleComment;
        public List<string> menus;  //可为空

        public ExcelTitle(string title, string titleComment)
        {
            this.title = title;
            this.titleComment = titleComment;
        }
    }

    public class TableUnitConfig : BaseLuaConfig
    {
        [LuaParam(isFirstLoad = true)]
        public TAB_TYPE type;
        [LuaParam(policy = LuaParamPolicy.OPTIONAL)]
        public string name;
        public IList items;
        public string alias;
        [LuaParam(policy = LuaParamPolicy.OPTIONAL)]
        public string idType = "normal";

        public List<ExcelTitle> unitTitles { set; get;}

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
        }

        public void Init()
        {
            var titles = new List<ExcelTitle>();
            if (type == TAB_TYPE.ENUM)
            {
                var menus = new List<string>();
                foreach (EnumItemConfig it in items)
                {
                    menus.Add(it.alias);
                }
                var title = new ExcelTitle(alias, alias + "_enum_comment") { menus = menus };
                titles.Add(title);
            } else {
                foreach (BeanItemConfig it in items)
                {
                    int listSize = 1;
                    string pre = "";
                    if (it.isList)
                    {
                        listSize = it.listSize;
                        pre = it.name;
                    }
                    for (int itemIndex = 1; itemIndex <= listSize; ++itemIndex)
                    {
                        if (it.isList)
                            pre += itemIndex;
                        if (TableLuaDefine.IsBaseType(it.type))
                        {
                            //基础类型
                            var title = new ExcelTitle(pre+it.name, it.name + "_comment" );
                            titles.Add(title);
                            continue;
                        }
                        var tableUnit = TableLuaDefine.Instance.GetItemTableUnit(it.type);
                        if (tableUnit == null)
                            throw new Exception(string.Format("undefined bean type:{0},tableName={1}", it.type, name));

                        //子类
                        foreach (var itTitle in tableUnit.unitTitles)
                        {
                            var title = new ExcelTitle(pre + it.name + itTitle.title, itTitle.titleComment) { menus = itTitle.menus };
                            if (tableUnit.type == TAB_TYPE.ENUM)
                            {
                                title.menus = itTitle.menus;
                            }
                            titles.Add(title);
                        }
                    }
                }
            }
            unitTitles = titles;
        }
    }

    public class TableConfig : BaseLuaConfig
    {
        public List<TableUnitConfig> tables;
        /// <summary>
        /// 对应生成proto文件名，已经
        /// </summary>
        public string configName;
        public string configAlias;
    }
}
