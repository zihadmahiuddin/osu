// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Preview;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Preview
{
    [Cached]
    public partial class CatchBeatmapPreviewer : BeatmapPreviewer<CatchHitObject>
    {
        public CatchBeatmapPreviewer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override DrawableRuleset<CatchHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new DrawableCatchPreviewerRuleset(ruleset, beatmap, mods);
    }
}
