using DataGenerate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/24 19:09:17
 Summary  : 搜索管理
 ===============================================================================================*/
namespace DataView
{
    public class SearchManager
    {
        public static SearchManager Instance = new SearchManager();

        public List<BTreeView> searchList = new List<BTreeView>();
        public int count = 0;
        public MainWindow win;

        public void Init(MainWindow win)
        {
            this.win = win;
        }

        public void AddSearchResult(List<ExcelDataItem> retList)
        {
            if (retList.Count <= 0)
            {
                win.ShowMessageBox("未找到相关的文件");
                return;
            }
            var tree = new BTreeView(new ExcelDataItem(null, 0, "搜索_" + (++count)));
            foreach (var it in retList)
            {
                tree.AddTable(new BTreeView(it));
            }
            tree.Init();
            tree.IsExpanded = true;
            searchList.Add(tree);
            win.SearchMulu.Items.Add(tree);
            win.SearchMulu.Visibility = Visibility.Visible;
        }

        public List<ExcelDataItem> RawSearch(string id)
        {
            var retList = new List<ExcelDataItem>();
            foreach (var it in win.allExcelDataConfigs)
            {
                var ret = it.Search(id);
                if (ret != null)
                {
                    retList.Add(ret);
                }
            }
            return retList;
        }
        public void Search(string id)
        {
            AddSearchResult(RawSearch(id));
        }

        public void SearchText(string pattern)
        {
            var retList = new List<ExcelDataItem>();
            foreach (var it in win.allExcelDataConfigs)
            {
                var ret = it.SearchText(pattern);
                if (ret != null)
                {
                    retList.AddRange(ret);
                }
            }
            AddSearchResult(retList);
        }
    }
}
