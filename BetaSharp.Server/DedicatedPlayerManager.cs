using System.Text;

using BetaSharp.Util;

using java.io;

using Microsoft.Extensions.Logging;

using org.omg.IOP;

using File = System.IO.File;
using StringReader = System.IO.StringReader;
using StringWriter = System.IO.StringWriter;

namespace BetaSharp.Server;

internal class DedicatedPlayerManager : PlayerManager
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

            using (StringReader sr = new( FileHelper.ReadText(_bannedPlayersFile) ))
            {
                string line = sr.ReadLine().ToLower().Trim();

                while (!string.IsNullOrWhiteSpace(line))
                {
                    bannedPlayers.Add(line);
                }
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
            FileHelper.CreateText(_bannedPlayersFile, bannedPlayers);
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

            using (StringReader sr = new( FileHelper.ReadText(_bannedIpsFile) ))
            {
                string line = sr.ReadLine().ToLower().Trim();

                while (!string.IsNullOrWhiteSpace(line))
                {
                    bannedIps.Add(line);
                }
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

            using (StringReader sr = new( FileHelper.ReadText(_operatorsFile) ))
            {
                string line = sr.ReadLine().ToLower().Trim();

                while (!string.IsNullOrWhiteSpace(line))
                {
                    ops.Add(line);
                }
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

            using (StringReader sr = new( FileHelper.ReadText(_whitelistFile) ))
            {
                string line = sr.ReadLine().ToLower().Trim();

                while (!string.IsNullOrWhiteSpace(line))
                {
                    whitelist.Add(line);
                }
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
