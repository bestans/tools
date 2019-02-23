using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aspose.Cells;

namespace bestan.common.excelparse
{
    class ExcelParseManager
    {

        public static StreamWriter OpenCsvFile(string msg_name, bool isNew)
        {
            string path = ParseManager.GetInstance().config.csvPath + msg_name + ".csv";
            StreamWriter file;
            try
            {
                var encoding = new UTF8Encoding(true);

                var append = isNew ? false : true;
                file = new StreamWriter(path, append, encoding);
            }
            catch (Exception e)
            {
                Console.WriteLine("OpenCsvFile failed:msg_name=" + msg_name + ",error=" + e.Message);
                return null;
            }
            return file;
        }

        public static void WriteCsvLine(StreamWriter file, List<string> lineData)
        {
            StringBuilder ss = new StringBuilder();
            bool isFirst = true;
            foreach (var it in lineData)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    ss.Append(",");
                }
                ss.Append(it);
            }
            ss.Append("\r\n");
            file.Write(ss.ToString());
        }
        public static List<string> GetLineData(Worksheet sh, int line)
        {
            var lineData = new List<string>();
            int maxColumn = sh.Cells.MaxDataColumn;
            for (int i = 0; i <= maxColumn; ++i)
            {
                var cellValue = sh.Cells[line, i].Value;
                string cellInfo = string.Empty;
                if (cellValue != null)
                {
                    cellInfo = cellValue.ToString().Trim();
                }
                lineData.Add(cellInfo);
            }
            return lineData;
        }

        public static Workbook ReadExcel(string path)
        {
            return new Workbook(path);
        }
        
        public static bool ParseExcel(string path, bool generalProto)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
            var workbook = new Workbook(path);
            var sh = workbook.Worksheets["数据表"];
            if (sh == null)
            {
                Console.WriteLine("发现错误，文件", path + "找不到数据表");
                return false;
            }

            int rowCount = sh.Cells.MaxDataRow;
            if (rowCount < 2)
            {
                Console.WriteLine("发现错误，文件", path + "不足3行");
                return false;
            }

            if (generalProto)
            {
                var descList = GetLineData(sh, 0);
                var nameList = GetLineData(sh, 1);
                var typeList = GetLineData(sh, 2);
                if (descList.Count <= 0)
                {
                    Console.WriteLine("发现错误，文件", path + "空的配置");
                    return false;
                }
                var tempFile = OpenCsvFile(fileName, true);
                if (null == tempFile)
                {
                    Console.WriteLine("发现错误，OpenCsvFile失败，msg_name=" + fileName);
                    return false;
                }
                WriteCsvLine(tempFile, descList);
                WriteCsvLine(tempFile, nameList);
                WriteCsvLine(tempFile, typeList);
                tempFile.Close();
                if (!ParseManager.GetInstance().BeginMessage(fileName, path, descList, nameList, typeList))
                {
                    return false;
                }
                return true;
            }
            
            if (!ParseManager.GetInstance().ImportPath(fileName))
            {
                return false;
            }

            var file = OpenCsvFile(fileName, false);
            if (null == file)
            {
                Console.WriteLine("发现错误，OpenCsvFile失败，msg_name=" + fileName);
                return false;
            }
            for (int row = 3; row <= sh.Cells.MaxDataRow; ++row)
            {
                var lineData = GetLineData(sh, row);
                WriteCsvLine(file, lineData);
                if (!ParseManager.GetInstance().AddLineData(row + 1, lineData))
                {
                    return false;
                }
            }
            file.Close();
            if (!ParseManager.GetInstance().EndMessage())
            {
                return false;
            }
            return true;
        }
    }
}
