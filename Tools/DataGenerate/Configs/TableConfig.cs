using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

namespace DataGenerate
{
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

        public List<ExcelTitle> GetExcelTitles()
        {
            var titles = new List<ExcelTitle>();
            foreach (var it in ExcelConfig.Instance.defaultTitles)
            {
                titles.Add(new ExcelTitle(it, it));
            }
            titles.AddRange(unitTitles);
            return titles;
        }

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
                        pre = it.alias;
                    }
                    for (int itemIndex = 1; itemIndex <= listSize; ++itemIndex)
                    {
                        var curTitle = it.isList ? pre + itemIndex : pre + it.alias;
                        if (TableLuaDefine.IsBaseType(it.type))
                        {
                            //基础类型
                            var title = new ExcelTitle(curTitle, curTitle + "_comment11111111" );
                            titles.Add(title);
                            continue;
                        }
                        var tableUnit = TableLuaDefine.Instance.GetItemTableUnit(it.type);
                        if (tableUnit == null)
                            throw new Exception(string.Format("undefined bean type:{0},tableName={1}", it.type, name));

                        //子类
                        foreach (var itTitle in tableUnit.unitTitles)
                        {
                            var title = new ExcelTitle(curTitle + itTitle.title, itTitle.titleComment) { menus = itTitle.menus };
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
