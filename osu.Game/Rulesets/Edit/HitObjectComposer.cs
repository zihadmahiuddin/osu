// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Edit
{
    public abstract class HitObjectComposer : CompositeDrawable
    {
        public IEnumerable<DrawableHitObject> HitObjects => DrawableRuleset.Playfield.AllHitObjects;

        protected readonly Ruleset Ruleset;

        protected readonly IBindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        protected IRulesetConfigManager Config { get; private set; }

        private readonly List<Container> layerContainers = new List<Container>();

        protected DrawableEditRuleset DrawableRuleset { get; private set; }

        private BlueprintContainer blueprintContainer;

        private InputManager inputManager;

        internal HitObjectComposer(Ruleset ruleset)
        {
            Ruleset = ruleset;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, IFrameBasedClock framedClock)
        {
            Beatmap.BindTo(beatmap);

            try
            {
                DrawableRuleset = CreateDrawableRuleset();
                DrawableRuleset.Clock = framedClock;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap sucessfully!");
                return;
            }

            var layerBelowRuleset = DrawableRuleset.CreatePlayfieldAdjustmentContainer();
            layerBelowRuleset.Child = new EditorPlayfieldBorder { RelativeSizeAxes = Axes.Both };

            var layerAboveRuleset = DrawableRuleset.CreatePlayfieldAdjustmentContainer();
            layerAboveRuleset.Child = blueprintContainer = new BlueprintContainer();

            layerContainers.Add(layerBelowRuleset);
            layerContainers.Add(layerAboveRuleset);

            RadioButtonCollection toolboxCollection;
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Name = "Sidebar",
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = 10 },
                            Children = new Drawable[]
                            {
                                new ToolboxGroup { Child = toolboxCollection = new RadioButtonCollection { RelativeSizeAxes = Axes.X } }
                            }
                        },
                        new Container
                        {
                            Name = "Content",
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                layerBelowRuleset,
                                DrawableRuleset,
                                layerAboveRuleset
                            }
                        }
                    },
                },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 200),
                }
            };

            toolboxCollection.Items =
                CompositionTools.Select(t => new RadioButton(t.Name, () => blueprintContainer.CurrentTool = t))
                                .Prepend(new RadioButton("Select", () => blueprintContainer.CurrentTool = null))
                                .ToList();

            toolboxCollection.Items[0].Select();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(this);
            Config = dependencies.Get<RulesetConfigCache>().GetConfigFor(Ruleset);

            return dependencies;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            layerContainers.ForEach(l =>
            {
                l.Anchor = DrawableRuleset.Playfield.Anchor;
                l.Origin = DrawableRuleset.Playfield.Origin;
                l.Position = DrawableRuleset.Playfield.Position;
                l.Size = DrawableRuleset.Playfield.Size;
            });
        }

        /// <summary>
        /// Whether the user's cursor is currently in an area of the <see cref="HitObjectComposer"/> that is valid for placement.
        /// </summary>
        public virtual bool CursorInPlacementArea => DrawableRuleset.Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

        internal abstract DrawableEditRuleset CreateDrawableRuleset();

        protected abstract IReadOnlyList<HitObjectCompositionTool> CompositionTools { get; }

        /// <summary>
        /// Creates a <see cref="SelectionBlueprint"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        public virtual SelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) => null;

        /// <summary>
        /// Creates a <see cref="SelectionHandler"/> which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
        /// </summary>
        public virtual SelectionHandler CreateSelectionHandler() => new SelectionHandler();
    }

    [Cached(Type = typeof(IPlacementHandler))]
    public abstract class HitObjectComposer<TObject> : HitObjectComposer, IPlacementHandler
        where TObject : HitObject
    {
        private Beatmap<TObject> playableBeatmap;

        [Cached]
        [Cached(typeof(IEditorBeatmap))]
        private EditorBeatmap<TObject> editorBeatmap;

        protected HitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var workingBeatmap = parent.Get<IBindable<WorkingBeatmap>>();
            playableBeatmap = (Beatmap<TObject>)workingBeatmap.Value.GetPlayableBeatmap(Ruleset.RulesetInfo, Array.Empty<Mod>());

            editorBeatmap = new EditorBeatmap<TObject>(playableBeatmap);
            editorBeatmap.HitObjectAdded += addHitObject;
            editorBeatmap.HitObjectRemoved += removeHitObject;

            return base.CreateChildDependencies(parent);
        }

        private void addHitObject(HitObject hitObject)
        {
            // Process object
            var processor = Ruleset.CreateBeatmapProcessor(playableBeatmap);

            processor?.PreProcess();
            hitObject.ApplyDefaults(playableBeatmap.ControlPointInfo, playableBeatmap.BeatmapInfo.BaseDifficulty);
            processor?.PostProcess();
        }

        private void removeHitObject(HitObject hitObject)
        {
            var processor = Ruleset.CreateBeatmapProcessor(playableBeatmap);

            processor?.PreProcess();
            processor?.PostProcess();
        }

        internal override DrawableEditRuleset CreateDrawableRuleset()
            => new DrawableEditRuleset<TObject>(CreateDrawableRuleset(Ruleset, Beatmap.Value, Array.Empty<Mod>()));

        protected abstract DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, WorkingBeatmap beatmap, IReadOnlyList<Mod> mods);

        public void BeginPlacement(HitObject hitObject)
        {
        }

        public void EndPlacement(HitObject hitObject) => editorBeatmap.Add(hitObject);

        public void Delete(HitObject hitObject) => editorBeatmap.Remove(hitObject);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editorBeatmap != null)
            {
                editorBeatmap.HitObjectAdded -= addHitObject;
                editorBeatmap.HitObjectRemoved -= removeHitObject;
            }
        }
    }
}
