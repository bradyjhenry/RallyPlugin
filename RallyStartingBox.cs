using System.Numerics;

namespace RallyPlugin;

internal class RallyStartingBox
{
    private readonly RallyConfiguration _configuration;

    public EntryCarRally? containedCar { get; set; }

    private readonly int _width;
    private readonly int _depth;
    private readonly Vector3 _position;
    private readonly Vector3 _forward;
    private Vector3 _right;
    private readonly Quaternion _rotation;

    public RallyStartingBox(RallyConfiguration configuration)
    {
        _configuration = configuration;
        _width = _configuration.Width;
        _depth = _configuration.Depth;
        _position = _configuration.StartingPosition;
        Vector3 forwardPosition = _configuration.Forward;

        Vector3 forwardDirection = _position - forwardPosition;
        _forward = Vector3.Normalize(forwardDirection);

        Vector3 up = Vector3.UnitY;
        _right = Vector3.Normalize(Vector3.Cross(up, _forward));
    }

    public bool isCarWithin(EntryCarRally entryCarRally)
    {
        Vector3 relativePosition = entryCarRally.EntryCar.Status.Position - _position;

        float forwardDistance = Vector3.Dot(relativePosition, _forward);
        float rightDistance = Vector3.Dot(relativePosition, _right);

        if (forwardDistance >= 0 && forwardDistance <= _depth &&
            rightDistance >= -_width / 2 && rightDistance <= _width / 2)
        {
            return true;
        }

        return false;
    }



}
