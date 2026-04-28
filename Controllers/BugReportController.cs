using ETSU_Marketplace.Models;
using ETSU_Marketplace.Services;
using Microsoft.AspNetCore.Mvc;

namespace ETSU_Marketplace.Controllers
{
    /// <summary>
    /// Controller responsible for handling bug report submissions.
    /// Allows users to submit issues which are sent to GitHub via the GitHubIssueService.
    /// </summary>
    public class BugReportController : Controller
    {
        // Service used to create GitHub issues from bug reports
        private readonly GitHubIssueService _gitHubIssueService;

        /// <summary>
        /// Constructor that injects the GitHub issue service.
        /// </summary>
        /// <param name="gitHubIssueService">Service used to submit bug reports to GitHub</param>
        public BugReportController(GitHubIssueService gitHubIssueService)
        {
            _gitHubIssueService = gitHubIssueService;
        }

        /// <summary>
        /// Displays the bug report form.
        /// </summary>
        /// <returns>Bug report form view</returns>
        [HttpGet]
        public IActionResult Index()
        {
            // Return empty form model to the view
            return View(new BugReportForm());
        }

        /// <summary>
        /// Handles submission of a bug report form.
        /// Validates input and sends the report to GitHub.
        /// </summary>
        /// <param name="model">Bug report form data submitted by the user</param>
        /// <returns>Redirects on success or returns view with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BugReportForm model)
        {
            // Check if form input is valid
            if (!ModelState.IsValid)
                return View(model);

            // Attempt to create a GitHub issue
            var success = await _gitHubIssueService.CreateIssueAsync(model);

            if (success)
            {
                // Increment metric for submitted bug reports
                MarketplaceMetrics.BugReportsSubmitted.Inc();

                // Store success message to display after redirect
                TempData["Success"] = "Bug report submitted to GitHub Issues.";

                return RedirectToAction(nameof(Index));
            }

            // Add error message if submission failed
            ModelState.AddModelError("", "Could not submit bug report.");

            return View(model);
        }
    }
}