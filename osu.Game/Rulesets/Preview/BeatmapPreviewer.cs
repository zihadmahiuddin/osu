// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Preview
{
    public abstract partial class BeatmapPreviewer<TObject> : BeatmapPreviewer, ISamplePlaybackDisabler
        where TObject : HitObject
    {
        /// <summary>
        /// Whether the playfield should be centered horizontally. Should be disabled for playfields which span the full horizontal width.
        /// </summary>
        protected bool ApplyHorizontalCentering => true;

        // Provides `Playfield`
        private DependencyContainer dependencies = null!;

        [Resolved]
        protected PreviewerClock PreviewerClock { get; private set; } = null!;

        [Resolved]
        protected PreviewerBeatmap PreviewerBeatmap { get; private set; } = null!;

        private DrawablePreviewerRulesetWrapper<TObject> drawableRulesetWrapper = null!;

        private readonly Bindable<bool> samplePlaybackDisabled = new Bindable<bool>();

        protected readonly Container LayerBelowRuleset = new Container { RelativeSizeAxes = Axes.Both };

        protected DrawableRuleset<TObject> DrawableRuleset { get; private set; } = null!;

        protected BeatmapPreviewer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            try
            {
                DrawableRuleset = CreateDrawableRuleset(Ruleset, PreviewerBeatmap.PlayableBeatmap, new[] { Ruleset.GetAutoplayMod()! });
                drawableRulesetWrapper = new DrawablePreviewerRulesetWrapper<TObject>(DrawableRuleset)
                {
                    Clock = PreviewerClock,
                    ProcessCustomClock = false
                };
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not load beatmap successfully!");
                return;
            }

            if (DrawableRuleset is IDrawableScrollingRuleset scrollingRuleset)
                dependencies.CacheAs(scrollingRuleset.ScrollingInfo);

            dependencies.CacheAs(drawableRulesetWrapper.Playfield);

            InternalChildren = new[]
            {
                wrapSkinnableContent(PlayfieldContentContainer = new Container
                {
                    Name = "Playfield content",
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        // layers below playfield
                        drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer().WithChild(LayerBelowRuleset),
                        drawableRulesetWrapper,
                        // layers above playfield
                        drawableRulesetWrapper.CreatePlayfieldAdjustmentContainer()
                    }
                }),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PreviewerClock.SeekingOrStopped.ValueChanged += onClockSeekingOrStoppedChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            PreviewerClock.SeekingOrStopped.ValueChanged -= onClockSeekingOrStoppedChanged;
        }

        private void onClockSeekingOrStoppedChanged(ValueChangedEvent<bool> e)
        {
            samplePlaybackDisabled.Value = e.NewValue;
        }

        private Drawable wrapSkinnableContent(Drawable content)
        {
            Debug.Assert(Ruleset != null);

            return new RulesetSkinProvidingContainer(Ruleset, PreviewerBeatmap.PlayableBeatmap, null).WithChild(content);
        }

        /// <summary>
        /// Houses all content relevant to the playfield.
        /// </summary>
        /// <remarks>
        /// Generally implementations should not be adding to this directly.
        /// Use <see cref="LayerBelowRuleset"/> or <see cref="BlueprintContainer{T}"/> instead.
        /// </remarks>
        protected Container PlayfieldContentContainer { get; private set; } = null!;

        protected override void Update()
        {
            base.Update();

            if (ApplyHorizontalCentering)
            {
                PlayfieldContentContainer.Anchor = Anchor.Centre;
                PlayfieldContentContainer.Origin = Anchor.Centre;

                // Ensure that the playfield is always centered but also doesn't get cut off by toolboxes.
                // TODO: this is copied from the editor, I don't think this should be kept since there are no toolboxes
                // TODO: but without it the playfield looks too big/zoomed in (at least in the test browser)
                PlayfieldContentContainer.Width = Math.Max(1024, DrawWidth) - TOOLBOX_CONTRACTED_SIZE_RIGHT * 2;
                PlayfieldContentContainer.X = 0;
            }
            else
            {
                PlayfieldContentContainer.Anchor = Anchor.CentreLeft;
                PlayfieldContentContainer.Origin = Anchor.CentreLeft;

                PlayfieldContentContainer.Width = Math.Max(1024, DrawWidth);
            }
        }

        /// <summary>
        /// Construct a drawable ruleset for the provided ruleset.
        /// </summary>
        /// <remarks>
        /// Can be overridden to add previewer-specific logical changes to a <see cref="Ruleset"/>'s standard <see cref="DrawableRuleset{TObject}"/>.
        /// For example, hit animations or judgement logic may be changed to give a better preview user experience.
        /// </remarks>
        /// <param name="ruleset">The ruleset used to construct its drawable counterpart.</param>
        /// <param name="beatmap">The loaded beatmap.</param>
        /// <param name="mods">The mods to be applied.</param>
        /// <returns>A preview-relevant <see cref="DrawableRuleset{TObject}"/>.</returns>
        protected virtual DrawableRuleset<TObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => (DrawableRuleset<TObject>)ruleset.CreateDrawableRulesetWith(beatmap, mods);

        public IBindable<bool> SamplePlaybackDisabled => samplePlaybackDisabled;
    }

    /// <summary>
    /// A non-generic definition of a beatmap previewer class.
    /// Generally used to access certain methods/properties without requiring a generic type for <see cref="BeatmapPreviewer{TObject}" />.
    /// </summary>
    [Cached]
    public abstract partial class BeatmapPreviewer : CompositeDrawable
    {
        public const float TOOLBOX_CONTRACTED_SIZE_RIGHT = 120;

        public readonly Ruleset Ruleset;

        protected BeatmapPreviewer(Ruleset ruleset)
        {
            Ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }
    }
}
