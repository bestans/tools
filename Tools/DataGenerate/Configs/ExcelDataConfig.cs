using System;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/19 13:39:49
 Summary  : excel生成的数据
 ===============================================================================================*/
namespace DataGenerate
{
    public class ExcelDataPath
    {
        public string configName;
        public string tableName;
    }
    public class ExcelDataTitleConfig : BaseLuaConfig
    {
        public string alias;
        public string type;
    }

    public class ExcelDataConfig : BaseLuaConfig
    {
        public Dictionary<int, ExcelDataTitleConfig> titles;
        public Dictionary<int, List<string>> data;

        public ExcelDataPath pathInfo {get; set;}

    }
}
