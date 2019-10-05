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

namespace DataView
{
    public class DataUnit
    { 
        public string FirstName { get; set; }
        public string SecondName { get; set; }

        public string content;
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static TabItemControl tabCtrl;
        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            tabCtrl = new TabItemControl(TabContent, this);
            InitTestView();
            InitTableItem();
        }

        private void LoadConfig()
        {
            LuaConfigs.LoadSingleConfig<MainConfig>("mainconfig.lua");

            SearchSelect.Items.Clear();
            SearchSelect.ItemsSource = MainConfig.Instance.searchMenus;
            SearchSelect.SelectedIndex = 0;
        }

        public void InitTestView()
        {
            int fontSize = 15;
            Mulu.Items.Clear();
            Mulu.Visibility = Visibility.Visible;
            for (int i = 0; i < 50; ++i)
            {
                var titem = new TreeViewItem() { Header = "item" + i, IsExpanded = false, Visibility = Visibility.Visible, FontSize= fontSize };
                var son = new TreeViewItem() { Header = "item_sonsssssssssssssssssssssssssssssssssssss" + i, IsExpanded = false, FontSize = fontSize };
                var son_son = new TreeViewItem() { Header = "xx_item_sonsssssssssssssssssssssssssssssssssssss" + i, IsExpanded = false, FontSize = fontSize };
                son.Items.Add(son_son);
                titem.Items.Add(son);
                Mulu.Items.Add(titem);
            }
        }

        private void InitTableItem()
        {
            for (int i = 0; i < 8; ++i)
            {
                var content = new TabItemContent();
                content.header = "item" + i;
                tabCtrl.AddItem(UCTabItemWithClose.NewItem(content));
            }
        }

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TabContent.Height = this.ActualHeight - 30;
            tabCtrl.RebuildItems();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        public void ShowMessageBox(string text)
        {
            MessageBox.Show(this, text);
        }

        private void ButtonSearchClick(object sender, RoutedEventArgs e)
        {
            string text = SearchText.Text;

            ShowMessageBox(text);
        }
    }
}
