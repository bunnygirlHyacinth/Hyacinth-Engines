using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Interaction;
using Content.Shared.Species.Components;
using Content.Shared.UserInterface;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
namespace Content.Shared._Gardenstation.DionaMirror;

public abstract class SharedDionaMirrorSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UISystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DionaMirrorComponent, AfterInteractEvent>(OnDionaMirrorInteract);
        SubscribeLocalEvent<DionaMirrorComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<DionaMirrorComponent, BoundUserInterfaceCheckRangeEvent>(OnDionaMirrorRangeCheck);
    }

    private void OnDionaMirrorInteract(Entity<DionaMirrorComponent> mirror, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;
        if (!HasComp<PrunableComponent>(args.Target))
            return;
        if (!UISystem.TryOpenUi(mirror.Owner, DionaMirrorUiKey.Key, args.User))
            return;


        UpdateInterface(mirror, args.Target.Value, mirror);
    }

    private void OnDionaMirrorRangeCheck(EntityUid uid, DionaMirrorComponent component, ref BoundUserInterfaceCheckRangeEvent args)
    {
        if (args.Result == BoundUserInterfaceRangeResult.Fail)
            return;
        DebugTools.Assert(component.Target != null && Exists(component.Target));
        if (!_interaction.InRangeUnobstructed(uid, component.Target.Value))
            args.Result = BoundUserInterfaceRangeResult.Fail;
    }

    private void OnBeforeUIOpen(Entity<DionaMirrorComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (args.User != ent.Comp.Target && ent.Comp.Target != null)
            return;

        UpdateInterface(ent, args.User, ent);
    }

    protected void UpdateInterface(EntityUid mirrorUid, EntityUid targetUid, DionaMirrorComponent component)
    {
        if (!TryComp<HumanoidAppearanceComponent>(targetUid, out var humanoid))
            return;
        component.Target ??= targetUid;

        var face = humanoid.MarkingSet.TryGetCategory(MarkingCategories.Face, out var faceMarkings)
            ? new List<Marking>(faceMarkings)
            : new();
        var head = humanoid.MarkingSet.TryGetCategory(MarkingCategories.Head, out var headMarkings)
            ? new List<Marking>(headMarkings)
            : new();
        var headTop = humanoid.MarkingSet.TryGetCategory(MarkingCategories.HeadTop, out var headTopMarkings)
            ? new List<Marking>(headTopMarkings)
            : new();
        var headSide = humanoid.MarkingSet.TryGetCategory(MarkingCategories.HeadSide, out var headSideMarkings)
            ? new List<Marking>(headSideMarkings)
            : new();
        var overlay = humanoid.MarkingSet.TryGetCategory(MarkingCategories.Overlay, out var overlayMarkings)
            ? new List<Marking>(overlayMarkings)
            : new();
        var state = new DionaMirrorUiState(
            humanoid.Species,
            face,
            humanoid.MarkingSet.PointsLeft(MarkingCategories.Face) + face.Count,
            head,
            humanoid.MarkingSet.PointsLeft(MarkingCategories.Head) + head.Count,
            headTop,
            humanoid.MarkingSet.PointsLeft(MarkingCategories.HeadTop) + headTop.Count,
            headSide,
            humanoid.MarkingSet.PointsLeft(MarkingCategories.HeadSide) + headSide.Count,
            overlay,
            humanoid.MarkingSet.PointsLeft(MarkingCategories.Overlay) + overlay.Count);

        // TODO: Component states
        component.Target = targetUid;
        UISystem.SetUiState(mirrorUid, DionaMirrorUiKey.Key, state);
        Dirty(mirrorUid, component);
    }
}

[Serializable, NetSerializable]
public enum DionaMirrorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum DionaMirrorCategory : byte
{
    Face,
    Head,
    HeadTop,
    HeadSide,
    Overlay
}

[Serializable, NetSerializable]
public sealed class DionaMirrorSelectMessage : BoundUserInterfaceMessage
{
    public DionaMirrorSelectMessage(DionaMirrorCategory category, string marking, int slot)
    {
        Category = category;
        Marking = marking;
        Slot = slot;
    }

    public DionaMirrorCategory Category { get; }
    public string Marking { get; }
    public int Slot { get; }
}

[Serializable, NetSerializable]
public sealed class DionaMirrorChangeColorMessage : BoundUserInterfaceMessage
{
    public DionaMirrorChangeColorMessage(DionaMirrorCategory category, List<Color> colors, int slot)
    {
        Category = category;
        Colors = colors;
        Slot = slot;
    }

    public DionaMirrorCategory Category { get; }
    public List<Color> Colors { get; }
    public int Slot { get; }
}

[Serializable, NetSerializable]
public sealed class DionaMirrorRemoveSlotMessage : BoundUserInterfaceMessage
{
    public DionaMirrorRemoveSlotMessage(DionaMirrorCategory category, int slot)
    {
        Category = category;
        Slot = slot;
    }

    public DionaMirrorCategory Category { get; }
    public int Slot { get; }
}

[Serializable, NetSerializable]
public sealed class DionaMirrorSelectSlotMessage : BoundUserInterfaceMessage
{
    public DionaMirrorSelectSlotMessage(DionaMirrorCategory category, int slot)
    {
        Category = category;
        Slot = slot;
    }

    public DionaMirrorCategory Category { get; }
    public int Slot { get; }
}

[Serializable, NetSerializable]
public sealed class DionaMirrorAddSlotMessage : BoundUserInterfaceMessage
{
    public DionaMirrorAddSlotMessage(DionaMirrorCategory category)
    {
        Category = category;
    }

    public DionaMirrorCategory Category { get; }
}

[Serializable, NetSerializable]
public sealed class DionaMirrorUiState : BoundUserInterfaceState
{
    public DionaMirrorUiState(string species, List<Marking> face, int faceSlotTotal, List<Marking> head, int headSlotTotal, List<Marking> headTop, int headTopSlotTotal, List<Marking> headSide, int headSideSlotTotal, List<Marking> overlay, int overlaySlotTotal)
    {
        Species = species;
        Face = face;
        FaceSlotTotal = faceSlotTotal;
        Head = head;
        HeadSlotTotal = headSlotTotal;
        HeadTop = headTop;
        HeadTopSlotTotal = headTopSlotTotal;
        HeadSide = headSide;
        HeadSideSlotTotal = headSideSlotTotal;
        Overlay = overlay;
        OverlaySlotTotal = overlaySlotTotal;
    }

    public NetEntity Target;

    public string Species;

    public List<Marking> Face;
    public int FaceSlotTotal;

    public List<Marking> Head;
    public int HeadSlotTotal;

    public List<Marking> HeadTop;
    public int HeadTopSlotTotal;

    public List<Marking> HeadSide;
    public int HeadSideSlotTotal;

    public List<Marking> Overlay;
    public int OverlaySlotTotal;
}

[Serializable, NetSerializable]
public sealed partial class DionaMirrorRemoveSlotDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
    public DionaMirrorCategory Category;
    public int Slot;
}

[Serializable, NetSerializable]
public sealed partial class DionaMirrorAddSlotDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
    public DionaMirrorCategory Category;
}

[Serializable, NetSerializable]
public sealed partial class DionaMirrorSelectDoAfterEvent : DoAfterEvent
{
    public DionaMirrorCategory Category;
    public int Slot;
    public string Marking = string.Empty;

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class DionaMirrorChangeColorDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
    public DionaMirrorCategory Category;
    public int Slot;
    public List<Color> Colors = new List<Color>();
}
