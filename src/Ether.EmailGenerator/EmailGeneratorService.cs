using Ether.ViewModels;
using Ether.ViewModels.Types;
using NetOffice.OutlookApi;
using NetOffice.OutlookApi.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Ether.EmailGenerator
{
    public class EmailGeneratorService
    {
        private string _instanceName;

        public EmailGeneratorService()
        {
            _instanceName = ConfigurationManager.AppSettings.Get("InstanceName");
        }

        public void Generate(IEnumerable<TeamAttendanceViewModel> attendanceViewModel, WorkItemsReportViewModel report, ProfileViewModel profile, string points)
        {
            if (string.IsNullOrEmpty(profile.EmailSubject))
            {
                profile.EmailSubject = $"No subject template for profile '{profile.Name}'";
            }

            if (string.IsNullOrEmpty(profile.EmailBody))
            {
                profile.EmailBody = $"No email template found.\r\nPlease go to 'AzureDevOps -> Profiles -> Edit {profile.Name} -> Email Options' and add email template.";
            }

            var subject = ApplyCommonPlaceholders(profile.EmailSubject, profile);
            var body = ApplyCommonPlaceholders(profile.EmailBody, profile);
            body = ApplyBodyPlaceholders(body, attendanceViewModel, report, points);
            body = body.Replace("style=\"\"", string.Empty);

            var app = new Application();
            var msg = app.CreateItem(OlItemType.olMailItem) as MailItem;
            msg.Subject = subject;
            msg.HTMLBody = body;
            msg.Display();
        }

        private string ApplyCommonPlaceholders(string value, ProfileViewModel profile)
        {
            return value
                .Replace("{Profile}", profile.Name)
                .Replace("{Date}", DateTime.Now.ToString("d"));
        }

        private string ApplyBodyPlaceholders(string value, IEnumerable<TeamAttendanceViewModel> teamAttendance, WorkItemsReportViewModel report, string pointsString)
        {
            var resolvedTable = CreateTable(report.ResolvedWorkItems, "#70AD47");
            var codeReviewTable = CreateTable(report.WorkItemsInReview, "#FFC000");
            var activeTable = CreateTable(report.ActiveWorkItems, "#5B9BD5");
            var points = CreatePoints(pointsString);
            var createTeam = CreateTeamTable(teamAttendance);
            var teamCount = GetTeamCount(teamAttendance);

            return value
                 .Replace("{ResolvedItems}", resolvedTable)
                 .Replace("{InReviewItems}", codeReviewTable)
                 .Replace("{ActiveItems}", activeTable)
                 .Replace("{ResolvedCount}", report.ResolvedWorkItems.Count().ToString())
                 .Replace("{InReviewCount}", report.WorkItemsInReview.Count().ToString())
                 .Replace("{ActiveCount}", report.ActiveWorkItems.Count().ToString())
                 .Replace("{TeamCount}", teamCount.ToString())
                 .Replace("{Points}", points)
                .Replace("{Team}", createTeam);
        }

        private string CreateTable(IEnumerable<WorkItemDetail> items, string color)
        {
            var table = new StringBuilder();
            table.Append($"<table style=\"border: 1px solid {color};border-collapse: collapse;\">");
            table.Append($"<thead style=\"background: {color};color: white;\">");
            table.Append("<tr>");
            table.Append($"<th style=\"border: 1px solid {color};border-right: 1px solid white;\">Id</th>");
            table.Append($"<th style=\"border: 1px solid {color};border-right: 1px solid white;\">Title</th>");
            table.Append($"<th style=\"border: 1px solid {color};border-right: 1px solid white;\">Type</th>");
            table.Append($"<th style=\"border: 1px solid {color};border-right: 1px solid white;\">Estimated (Days)</th>");
            table.Append($"<th style=\"border: 1px solid {color};border-right: 1px solid white;\">Time Spent (Days)</th>");
            table.Append("</tr>");
            table.Append("</thead>");
            table.Append("<tbody>");
            foreach (var item in items)
            {
                table.Append($"<tr>");
                table.Append($"<td style=\"border: solid {color} 1.0pt;border-right: none;margin-left:5pt;margin-right:5pt;\"><a href=\"{GetWorkItemUrl(item)}\">{item.WorkItemId}</a></td>");
                table.Append($"<td style=\"border: solid {color} 1.0pt;border-right: none;margin-left:5pt;margin-right:5pt;\">{item.WorkItemTitle}</td>");
                table.Append($"<td style=\"border: solid {color} 1.0pt;border-right: none;margin-left:5pt;margin-right:5pt;\">{item.WorkItemType}</td>");
                table.Append($"<td style=\"border: solid {color} 1.0pt;border-right: none;margin-left:5pt;margin-right:5pt;\">{item.EstimatedToComplete}</td>");
                table.Append($"<td style=\"border: solid {color} 1.0pt;margin-left:5pt;margin-right:5pt;\">{item.TimeSpent}</td>");
                table.Append("</tr>");
            }

            table.Append("</tbody>");
            table.Append("</table>");

            return table.ToString();
        }

        private string CreateTeamTable(IEnumerable<TeamAttendanceViewModel> attendance)
        {
            var table = new StringBuilder();
            table.Append("<table>");
            table.Append("<thead>");
            table.Append("<tr>");
            table.Append("<th style=\"border: 1px solid black;\">Member</th>");
            table.Append("<th style=\"border: 1px solid black;\">Monday</th>");
            table.Append("<th style=\"border: 1px solid black;\">Tuesday</th>");
            table.Append("<th style=\"border: 1px solid black;\">Wednesday</th>");
            table.Append("<th style=\"border: 1px solid black;\">Thursday</th>");
            table.Append("<th style=\"border: 1px solid black;\">Friday</th>");
            table.Append("<th style=\"border: 1px solid black;\">Saturday</th>");
            table.Append("<th style=\"border: 1px solid black;\">Sunday</th>");
            table.Append("</tr>");
            table.Append("</thead>");
            table.Append("<tbody>");
            foreach (var memberAttendance in attendance)
            {
                table.Append("<tr>");
                table.Append($"<td style=\"border: 1px solid black;\">{memberAttendance.MemberName}</td>");
                table.Append($"<td style=\"background:{GetColor(memberAttendance.Attendance[0])}; border: 1px solid black;width: 60pt;\">{(memberAttendance.Attendance[0] ? "V" : "OOF")}</td>");
                table.Append($"<td style=\"background:{GetColor(memberAttendance.Attendance[1])}; border: 1px solid black;width: 60pt;\">{(memberAttendance.Attendance[1] ? "V" : "OOF")}</td>");
                table.Append($"<td style=\"background:{GetColor(memberAttendance.Attendance[2])}; border: 1px solid black;width: 60pt;\">{(memberAttendance.Attendance[2] ? "V" : "OOF")}</td>");
                table.Append($"<td style=\"background:{GetColor(memberAttendance.Attendance[3])}; border: 1px solid black;width: 60pt;\">{(memberAttendance.Attendance[3] ? "V" : "OOF")}</td>");
                table.Append($"<td style=\"background:{GetColor(memberAttendance.Attendance[4])}; border: 1px solid black;width: 60pt;\">{(memberAttendance.Attendance[4] ? "V" : "OOF")}</td>");
                table.Append("<td style=\"background:#A6A6A6; border: 1px solid black;width: 60pt;\">OOF</td>");
                table.Append("<td style=\"background:#A6A6A6; border: 1px solid black;width: 60pt;\">OOF</td>");
                table.Append("</tr>");
            }

            table.Append("</tbody>");

            table.Append("</table>");

            return table.ToString();
        }

        private int GetTeamCount(IEnumerable<TeamAttendanceViewModel> attendance)
        {
            return (int)Math.Round(attendance.Sum(t => t.Attendance.Count(a => a)) / 5.0d, MidpointRounding.AwayFromZero);
        }

        private string GetColor(bool isAttending)
        {
            return isAttending ? "#A8D08D" : "#FFE599";
        }

        private string CreatePoints(string points)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<ul>");
            foreach (var line in points.Split('\n'))
            {
                sb.Append($"<li>{line}</li>");
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        // TODO: This should be returned by API
        private string GetWorkItemUrl(WorkItemDetail item)
        {
            return $"https://{_instanceName}.visualstudio.com/{item.WorkItemProject}/_workitems/edit/{item.WorkItemId}";
        }
    }
}
