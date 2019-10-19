using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Aspose.Cells;

/*=============================================================================================== 
 Author   : yeyouhuan
 Created  : 2019/10/8 17:53:08
 Summary  : excel操作
 ===============================================================================================*/
namespace DataGenerate
{
    public class ExcelConst
    {
        /// <summary>
        /// 标题行索引
        /// </summary>
        public const int TITLE_ROW = 1;
        public const uint color = 0xFFC0C0C0;
    }
    public class ExcelMan
    {
        public static ExcelMan Instance = new ExcelMan();

        public void InitExcel(TableUnitConfig table, string path)
        {
            Workbook wb = null;
            if (File.Exists(path))
            {
                wb = new Workbook(path);
            } else
            {
                wb = new Workbook();
                wb.Worksheets[0].Name = table.alias;
            }
            Console.WriteLine("excel format:" + wb.FileFormat);
            var st = wb.Worksheets[table.alias];
            if (st == null)
            {
                st = wb.Worksheets.Add(table.alias);
            }

            var sectionList = table.GetExcelTitles();
            //重新生成 每一列的下拉菜单
            st.Validations.Clear();
            st.Comments.Clear();
            for (int i = 0; i < sectionList.Count; ++i)
            {
                var titleInfo = sectionList[i];
                bool find = false;
                for (int titleIndex = 0; titleIndex <= st.Cells.MaxDataColumn; ++titleIndex)
                {
                    var cell = st.Cells[ExcelConst.TITLE_ROW, titleIndex];
                    if (cell == null || cell.Value == null) continue;
                    var title = cell.Value.ToString();
                    if (title.Length <= 0) continue;
                    if (title == titleInfo.title)
                    {
                        SwitchColomn(st, i, titleIndex);
                        find = true;
                    }
                }
                if (!find)
                {
                    //原来的配置中没有找着，插入新的一列
                    st.Cells.InsertColumn(i);
                    st.Cells[ExcelConst.TITLE_ROW, i].Value = titleInfo.title;
                }

                //下拉菜单
                if (titleInfo.menus != null && titleInfo.menus.Count > 0)
                {
                    AddCombobox(st, titleInfo.menus, i);
                }
            }
            //冻结标题栏
            st.FreezePanes(ExcelConst.TITLE_ROW + 1, ExcelConfig.Instance.defaultTitles.Count, ExcelConst.TITLE_ROW + 1, ExcelConfig.Instance.defaultTitles.Count);

            //列
            var colStyle = wb.CreateStyle();
            colStyle.Font.Size = 11;
            for (int i = 0; i < sectionList.Count; ++i)
            {
                st.Cells.ApplyColumnStyle(i, colStyle, new StyleFlag() { All = true });
                st.Cells.Columns[i].Width = 9.6;
                var titleInfo = sectionList[i];
                AddComment(wb, st, titleInfo, i);
            }

            {//标题栏
                //标题栏高度
                st.Cells.SetRowHeight(ExcelConst.TITLE_ROW, ExcelConfig.Instance.titleHeight);

                //标题栏样式
                var rowStyle = wb.CreateStyle();
                var colorValue = ExcelConst.color;
                rowStyle.ForegroundColor = Color.FromArgb((int)colorValue);
                //rowStyle.ForegroundColor = Color.Red;
                rowStyle.HorizontalAlignment = TextAlignmentType.Center;
                rowStyle.IsTextWrapped = true;
                rowStyle.Font.Size = 12;
                //一定要设置 不然颜色不生效
                rowStyle.Pattern = BackgroundType.Solid;
                st.Cells.GetRow(ExcelConst.TITLE_ROW).ApplyStyle(rowStyle, new StyleFlag() { All = true, });
            }
            wb.Save(path);
            wb.Dispose();
        }

        private void AddComment(Workbook wb, Worksheet st, ExcelTitle title, int column)
        {
            var cell = st.Cells[ExcelConst.TITLE_ROW, column];
            if (cell == null) return;

            CellArea area = new CellArea();
            area.StartRow = ExcelConst.TITLE_ROW;
            area.EndRow = ExcelConst.TITLE_ROW;
            area.StartColumn = column;
            area.EndColumn = column;
            int validationIndex = st.Validations.Add(area);
            Validation validation = st.Validations[validationIndex];
            validation.Type = (ValidationType.TextLength);
            validation.InputMessage = title.titleComment;
            validation.InputTitle = title.title;
        }
        private void AddCombobox(Worksheet st, List<string> menus, int index)
        {
            CellArea area = new CellArea();
            area.StartRow = ExcelConst.TITLE_ROW + 1;
            area.EndRow = st.Cells.MaxRow +1000;
            area.StartColumn = index;
            area.EndColumn = index;
            string strMenus = "";
            foreach (var it in menus)
            {
                if (strMenus.Length > 0)
                    strMenus += ",";
                strMenus += it;
            }
            int validationIndex = st.Validations.Add(area);
            Validation validation = st.Validations[validationIndex];
            // Set the validation type.
            validation.Type = (ValidationType.List);
            // Set the in cell drop down.
            validation.IgnoreBlank = (true);
            validation.InCellDropDown = (true);
            validation.ShowError = (true);
            validation.ErrorTitle = ("Error");
            validation.ErrorMessage = ("You must select one of the value available in the drop-down list box");
            validation.ShowInput = (true);
            validation.InputMessage = ("");
            // Specify the validation area of cells
            // Set the formula1.
            validation.Formula1 = strMenus;
            // Add the Validation area.
            //validation.AreaList.Add(area);

            //Note : strValidation are just strings that
            //have the folowing format value1;value2;value3;value4
        }
        private bool CheckColumnIndexValid(Worksheet st, int colIndex)
        {
            return colIndex >= 0 && colIndex <= st.Cells.MaxColumn;
        }

        private void SwitchColomn(Worksheet st, int colIndex1, int colIndex2)
        {
            if (colIndex1 == colIndex2) return;
            if (!CheckColumnIndexValid(st, colIndex1)) return;
            if (!CheckColumnIndexValid(st, colIndex2)) return;

            //中转
            st.Cells.CopyColumn(st.Cells, colIndex1, 0);

            st.Cells.CopyColumn(st.Cells, colIndex2, colIndex1);
            st.Cells.CopyColumn(st.Cells, 0, colIndex2);
        }
    }
}
