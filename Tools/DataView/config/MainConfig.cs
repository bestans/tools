using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bestan.Common;

namespace DataView
{
    public class MainConfig : BaseLuaSingleton<MainConfig>
    {
        //菜单
        public List<string> searchMenus;
        //数据列
        public List<GridColomnConfig> dataGridColumns;
        //数据列宽度
        public int dataGridWidth;
        //数据列最大宽度额外值
        public int dataGridExtraMaxWidth;
        //数据页签宽度
        public int tableItemWidth;
        //左右箭头页签宽度
        public int tableArrowItemWidth;
        //数据页签余留宽度
        public int tableItemExtraWidth;
        //主界面最小宽度
        public int winMinWidth;
        //主界面最小高度
        public int winMinHeight;

        public class GridColomnConfig : BaseLuaConfig
        {
            public string title;
            public string bindKey;
        }
    }
}
