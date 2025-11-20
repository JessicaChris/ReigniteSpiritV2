using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NLWeb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AskController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public AskController(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestModel request)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            var projectId = _config["OpenAI:ProjectId"];

            var url = "https://api.openai.com/v1/chat/completions";

            Console.WriteLine($"🔑 API KEY starts with: {apiKey?.Substring(0, 10)}...");
            Console.WriteLine($"📦 PROJECT ID: {projectId}");

            // Career-only system prompt
            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content =
"You are Alice, a career exploration assistant. You ONLY answer questions about careers, jobs, skills, education paths, salaries, and future job trends. You help users explore ANY field — architecture, design, aviation, medicine, engineering, arts, law, business, trades, IT, etc. If the question is vague, assume they mean career guidance. If it's NOT about careers at all, politely redirect."
                    },
                    new { role = "user", content = request.Question }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            // Project header only if required
            if (!apiKey.StartsWith("sk-proj-") && !string.IsNullOrWhiteSpace(projectId))
            {
                _httpClient.DefaultRequestHeaders.Add("OpenAI-Project-Id", projectId);
            }

            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"📤 Response: {response.StatusCode}");
            Console.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseBody);

            using var doc = JsonDocument.Parse(responseBody);

            var answer = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { answer });
        }
    }

    public class ChatRequestModel
    {
        public string Question { get; set; }
    }
}
