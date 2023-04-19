using AssettoServer.Network.Tcp;
using AssettoServer.Server;
using AssettoServer.Server.Configuration;
using AssettoServer.Server.Plugin;
using AssettoServer.Utils;
using Microsoft.Extensions.Hosting;
using RallyPlugin.Packets;
using Serilog;
using System.Diagnostics;
using System.Reflection;


namespace RallyPlugin;

public class Rally : CriticalBackgroundService, IAssettoServerAutostart
{

    private readonly EntryCarManager _entryCarManager;

    private readonly ACServerConfiguration _serverConfiguration;

    private readonly Func<EntryCar, EntryCarRally> _entryCarRallyFactory;

    private readonly Dictionary<int, EntryCarRally> _instances = new();

    private readonly RallyConfiguration _configuration;

    private readonly RallyStartingBox _startingBox;

    private bool rallyReady;


    public Rally(EntryCarManager entryCarManager, Func<EntryCar, EntryCarRally> entryCarRallyFactory, RallyConfiguration configuration, ACServerConfiguration serverConfiguration, CSPServerScriptProvider scriptProvider, IHostApplicationLifetime applicationLifetime) : base(applicationLifetime)
    {
        _entryCarManager = entryCarManager;
        _entryCarRallyFactory = entryCarRallyFactory;
        _serverConfiguration = serverConfiguration;
        _configuration = configuration;

        _startingBox = new RallyStartingBox(_configuration);

        rallyReady = true;

        Log.Debug("Rally Plugin Loaded");

        if (_serverConfiguration.Extra.EnableClientMessages)
        {
            using var streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("RallyPlugin.lua.rallyplugin.lua")!);
            scriptProvider.AddScript(streamReader.ReadToEnd(), "rallyplugin.lua");
        }
        else
        {
            throw new ConfigurationException("RallyPlugin: EnableClientMessages must be set to true in extra_cfg!");
        }
    }

    private void SendRallyFlagsPacket(ACTcpClient client, Flags flag)
    {
        client.SendPacket(new RallyFlags { Flags = flag });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Stopwatch timer = new Stopwatch();

        foreach (var entryCar in _entryCarManager.EntryCars)
        {
            _instances.Add(entryCar.SessionId, _entryCarRallyFactory(entryCar));
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                UpdateRallyReadyState(timer);
                ProcessInstances();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during rally update");
            }
            finally
            {
                await Task.Delay(300, stoppingToken);
            }
        }
    }

    private void UpdateRallyReadyState(Stopwatch timer)
    {
        if (_startingBox.containedCar != null && !_startingBox.isCarWithin(_startingBox.containedCar))
        {
            _startingBox.containedCar.hasStopped = false;
            timer.Restart();
            rallyReady = false;
        }

        if (timer.Elapsed >= TimeSpan.FromSeconds(10))
        {
            rallyReady = true;
            timer.Stop();
        }
    }

    private void ProcessInstances()
    {
        foreach (var instance in _instances)
        {
            var client = instance.Value.EntryCar.Client;
            if (client == null || !client.HasSentFirstUpdate)
                continue;

            bool isCarWithinStartingBox = _startingBox.isCarWithin(instance.Value);
            instance.Value.insideStartingBox = isCarWithinStartingBox;

            if (isCarWithinStartingBox)
            {
                HandleCarInStartingBox(instance, client);
            }
            else
            {
                HandleCarOutsideStartingBox(instance, client);
            }
        }
    }

    private void HandleCarInStartingBox(KeyValuePair<int, EntryCarRally> instance, ACTcpClient client)
    {
        if (_startingBox.containedCar != instance.Value && _startingBox.containedCar != null)
        {
            SendRallyFlagsPacket(client, Flags.Penalty);
        }
        else
        {
            _startingBox.containedCar = instance.Value;
            if (rallyReady)
            {
                SendRallyFlagsPacket(client, _startingBox.containedCar.hasStopped ? Flags.Ready : Flags.Stop);
                _startingBox.containedCar.isReadySent = _startingBox.containedCar.hasStopped;
            }
            else
            {
                SendRallyFlagsPacket(client, Flags.Wait);
            }
        }
    }

    private void HandleCarOutsideStartingBox(KeyValuePair<int, EntryCarRally> instance, ACTcpClient client)
    {
        if (_startingBox.containedCar != null && _startingBox.containedCar.Equals(instance.Value))
        {
            if (!_startingBox.containedCar.isReadySent)
            {
                SendRallyFlagsPacket(client, Flags.Penalty);
            }
            _startingBox.containedCar.isReadySent = false;
            _startingBox.containedCar = null;
        }
        else
        {
            SendRallyFlagsPacket(client, _startingBox.containedCar != null ? Flags.Occupied : Flags.Open);
        }
    }



}
