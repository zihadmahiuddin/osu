// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Preview;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Preview
{
    [Cached]
    public partial class OsuBeatmapPreviewer : BeatmapPreviewer<OsuHitObject>
    {
        public OsuBeatmapPreviewer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override DrawableRuleset<OsuHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new DrawableOsuPreviewerRuleset(ruleset, beatmap, mods);

        [Cached]
        protected readonly FreehandSliderToolboxGroup FreehandSliderToolboxGroup = new FreehandSliderToolboxGroup();

        [BackgroundDependencyLoader]
        private void load()
        {
            // Give a bit of breathing room around the playfield content.
            PlayfieldContentContainer.Padding = new MarginPadding(10);
        }
    }
}
