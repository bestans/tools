using System;
using System.Collections.Generic;
using System.Text;

namespace ExcelCodeGenerate
{
    public class GenerateCodeConfig
    {
        public string protoDataPath = "all_config.proto.dat";
        public string dotnetCodeTemplatePath = "ConfigManager.cs.Template";
        public string dotnetCodePath = "ConfigManager.cs";
    }
}
