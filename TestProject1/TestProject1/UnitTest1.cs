using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestProject1;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    private readonly string _baseUrl = "http://localhost:5238"; // Замените на ваш базовый URL

    [Test]
    public async Task SendTextEndpoint_Returns_Correct_Response_For_Init_Chat_Multiple_Times()
    {
        var tasks = new List<Task>();
    
        for (var i = 0; i < 600; i++)
        {
            tasks.Add(SendTextEndpoint_Returns_Correct_Response_For_Init_Chat(i));
        }

        await Task.WhenAll(tasks);
    }
    
    private async Task SendTextEndpoint_Returns_Correct_Response_For_Init_Chat(int id)
    {
        const string incomeMessage = "test";
        const string typeChat = "dialog";
        var token = Guid.NewGuid().ToString().Replace("-",$"{id}");

        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine("---------------");
            Console.WriteLine($"FOR USER {token}");
            Console.WriteLine("---------------");
            var actualResponse = await SendPostRequest($"{_baseUrl}/send-text/{incomeMessage}&{typeChat}&{token}");
            Console.WriteLine(actualResponse);
            Console.WriteLine("---------------");
        }
    }

    private async Task<string> SendPostRequest(string url)
    {
        using var client = new HttpClient();
        var response = await client.PostAsync(url, null);
        response.EnsureSuccessStatusCode();
        var resultString = new StringBuilder();
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var streamReader = new StreamReader(responseStream, Encoding.UTF8);

        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync() ?? string.Empty;

            if (!line.Contains("data: "))
                continue;

            var dataIndex = line.IndexOf("data: ", StringComparison.Ordinal) + 6;

            var json = line[dataIndex..];

            var messageDto = JsonSerializer.Deserialize<PythonMessageDto>(json);

            resultString.Append(messageDto?.Message ?? string.Empty);
        }
        return resultString.ToString();
    }
    
    public class PythonMessageDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("sender")]
        public string Sender { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type_message")]
        public string TypeMessage { get; set; }
    }
}