using Content.Shared.Radio;
using Content.Shared.Timing;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._RMC14.Dropship;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedDropshipSystem))]
public sealed partial class DropshipComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Destination;

    [DataField, AutoNetworkedField]
    public bool Crashed;

    [DataField, AutoNetworkedField]
    public ProtoId<RadioChannelPrototype> AnnounceHijackIn = "MarineCommon";

    [DataField, AutoNetworkedField]
    public SoundSpecifier LocalHijackSound = new SoundPathSpecifier("/Audio/_RMC14/Machines/Shuttle/queen_alarm.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier MarineHijackSound = new SoundPathSpecifier("/Audio/_RMC14/Announcements/ARES/hijack.ogg", AudioParams.Default.WithVolume(-6));

    [DataField, AutoNetworkedField]
    public SoundSpecifier HijackrashWarningSound = new SoundPathSpecifier("/Audio/_RMC14/Announcements/ARES/dropship_emergency.ogg", AudioParams.Default.WithVolume(-6));

    [DataField, AutoNetworkedField]
    public SoundSpecifier HijackIncomingSound = new SoundPathSpecifier("/Audio/_RMC14/Effects/Hijack/dropship_incoming.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier HijackCrashingSound = new SoundPathSpecifier("/Audio/_RMC14/Effects/Hijack/dropship_crash.ogg");

    [DataField, AutoNetworkedField]
    public TimeSpan HijackCrashWarningAt = TimeSpan.FromSeconds(50);

    [DataField, AutoNetworkedField]
    public TimeSpan HijackIncomingAt = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public TimeSpan HijackCrashAt = TimeSpan.FromSeconds(7);

    [DataField, AutoNetworkedField]
    public bool Locked;

    [DataField, AutoNetworkedField]
    public TimeSpan LockCooldown = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan LastLocked;
}
