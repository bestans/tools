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
        /// proto配置所在目录
        /// </summary>
        public string tableProtoPath;
        /// <summary>
        /// excel所在根目录
        /// </summary>
        public string tableExcelRootPath;
        /// <summary>
        /// lua表格通用配置文件名
        /// </summary>
        public string tableLuaCommonName;

        /// <summary>
        /// 数据配置文件
        /// </summary>
        public string dataConfigPath;
        /// <summary>
        /// 自定义表格配置
        /// </summary>
        public List<string> customConfigs;
        /// <summary>
        /// 基础类型
        /// </summary>
        public List<string> baseTypes;

        /// <summary>
        /// excel属性配置
        /// </summary>
        public string excelPropConfigPath;
        /// <summary>
        /// 获取common配置路径
        /// </summary>
        /// <returns></returns>
        public string GetCommonConfigPath()
        {
            return tableLuaDefinePath + tableLuaCommonName;
        }
    }
}
