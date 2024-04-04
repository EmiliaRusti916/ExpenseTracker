using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDBContext _dbContext;

        public DashboardController(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ActionResult> Index()
        {
            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;

            List<Transaction> selectedTransactions = await _dbContext.Transactions.Include(x => x.Category).Where(y => y.Date >= startDate && y.Date <= endDate).ToListAsync();

            int totalIncome = selectedTransactions.Where(x => x.Category.Type == "Income").Sum(y => y.Amount);
            ViewBag.TotalIncome = totalIncome.ToString("C0");

            int totalExpense = selectedTransactions.Where(x => x.Category.Type == "Expense").Sum(y => y.Amount);
            ViewBag.TotalExpense = totalExpense.ToString("C0");

            int balance = totalIncome - totalExpense;
            ViewBag.Balance = balance.ToString("C0");
      

            ViewBag.DonutChartData = selectedTransactions.Where(i => i.Category.Type == "Expense").
                GroupBy(j => j.Category.CategoryID).
                Select(k => new {
                    categoryWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    specificAmount = k.Sum(j => j.Amount).ToString("C0")

                }).OrderByDescending(l => l.amount).ToList();

            List<SplineChartData> incomeSummary = selectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            //Expense
            List<SplineChartData> ixpenseSummary = selectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            //Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in incomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ixpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
            ViewBag.RecentTransactions = await _dbContext.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}
