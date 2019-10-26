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
            }
            else
            {
                wb = new Workbook();
                wb.Worksheets[0].Name = table.alias;
            }
            //Console.WriteLine("excel format:" + wb.FileFormat);
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
                var colIndex = i + 1;
                bool find = false;
                for (int titleIndex = 0; titleIndex <= st.Cells.MaxDataColumn; ++titleIndex)
                {
                    var cell = st.Cells[ExcelConst.TITLE_ROW, titleIndex];
                    if (cell == null || cell.Value == null) continue;
                    var title = cell.Value.ToString();
                    if (title.Length <= 0) continue;
                    if (title == titleInfo.title)
                    {
                        SwitchColomn(st, colIndex, titleIndex);
                        find = true;
                    }
                }
                if (!find)
                {
                    //原来的配置中没有找着，插入新的一列
                    st.Cells.InsertColumn(colIndex);
                    st.Cells[ExcelConst.TITLE_ROW, colIndex].Value = titleInfo.title;
                }

                //下拉菜单
                if (titleInfo.menus != null && titleInfo.menus.Count > 0)
                {
                    AddCombobox(st, titleInfo.menus, colIndex);
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
            //隐藏第一列（用来作交换数据的列）
            //st.Cells.Columns[0].IsHidden = true;
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
            area.EndRow = st.Cells.MaxRow + 1000;
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

        public void ReadExcel2Templ(TableUnitConfig table, string path, string configAlias)
        {
            Workbook wb;
            try
            {
                wb = new Workbook(path);
            }
            catch (Exception e)
            {
                throw new DataException(ERROR_CODE.OPEN_EXCEL_FAILED_WHEN_READ_EXCEL, string.Format("excel read failed:alias={0},path={1},error={2}", table.alias, path, e.Message));
            }
            var st = wb.Worksheets[table.alias];
            if (st == null)
            {
                throw new DataException(ERROR_CODE.LACK_EXCEL_SHEET, string.Format("excel do not have {0} sheet,path={1}", table.alias, path));
            }
            var sectionList = table.GetExcelTitles();
            if (sectionList.Count <= ExcelConfig.Instance.defaultTitles.Count)
            {
                throw new DataException(ERROR_CODE.EXCEL_TITLES_NO_DATA, string.Format("excel titles no data:alias={0},path={1}", table.alias, path));
            }
            var data = new ExcelDataConfig(table.alias, configAlias);
            for (int i = 0; i < sectionList.Count; ++i)
            {
                var colIndex = i + 1;
                var section = sectionList[i];
                var cell = st.Cells[ExcelConst.TITLE_ROW, colIndex];
                if (cell == null || cell.Value.ToString() != section.title)
                {
                    throw new DataException(ERROR_CODE.EXCEL_TITLE_NOT_MATCH_DEFINE, string.Format("excel title not match define:alias={0},path={1},columnIndex={2}", table.alias, path, colIndex + 1));
                }
                //加入标题信息
                data.titles.Add(new ExcelDataTitleConfig() { alias = section.title, type = section.typeAlias });
            }
            for (int row = ExcelConst.TITLE_ROW + 1; row < st.Cells.MaxDataRow; ++row)
            {
                var idCell = st.Cells[row, 1];
                if (idCell == null || !int.TryParse(idCell.Value.ToString(), out int id))
                {
                    throw new DataException(ERROR_CODE.EXCEL_DATA_ID_INVALID, string.Format("excel id invalid:alias={0},path={1},rowIndex={2}", table.alias, path, row + 1));
                }
                var lineData = new List<string>();
                for (int col = 0; col < sectionList.Count; ++col)
                {
                    var realCol = col + 1;
                    var cell = st.Cells[row, realCol];
                    var cellValue = cell == null ? string.Empty : cell.Value.ToString();
                    lineData.Add(cellValue);
                }
                data.data[id] = lineData;
            }
            string templInfoPath = TableGlobalConfig.Instance.tableTemplDataPath + table.name + ".lua";
            try
            {
                data.WriteToFile(templInfoPath);
            }
            catch (Exception e)
            {
                throw new DataException(ERROR_CODE.EXCEL_INFO_WRITE_TO_FILE_FAILED, string.Format("excel info write to tempinfo file failed:alias={0},path={1},templInfoPath={2},error=[{3}]", table.alias, path, templInfoPath, e.Message));
            }
        }
    }
}
