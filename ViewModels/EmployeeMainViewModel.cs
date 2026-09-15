using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LostAndFoundAgency.Commands;
using LostAndFoundAgency.Data;
using LostAndFoundAgency.Models;

namespace LostAndFoundAgency.ViewModels
{
    public class EmployeeMainViewModel : ViewModelBase
    {
        private readonly Employee _currentEmployee;
        private PotentialMatch? _selectedMatch;

        public ObservableCollection<LostRequest> AllLostRequests { get; set; } = new ObservableCollection<LostRequest>();
        public ObservableCollection<FoundItem> AllFoundItems { get; set; } = new ObservableCollection<FoundItem>();
        public ObservableCollection<PotentialMatch> PotentialMatches { get; set; } = new ObservableCollection<PotentialMatch>();
        public ObservableCollection<ReportMetric> ReportMetrics { get; set; } = new ObservableCollection<ReportMetric>();
        public ObservableCollection<CategoryReportRow> CategoryReport { get; set; } = new ObservableCollection<CategoryReportRow>();
        public ObservableCollection<StatusReportRow> StatusReport { get; set; } = new ObservableCollection<StatusReportRow>();
        public ObservableCollection<ReturnReportRow> ReturnReport { get; set; } = new ObservableCollection<ReturnReportRow>();

        public PotentialMatch? SelectedMatch
        {
            get => _selectedMatch;
            set { _selectedMatch = value; OnPropertyChanged(); }
        }

        public ICommand GenerateDataCommand { get; }
        public ICommand RefreshDataCommand { get; }
        public ICommand ConfirmMatchCommand { get; }

        public EmployeeMainViewModel(Employee currentEmployee)
        {
            _currentEmployee = currentEmployee;

            GenerateDataCommand = new RelayCommand(GenerateData);
            RefreshDataCommand = new RelayCommand(o => LoadAllData());
            ConfirmMatchCommand = new RelayCommand(ConfirmMatch, o => SelectedMatch != null);

            LoadAllData();
        }

        private void LoadAllData()
        {
            using (var context = new AppDbContext())
            {
                var lost = context.LostRequests
                    .Include(r => r.Category)
                    .Include(r => r.Location)
                    .Include(r => r.Person)
                    .AsNoTracking()
                    .OrderByDescending(r => r.RequestDate)
                    .ToList();

                var found = context.FoundItems
                    .Include(i => i.Category)
                    .Include(i => i.Location)
                    .Include(i => i.Finder)
                    .AsNoTracking()
                    .OrderByDescending(i => i.DateFound)
                    .ToList();

                AllLostRequests = new ObservableCollection<LostRequest>(lost);
                AllFoundItems = new ObservableCollection<FoundItem>(found);

                var activeLost = lost.Where(r => r.Status == ItemStatuses.Pending).ToList();
                var activeFound = found.Where(i => i.Status == ItemStatuses.Pending).ToList();
                var categories = context.Categories.AsNoTracking().ToList();

                var matchesList = (from l in activeLost
                                   from f in activeFound
                                   where l.CategoryId == f.CategoryId
                                   where f.DateFound >= l.DateLost
                                   where ContainsKeywords(l.ItemName, f.ItemName)
                                   select new PotentialMatch
                                   {
                                       Lost = l,
                                       Found = f,
                                       CategoryName = categories.FirstOrDefault(c => c.CategoryId == l.CategoryId)?.CategoryName ?? "Загальна"
                                   }).ToList();

                PotentialMatches = new ObservableCollection<PotentialMatch>(matchesList);
                LoadReports(context, lost, found);

                OnPropertyChanged(nameof(AllLostRequests));
                OnPropertyChanged(nameof(AllFoundItems));
                OnPropertyChanged(nameof(PotentialMatches));
                OnPropertyChanged(nameof(ReportMetrics));
                OnPropertyChanged(nameof(CategoryReport));
                OnPropertyChanged(nameof(StatusReport));
                OnPropertyChanged(nameof(ReturnReport));
            }
        }

        private void LoadReports(AppDbContext context, List<LostRequest> lost, List<FoundItem> found)
        {
            var matches = context.Matches.AsNoTracking().ToList();
            var returns = context.Returns
                .Include(r => r.FoundItem)
                .Include(r => r.Person)
                .Include(r => r.Employee)
                .AsNoTracking()
                .OrderByDescending(r => r.ReturnDate)
                .ToList();

            ReportMetrics = new ObservableCollection<ReportMetric>
            {
                new ReportMetric { Title = "Заяв про втрату", Value = lost.Count, Note = "Усі записи LostRequests" },
                new ReportMetric { Title = "Зареєстровано знахідок", Value = found.Count, Note = "Усі записи FoundItems" },
                new ReportMetric { Title = "Активних у роботі", Value = lost.Count(r => r.Status == ItemStatuses.Pending) + found.Count(i => i.Status == ItemStatuses.Pending), Note = "Очікують обробки" },
                new ReportMetric { Title = "Підтверджених збігів", Value = matches.Count(m => m.MatchStatus == MatchStatuses.Confirmed), Note = "Matches зі статусом підтверджено" },
                new ReportMetric { Title = "Повернень", Value = returns.Count, Note = "Факти видачі речей" }
            };

            var foundByCategory = found.GroupBy(i => i.CategoryId).ToDictionary(g => g.Key, g => g.Count());
            var lostByCategory = lost.GroupBy(r => r.CategoryId).ToDictionary(g => g.Key, g => g.Count());
            var returnedByCategory = returns
                .Where(r => r.FoundItem != null)
                .GroupBy(r => r.FoundItem!.CategoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            CategoryReport = new ObservableCollection<CategoryReportRow>(
                context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.CategoryName)
                    .Select(c => new CategoryReportRow
                    {
                        CategoryName = c.CategoryName,
                        LostCount = lostByCategory.ContainsKey(c.CategoryId) ? lostByCategory[c.CategoryId] : 0,
                        FoundCount = foundByCategory.ContainsKey(c.CategoryId) ? foundByCategory[c.CategoryId] : 0,
                        ReturnedCount = returnedByCategory.ContainsKey(c.CategoryId) ? returnedByCategory[c.CategoryId] : 0
                    })
                    .ToList());

            var allStatuses = lost.Select(r => r.Status)
                .Concat(found.Select(i => i.Status))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            StatusReport = new ObservableCollection<StatusReportRow>(
                allStatuses.Select(status => new StatusReportRow
                {
                    Status = status,
                    LostCount = lost.Count(r => r.Status == status),
                    FoundCount = found.Count(i => i.Status == status)
                }));

            ReturnReport = new ObservableCollection<ReturnReportRow>(
                returns.Select(r => new ReturnReportRow
                {
                    ReturnDate = r.ReturnDate,
                    ItemName = r.FoundItem?.ItemName ?? "-",
                    OwnerName = r.Person?.FullName ?? "-",
                    EmployeeName = r.Employee?.FullName ?? "-",
                    DocumentNumber = r.DocumentNumber ?? "-"
                }));
        }

        private bool ContainsKeywords(string? name1, string? name2)
        {
            if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2)) return false;

            var words1 = name1.ToLower().Split(new[] { ' ', ',', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
            var words2 = name2.ToLower().Split(new[] { ' ', ',', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

            return words1.Any(w => w.Length > 2 && words2.Contains(w)) ||
                   words2.Any(w => w.Length > 2 && words1.Contains(w));
        }

        private void ConfirmMatch(object? obj)
        {
            if (SelectedMatch == null) return;

            var result = MessageBox.Show(
                $"Підтвердити збіг для речі \"{SelectedMatch.ItemName}\"?\nБуде створено запис про збіг і повернення речі.",
                "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            using (var context = new AppDbContext())
            {
                var dbLost = context.LostRequests.FirstOrDefault(r => r.LostRequestId == SelectedMatch.Lost.LostRequestId);
                var dbFound = context.FoundItems.FirstOrDefault(i => i.FoundItemId == SelectedMatch.Found.FoundItemId);

                if (dbLost == null || dbFound == null)
                {
                    MessageBox.Show("Не вдалося знайти записи в базі даних. Оновіть таблиці та спробуйте ще раз.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingMatch = context.Matches.FirstOrDefault(m =>
                    m.FoundItemId == dbFound.FoundItemId &&
                    m.LostRequestId == dbLost.LostRequestId);

                if (existingMatch == null)
                {
                    context.Matches.Add(new Match
                    {
                        FoundItemId = dbFound.FoundItemId,
                        LostRequestId = dbLost.LostRequestId,
                        MatchPercent = CalculateMatchPercent(dbLost, dbFound),
                        MatchStatus = MatchStatuses.Confirmed,
                        Comment = "Збіг підтверджено співробітником.",
                        CreatedAt = DateTime.Now
                    });
                }
                else
                {
                    existingMatch.MatchStatus = MatchStatuses.Confirmed;
                    existingMatch.MatchPercent = CalculateMatchPercent(dbLost, dbFound);
                }

                if (!context.Returns.Any(r => r.FoundItemId == dbFound.FoundItemId))
                {
                    context.Returns.Add(new Return
                    {
                        FoundItemId = dbFound.FoundItemId,
                        PersonId = dbLost.PersonId,
                        EmployeeId = _currentEmployee.EmployeeId,
                        ReturnDate = DateTime.Now,
                        Comment = "Повернення створено автоматично після підтвердження збігу."
                    });
                }

                dbLost.EmployeeId = _currentEmployee.EmployeeId;
                dbFound.EmployeeId = _currentEmployee.EmployeeId;
                dbLost.Status = ItemStatuses.LostClosedFound;
                dbFound.Status = ItemStatuses.FoundClosedReturned;

                context.SaveChanges();

                MessageBox.Show("Збіг підтверджено, повернення записано в базу даних.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAllData();
            }
        }

        private static int CalculateMatchPercent(LostRequest lost, FoundItem found)
        {
            var score = 50;

            if (lost.CategoryId == found.CategoryId)
            {
                score += 20;
            }

            if (found.DateFound >= lost.DateLost)
            {
                score += 15;
            }

            if (string.Equals(lost.ItemName, found.ItemName, StringComparison.CurrentCultureIgnoreCase))
            {
                score += 15;
            }

            return Math.Min(score, 100);
        }

        private void GenerateData(object? obj)
        {
            DatabaseSeeder.GenerateDemoRequests();
            LoadAllData();
            MessageBox.Show("Тестові дані успішно згенеровано! Перевірте вкладки.", "БД", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
