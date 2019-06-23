using System;
using System.IO;
using Ether.ViewModels;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace Ether.Types.Excel
{
    public class PullRequestsReportToExcelConverter : ReportToExcelConverter
    {
        private static readonly string[] HeaderColumns = new[]
        {
            "Name",
            "Created",
            "Completed",
            "Active",
            "Abandoned",
            "Iterations",
            "Avg iterations",
            "Code quality (%)",
            "Comments",
            "Avg comments",
            "Avg lifespan (days)"
        };

        public override byte[] Convert(ReportViewModel report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var prReport = report as PullRequestReportViewModel;
            if (prReport == null)
            {
                throw new NotSupportedException($"Report of type {report.GetType()} is not supported by {nameof(PullRequestsReportToExcelConverter)}");
            }

            var memory = new MemoryStream();
            {
                var workbook = new XSSFWorkbook();
                var excelSheet = workbook.CreateSheet($"{prReport.ProfileName}_{prReport.StartDate.ToString("yyyy_MM_dd")}_{prReport.EndDate.ToString("yyyy_MM_dd")}");
                SetHeader(excelSheet);

                var rowIdx = 1;
                foreach (var reportEntry in prReport.IndividualReports)
                {
                    var cellIdx = 0;
                    var row = excelSheet.CreateRow(rowIdx);
                    row.CreateCell(cellIdx++, CellType.String).SetCellValue(reportEntry.TeamMember);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.Created);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.Completed);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.Active);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.Abandoned);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.TotalIterations);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.AverageIterations.ToString("F2"));
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.CodeQuality.ToString("F2"));
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.TotalComments);
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.AverageComments.ToString("F2"));
                    row.CreateCell(cellIdx++, CellType.Numeric).SetCellValue(reportEntry.AveragePRLifespan.TotalDays.ToString("F2"));
                    rowIdx++;
                }

                var summaryRow1 = excelSheet.CreateRow(rowIdx++);
                var summaryRow2 = excelSheet.CreateRow(rowIdx++);
                var summaryRow3 = excelSheet.CreateRow(rowIdx++);
                var summaryRow4 = excelSheet.CreateRow(rowIdx++);
                InitCells(summaryRow1, cellsCount: 7);
                InitCells(summaryRow2, cellsCount: 7);
                InitCells(summaryRow3, cellsCount: 7);
                InitCells(summaryRow4, cellsCount: 7);

                CreateSummaryRow(excelSheet, summaryRow1, "Total created:", "Avg iterations:", prReport.TotalCreated, prReport.AverageIterations);
                CreateSummaryRow(excelSheet, summaryRow2, "Total completed:", "Avg comments:", prReport.TotalCompleted, prReport.AverageComments);
                CreateSummaryRow(excelSheet, summaryRow3, "Total active:", "Code quality:", prReport.TotalActive, prReport.CodeQuality);
                CreateSummaryRow(excelSheet, summaryRow4, "Total abandoned:", "Avg lifespan:", prReport.TotalAbandoned, prReport.AveragePRLifespan.TotalDays);

                workbook.Write(memory);
                return memory.ToArray();
            }
        }

        private static void CreateSummaryRow(ISheet sheet, IRow row, string header1, string header2, double value1, double value2)
        {
            var greyBackground = sheet.Workbook.CreateFont();
            greyBackground.Boldweight = (short)FontBoldWeight.Bold;
            var greyStyle = sheet.Workbook.CreateCellStyle();
            greyStyle.SetFont(greyBackground);
            greyStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            greyStyle.FillPattern = FillPattern.SolidForeground;

            var greenBackground = sheet.Workbook.CreateFont();
            var greenStyle = sheet.Workbook.CreateCellStyle();
            greenStyle.SetFont(greenBackground);
            greenStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
            greenStyle.FillPattern = FillPattern.SolidForeground;

            var totalCreatedHeader = row.GetCell(0);
            totalCreatedHeader.SetCellValue(header1);
            totalCreatedHeader.CellStyle = greyStyle;
            sheet.AddMergedRegion(new CellRangeAddress(row.RowNum, row.RowNum, 1, 2));
            row.GetCell(1).SetCellValue(value1.ToString("F2"));
            row.GetCell(1).CellStyle = greenStyle;

            sheet.AddMergedRegion(new CellRangeAddress(row.RowNum, row.RowNum, 3, 4));
            var avgIterationsHeader = row.GetCell(3);
            avgIterationsHeader.SetCellValue(header2);
            avgIterationsHeader.CellStyle = greyStyle;
            sheet.AddMergedRegion(new CellRangeAddress(row.RowNum, row.RowNum, 5, 6));
            row.GetCell(5).SetCellValue(value2.ToString("F2"));
            row.GetCell(5).CellStyle = greenStyle;
        }

        private void SetHeader(ISheet sheet)
        {
            var boldFont = sheet.Workbook.CreateFont();
            boldFont.Boldweight = (short)FontBoldWeight.Bold;
            var boldStyle = sheet.Workbook.CreateCellStyle();
            boldStyle.SetFont(boldFont);

            var row = sheet.CreateRow(0);
            int idx = 0;
            foreach (var column in HeaderColumns)
            {
                var cell = row.CreateCell(idx, CellType.String);
                cell.SetCellValue(column);
                cell.CellStyle = boldStyle;
                idx++;
            }
        }

        private void InitCells(IRow row, int cellsCount)
        {
            for (int i = 0; i < cellsCount; i++)
            {
                row.CreateCell(i);
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
