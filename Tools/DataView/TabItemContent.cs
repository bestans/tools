using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    public class TabItemContent
    {
        public string header;
        public TAB_ITEM tabItem = TAB_ITEM.CONTENT;

        public List<DataUnit> DataList {
            get
            {
                List<DataUnit> dataUnitList = new List<DataUnit>();
                for (int i = 0; i < 100; ++i)
                {
                    dataUnitList.Add(new DataUnit() { FirstName = "FirstName111111111111111111" + i, SecondName = "SecondName" + i, content = i.ToString(), });
                }
                return dataUnitList;
            }
        }
    }
}
