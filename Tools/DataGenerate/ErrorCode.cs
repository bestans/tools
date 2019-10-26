using System;
using System.Collections.Generic;
using System.Text;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/21 16:01:59
 Summary  : 错误码
 ===============================================================================================*/
namespace DataGenerate
{
    public enum ERROR_CODE
    {
        OPEN_EXCEL_FAILED_WHEN_READ_EXCEL, //打开excel失败，excel不存在或者被其他进程占用
        LACK_EXCEL_SHEET,   //excel缺少指定名称的分页
        EXCEL_TITLES_NO_DATA,   //excel标题栏没有效数据
        EXCEL_TITLE_NOT_MATCH_DEFINE, //excel标题与定义不符，可能需要重新生成excel
        EXCEL_DATA_ID_INVALID,  //excel中的行数据的id不是有效数字
        EXCEL_INFO_WRITE_TO_FILE_FAILED,    //excel表数据写入到lua文件失败，可能是该文件被锁定了
    }

    public class DataException : Exception
    {
        private ERROR_CODE err;
        public DataException(ERROR_CODE err, string message) : base(message)
        {
            this.err = err;
        }
        public string GetLog()
        {
            return string.Format("ErrorCode={0},ErrorInfo=[{1}]", err, Message);
        }
    }
}
