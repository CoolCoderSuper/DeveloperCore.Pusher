namespace TestApp;

public class Config
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 7166;
    public string Key { get; set; } = "key";
    public string ChannelName { get; set; } = "test";
    public string Event { get; set; } = "Nice";
    public string Data { get; set; } = """
                                       {
                                        "name":"hello",
                                        "age":9
                                       }
                                       """;
}