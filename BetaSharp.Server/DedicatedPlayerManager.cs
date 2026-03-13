using java.io;
using Microsoft.Extensions.Logging;

namespace BetaSharp.Server;

internal class DedicatedPlayerManager : PlayerManager
{
    private readonly ILogger<DedicatedPlayerManager> _logger = Log.Instance.For<DedicatedPlayerManager>();
    private readonly java.io.File _bannedPlayersFile;
    private readonly java.io.File _bannedIpsFile;
    private readonly java.io.File _operatorsFile;
    private readonly java.io.File _whitelistFile;

    public DedicatedPlayerManager(BetaSharpServer server) : base(server)
    {
        _bannedPlayersFile = server.getFile("banned-players.txt");
        _bannedIpsFile = server.getFile("banned-ips.txt");
        _operatorsFile = server.getFile("ops.txt");
        _whitelistFile = server.getFile("white-list.txt");

        loadBannedPlayers();
        loadBannedIps();
        loadOperators();
        loadWhitelist();
        saveBannedPlayers();
        saveBannedIps();
        saveOperators();
        saveWhitelist();
    }

    protected override void loadBannedPlayers()
    {
        try
        {
            bannedPlayers.Clear();
            BufferedReader reader = new( new FileReader(_bannedPlayersFile) );

            while (reader.readLine() is { } line)
            {
                bannedPlayers.Add(line.Trim().ToLower());
            }

            reader.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load ban list: {Exception}", exception);
        }
    }

    protected override void saveBannedPlayers()
    {
        try
        {
            PrintWriter writer = new( new FileWriter(_bannedPlayersFile, false) );

            foreach (string line in bannedPlayers)
            {
                writer.println(line);
            }

            writer.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save ban list: {Exception}", exception);
        }
    }

    protected override void loadBannedIps()
    {
        try
        {
            bannedIps.Clear();
            BufferedReader reader = new( new FileReader(_bannedIpsFile) );

            while (reader.readLine() is { } line)
            {
                bannedIps.Add(line.Trim().ToLower());
            }

            reader.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load ip ban list: {Exception}", exception);
        }
    }

    protected override void saveBannedIps()
    {
        try
        {
            PrintWriter writer = new( new FileWriter(_bannedIpsFile, false) );

            foreach (string line in bannedIps)
            {
                writer.println(line);
            }

            writer.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save ip ban list: {Exception}", exception);
        }
    }

    protected override void loadOperators()
    {
        try
        {
            ops.Clear();
            BufferedReader reader = new( new FileReader(_operatorsFile) );

            while (reader.readLine() is { } line)
            {
                ops.Add(line.Trim().ToLower());
            }

            reader.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load ip ban list: {Exception}", exception);
        }
    }

    protected override void saveOperators()
    {
        try
        {
            PrintWriter writer = new( new FileWriter(_operatorsFile, false) );

            foreach (string line in ops)
            {
                writer.println(line);
            }

            writer.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save ip ban list: {Exception}", exception);
        }
    }

    protected override void loadWhitelist()
    {
        try
        {
            whitelist.Clear();
            BufferedReader reader = new( new FileReader(_whitelistFile) );

            while (reader.readLine() is { } line)
            {
                whitelist.Add(line.Trim().ToLower());
            }

            reader.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load white-list: {Exception}", exception);
        }
    }

    protected override void saveWhitelist()
    {
        try
        {
            PrintWriter writer = new( new FileWriter(_whitelistFile, false) );

            foreach (string line in whitelist)
            {
                writer.println(line);
            }

            writer.close();
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save white-list: {Exception}", exception);
        }
    }
}
