public enum AttackInputType
{
    OnPress,
    OnHold,
    OnRelease
}

public enum AttackStrengthType
{
    None,
    StrongAttack,
    LightAttack
}

public enum AttackSequenceType
{
    Optional,
    Forced
}

public enum AttackDurationType
{
    UntilDurationExpires,
    UntilTouchingGround
}

public enum AttackStatus
{
    None,
    Charge,
    Windup,
    Release,
    Cooldown,
    Sequence,
    ForcedSequence
}

public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum SpeedType
{
    Impulse,
    ConstantSpeed,
    SetSpeed,
    LowFriction
}