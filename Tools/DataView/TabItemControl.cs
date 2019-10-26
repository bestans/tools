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
    public class BDataGridTextColumn : DataGridTextColumn
    {
        public int index;
    }
    public class TabItemControl
    {
        public List<UCTabItemWithClose> leftTabs = new List<UCTabItemWithClose>();
        public List<UCTabItemWithClose> rightTabs = new List<UCTabItemWithClose>();

        public List<UCTabItemWithClose> allTabls = new List<UCTabItemWithClose>();
        //[leftIndex, rightIndex)
        public int leftIndex = 0;
        public int rightIndex = 0;
        public int tabSelectIndex = 0;

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
            int count = 0;
            foreach (var it in MainConfig.Instance.dataGridColumns)
            {
                count++;
                var col = new BDataGridTextColumn() { Header = it.title, Binding = new Binding(it.bindKey), index = count - 1 };
                col.IsReadOnly = true;
                col.Width = count == 1 ? MainConfig.Instance.dataGridWidthIndex1 : MainConfig.Instance.dataGridWidth;
                gridConfigs.Add(col);
            }
            return gridConfigs;
        }
        public void Init()
        {
            var gridConfigs = NewGridConfig();
            TABLE_MAX_WIDTH = MainConfig.Instance.dataGridWidthIndex1;
            TABLE_MAX_WIDTH += (gridConfigs.Count-1) * MainConfig.Instance.dataGridWidth + MainConfig.Instance.dataGridExtraMaxWidth;
        }

        public void AddItem(UCTabItemWithClose item)
        {
            for (int i = 0; i < m_Parent.Items.Count; ++i)
            {
                if (item.data != null && item.data.SameItem(((UCTabItemWithClose)m_Parent.Items[i]).data))
                {
                    m_Parent.SelectedIndex = i;
                    return;
                }
            }
            for (int i = leftTabs.Count - 1; i >= 0; --i)
            {
                if (item.data != null && item.data.SameItem(leftTabs[i].data))
                {
                    return;
                }
            }
            m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, item);
            m_Parent.SelectedIndex = m_Parent.Items.Count - 1;
            RebuildItems();
            AjustTabSelect();
        }

        public void CloseClick(UCTabItemWithClose item)
        {
            if (m_Parent.Items.Count <= 0) return;
            if (item.itemType != TAB_ITEM.CONTENT) return;

            m_Parent.Items.Remove(item);
            int index = allTabls.IndexOf(item);
            if (index >= 0)
            {
                if (index < leftIndex)
                    leftIndex--;
                if (index < rightIndex)
                    rightIndex--;
            }

            RebuildItems();
            AjustTabSelect();
        }

        public void RightClick(object sender, RoutedEventArgs e)
        {
            if (SWITCH_COUNT <= 0) return;
            if (leftIndex <= 0) return;

            leftIndex--;
            rightIndex--;

            var removeIndex = m_Parent.Items.Count - SWITCH_COUNT - 1;
            m_Parent.Items.RemoveAt(removeIndex);
            m_Parent.Items.Insert(0, allTabls[leftIndex]);

            AjustTabSelect();
        }

        public void LeftClick(object sender, RoutedEventArgs e)
        {
            if (SWITCH_COUNT <= 0) return;
            if (rightIndex >= allTabls.Count - 1) return;

            leftIndex++;
            rightIndex++;

            m_Parent.Items.RemoveAt(0);
            m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, allTabls[rightIndex]);

            AjustTabSelect();
        }
        private void AjustTabSelect()
        {
            if (tabSelectIndex < leftIndex)
                tabSelectIndex = leftIndex;
            if (tabSelectIndex > rightIndex)
                tabSelectIndex = rightIndex;

            if (m_Parent.Items.Count <= 0) return;
            m_Parent.SelectedIndex = tabSelectIndex - leftIndex;
        }

        public void RebuildItems()
        {
            if (m_Parent.Items.Count <= 0 || allTabls.Count <= 0 || leftIndex < 0) return;

            var tabMaxWidth = win.TabContent.ActualWidth;
            if (tabMaxWidth <= 10)
            {
                return;
            }
            var criticalCount = (int)((tabMaxWidth - MainConfig.Instance.tableItemExtraWidth) / MainConfig.Instance.tableItemWidth);
            if (allTabls.Count > criticalCount)
            {
                if (SWITCH_COUNT <= 0)
                {
                    m_Parent.Items.Add(UCTabItemWithClose.NewItem(null, TAB_ITEM.LEFT_ARROW));
                    m_Parent.Items.Add(UCTabItemWithClose.NewItem(null, TAB_ITEM.RIGHT_ARROW));
                    SWITCH_COUNT = 2;
                }
            } else
            {
                if (SWITCH_COUNT > 0)
                {
                    m_Parent.Items.RemoveAt(m_Parent.Items.Count - 1);
                    m_Parent.Items.RemoveAt(m_Parent.Items.Count - 1);
                    SWITCH_COUNT = 0;
                }
            }

            criticalCount = (int)(tabMaxWidth - MainConfig.Instance.tableArrowItemWidth * SWITCH_COUNT - MainConfig.Instance.tableItemExtraWidth) / MainConfig.Instance.tableItemWidth;
            if (criticalCount <= 0)
                criticalCount = 1;

            if (tabSelectIndex < 0)
                tabSelectIndex = 0;
            if (tabSelectIndex < leftIndex)
            {
                for (int i = tabSelectIndex; i < leftIndex; ++i)
                {
                    m_Parent.Items.Insert(0, allTabls[i]);
                }
                leftIndex = tabSelectIndex;
            }
            if (tabSelectIndex >= rightIndex)
            {
                for (int i = rightIndex; i <= tabSelectIndex; ++i)
                {
                    m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT - 1, allTabls[i]);
                }
                rightIndex = tabSelectIndex + 1;
            }

            if (m_Parent.Items.Count - SWITCH_COUNT > criticalCount)
            {
                var removeCount = m_Parent.Items.Count - criticalCount - SWITCH_COUNT;

                var leftAdd = Math.Min(removeCount, tabSelectIndex - leftIndex);
                var rightDec = removeCount - leftAdd;
                for (int i = 0; i < leftAdd; ++i)
                {
                    m_Parent.Items.RemoveAt(0);
                }
                for (int i = 0; i < rightDec; ++i)
                {
                    m_Parent.Items.RemoveAt(m_Parent.Items.Count - SWITCH_COUNT - 1);
                }
                leftIndex += leftAdd;
                rightIndex -= rightDec;
            }
            else
            {
                var maxRemoveCount = criticalCount + SWITCH_COUNT - m_Parent.Items.Count;
                var leftDec = Math.Min(maxRemoveCount, leftIndex);
                var rightAdd = Math.Min(maxRemoveCount - leftDec, allTabls.Count - rightIndex);
                for (int i = leftIndex - 1; i >= leftIndex - leftDec; --i)
                {
                    m_Parent.Items.Insert(0, allTabls[i]);
                }
                for (int i = rightIndex + 1; i < rightIndex + rightAdd; ++i)
                {
                    m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, allTabls[i]);
                }
                leftIndex -= leftDec;
                rightIndex += rightAdd;
            }
            AjustTabSelect();
        }

        //搜索
        public void SearchText(string text)
        {
            if (text == null || text.Length == 0)
            {
                return;
            }
            if (win.SearchSelect.SelectedIndex == 0)
            {
                SearchManager.Instance.Search(text);
            } else
            {

                SearchManager.Instance.SearchText(text);
            }
        }

        public void OnMuluItemSelected(object sender, RoutedEventArgs e)
        {
            var tree = (BTreeView)(sender);

            AddItem(UCTabItemWithClose.NewItem(tree.data));
        }
    }
}
