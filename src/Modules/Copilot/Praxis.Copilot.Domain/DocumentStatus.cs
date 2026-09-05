namespace Praxis.Copilot.Domain;

public enum DocumentStatus
{
    /// <summary>Received, not yet split and embedded.</summary>
    Pending = 1,

    Indexed = 2,

    /// <summary>Superseded by newer material. Kept, but never retrieved.</summary>
    Retired = 3,
}
