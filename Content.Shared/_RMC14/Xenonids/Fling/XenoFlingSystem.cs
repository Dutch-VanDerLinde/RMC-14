﻿using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Slow;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids.Heal;
using Content.Shared._RMC14.Xenonids.Rage;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.FixedPoint;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._RMC14.Xenonids.Fling;

public sealed class XenoFlingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly RMCSlowSystem _rmcSlow = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly SharedRMCMeleeWeaponSystem _rmcMelee = default!;
    [Dependency] private readonly SharedXenoHealSystem _xenoHeal = default!;
    [Dependency] private readonly XenoRageSystem _rage = default!;
    [Dependency] private readonly RMCDazedSystem _dazed = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenoFlingComponent, XenoFlingActionEvent>(OnXenoFlingAction);
    }

    private void OnXenoFlingAction(Entity<XenoFlingComponent> xeno, ref XenoFlingActionEvent args)
    {
        if (!_xeno.CanAbilityAttackTarget(xeno, args.Target))
            return;

        if (args.Handled)
            return;

        var attempt = new XenoFlingAttemptEvent();
        RaiseLocalEvent(xeno, ref attempt);

        if (attempt.Cancelled)
            return;

        if (_net.IsServer)
        {
            args.Handled = true;
            _audio.PlayPvs(xeno.Comp.Sound, xeno);
        }

        var rage = _rage.GetRage(xeno.Owner);

        var targetId = args.Target;
        _rmcPulling.TryStopAllPullsFromAndOn(targetId);

        var damage = _damageable.TryChangeDamage(targetId, xeno.Comp.Damage, origin: xeno, tool: xeno);
        if (damage?.GetTotal() > FixedPoint2.Zero)
        {
            var filter = Filter.Pvs(targetId, entityManager: EntityManager).RemoveWhereAttachedEntity(o => o == xeno.Owner);
            _colorFlash.RaiseEffect(Color.Red, new List<EntityUid> { targetId }, filter);
        }

        var healAmount = xeno.Comp.HealAmount;
        var throwRange = xeno.Comp.Range;

        if (rage >= 2)
        {
            throwRange += xeno.Comp.EnragedRange;
            healAmount += xeno.Comp.EnragedHealAmount;
        }

        var origin = _transform.GetMapCoordinates(xeno);
        var target = _transform.GetMapCoordinates(targetId);
        var diff = target.Position - origin.Position;
        diff = diff.Normalized() * throwRange;

        _rmcMelee.DoLunge(xeno, targetId);
        _xenoHeal.CreateHealStacks(xeno, healAmount, xeno.Comp.HealDelay, 1, xeno.Comp.HealDelay);

        if (!_net.IsServer)
            return;

        _rmcSlow.TrySlowdown(targetId, xeno.Comp.SlowTime);
        _dazed.TryDaze(targetId, xeno.Comp.DazeTime, true);
        _stun.TryParalyze(targetId, xeno.Comp.ParalyzeTime, true);
        _throwing.TryThrow(targetId, diff, xeno.Comp.ThrowSpeed);

        SpawnAttachedTo(xeno.Comp.Effect, targetId.ToCoordinates());
    }
}
