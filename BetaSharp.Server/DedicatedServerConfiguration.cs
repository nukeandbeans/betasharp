using java.io;
using java.lang;
using java.util;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace BetaSharp.Server;

internal class DedicatedServerConfiguration : IServerConfiguration
{
    public static readonly ILogger<DedicatedServerConfiguration> logger = Log.Instance.For<DedicatedServerConfiguration>();
    private readonly Properties _properties = new();
    private readonly java.io.File _propertiesFile;

    public DedicatedServerConfiguration(java.io.File file)
    {
        _propertiesFile = file;

        if (file.exists())
        {
            try
            {
                _properties.load(new FileInputStream(file));
            }
            catch (Exception exception)
            {
                logger.LogWarning("Failed to load file: {@File}\n{Exception}", file, exception);
                GenerateNew();
            }
        }
        else
        {
            logger.LogWarning("{@File} does not exist", file);
            GenerateNew();
        }
    }

    public void GenerateNew()
    {
        logger.LogInformation("Generating new properties file");
        Save();
    }

    public void Save()
    {
        try
        {
            _properties.store(new FileOutputStream(_propertiesFile), "BetaSharp server properties");
        }
        catch (Exception exception)
        {
            logger.LogWarning("Failed to save property file: {@PropertiesFile}\n{Exception}", _propertiesFile, exception);
            GenerateNew();
        }
    }

    public string GetProperty(string property, string fallback)
    {
        if (!_properties.containsKey(property))
        {
            _properties.setProperty(property, fallback);
            Save();
        }

        return _properties.getProperty(property, fallback);
    }

    public int GetProperty(string property, int fallback)
    {
        try
        {
            return Integer.parseInt(GetProperty(property, fallback.ToString()));
        }
        catch (Exception)
        {
            _properties.setProperty(property, fallback.ToString());

            return fallback;
        }
    }

    public bool GetProperty(string property, bool fallback)
    {
        try
        {
            return java.lang.Boolean.parseBoolean(GetProperty(property, fallback.ToString()));
        }
        catch (Exception)
        {
            _properties.setProperty(property, fallback.ToString());

            return fallback;
        }
    }

    public void SetProperty(string property, bool value)
    {
        _properties.setProperty(property, value.ToString());
        Save();
    }

    public string GetServerIp(string fallback) => GetProperty("server-ip", fallback);
    public int GetServerPort(int fallback) => GetProperty("server-port", fallback);
    public bool GetDualStack(bool fallback) => GetProperty("dual-stack", fallback);
    public bool GetOnlineMode(bool fallback) => GetProperty("online-mode", fallback);
    public bool GetSpawnAnimals(bool fallback) => GetProperty("spawn-animals", fallback);
    public bool GetPvpEnabled(bool fallback) => GetProperty("pvp", fallback);
    public bool GetAllowFlight(bool fallback) => GetProperty("allow-flight", fallback);
    public string GetLevelName(string fallback) => GetProperty("level-name", fallback);
    public string GetLevelType(string fallback) => GetProperty("level-type", fallback);
    public string GetLevelSeed(string fallback) => GetProperty("level-seed", fallback);
    public string GetLevelOptions(string fallback) => GetProperty("generator-settings", fallback);
    public bool GetSpawnMonsters(bool fallback) => GetProperty("spawn-monsters", fallback);
    public bool GetAllowNether(bool fallback) => GetProperty("allow-nether", fallback);
    public int GetMaxPlayers(int fallback) => GetProperty("max-players", fallback);
    public int GetViewDistance(int fallback) => GetProperty("view-distance", fallback);
    public bool GetWhiteList(bool fallback) => GetProperty("white-list", fallback);
    public int GetSpawnRegionSize(int fallback) => GetProperty("spawn-region-size", fallback);
}
