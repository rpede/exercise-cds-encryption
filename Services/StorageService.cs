using System.Text.Json;

namespace cds_encryption.Services;

/// <summary>
/// Facilitate storage for <c>EncryptedMessage</c>.
/// </summary
public interface IStorageService
{
    EncryptedMessage Load();
    void Save(EncryptedMessage message);
}

/// <summary>
/// Implementation storing as JSON in a file.
/// </summary
public class StorageService : IStorageService
{
    private readonly string _filename;

    public StorageService(string filename)
    {
        _filename = filename;
    }

    public static StorageService CreateWithDefaultPath()
    {
        // Set a variable to the Documents path.
        var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // Set file path for encrypted message
        var filename = Path.Combine(docPath, "secret-message.json");

        return new StorageService(filename);
    }

    public void Save(EncryptedMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        using var outputFile = new StreamWriter(_filename);
        outputFile.WriteLine(json);
    }

    public EncryptedMessage Load()
    {
        using var reader = new StreamReader(_filename);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<EncryptedMessage>(json)!;
    }
}
