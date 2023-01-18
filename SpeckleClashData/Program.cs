// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

// fetch data rest api 
var client = new HttpClient();
var response = await client.GetAsync("https://speckle.xyz/objects/14568be237/ee3f2685bc7b757c3c254c56139dbc32");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);