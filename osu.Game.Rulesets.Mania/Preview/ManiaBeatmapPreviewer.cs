// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Preview;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Preview
{
    [Cached]
    public partial class ManiaBeatmapPreviewer : BeatmapPreviewer<ManiaHitObject>
    {
        public ManiaBeatmapPreviewer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override DrawableRuleset<ManiaHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new DrawableManiaPreviewerRuleset(ruleset, beatmap, mods);
    }
}
