using Content.Server.Medical;
using Content.Server.Mood;
using Content.Server.Stunnable;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Mood;

namespace Content.Server.Abilities.Psionics
{
    public sealed class WhisperTruthPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StunSystem _stunSystem = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly VomitSystem _vomitSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<WhisperTruthPowerActionEvent>(OnPowerUsed);
        }

        private void OnPowerUsed(WhisperTruthPowerActionEvent args)
        {
            if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, "Whisper Truth", true))
                return;


            _stunSystem.TryParalyze(args.Target, TimeSpan.FromSeconds(5), false);
         //   _statusEffectsSystem.TryAddStatusEffect(args.Target, "MoodEffectEvent", TimeSpan.FromSeconds(500), false,"EldritchHorror"); // its not do work. help . help mee heeeeelp heelp mee
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "Muted", TimeSpan.FromSeconds(30), false, "StutteringAccent");
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "PsionicsDisabled", TimeSpan.FromSeconds(100), false, "PsionicsDisabled");
            _statusEffectsSystem.TryAddStatusEffect(args.Target, "PsionicallyInsulated", TimeSpan.FromSeconds(10), false, "PsionicInsulation");
           // _vomitSystem.Vomit(args.Target, -120, -120);
            _psionics.LogPowerUsed(args.Performer, "Whisper Truth");
            args.Handled = true;
        }
    }
}
