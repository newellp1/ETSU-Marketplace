using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers
{
    public class BugReportController : Controller
    {
        private readonly GitHubIssueService _gitHubIssueService;

        public BugReportController(GitHubIssueService gitHubIssueService)
        {
            _gitHubIssueService = gitHubIssueService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new BugReportForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BugReportForm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _gitHubIssueService.CreateIssueAsync(model);

            if (success)
            {
                MarketplaceMetrics.BugReportsSubmitted.Inc();

                TempData["Success"] = "Bug report submitted to GitHub Issues.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Could not submit bug report.");
            return View(model);
        }
    }
}