using JetBrains.Annotations;
using System.Numerics;

namespace RallyPlugin;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]

public class RallyConfiguration
{
    public Vector3 StartingPosition { get; set; }
    public Vector3 Forward { get; set; }

    public int Width { get; set; }
    public int Depth { get; set; }
    public int Height { get; set; }
}
