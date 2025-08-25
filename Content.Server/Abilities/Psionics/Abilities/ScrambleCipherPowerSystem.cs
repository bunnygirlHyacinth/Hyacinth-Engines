
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.StatusEffect;

namespace Content.Server.Abilities.Psionics
{
    public sealed class ScrambleCipherPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ScrambleCipherPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(ScrambleCipherPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, "scramble cipher", false))
                return;


            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Scrambled", TimeSpan.FromSeconds(10), false, "BackwardsAccent");
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "PsionicallyInsulated", TimeSpan.FromSeconds(30), false, "PsionicInsulation");
            _psionics.LogPowerUsed(args.Performer, "scramble cipher");
            args.Handled = true;
        }
    }
}
