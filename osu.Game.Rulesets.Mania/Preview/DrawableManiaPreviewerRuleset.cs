// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Preview
{
    public partial class DrawableManiaPreviewerRuleset : DrawableManiaRuleset
    {
        public DrawableManiaPreviewerRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new ManiaPreviewerPlayfield(Beatmap.Stages);

        private partial class ManiaPreviewerPlayfield : ManiaPlayfield
        {
            protected override GameplayCursorContainer? CreateCursor() => null;

            public ManiaPreviewerPlayfield(List<StageDefinition> stages)
                : base(stages)
            {
            }
        }
    }
}
