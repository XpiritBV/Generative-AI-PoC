using System;

namespace GenerativeAi.Functions;

public class AzureStorageSettings
{
    public string Name { get; set; }
    public string Key { get; set; }
    public Uri EndPoint => new($"UseDevelopmentStorage=true");
}