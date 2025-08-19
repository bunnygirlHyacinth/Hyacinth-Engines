using Content.Shared.Humanoid.Markings;
using Content.Shared._Gardenstation.DionaMirror;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._Gardenstation.DionaMirror;

public sealed class DionaMirrorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private DionaMirrorWindow? _window;

    public DionaMirrorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DionaMirrorWindow>();

        _window.OnFaceSelected += tuple => SelectFace(DionaMirrorCategory.Face, tuple.id, tuple.slot);
        _window.OnFaceColorChanged += args => ChangeColor(DionaMirrorCategory.Face, args.marking, args.slot);
        _window.OnFaceSlotAdded += delegate () { AddSlot(DionaMirrorCategory.Face); };
        _window.OnFaceSlotRemoved += args => RemoveSlot(DionaMirrorCategory.Face, args);

        _window.OnHeadSelected += tuple => SelectHead(DionaMirrorCategory.Head, tuple.id, tuple.slot);
        _window.OnHeadColorChanged += args => ChangeColor(DionaMirrorCategory.Head, args.marking, args.slot);
        _window.OnHeadSlotAdded += delegate () { AddSlot(DionaMirrorCategory.Head); };
        _window.OnHeadSlotRemoved += args => RemoveSlot(DionaMirrorCategory.Head, args);

        _window.OnHeadTopSelected += tuple => SelectHeadTop(DionaMirrorCategory.HeadTop, tuple.id, tuple.slot);
        _window.OnHeadTopColorChanged += args => ChangeColor(DionaMirrorCategory.HeadTop, args.marking, args.slot);
        _window.OnHeadTopSlotAdded += delegate () { AddSlot(DionaMirrorCategory.HeadTop); };
        _window.OnHeadTopSlotRemoved += args => RemoveSlot(DionaMirrorCategory.HeadTop, args);

        _window.OnHeadSideSelected += tuple => SelectHeadSide(DionaMirrorCategory.HeadSide, tuple.id, tuple.slot);
        _window.OnHeadSideColorChanged += args => ChangeColor(DionaMirrorCategory.HeadSide, args.marking, args.slot);
        _window.OnHeadSideSlotAdded += delegate () { AddSlot(DionaMirrorCategory.HeadSide); };
        _window.OnHeadSideSlotRemoved += args => RemoveSlot(DionaMirrorCategory.HeadSide, args);

        _window.OnOverlaySelected += tuple => SelectOverlay(DionaMirrorCategory.Overlay, tuple.id, tuple.slot);
        _window.OnOverlayColorChanged += args => ChangeColor(DionaMirrorCategory.Overlay, args.marking, args.slot);
        _window.OnOverlaySlotAdded += delegate () { AddSlot(DionaMirrorCategory.Overlay); };
        _window.OnOverlaySlotRemoved += args => RemoveSlot(DionaMirrorCategory.Overlay, args);
    }

    private void SelectFace(DionaMirrorCategory category, string marking, int slot)
    {
        SendMessage(new DionaMirrorSelectMessage(category, marking, slot));
    }

    private void SelectHead(DionaMirrorCategory category, string marking, int slot)
    {
        SendMessage(new DionaMirrorSelectMessage(category, marking, slot));
    }

    private void SelectHeadTop(DionaMirrorCategory category, string marking, int slot)
    {
        SendMessage(new DionaMirrorSelectMessage(category, marking, slot));
    }

    private void SelectHeadSide(DionaMirrorCategory category, string marking, int slot)
    {
        SendMessage(new DionaMirrorSelectMessage(category, marking, slot));
    }

    private void SelectOverlay(DionaMirrorCategory category, string marking, int slot)
    {
        SendMessage(new DionaMirrorSelectMessage(category, marking, slot));
    }

    private void ChangeColor(DionaMirrorCategory category, Marking marking, int slot)
    {
        SendMessage(new DionaMirrorChangeColorMessage(category, new(marking.MarkingColors), slot));
    }

    private void RemoveSlot(DionaMirrorCategory category, int slot)
    {
        SendMessage(new DionaMirrorRemoveSlotMessage(category, slot));
    }

    private void AddSlot(DionaMirrorCategory category)
    {
        SendMessage(new DionaMirrorAddSlotMessage(category));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not DionaMirrorUiState data || _window == null)
        {
            return;
        }

        _window.UpdateState(data);
    }
}

