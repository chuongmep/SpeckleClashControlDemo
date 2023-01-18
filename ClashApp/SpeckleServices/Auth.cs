namespace BlazorApp1.SpeckleServices;

public class Authentication
{
    public string Token { get; set; }
    public Authentication(string token)
    {
        Token = token;
    }
    
}