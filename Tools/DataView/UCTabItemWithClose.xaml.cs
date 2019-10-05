using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DataView
{
    public enum TAB_ITEM
    {
        CONTENT,
        LEFT_ARROW,
        RIGHT_ARROW,
    }

    /// <summary>
    /// UCTabItemWithClose.xaml 的交互逻辑
    /// </summary>
    public partial class UCTabItemWithClose : TabItem
    {
        public UCTabItemWithClose(TAB_ITEM itemType)
        {
            this.itemType = itemType;
            InitializeComponent();
        }
        #region 成员变量
        public TAB_ITEM itemType = TAB_ITEM.CONTENT;
        #endregion

        #region 关闭按钮
        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (itemType == TAB_ITEM.CONTENT)
            {
                MainWindow.tabCtrl.CloseClick(this);
            }
        }
        #endregion

        #region 递归找父级TabControl
        /// <summary>
        /// 递归找父级TabControl
        /// </summary>
        /// <param name="reference">依赖对象</param>
        /// <returns>TabControl</returns>
        private TabControl FindParentTabControl(DependencyObject reference)
        {
            DependencyObject dObj = VisualTreeHelper.GetParent(reference);
            if (dObj == null)
                return null;
            if (dObj.GetType() == typeof(TabControl))
                return dObj as TabControl;
            else
                return FindParentTabControl(dObj);
        }
        #endregion

        private void btn_Close_Initialized(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var res = "CloseButtonStyle2";
            switch (itemType)
            {
                case TAB_ITEM.CONTENT:
                default:
                    break;
                case TAB_ITEM.LEFT_ARROW:
                    res = "LeftArrow";
                    button.Width = this.Width = 20;
                    button.Click += MainWindow.tabCtrl.LeftClick;
                    break;
                case TAB_ITEM.RIGHT_ARROW:
                    res = "RightArrow";
                    button.Width = this.Width = 20;
                    button.Click += MainWindow.tabCtrl.RightClick;
                    break;
            }
            button.Style = this.FindResource(res) as Style;
        }

        public static UCTabItemWithClose NewItem(TabItemContent content, TAB_ITEM itemType = TAB_ITEM.CONTENT)
        {
            UCTabItemWithClose item = new UCTabItemWithClose(itemType);
            if (content != null)
            {
                item.Header = content.header;
                item.ToolTip = content.header;
            }
            item.FontSize = 13;
            DataGrid data = new DataGrid();
            //让DataUnit不会自动生成列
            data.AutoGenerateColumns = false;
            //不能主动添加行
            data.CanUserAddRows = false;
            //不能排序
            data.CanUserSortColumns = false;
            //单元格选中
            data.SelectionUnit = DataGridSelectionUnit.Cell;
            //单元格单选
            data.SelectionMode = DataGridSelectionMode.Single;
            //单元格选中后更新右键菜单
            data.SelectedCellsChanged += (object sender, SelectedCellsChangedEventArgs e) =>
            {
                OnRightClick(data, sender, e);
            };
            //data.SizeChanged += (object sender, SizeChangedEventArgs e) =>
            //{
            //    int width = 0;
            //    foreach (var it in data.Columns)
            //    {
            //        width += (int)it.ActualWidth;
            //    }
            //    data.Width = width + 25;
            //};
            //data.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            data.ContextMenu = new ContextMenu();
            foreach (var it in TabItemControl.NewGridConfig())
            {
                data.Columns.Add(it);
            }
            data.MaxWidth = TabItemControl.TABLE_MAX_WIDTH;
            if (content != null)
            {
                data.ItemsSource = new ObservableCollection<DataUnit>(content.DataList);
            }
            Grid grid = new Grid();
            grid.Children.Add(data);
            item.Content = grid;

            if (itemType != TAB_ITEM.CONTENT)
            {
                item.Width = MainConfig.Instance.tableArrowItemWidth;
            }
            else
            {
                item.Width = MainConfig.Instance.tableItemWidth;
            }
            return item;
        }

        private static void OnRightClick(DataGrid grid, object sender, SelectedCellsChangedEventArgs e)
        {
            var data = ((DataUnit)(grid.SelectedCells[0].Item));
            if (data == null)
            {
                return;
            }
            grid.ContextMenu.Items.Clear();
            {//复制
                var menuItem = new MenuItem();
                menuItem.Header = "复制到剪贴板";
                menuItem.Click += (object newSender, RoutedEventArgs newE) =>
                {
                    Clipboard.SetText(data.content);
                };
                grid.ContextMenu.Items.Add(menuItem);
            }
        }

        private static void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn column)
            {
                var style = new Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                column.ElementStyle = style;
            }
        }
    }
}
