using System;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

namespace DataGenerate
{
    public class TableDataConfig : BaseLuaSingleton<TableDataConfig>
    {
        public Dictionary<string, int> idspaceMap;
    }
}
