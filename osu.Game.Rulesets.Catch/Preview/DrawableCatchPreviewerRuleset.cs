// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Preview
{
    public partial class DrawableCatchPreviewerRuleset : DrawableCatchRuleset
    {
        public DrawableCatchPreviewerRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new CatchPreviewerPlayfield(Beatmap.Difficulty);

        private partial class CatchPreviewerPlayfield : CatchPlayfield
        {
            public CatchPreviewerPlayfield(IBeatmapDifficultyInfo difficulty)
                : base(difficulty) { }

            protected override GameplayCursorContainer? CreateCursor() => null;
        }
    }
}
