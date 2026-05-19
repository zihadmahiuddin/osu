// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Preview;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual.Preview
{
    [TestFixture]
    public partial class TestSceneBeatmapPreviewer : PreviewerClockTestScene
    {
        private Ruleset? ruleset;
        private BeatmapPreviewer? beatmapPreviewer;

        private void testRuleset(Func<Ruleset> createRuleset)
        {
            AddStep("clear previous preview", () =>
            {
                Clear();
                beatmapPreviewer = null;
                ruleset = null;
            });

            AddStep("create ruleset", () => ruleset = createRuleset());

            AddStep("create beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(ruleset!.RulesetInfo);
            });

            AddStep("create previewer", () =>
            {
                if (ruleset == null) return;

                beatmapPreviewer = ruleset.CreateBeatmapPreviewer()?.With(bp =>
                {
                    // force the previewer to fully overlap the playfield area by setting a 4:3 aspect ratio.
                    bp.FillMode = FillMode.Fit;
                    bp.FillAspectRatio = 4f / 3f;
                });
                if (beatmapPreviewer == null) return;

                Child = new PreviewerBeatmapContainer(Beatmap.Value, ruleset)
                {
                    Child = beatmapPreviewer
                };
            });

            AddStep("start previewer clock", () =>
            {
                PreviewerClock.Start();
            });
        }

        [Test]
        public void TestOsuBeatmapPreview()
        {
            testRuleset(() => new OsuRuleset());
        }

        [Test]
        public void TestManiaBeatmapPreview()
        {
            testRuleset(() => new ManiaRuleset());
        }

        [Test]
        public void TestTaikoBeatmapPreview()
        {
            testRuleset(() => new TaikoRuleset());
        }

        // TODO: some objects might be appearing outside the playfield boundary?
        // TODO: I don't play catch so I can't tell if it's normal
        [Test]
        public void TestCatchBeatmapPreview()
        {
            testRuleset(() => new CatchRuleset());
        }

        public partial class PreviewerBeatmapContainer : PopoverContainer
        {
            private readonly IWorkingBeatmap working;
            private readonly Ruleset ruleset;

            public PreviewerBeatmap PreviewerBeatmap { get; private set; } = null!;

            public PreviewerBeatmapContainer(IWorkingBeatmap working, Ruleset ruleset)
            {
                this.working = working;
                this.ruleset = ruleset;

                RelativeSizeAxes = Axes.Both;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

                PreviewerBeatmap = new PreviewerBeatmap(working.GetPlayableBeatmap(ruleset.RulesetInfo));

                dependencies.CacheAs(PreviewerBeatmap);
                dependencies.CacheAs<IBeatSnapProvider>(PreviewerBeatmap);

                return dependencies;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Add(PreviewerBeatmap);
            }
        }
    }
}
