using BetaSharp.IO;
using java.util.logging;
using Exception = System.Exception;
using File = System.IO.File;

namespace BetaSharp.Server;

internal class DedicatedServerConfiguration : IServerConfiguration
{
    public static readonly Logger logger = Logger.getLogger("Minecraft");

    private readonly PropertiesFile _propertiesFile = new();
    private readonly FileInfo _file;

    public DedicatedServerConfiguration(FileInfo fileInfo)
    {
        _file = fileInfo;

        if (File.Exists(_file.FullName))
        {
            try
            {
                _propertiesFile.Load(_file.OpenRead());
            }
            catch (Exception e)
            {
                logger.log(Level.WARNING, $"Failed to load {_file.FullName}", e);
                Save();
            }
        }
        else
        {
            logger.log(Level.WARNING, $"{_file.FullName} does not exist");
            Save();
        }
    }

    public string GetServerIp(string fallback) => GetProperty("server-ip", fallback);
    public int GetServerPort(int fallback) => GetProperty("server-port", fallback);
    public bool GetDualStack(bool fallback) => GetProperty("dual-stack", fallback);
    public bool GetOnlineMode(bool fallback) => GetProperty("online-mode", fallback);
    public bool GetSpawnAnimals(bool fallback) => GetProperty("spawn-animals", fallback);
    public bool GetPvpEnabled(bool fallback) => GetProperty("pvp", fallback);
    public bool GetAllowFlight(bool fallback) => GetProperty("allow-flight", fallback);
    public string GetLevelName(string fallback) => GetProperty("level-name", fallback);
    public string GetLevelSeed(string fallback) => GetProperty("level-seed", fallback);
    public bool GetSpawnMonsters(bool fallback) => GetProperty("spawn-monsters", fallback);
    public bool GetAllowNether(bool fallback) => GetProperty("allow-nether", fallback);
    public int GetMaxPlayers(int fallback) => GetProperty("max-players", fallback);
    public int GetViewDistance(int fallback) => GetProperty("view-distance", fallback);
    public bool GetWhiteList(bool fallback) => GetProperty("white-list", fallback);

    public void Save()
    {
        try
        {
            _propertiesFile.Clear();
            _propertiesFile.SetProperty("level-name", "world");
            _propertiesFile.SetProperty("allow-nether", true);
            _propertiesFile.SetProperty("view-distance", 10);
            _propertiesFile.SetProperty("spawn-monsters", true);
            _propertiesFile.SetProperty("online-mode", true);
            _propertiesFile.SetProperty("spawn-animals", true);
            _propertiesFile.SetProperty("max-players", 20);
            _propertiesFile.SetProperty("server-ip", string.Empty);
            _propertiesFile.SetProperty("pvp", true);
            _propertiesFile.SetProperty("level-seed", string.Empty);
            _propertiesFile.SetProperty("server-port", 25565);
            _propertiesFile.SetProperty("allow-flight", false);
            _propertiesFile.SetProperty("white-list", false);
            _propertiesFile.Store(new FileStream(_file.FullName, FileMode.Create), "Minecraft server properties");
        }
        catch (Exception e)
        {
            logger.log(Level.WARNING, $"Failed to save {_file.FullName}", e);
        }
    }

    public bool GetProperty(string property, bool fallback)
    {
        return _propertiesFile.GetProperty(property, fallback);
    }

    public int GetProperty(string property, int fallback)
    {
        return _propertiesFile.GetProperty(property, fallback);
    }

    public string GetProperty(string property, string fallback)
    {
        return _propertiesFile.GetProperty(property, fallback);
    }

    public void SetProperty(string property, bool value)
    {
        _propertiesFile.SetProperty(property, value);
    }

    public void SetProperty(string property, string value)
    {
        _propertiesFile.SetProperty(property, value);
    }

    public void SetProperty(string property, int value)
    {
        _propertiesFile.SetProperty(property, value);
    }

    public void SetProperty(string property, float value)
    {
        _propertiesFile.SetProperty(property, value);
    }
}
