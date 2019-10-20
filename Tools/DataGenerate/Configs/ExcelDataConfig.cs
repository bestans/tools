using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Bestan.Common;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/19 13:39:49
 Summary  : excel生成的数据
 ===============================================================================================*/
namespace DataGenerate
{
    public enum EXCEL_DATA
    {
        ID = 0,
        NAME = 1,
        CATEGORY_START = 2,
    }

    public enum VIEW_DATA
    {
        INDEX   = 0,        //序号
        NAME    = 1,        //数据名
        TYPE    = 2,        //数据类型
        VALUE   = 3,        //数据值
        COUNT   = 4,        //数量
    }

    public static class EnumEntens
    {
        public static int ToInt(this Enum data)
        {
            return Convert.ToInt32(data);
        }
    }

    public class ExcelDataInfo
    {
        /// <summary>
        /// 配置名
        /// </summary>
        public string configName;
        /// <summary>
        /// 表格名
        /// </summary>
        public string tableName;
        /// <summary>
        /// id索引类型
        /// </summary>
        public string idspace;
    }

    public class ExcelDataTitleConfig : BaseLuaConfig
    {
        public static ExcelDataTitleConfig Instance = new ExcelDataTitleConfig() { alias = string.Empty, type = string.Empty };

        public string alias;
        public string type;
    }

    public class SearchResult
    {
        ExcelDataConfig config;
        public string itemName;
        public List<string> excelDataList;

        public SearchResult(ExcelDataConfig config, List<string> dataList)
        {
            this.config = config;
            this.itemName = dataList[EXCEL_DATA.ID.ToInt()] + "_" + dataList[EXCEL_DATA.ID.ToInt()];
            this.excelDataList = dataList;
        }

        public string GetValue(int index)
        {
            return (index >= 0 && index < excelDataList.Count) ? excelDataList[index] : string.Empty;
        }
        public ExcelDataTitleConfig GetTitle(int index)
        {
            return (index >= 0 && index < config.titles.Count) ? config.titles[index] : ExcelDataTitleConfig.Instance;
        }
        /// <summary>
        /// 条目数量
        /// </summary>
        public int MaxTitleCount { get
            {
                return config.titles.Count;
            }
        }
    }

    public class ExcelDataItem
    {
        ExcelDataConfig config;
        public string header;
        public int id;
        public Dictionary<KeyValuePair<int, string>, ExcelDataItem> items;

        public ExcelDataItem(ExcelDataConfig config, int id, string header)
        {
            this.config = config;
            this.id = id;
            this.header = header;
            this.items = new Dictionary<KeyValuePair<int, string>, ExcelDataItem>();
        }

        public void AddItem(ExcelDataItem item)
        {
            items[new KeyValuePair<int, string>(item.id, item.header)] = item;
        }

        public int GetMaxDataCount()
        {
            return config.titles.Count;
        }

        public string[] GetDataInfo(int dataIndex)
        {
            var datas = new string[VIEW_DATA.COUNT.ToInt()];
            datas[VIEW_DATA.INDEX.ToInt()] = (dataIndex + 1).ToString();
            var title = config.GetTitle(dataIndex);
            datas[VIEW_DATA.NAME.ToInt()] = title.alias;
            datas[VIEW_DATA.TYPE.ToInt()] = title.type;
            datas[VIEW_DATA.VALUE.ToInt()] = config.GetValue(id, dataIndex);
            return datas;
        }
    }

    public class ExcelDataConfig : BaseLuaConfig
    {
        public string configAlias;
        public List<ExcelDataTitleConfig> titles;
        public Dictionary<int, List<string>> data;

        public ExcelDataItem itemDir { get; set; }
        
        protected override void AfterLoad()
        {
            itemDir = new ExcelDataItem(this, 0, configAlias);
            var categoryStartIndex = (int)EXCEL_DATA.CATEGORY_START;
            var categoryEndIndex = ExcelConfig.Instance.defaultTitles.Count - 1;
            foreach (var entry in data)
            {
                var curItemDir = itemDir;
                var lineData = entry.Value;
                var id = entry.Key;
                var itemName = id.ToString() + "_" + lineData[(int)EXCEL_DATA.NAME];
                for (int i = categoryStartIndex; i <= categoryEndIndex && i < lineData.Count; ++i)
                {
                    var category = lineData[i];
                    if (category == null || category == string.Empty)
                    {
                        break;
                    }
                    if (!curItemDir.items.TryGetValue(new KeyValuePair<int, string>(0, category), out ExcelDataItem value))
                    {
                        value = new ExcelDataItem(this, 0, category);
                        curItemDir.AddItem(value);
                    }
                    curItemDir = value;
                }
                curItemDir.AddItem(new ExcelDataItem(this, id, itemName));
            }
        }

        public ExcelDataTitleConfig GetTitle(int dataIndex)
        {
            if (dataIndex >= 0 && dataIndex < titles.Count)
            {
                return titles[dataIndex];
            }
            return ExcelDataTitleConfig.Instance;
        }

        public string GetValue(int id, int dataIndex)
        {
            if (data.TryGetValue(id, out List<string> valueList))
            {
                if (dataIndex >= 0 && dataIndex < valueList.Count)
                {
                    return valueList[dataIndex];
                }
            }
            return string.Empty;
        }
        public ExcelDataItem Search(string id)
        {
            if (!int.TryParse(id, out int index)) return null;
            if (!data.TryGetValue(index, out List<string> dataList)) return null;

            var header = id + "_" + dataList[(int)EXCEL_DATA.NAME];
            return new ExcelDataItem(this, index, header);
        }

        public List<ExcelDataItem> SearchText(string pattern)
        {
            var result = new List<ExcelDataItem>();
            Regex rx = new Regex(pattern);
            foreach (var entry in data)
            {
                var dataList = entry.Value;
                bool find = false;
                foreach (var value in dataList)
                {
                    if (rx.IsMatch(value))
                    {
                        find = true;
                        break;
                    }
                }
                if (!find) continue;
                var header = entry.Key.ToString() + "_" + dataList[(int)EXCEL_DATA.NAME];
                result.Add(new ExcelDataItem(this, entry.Key, header));
            }
            return result;
        }
    }
}
