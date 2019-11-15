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

        public void OnSelect(UCTabItemWithClose item)
        {
            for (int i = 0; i < allTabls.Count; ++i)
            {
                if (item.data != null && item.data.SameItem(allTabls[i].data))
                {
                    tabSelectIndex = i;
                    return;
                }
            }
        }

        public void AddItem(UCTabItemWithClose item)
        {
            for (int i = 0; i < allTabls.Count; ++i)
            {
                if (item.data != null && item.data.SameItem(allTabls[i].data))
                {
                    tabSelectIndex = i;
                    RebuildItems();
                    return;
                }
            }
            m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, item);
            allTabls.Insert(rightIndex, item);
            rightIndex++;
            tabSelectIndex = rightIndex - 1;
            RebuildItems();
        }

        public void CloseClick(UCTabItemWithClose item)
        {
            if (m_Parent.Items.Count <= 0) return;
            if (item.itemType != TAB_ITEM.CONTENT) return;

            m_Parent.Items.Remove(item);
            int index = allTabls.IndexOf(item);
            if (index >= 0)
            {
                allTabls.RemoveAt(index);
                if (index < leftIndex)
                    leftIndex--;
                if (index < rightIndex)
                    rightIndex--;
            }
            AjustTabSelect();
            RebuildItems();
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
            if (tabSelectIndex >= rightIndex)
                tabSelectIndex = rightIndex - 1;

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

            var curTabIndex = tabSelectIndex;
            if (curTabIndex < 0)
                curTabIndex = 0;
            if (curTabIndex < leftIndex)
            {
                for (int i = leftIndex - 1; i >= curTabIndex; i--)
                {
                    m_Parent.Items.Insert(0, allTabls[i]);
                }
                leftIndex = curTabIndex;
            }
            if (curTabIndex >= rightIndex)
            {
                for (int i = rightIndex; i <= curTabIndex; ++i)
                {
                    m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, allTabls[i]);
                }
                rightIndex = curTabIndex + 1;
            }

            if (m_Parent.Items.Count - SWITCH_COUNT > criticalCount)
            {
                var removeCount = m_Parent.Items.Count - criticalCount - SWITCH_COUNT;

                var leftAdd = Math.Min(removeCount, curTabIndex - leftIndex);
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
                for (int i = rightIndex; i < rightIndex + rightAdd; ++i)
                {
                    m_Parent.Items.Insert(m_Parent.Items.Count - SWITCH_COUNT, allTabls[i]);
                }
                leftIndex -= leftDec;
                rightIndex += rightAdd;
            }
            tabSelectIndex = curTabIndex;
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
