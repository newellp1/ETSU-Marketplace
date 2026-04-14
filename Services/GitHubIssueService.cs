using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ETSU_Marketplace.Models;

namespace ETSU_Marketplace.Services
{
    public class GitHubIssueService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public GitHubIssueService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<bool> CreateIssueAsync(BugReportForm bugReport)
        {
            var owner = _config["GitHub:Owner"];
            var repo = _config["GitHub:Repo"];
            var token = _config["GitHub:Token"];

            if (string.IsNullOrWhiteSpace(owner) ||
                string.IsNullOrWhiteSpace(repo) ||
                string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var body = new StringBuilder();
            body.AppendLine("## Bug Report");
            body.AppendLine();
            body.AppendLine(bugReport.Description);

            var payload = new
            {
                title = bugReport.Title,
                body = body.ToString(),
                labels = new[] { "bug" }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.github.com/repos/{owner}/{repo}/issues"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("ETSU-Marketplace", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}