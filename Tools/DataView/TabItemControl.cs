using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DataView
{
    public class TabItemControl
    {
        public List<UCTabItemWithClose> leftTabs = new List<UCTabItemWithClose>();
        public List<UCTabItemWithClose> rightTabs = new List<UCTabItemWithClose>();
        private TabControl m_Parent;
        private int SWITCH_COUNT = 0;

        //数据页宽度
        public static int TABLE_MAX_WIDTH = 100;

        private MainWindow win;

        public TabItemControl(TabControl parent, MainWindow win)
        {
            this.win = win;
            m_Parent = parent;
            Init();
        }
        public static List<DataGridColumn> NewGridConfig()
        {
            var gridConfigs = new List<DataGridColumn>();
            foreach (var it in MainConfig.Instance.dataGridColumns)
            {
                var col = new DataGridTextColumn() { Header = it.title, Binding = new Binding(it.bindKey) };
                col.IsReadOnly = true;
                col.Width = MainConfig.Instance.dataGridWidth;
                gridConfigs.Add(col);
            }
            return gridConfigs;
        }
        public void Init()
        {
            var gridConfigs = NewGridConfig();
            TABLE_MAX_WIDTH = gridConfigs.Count * MainConfig.Instance.dataGridWidth + MainConfig.Instance.dataGridExtraMaxWidth;
        }

        public void AddItem(UCTabItemWithClose item)
        {
            m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, item);
            RebuildItems();
            AjustTabSelect();
        }

        public void CloseClick(UCTabItemWithClose item)
        {
            if (m_Parent.Items.Count <= 0) return;
            if (item.itemType != TAB_ITEM.CONTENT) return;

            m_Parent.Items.Remove(item);

            RebuildItems();
            AjustTabSelect();
        }

        public void RightClick(object sender, RoutedEventArgs e)
        {
            if (m_Parent.Items.Count <= 0) return;
            if (leftTabs.Count <= 0) return;
            if (SWITCH_COUNT <= 0) return;

            var removeIndex = m_Parent.Items.Count - SWITCH_COUNT - 1;
            var removeItem = (UCTabItemWithClose)m_Parent.Items[removeIndex];
            m_Parent.Items.RemoveAt(removeIndex);
            rightTabs.Insert(0, removeItem);

            //左边的标签补充到开头
            m_Parent.Items.Insert(0, (leftTabs[leftTabs.Count - 1]));
            leftTabs.RemoveAt(leftTabs.Count - 1);

            AjustTabSelect();
        }

        public void LeftClick(object sender, RoutedEventArgs e)
        {
            if (m_Parent.Items.Count <= 0) return;
            if (rightTabs.Count <= 0) return;
            if (SWITCH_COUNT <= 0) return;

            var removeItem = (UCTabItemWithClose)m_Parent.Items[0];
            m_Parent.Items.RemoveAt(0);
            leftTabs.Add(removeItem);

            m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, rightTabs[0]);
            rightTabs.RemoveAt(0);

            AjustTabSelect();
        }
        private void AjustTabSelect()
        {
            if (m_Parent.Items.Count <= 0) return;
            var index = m_Parent.SelectedIndex;
            if (index < 0)
            {
                m_Parent.SelectedIndex = 0;
            }
            else if (index >= m_Parent.Items.Count - SWITCH_COUNT)
            {
                m_Parent.SelectedIndex = m_Parent.Items.Count - SWITCH_COUNT - 1;
            }
        }

        public void RebuildItems()
        {
            var tabMaxWidth = win.TabContent.ActualWidth;
            if (tabMaxWidth <= 10)
            {
                return;
            }
            var criticalCount = (int)((tabMaxWidth - MainConfig.Instance.tableItemExtraWidth) / MainConfig.Instance.tableItemWidth);
            if (m_Parent.Items.Count >= criticalCount)
            {
                if (SWITCH_COUNT <= 0)
                {
                    m_Parent.Items.Add(UCTabItemWithClose.NewItem(null, TAB_ITEM.LEFT_ARROW));
                    m_Parent.Items.Add(UCTabItemWithClose.NewItem(null, TAB_ITEM.RIGHT_ARROW));
                    SWITCH_COUNT = 2;
                }

                criticalCount = (int)(tabMaxWidth - MainConfig.Instance.tableArrowItemWidth * 2 - MainConfig.Instance.tableItemExtraWidth) / MainConfig.Instance.tableItemWidth;
                if (criticalCount <= 0)
                    criticalCount = 0;
                var removeCount = m_Parent.Items.Count - criticalCount - 2;
                for (int i = 0; i < removeCount; ++i)
                {
                    var removeItem = (UCTabItemWithClose)m_Parent.Items[0];
                    m_Parent.Items.RemoveAt(0);
                    leftTabs.Add(removeItem);
                }
            }
            else
            {
                if (SWITCH_COUNT > 0)
                {
                    m_Parent.Items.RemoveAt(m_Parent.Items.Count - 1);
                    m_Parent.Items.RemoveAt(m_Parent.Items.Count - 1);
                    SWITCH_COUNT = 0;
                }

                var maxRemoveCount = criticalCount - m_Parent.Items.Count;
                for (int i = leftTabs.Count - 1; i >= 0 && maxRemoveCount > 0; --i, --maxRemoveCount)
                {
                    m_Parent.Items.Insert(0, leftTabs[i]);
                    leftTabs.RemoveAt(i);
                }

                for (int i = 0; i < rightTabs.Count && maxRemoveCount > 0; --maxRemoveCount)
                {
                    m_Parent.Items.Add(rightTabs[i]);
                    rightTabs.RemoveAt(i);
                }
            }
            AjustTabSelect();
        }
    }
}
