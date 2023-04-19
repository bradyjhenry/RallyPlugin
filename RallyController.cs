using AssettoServer.Server;
using Microsoft.AspNetCore.Mvc;

namespace RallyPlugin;

[ApiController]
[Route("RallyPlugin")]
public class RallyController : ControllerBase
{
    private readonly ACServer _server;
    private readonly RallyConfiguration _configuration;

    public RallyController(ACServer server, RallyConfiguration configuration)
    {
        _server = server;
        _configuration = configuration;
    }

    [HttpGet("config")]
    [Produces("text/x-lua", new string[] { })]
    public RallyConfiguration Config() => _configuration;
}
