using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DataGenerate;
using System.Windows;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/24 15:04:40
 Summary  : 
 ===============================================================================================*/
namespace DataView
{
    public class BTreeView : TreeViewItem
    {
        public ExcelDataItem data;
        private List<BTreeView>  itemList = new List<BTreeView>();

        public BTreeView() { }
        public BTreeView(ExcelDataItem data)
        {
            this.data = data;
            this.Header = data.header;
            this.FontSize = 15;
            this.IsExpanded = false;
            this.Visibility = Visibility.Visible;
            //this.ItemsSource = itemList;
        }
        public void Init()
        {
            if (data.items != null)
            {
                foreach (var it in data.items)
                {
                    var temp = new BTreeView(it.Value);
                    this.itemList.Add(temp);
                    this.Items.Add(temp);
                }
            }
            foreach (var it in this.itemList)
            {
                it.Init();
            }
            if (data.config != null && data.id > 0)
            {
                this.Selected += MainWindow.tabCtrl.OnMuluItemSelected;
            }
        }
        public void AddTable(BTreeView data)
        {
            this.Items.Add(data);
            this.itemList.Add(data);
            //this.itemList.Add(data);
            //this.ItemsSource = itemList;
        }
    }
}
