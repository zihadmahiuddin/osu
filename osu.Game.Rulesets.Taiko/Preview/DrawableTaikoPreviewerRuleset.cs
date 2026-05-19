// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Preview
{
    public partial class DrawableTaikoPreviewerRuleset : DrawableTaikoRuleset
    {
        public DrawableTaikoPreviewerRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new TaikoPreviewerPlayfield();

        private partial class TaikoPreviewerPlayfield : TaikoPlayfield
        {
            protected override GameplayCursorContainer? CreateCursor() => null;
        }
    }
}
