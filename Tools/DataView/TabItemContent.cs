using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGenerate;

namespace DataView
{
    public class TabItemContent
    {
        public string header;
        public ExcelDataItem data;

        public TAB_ITEM tabItem = TAB_ITEM.CONTENT;

        public List<DataUnit> DataList {
            get
            {
                List<DataUnit> dataUnitList = new List<DataUnit>();
                for (int i = 0; i < data.config.titles.Count; ++i)
                {
                    var dataInfo = data.GetDataInfo(i);
                    var unit = new DataUnit();
                    unit.ID = dataInfo[0];
                    unit.Section = dataInfo[1];
                    unit.TypeName = dataInfo[2];
                    unit.Value = dataInfo[3];
                    dataUnitList.Add(unit);
                }
                return dataUnitList;
            }
        }
    }
}
