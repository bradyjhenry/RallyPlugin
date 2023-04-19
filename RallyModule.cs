using AssettoServer.Server.Plugin;
using Autofac;

namespace RallyPlugin;

public class RallyModule : AssettoServerModule<RallyConfiguration>
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Rally>().AsSelf().As<IAssettoServerAutostart>().SingleInstance();
        builder.RegisterType<EntryCarRally>().AsSelf();
    }
}
