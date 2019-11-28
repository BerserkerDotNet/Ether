using Ether.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Ether.EmailGenerator
{
    public partial class Main : Window
    {
        private readonly EtherApiClient _etherClient;
        private readonly EmailGeneratorService _generator;
        private IEnumerable<ProfileViewModel> _profiles;
        private IEnumerable<TeamAttendanceViewModel> _teamAttendances;

        public Main(EtherApiClient client, EmailGeneratorService generator)
        {
            InitializeComponent();
            _etherClient = client;
            _generator = generator;
        }

        protected override async void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            InitializeTeamGrid();

            await Execute(async () =>
            {
                var token = await _etherClient.GetToken()
                    .ConfigureAwait(false);
                _etherClient.SetAccessToken(token);

                _profiles = await _etherClient.GetProfiles()
                    .ConfigureAwait(false);

                Dispatcher.Invoke(() =>
                {
                    foreach (var profile in _profiles)
                    {
                        _team.Items.Add(new ComboBoxItem { Tag = profile.Id, Content = profile.Name });
                    }
                });
            });
        }

        private void InitializeTeamGrid()
        {
            var memberNameColumn = new DataGridTextColumn
            {
                Header = "Member",
                Width = DataGridLength.SizeToCells,
                Binding = new Binding("MemberName"),
                IsReadOnly = true
            };
            _teamAttendance.Columns.Add(memberNameColumn);

            var idx = 0;
            foreach (var dayOfWeek in new[] {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday" })
            {
                var btn = new FrameworkElementFactory(typeof(Button));
                btn.SetValue(Button.TagProperty, idx);
                btn.SetBinding(Button.ContentProperty, new Binding($"Attendance[{idx}]") { Converter = new BoolToStatusConverter() });
                btn.SetBinding(Button.BackgroundProperty, new Binding($"Attendance[{idx}]") { Converter = new BoolToColorConverter() });
                btn.AddHandler(Button.ClickEvent, new RoutedEventHandler((s, e) =>
                {
                    var cellIdx = (int)((Button)s).Tag;
                    var item = (TeamAttendanceViewModel)_teamAttendance.SelectedItem;
                    item.Attendance[cellIdx] = !item.Attendance[cellIdx];
                    CollectionViewSource.GetDefaultView(_teamAttendance.ItemsSource).Refresh();
                }));

                var column = new DataGridTemplateColumn
                {
                    Header = dayOfWeek,
                    Width = 100,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate
                    {
                        VisualTree = btn
                    }
                };
                _teamAttendance.Columns.Add(column);
                idx++;
            }

            foreach (var dayOfWeek in new[] { "Saturday", "Sunday" })
            {
                var tb = new FrameworkElementFactory(typeof(TextBlock));
                tb.SetValue(TextBlock.BackgroundProperty, Brushes.LightGray);
                var column = new DataGridTemplateColumn
                {
                    Header = dayOfWeek,
                    Width = 100,
                    IsReadOnly = true,
                    CellTemplate = new DataTemplate
                    {
                        VisualTree = tb
                    }
                };
                _teamAttendance.Columns.Add(column);
                idx++;
            }

        }

        private async void OnGenerate(object sender, RoutedEventArgs e)
        {
            await Execute(async () =>
            {
                var selectedProfile = _team.SelectedItem as ComboBoxItem;
                var profileId = (Guid)selectedProfile.Tag;
                var profile = _profiles.Single(p => p.Id == profileId);

                var report = await _etherClient.GenerateWorkItemsReport(profileId);

                _generator.Generate(_teamAttendances, report, profile, _points.Text);

                await Reset();
            });
        }

        private async void OnReset(object sender, RoutedEventArgs e)
        {
            await Reset();
        }

        private async Task Reset()
        {
            await Execute(async () =>
            {
                _teamAttendances.AsParallel().ForAll(t => t.Attendance = GetDefaultAttendance());
                await Dispatcher.InvokeAsync(() =>
                {
                    _points.Text = string.Empty;
                    CollectionViewSource.GetDefaultView(_teamAttendance.ItemsSource).Refresh();
                });
            });
        }

        private async void OnTeamSelected(object sender, SelectionChangedEventArgs e)
        {
            await Execute(async () =>
            {
                var selected = _team.SelectedItem as ComboBoxItem;
                var id = (Guid)selected.Tag;
                var profile = _profiles.Single(p => p.Id == id);

                var members = await _etherClient.GetTeamMembers()
                    .ConfigureAwait(false);

                _teamAttendances = members
                    .Where(m => profile.Members.Contains(m.Id))
                    .Select(m => new TeamAttendanceViewModel
                    {
                        MemberId = m.Id,
                        MemberName = m.DisplayName,
                        Attendance = GetDefaultAttendance()
                    }).ToList();

                Dispatcher.Invoke(() =>
                {
                    _teamAttendance.ItemsSource = _teamAttendances;
                });
            });
        }

        private async Task Execute(Func<Task> action)
        {
            try
            {
                _spinner.Visibility = Visibility.Visible;
                await action();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                _spinner.Visibility = Visibility.Hidden;
            }
        }

        private bool[] GetDefaultAttendance()
        {
            return new[] { true, true, true, true, true };
        }
    }
}
