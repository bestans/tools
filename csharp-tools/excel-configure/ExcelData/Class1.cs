using System;
using System.IO;
using System.Collections.Generic;

namespace bestan.tools.exceldata
{
    class Cls
    {
        public int value = 10;
    }

    struct Data
    {
        public int value;
    }
    public class Utils
    {
        public static void test1()
        {
            Data d;
            d.value = 10;
            Console.WriteLine(d.value);
        }

        public static void WriteCsvLine(StreamWriter file, List<String> lineData)
        {
            bool first = true;
            foreach (var item in lineData)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    file.Write("\t");
                }
                file.Write(item);
            }
            file.WriteLine();
        }
    }
}
