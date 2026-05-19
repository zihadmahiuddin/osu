// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Preview
{
    public partial class DrawableOsuPreviewerRuleset : DrawableOsuRuleset
    {
        public DrawableOsuPreviewerRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new OsuPreviewerPlayfield();

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer { Size = Vector2.One };

        private partial class OsuPreviewerPlayfield : OsuPlayfield
        {
            protected override GameplayCursorContainer? CreateCursor() => null;

            public OsuPreviewerPlayfield()
            {
                HitPolicy = new AnyOrderHitPolicy();
            }
        }
    }
}
