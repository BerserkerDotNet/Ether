using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ether.Types.Excel
{
    public class WorkItemsReportToExcelConverter : ReportToExcelConverter
    {
        public override byte[] Convert(ReportViewModel report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var prReport = report as WorkItemsReportViewModel;
            if (prReport == null)
            {
                throw new NotSupportedException($"Report of type {report.GetType()} is not supported by {nameof(WorkItemsReportToExcelConverter)}");
            }

            var memory = new MemoryStream();
            {
                var workbook = new XSSFWorkbook();
                CreateResolvedWorkItemsReport(workbook, "Resolved Work Items", prReport.ResolvedWorkItems, prReport);
                CreateWorkItemsInPullRequestReport(workbook, "Work Items In Pull Request", prReport.WorkItemsInReview, prReport);
                CreateActiveWorkItemsReport(workbook, "Active Work Items", prReport.ActiveWorkItems, prReport);

                workbook.Write(memory);
                return memory.ToArray();
            }
        }

        private ExcelColumn[] GetHeaderColumns() => new[]
        {
            new ExcelColumn("Workitem"),
            new ExcelColumn("Title"),
            new ExcelColumn("Type"),
            new ExcelColumn("Tags", false),
            new ExcelColumn("Reason", false),
            new ExcelColumn("Estimated (Days)"),
            new ExcelColumn("Time Spent (Days)")
        };

        private bool ShouldInclude(ExcelColumn[] columns, string columnName) => columns.Single(col => col.Name == columnName).IsDisplayed;

        private void CreateResolvedWorkItemsReport(XSSFWorkbook workbook, string sectionName, IEnumerable<WorkItemDetail> workItems, WorkItemsReportViewModel prReport)
        {
            var headerColumns = GetHeaderColumns();
            headerColumns.Single(x => x.Name == "Reason").IsDisplayed = true;
            CreateReportSheet(workbook, sectionName, workItems, prReport, headerColumns);
        }

        private void CreateWorkItemsInPullRequestReport(XSSFWorkbook workbook, string sectionName, IEnumerable<WorkItemDetail> workItems, WorkItemsReportViewModel prReport)
        {
            var headerColumns = GetHeaderColumns();
            CreateReportSheet(workbook, sectionName, workItems, prReport, headerColumns);
        }

        private void CreateActiveWorkItemsReport(XSSFWorkbook workbook, string sectionName, IEnumerable<WorkItemDetail> workItems, WorkItemsReportViewModel prReport)
        {
            var headerColumns = GetHeaderColumns();
            headerColumns.Single(x => x.Name == "Tags").IsDisplayed = true;
            CreateReportSheet(workbook, sectionName, workItems, prReport, headerColumns);
        }

        private void CreateReportSheet(XSSFWorkbook workbook, string sectionName, IEnumerable<WorkItemDetail> workItems, WorkItemsReportViewModel prReport, ExcelColumn[] columns)
        {
            var excelSheet = workbook.CreateSheet(sectionName);
            var creationHelper = workbook.GetCreationHelper();

            var hlinkstyle = workbook.CreateCellStyle();
            var hlinkfont = workbook.CreateFont();
            hlinkfont.Underline = FontUnderlineType.Single;
            hlinkfont.Color = HSSFColor.Blue.Index;
            hlinkstyle.SetFont(hlinkfont);
            var includeTags = ShouldInclude(columns, "Tags");
            var includeReason = ShouldInclude(columns, "Reason");

            SetHeader(excelSheet, columns);
            int rowIdx = 1, cellIdx;
            foreach (var reportEntry in workItems)
            {
                cellIdx = 0;
                var row = excelSheet.CreateRow(rowIdx);
                var idCell = row.CreateCell(cellIdx++, CellType.String);
                idCell.SetCellValue(reportEntry.WorkItemId);
                var link = creationHelper.CreateHyperlink(HyperlinkType.Url);
                link.Address = $"https://dynamicscrm.visualstudio.com/{reportEntry.WorkItemProject}/_workitems/edit/{reportEntry.WorkItemId}";
                idCell.Hyperlink = link;

                row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.WorkItemTitle);
                row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.WorkItemType);
                if (includeTags)
                {
                    row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.Tags);
                }

                if (includeReason)
                {
                    row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.Reason);
                }

                row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.EstimatedToComplete);
                row.CreateCell(cellIdx, CellType.Numeric).SetCellValue(reportEntry.TimeSpent);
                rowIdx++;
            }

            var summaryRow = excelSheet.CreateRow(rowIdx);
            cellIdx = 0;
            summaryRow.CreateCell(cellIdx++, CellType.String).SetCellValue("Total: ");
            summaryRow.CreateCell(cellIdx++, CellType.String).SetCellValue($"{prReport.GetTotalBugs(workItems)} bugs / {prReport.GetTotalTasks(workItems)} tasks");
            summaryRow.CreateCell(cellIdx++, CellType.String);
            if (includeTags)
            {
                _ = summaryRow.CreateCell(cellIdx++, CellType.String);
            }

            if (includeReason)
            {
                _ = summaryRow.CreateCell(cellIdx++, CellType.String);
            }

            summaryRow.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(prReport.GetTotalEstimated(workItems));
            summaryRow.CreateCell(cellIdx, CellType.Numeric).SetCellValue(prReport.GetTotalTimeSpent(workItems));

            // AutosizeCells(excelSheet, summaryRow.Cells.Count);
        }

        private void SetHeader(ISheet sheet, ExcelColumn[] columns)
        {
            var boldFont = sheet.Workbook.CreateFont();
            boldFont.Boldweight = (short)FontBoldWeight.Bold;
            var boldStyle = sheet.Workbook.CreateCellStyle();
            boldStyle.SetFont(boldFont);

            var row = sheet.CreateRow(0);
            int idx = 0;
            foreach (var column in columns.Where(col => col.IsDisplayed))
            {
                var cell = row.CreateCell(idx, CellType.String);
                cell.SetCellValue(column.Name);
                cell.CellStyle = boldStyle;
                idx++;
            }
        }

        private void AutosizeCells(ISheet sheet, int cellsCount)
        {
            for (int i = 0; i < cellsCount; i++)
            {
                sheet.AutoSizeColumn(i);
            }
        }
    }
}
