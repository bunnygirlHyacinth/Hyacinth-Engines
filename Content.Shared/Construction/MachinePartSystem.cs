using System.Linq;
using Content.Shared.Construction.Components;
using Content.Shared.Examine;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction
{
    /// <summary>
    /// Deals with machine parts and machine boards.
    /// </summary>
    public sealed class MachinePartSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly SharedLatheSystem _lathe = default!;
        [Dependency] private readonly SharedConstructionSystem _construction = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MachineBoardComponent, ExaminedEvent>(OnMachineBoardExamined);
            SubscribeLocalEvent<MachinePartComponent, ExaminedEvent>(OnMachinePartExamined);
        }

        private void OnMachineBoardExamined(EntityUid uid, MachineBoardComponent component, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            using (args.PushGroup(nameof(MachineBoardComponent)))
            {
                args.PushMarkup(Loc.GetString("machine-board-component-on-examine-label"));
                foreach (var (machinePartId, amount) in component.MachinePartRequirements)
                {
                    var machinePart = _prototype.Index(machinePartId);

                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", amount),
                        ("requiredElement", Loc.GetString(machinePart.Name))));
                }

                foreach (var (material, amount) in component.StackRequirements)
                {
                    var stack = _prototype.Index(material);
                    var name = _prototype.Index(stack.Spawn).Name;

                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", amount),
                        ("requiredElement", Loc.GetString(name))));
                }

                foreach (var (_, info) in component.ComponentRequirements)
                {
                    var examineName = _construction.GetExamineName(info);
                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", info.Amount),
                        ("requiredElement", examineName)));
                }

                foreach (var (_, info) in component.TagRequirements)
                {
                    var examineName = _construction.GetExamineName(info);
                    args.PushMarkup(Loc.GetString("machine-board-component-required-element-entry-text",
                        ("amount", info.Amount),
                        ("requiredElement", examineName)));
                }
            }
        }

        private void OnMachinePartExamined(EntityUid uid, MachinePartComponent component, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            using (args.PushGroup(nameof(MachinePartComponent)))
            {
                args.PushMarkup(Loc.GetString("machine-part-component-on-examine-rating-text",
                    ("rating", component.Rating)));
                args.PushMarkup(Loc.GetString("machine-part-component-on-examine-type-text",
                    ("type", Loc.GetString(_prototype.Index(component.PartType).Name))));
            }
        }

        public Dictionary<string, int> GetMachineBoardMaterialCost(Entity<MachineBoardComponent> entity, int coefficient = 1)
        {
            var (_, comp) = entity;

            var materials = new Dictionary<string, int>();
            foreach (var (machinePartId, amount) in comp.MachinePartRequirements)
            {
                var machinePart = _prototype.Index(machinePartId);

                if (!_lathe.TryGetRecipesFromEntity(machinePart.StockPartPrototype, out var recipes))
                    continue;

                var partRecipe = recipes[0];
                if (recipes.Count > 1)
                    partRecipe = recipes.MinBy(p => p.Materials.Values.Sum());

                foreach (var (mat, matAmount) in partRecipe!.Materials)
                {
                    materials.TryAdd(mat, 0);
                    materials[mat] += matAmount * amount * coefficient;
                }
            }

            foreach (var (stackId, amount) in comp.StackRequirements)
            {
                var stackProto = _prototype.Index(stackId);
                var defaultProto = _prototype.Index(stackProto.Spawn);

                if (defaultProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp, EntityManager.ComponentFactory))
                {
                    foreach (var (mat, matAmount) in physComp.MaterialComposition)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
                else if (_lathe.TryGetRecipesFromEntity(stackProto.Spawn, out var recipes))
                {
                    var partRecipe = recipes[0];
                    if (recipes.Count > 1)
                        partRecipe = recipes.MinBy(p => p.Materials.Values.Sum());

                    foreach (var (mat, matAmount) in partRecipe!.Materials)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
            }

            var genericPartInfo = comp.ComponentRequirements.Values.Concat(comp.ComponentRequirements.Values);
            foreach (var info in genericPartInfo)
            {
                var amount = info.Amount;
                var defaultProtoId = info.DefaultPrototype;

                if (_lathe.TryGetRecipesFromEntity(defaultProtoId, out var recipes))
                {
                    var partRecipe = recipes[0];
                    if (recipes.Count > 1)
                        partRecipe = recipes.MinBy(p => p.Materials.Values.Sum());

                    foreach (var (mat, matAmount) in partRecipe!.Materials)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
                else if (_prototype.TryIndex(defaultProtoId, out var defaultProto) &&
                         defaultProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp, EntityManager.ComponentFactory))
                {
                    foreach (var (mat, matAmount) in physComp.MaterialComposition)
                    {
                        materials.TryAdd(mat, 0);
                        materials[mat] += matAmount * amount * coefficient;
                    }
                }
            }

            return materials;
        }
    }
}
