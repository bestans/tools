using System;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

namespace DataGenerate
{
    public class BeanItemConfig : TableSection
    {
        /// <summary>
        /// 字段类型
        /// </summary>
        public string type;
        /// <summary>
        /// 是否list结构
        /// </summary>
        [LuaParam(policy = LuaParamPolicy.OPTIONAL)]
        public bool isList;
        /// <summary>
        /// 如果是list结构，长度是多少
        /// </summary>
        [LuaParam(policy = LuaParamPolicy.OPTIONAL)]
        public int listSize;
        public string alias;
    }
}
