using BetaSharp.Util;

using Microsoft.Extensions.Logging;

namespace BetaSharp.Server;

public sealed class DedicatedPlayerManager : PlayerManager
{
    private readonly ILogger<DedicatedPlayerManager> _logger = Log.Instance.For<DedicatedPlayerManager>();
    private readonly FileInfo _bannedPlayersFile;
    private readonly FileInfo _bannedIpsFile;
    private readonly FileInfo _operatorsFile;
    private readonly FileInfo _whitelistFile;

    public DedicatedPlayerManager(MinecraftServer server) : base(server)
    {
        _bannedPlayersFile = server.GetFile("banned-players.txt");
        _bannedIpsFile = server.GetFile("banned-ips.txt");
        _operatorsFile = server.GetFile("ops.txt");
        _whitelistFile = server.GetFile("white-list.txt");

        LoadBannedPlayers();
        LoadBannedIps();
        LoadOperators();
        LoadWhitelist();
        SaveBannedPlayers();
        SaveBannedIps();
        SaveOperators();
        SaveWhitelist();
    }

    protected override void LoadBannedPlayers()
    {
        try
        {
            bannedPlayers.Clear();

            while (FileHelper.ReadText(_bannedPlayersFile.FullName) is { } line)
            {
                bannedPlayers.Add(line.Trim().ToLower());
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load ban list: {Exception}", exception);
        }
    }

    protected override void SaveBannedPlayers()
    {
        try
        {
            FileHelper.CreateText(_bannedPlayersFile.FullName, bannedPlayers);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save ban list: {Exception}", exception);
        }
    }

    protected override void LoadBannedIps()
    {
        try
        {
            bannedIps.Clear();

            while (FileHelper.ReadText(_bannedIpsFile) is { } line)
            {
                bannedIps.Add(line.Trim().ToLower());
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load ip ban list: {Exception}", exception);
        }
    }

    protected override void SaveBannedIps()
    {
        try
        {
            FileHelper.CreateText(_bannedIpsFile, bannedIps);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save ip ban list: {Exception}", exception);
        }
    }

    protected override void LoadOperators()
    {
        try
        {
            ops.Clear();

            while (FileHelper.ReadText(_operatorsFile) is { } line)
            {
                ops.Add(line.Trim().ToLower());
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load ip ban list: {Exception}", exception);
        }
    }

    protected override void SaveOperators()
    {
        try
        {
            FileHelper.CreateText(_operatorsFile, ops);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save ip ban list: {Exception}", exception);
        }
    }

    protected override void LoadWhitelist()
    {
        try
        {
            whitelist.Clear();

            while (FileHelper.ReadText(_whitelistFile) is { } line)
            {
                whitelist.Add(line.Trim().ToLower());
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to load white-list: {Exception}", exception);
        }
    }

    protected override void SaveWhitelist()
    {
        try
        {
            FileHelper.CreateText(_whitelistFile, whitelist);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("Failed to save white-list: {Exception}", exception);
        }
    }
}
