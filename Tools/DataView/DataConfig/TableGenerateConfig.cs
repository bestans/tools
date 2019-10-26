using System;
using System.Collections.Generic;
using System.Text;
using Bestan.Common;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/24 14:35:14
 Summary  : table生成信息
 ===============================================================================================*/
namespace DataGenerate
{
    public class TableGenerateConfig : BaseLuaSingleton<TableGenerateConfig>
    {
        public List<string> tableList;
    }
}
