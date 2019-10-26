using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bestan.Common;
using DataGenerate;

namespace DataView
{
    public class DataUnit
    { 
        public string ID { get; set; }
        public string Section { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }

        public string content;
        public string[] dataInfo;
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TabItemControl tabCtrl;
        public List<ExcelDataConfig> allExcelDataConfigs = new List<ExcelDataConfig>();
        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            tabCtrl = new TabItemControl(TabContent, this);
            //InitTestView();
            InitDirectionary();
            //InitTableItem();
            SearchManager.Instance.Init(this);
        }

        private void LoadConfig()
        {
            LuaConfigs.LoadSingleConfig<MainConfig>("mainconfig.lua");
            LoadGenerateConfig();

            SearchSelect.Items.Clear();
            SearchSelect.ItemsSource = MainConfig.Instance.searchMenus;
            SearchSelect.SelectedIndex = 0;

            this.MinWidth = MainConfig.Instance.winMinWidth;
            this.MinHeight = MainConfig.Instance.winMinHeight;
            ((TabItem)TabIndex.Items[0]).Header = MainConfig.Instance.muluText;
            ((TabItem)TabIndex.Items[1]).Header = MainConfig.Instance.searchText;
        }

        private void LoadGenerateConfig()
        {
            //载入基础配置
            LuaConfigs.LoadSingleConfig<TableGlobalConfig>("run_config\\global.lua");
            //载入excel属性配置
            LuaConfigs.LoadSingleConfig<ExcelConfig>(TableGlobalConfig.Instance.excelPropConfigPath);
            //载入excel属性配置
            LuaConfigs.LoadSingleConfig<TableGenerateConfig>(TableGlobalConfig.Instance.tableTemplDataPath + TableGlobalConfig.Instance.tableGenerateConfigName);

            //自定义表格配置数据
            foreach (var customTableName in TableGenerateConfig.Instance.tableList)
            {
                var customTable = LuaConfigs.LoadSingleConfig<ExcelDataConfig>(TableGlobalConfig.Instance.tableTemplDataPath + customTableName + ".lua");
                allExcelDataConfigs.Add(customTable);
            }
        }

        /// <summary>
        /// 初始化目录
        /// </summary>
        private void InitDirectionary()
        {
            Dictionary<string, List<BTreeView>> mainTree = new Dictionary<string, List<BTreeView>>();
            foreach (var tableData in allExcelDataConfigs)
            {
                if (!mainTree.TryGetValue(tableData.configAlias, out List<BTreeView> value))
                {
                    value = new List<BTreeView>();
                    mainTree[tableData.configAlias] = value;
                }
                value.Add(new BTreeView(tableData.itemDir));
            }
            var itemList = new List<BTreeView>();
            foreach (var it in mainTree)
            {
                var item = new BTreeView(new ExcelDataItem(null, 0, it.Key));
                foreach (var table in it.Value)
                {
                    item.AddTable(table);
                }
                itemList.Add(item);
            }
            foreach (var it in itemList)
            {
                it.Init();
            }
            Mulu.Items.Clear();
            Mulu.Visibility = Visibility.Visible;
            Mulu.ItemsSource = itemList;
        }

        private List<BTreeView> GetTestList()
        {
            int fontSize = 15;
            var itemList = new List<BTreeView>();
            for (int i = 0; i < 50; ++i)
            {
                var titem = new BTreeView() { Header = "item" + i, IsExpanded = false, Visibility = Visibility.Visible, FontSize = fontSize };
                var son = new BTreeView() { Header = "item_sonsssssssssssssssssssssssssssssssssssss" + i, IsExpanded = false, FontSize = fontSize };
                var son_son = new BTreeView() { Header = "xx_item_sonsssssssssssssssssssssssssssssssssssss" + i, IsExpanded = false, FontSize = fontSize };
                son_son.Selected += tabCtrl.OnMuluItemSelected;
                son.Items.Add(son_son);
                titem.Items.Add(son);
                itemList.Add(titem);
            }
            return itemList;
        }
        public void InitTestView()
        {
            Mulu.Items.Clear();
            Mulu.Visibility = Visibility.Visible;
            Mulu.ItemsSource = GetTestList();
        }

        private void InitTableItem()
        {
            for (int i = 0; i < 8; ++i)
            {
                var content = new TabItemContent();
                content.header = "item" + i;
                //tabCtrl.AddItem(UCTabItemWithClose.NewItem(content));
            }
        }

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TabContent.Height = this.ActualHeight - 30;
            tabCtrl.RebuildItems();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Height < MinHeight)
            {
                Height = MinHeight;
            }
            if (Width < MinWidth)
            {
                Width = MinWidth;
            }
        }

        public void ShowMessageBox(string text)
        {
            MessageBox.Show(this, text);
        }

        private void ButtonSearchClick(object sender, RoutedEventArgs e)
        {
            tabCtrl.SearchText(SearchText.Text);
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //回车
                tabCtrl.SearchText(SearchText.Text);
            }
        }

        private void TabContent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;

            var addItem = (UCTabItemWithClose)e.AddedItems[0];
            if (addItem.itemType != TAB_ITEM.CONTENT)
            {
                if (e.RemovedItems.Count > 0)
                {
                    var removeItem = (UCTabItemWithClose)e.RemovedItems[0];
                    removeItem.IsSelected = true;
                }
                else
                {
                    TabContent.SelectedIndex = -1;
                }
            }
        }
    }
}
