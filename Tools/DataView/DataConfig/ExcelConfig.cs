using System;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/10 19:10:57
 Summary  : excel相关的配置
 ===============================================================================================*/
namespace DataGenerate
{
    public class ExcelConfig : BaseLuaSingleton<ExcelConfig>
    {
        /// <summary>
        /// 标题栏高度
        /// </summary>
        public double titleHeight = 40;
        /// <summary>
        /// 默认标题 key:标题 value:类型
        /// </summary>
        public List<DefaultTitle> defaultTitles;

        public class DefaultTitle : BaseLuaConfig
        {
            public string title;
            public string type;
        }
    }
}
