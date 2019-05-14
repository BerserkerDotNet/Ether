using System;
using System.Collections.Generic;
using System.IO;
using Ether.ViewModels;
using Ether.ViewModels.Types;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ether.Types.Excel
{
    public class WorkItemsReportToExcelConverter : ReportToExcelConverter
    {
        private static readonly string[] HeaderColumns = new[]
        {
            "Workitem",
            "Title",
            "Type",
            "Estimated (Days)",
            "Time Spent (Days)",
        };

        private static readonly string[] HeaderColumnsWithTags = new[]
        {
            "Workitem",
            "Title",
            "Type",
            "Tags",
            "Estimated (Days)",
            "Time Spent (Days)",
        };

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
                CreateReportSheet(workbook, "Resolved Work Items", prReport.ResolvedWorkItems, prReport);
                CreateReportSheet(workbook, "Work Items In Pull Request", prReport.WorkItemsInReview, prReport);
                CreateReportSheet(workbook, "Active Work Items", prReport.ActiveWorkItems, prReport, true);

                workbook.Write(memory);
                return memory.ToArray();
            }
        }

        private void CreateReportSheet(XSSFWorkbook workbook, string sectionName, IEnumerable<WorkItemDetail> workItems, WorkItemsReportViewModel prReport, bool includeTags = false)
        {
            var excelSheet = workbook.CreateSheet(sectionName);
            var creationHelper = workbook.GetCreationHelper();

            var hlinkstyle = workbook.CreateCellStyle();
            var hlinkfont = workbook.CreateFont();
            hlinkfont.Underline = FontUnderlineType.Single;
            hlinkfont.Color = HSSFColor.Blue.Index;
            hlinkstyle.SetFont(hlinkfont);

            SetHeader(excelSheet, includeTags);
            int rowIdx = 1, cellIdx;
            foreach (var reportEntry in workItems)
            {
                cellIdx = 0;
                var row = excelSheet.CreateRow(rowIdx);

                var idCell = row.CreateCell(cellIdx++, CellType.String);
                idCell.SetCellValue(reportEntry.WorkItemId);
                // idCell.CellStyle = hlinkstyle;
                var link = creationHelper.CreateHyperlink(HyperlinkType.Url);
                link.Address = $"https://dynamicscrm.visualstudio.com/{reportEntry.WorkItemProject}/_workitems/edit/{reportEntry.WorkItemId}";
                idCell.Hyperlink = link;

                row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.WorkItemTitle);
                row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.WorkItemType);
                if (includeTags)
                {
                    row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.Tags);
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

            summaryRow.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(prReport.GetTotalEstimated(workItems));
            summaryRow.CreateCell(cellIdx, CellType.Numeric).SetCellValue(prReport.GetTotalTimeSpent(workItems));

            // AutosizeCells(excelSheet, summaryRow.Cells.Count);
        }

        private void SetHeader(ISheet sheet, bool includeTags)
        {
            var boldFont = sheet.Workbook.CreateFont();
            boldFont.Boldweight = (short)FontBoldWeight.Bold;
            var boldStyle = sheet.Workbook.CreateCellStyle();
            boldStyle.SetFont(boldFont);

            var row = sheet.CreateRow(0);
            int idx = 0;
            var columns = includeTags ? HeaderColumnsWithTags : HeaderColumns;
            foreach (var column in columns)
            {
                var cell = row.CreateCell(idx, CellType.String);
                cell.SetCellValue(column);
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
