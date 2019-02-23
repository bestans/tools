using System;
using System.Collections.Generic;
using System.Text;

namespace bestan.common.excelparse
{
    public class ParseConfig
    {
        public string protoPath = "./";
        public string protoDataPath = "./";
        public string excelPath = "./";
        public string excelDataPath = "./";
        public string csvPath = "./";
        public List<string> ignoreFiles = new List<string>();
        public string rootDir = "./";
    }
}
