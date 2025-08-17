using System.Linq;
using Content.Server.DoAfter;
using Content.Server.Humanoid;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction;
using Content.Shared._Gardenstation.DionaMirror;
using Content.Shared.Species.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Gardenstation.DionaMirror;

/// <summary>
/// Allows humanoids to change their appearance mid-round.
/// </summary>
public sealed class DionaMirrorSystem : SharedDionaMirrorSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly MarkingManager _markings = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<DionaMirrorComponent>(DionaMirrorUiKey.Key,
            subs =>
        {
            subs.Event<BoundUIClosedEvent>(OnUiClosed);
            subs.Event<DionaMirrorSelectMessage>(OnDionaMirrorSelect);
            subs.Event<DionaMirrorChangeColorMessage>(OnTryDionaMirrorChangeColor);
            subs.Event<DionaMirrorAddSlotMessage>(OnTryDionaMirrorAddSlot);
            subs.Event<DionaMirrorRemoveSlotMessage>(OnTryDionaMirrorRemoveSlot);
        });


        SubscribeLocalEvent<DionaMirrorComponent, DionaMirrorSelectDoAfterEvent>(OnSelectSlotDoAfter);
        SubscribeLocalEvent<DionaMirrorComponent, DionaMirrorChangeColorDoAfterEvent>(OnChangeColorDoAfter);
        SubscribeLocalEvent<DionaMirrorComponent, DionaMirrorRemoveSlotDoAfterEvent>(OnRemoveSlotDoAfter);
        SubscribeLocalEvent<DionaMirrorComponent, DionaMirrorAddSlotDoAfterEvent>(OnAddSlotDoAfter);
    }

    private void OnDionaMirrorSelect(EntityUid uid, DionaMirrorComponent component, DionaMirrorSelectMessage message)
    {
        if (component.Target is not { } target)
            return;

        _doAfterSystem.Cancel(component.DoAfter);
        component.DoAfter = null;
        if (!HasComp<PrunableComponent>(component.Target))
            return;

        var doAfter = new DionaMirrorSelectDoAfterEvent()
        {
            Category = message.Category,
            Slot = message.Slot,
            Marking = message.Marking,
        };

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, message.Actor, component.SelectSlotTime, doAfter, uid, target: target, used: uid)
        {
            DistanceThreshold = SharedInteractionSystem.InteractionRange,
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        },
            out var doAfterId);

        component.DoAfter = doAfterId;
        _audio.PlayPvs(component.ChangeHairSound, uid);
    }

    private void OnSelectSlotDoAfter(EntityUid uid, DionaMirrorComponent component, DionaMirrorSelectDoAfterEvent args)
    {
        if (!HasComp<PrunableComponent>(component.Target))
            return;

        if (args.Handled || args.Target == null || args.Cancelled)
            return;

        if (component.Target != args.Target)
            return;

        MarkingCategories category;

        switch (args.Category)
        {
            case DionaMirrorCategory.Face:
                category = MarkingCategories.Face;
                break;
            case DionaMirrorCategory.Head:
                category = MarkingCategories.Head;
                break;
            case DionaMirrorCategory.HeadTop:
                category = MarkingCategories.HeadTop;
                break;
            case DionaMirrorCategory.HeadSide:
                category = MarkingCategories.HeadSide;
                break;
            case DionaMirrorCategory.Overlay:
                category = MarkingCategories.Overlay;
                break;
            default:
                return;
        }

        _humanoid.SetMarkingId(component.Target.Value, category, args.Slot, args.Marking);

        UpdateInterface(uid, component.Target.Value, component);
    }

    private void OnTryDionaMirrorChangeColor(EntityUid uid, DionaMirrorComponent component, DionaMirrorChangeColorMessage message)
    {
        if (component.Target is not { } target)
            return;
        if (!HasComp<PrunableComponent>(component.Target))
            return;

        _doAfterSystem.Cancel(component.DoAfter);
        component.DoAfter = null;

        var doAfter = new DionaMirrorChangeColorDoAfterEvent()
        {
            Category = message.Category,
            Slot = message.Slot,
            Colors = message.Colors,
        };

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, message.Actor, component.ChangeSlotTime, doAfter, uid, target: target, used: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true
        },
            out var doAfterId);

        component.DoAfter = doAfterId;
    }
    private void OnChangeColorDoAfter(EntityUid uid, DionaMirrorComponent component, DionaMirrorChangeColorDoAfterEvent args)
    {
        if (!HasComp<PrunableComponent>(component.Target))
            return;
        if (args.Handled || args.Target == null || args.Cancelled)
            return;

        if (component.Target != args.Target)
            return;

        MarkingCategories category;
        switch (args.Category)
        {
            case DionaMirrorCategory.Face:
                category = MarkingCategories.Face;
                break;
            case DionaMirrorCategory.Head:
                category = MarkingCategories.Head;
                break;
            case DionaMirrorCategory.HeadTop:
                category = MarkingCategories.HeadTop;
                break;
            case DionaMirrorCategory.HeadSide:
                category = MarkingCategories.HeadSide;
                break;
            case DionaMirrorCategory.Overlay:
                category = MarkingCategories.Overlay;
                break;
            default:
                return;
        }

        _humanoid.SetMarkingColor(component.Target.Value, category, args.Slot, args.Colors);

        // using this makes the UI feel like total ass
        // que
        // UpdateInterface(uid, component.Target, message.Session);
    }

    private void OnTryDionaMirrorRemoveSlot(EntityUid uid, DionaMirrorComponent component, DionaMirrorRemoveSlotMessage message)
    {
        if (component.Target is not { } target)
            return;
        if (!HasComp<PrunableComponent>(component.Target))
            return;
        _doAfterSystem.Cancel(component.DoAfter);
        component.DoAfter = null;

        var doAfter = new DionaMirrorRemoveSlotDoAfterEvent()
        {
            Category = message.Category,
            Slot = message.Slot,
        };

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, message.Actor, component.RemoveSlotTime, doAfter, uid, target: target, used: uid)
        {
            DistanceThreshold = SharedInteractionSystem.InteractionRange,
            BreakOnDamage = true,
            NeedHand = true
        },
            out var doAfterId);

        component.DoAfter = doAfterId;
        _audio.PlayPvs(component.ChangeHairSound, uid);
    }

    private void OnRemoveSlotDoAfter(EntityUid uid, DionaMirrorComponent component, DionaMirrorRemoveSlotDoAfterEvent args)
    {
        if (!HasComp<PrunableComponent>(component.Target))
            return;
        if (args.Handled || args.Target == null || args.Cancelled)
            return;

        if (component.Target != args.Target)
            return;

        MarkingCategories category;

        switch (args.Category)
        {
            case DionaMirrorCategory.Face:
                category = MarkingCategories.Face;
                break;
            case DionaMirrorCategory.Head:
                category = MarkingCategories.Head;
                break;
            case DionaMirrorCategory.HeadTop:
                category = MarkingCategories.HeadTop;
                break;
            case DionaMirrorCategory.HeadSide:
                category = MarkingCategories.HeadSide;
                break;
            case DionaMirrorCategory.Overlay:
                category = MarkingCategories.Overlay;
                break;
            default:
                return;
        }

        _humanoid.RemoveMarking(component.Target.Value, category, args.Slot);

        UpdateInterface(uid, component.Target.Value, component);
    }

    private void OnTryDionaMirrorAddSlot(EntityUid uid, DionaMirrorComponent component, DionaMirrorAddSlotMessage message)
    {
        if (component.Target == null)
            return;
        if (!HasComp<PrunableComponent>(component.Target))
            return;

        _doAfterSystem.Cancel(component.DoAfter);
        component.DoAfter = null;

        var doAfter = new DionaMirrorAddSlotDoAfterEvent()
        {
            Category = message.Category,
        };

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, message.Actor, component.AddSlotTime, doAfter, uid, target: component.Target.Value, used: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        },
            out var doAfterId);

        component.DoAfter = doAfterId;
        _audio.PlayPvs(component.ChangeHairSound, uid);
    }
    private void OnAddSlotDoAfter(EntityUid uid, DionaMirrorComponent component, DionaMirrorAddSlotDoAfterEvent args)
    {
        if (args.Handled || args.Target == null || args.Cancelled || !TryComp(component.Target, out HumanoidAppearanceComponent? humanoid))
            return;

        MarkingCategories category;

        switch (args.Category)
        {
            case DionaMirrorCategory.Face:
                category = MarkingCategories.Face;
                break;
            case DionaMirrorCategory.Head:
                category = MarkingCategories.Head;
                break;
            case DionaMirrorCategory.HeadTop:
                category = MarkingCategories.HeadTop;
                break;
            case DionaMirrorCategory.HeadSide:
                category = MarkingCategories.HeadSide;
                break;
            case DionaMirrorCategory.Overlay:
                category = MarkingCategories.Overlay;
                break;
            default:
                return;
        }

        var marking = _markings.MarkingsByCategoryAndSpecies(category, humanoid.Species).Keys.FirstOrDefault();

        if (string.IsNullOrEmpty(marking))
            return;

        _humanoid.AddMarking(component.Target.Value, marking, Color.Black);

        UpdateInterface(uid, component.Target.Value, component);

    }

    private void OnUiClosed(Entity<DionaMirrorComponent> ent, ref BoundUIClosedEvent args)
    {
        ent.Comp.Target = null;
        Dirty(ent);
    }
}
