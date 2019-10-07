using System;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

namespace DataGenerate
{
    public class TableGlobalConfig : BaseLuaSingleton<TableGlobalConfig>
    {
        /// <summary>
        /// lua定义表格所在路径
        /// </summary>
        public string tableLuaDefinePath;
        /// <summary>
        /// lua表格通用配置文件名
        /// </summary>
        public string tableLuaCommonName;

        public string tableProtoPath;
    }
}
