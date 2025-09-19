using System.Net.Http.Headers;
using System.Text.Json;

var client = new HttpClient();

// Login to get token
var loginData = new { email = "admin@elibrary.com", password = "Admin@123" };
var loginJson = JsonSerializer.Serialize(loginData);
var loginContent = new StringContent(loginJson, System.Text.Encoding.UTF8, "application/json");

var loginResponse = await client.PostAsync("https://localhost:7125/api/auth/login", loginContent);
if (!loginResponse.IsSuccessStatusCode)
{
    Console.WriteLine("Login failed");
    return;
}

var loginResult = await loginResponse.Content.ReadAsStringAsync();
var tokenData = JsonSerializer.Deserialize<JsonElement>(loginResult);
var token = tokenData.GetProperty("token").GetString();

Console.WriteLine($"Token: {token}");

// Test borrow records endpoint
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
var borrowResponse = await client.GetAsync("https://localhost:7125/api/borrow/admin/all");

if (borrowResponse.IsSuccessStatusCode)
{
    var borrowData = await borrowResponse.Content.ReadAsStringAsync();
    Console.WriteLine("Borrow records response:");
    Console.WriteLine(borrowData);
}
else
{
    Console.WriteLine($"Failed to get borrow records: {borrowResponse.StatusCode}");
}