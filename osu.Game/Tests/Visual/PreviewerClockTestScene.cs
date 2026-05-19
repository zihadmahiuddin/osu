// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Preview;
using osu.Game.Screens.Edit;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    public abstract partial class PreviewerClockTestScene : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        protected readonly BindableBeatDivisor BeatDivisor = new BindableBeatDivisor();

        protected PreviewerClock PreviewerClock = null!;

        private readonly Bindable<double> frequencyAdjustment = new BindableDouble(1);

        private IBeatmap previewerClockBeatmap = null!;
        protected virtual bool ScrollUsingMouseWheel => true;

        protected override Container<Drawable> Content => content;

        private readonly Container<Drawable> content = new Container { RelativeSizeAxes = Axes.Both };

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            previewerClockBeatmap = CreatePreviewerClockBeatmap();

            base.Content.AddRange(new Drawable[]
            {
                PreviewerClock = new PreviewerClock(previewerClockBeatmap, BeatDivisor),
                content
            });

            dependencies.Cache(BeatDivisor);
            dependencies.CacheAs(PreviewerClock);

            return dependencies;
        }

        protected override void LoadComplete()
        {
            Beatmap.Value = CreateWorkingBeatmap(previewerClockBeatmap);

            base.LoadComplete();

            Beatmap.BindValueChanged(beatmapChanged, true);

            AddSliderStep("previewer clock rate", 0.0, 2.0, 1.0, v => frequencyAdjustment.Value = v);
        }

        protected IBeatmap CreatePreviewerClockBeatmap() => new Beatmap();

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> e)
        {
            e.OldValue.Track.RemoveAdjustment(AdjustableProperty.Frequency, frequencyAdjustment);
            e.NewValue.Track.AddAdjustment(AdjustableProperty.Frequency, frequencyAdjustment);
            PreviewerClock.ChangeSource(e.NewValue.Track);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (!ScrollUsingMouseWheel)
                return false;

            if (e.ScrollDelta.Y > 0)
                PreviewerClock.SeekBackward(true);
            else
                PreviewerClock.SeekForward(true);

            return true;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            if (e.ControlPressed || e.ShiftPressed || e.AltPressed || e.SuperPressed)
                return false;

            switch (e.Key)
            {
                case Key.Space:
                {
                    if (PreviewerClock.IsRunning)
                        PreviewerClock.Stop();
                    else
                        PreviewerClock.Start();
                    return true;
                }

                case Key.Left:
                    PreviewerClock.SeekBackward();
                    return true;

                case Key.Right:
                    PreviewerClock.SeekForward();
                    return true;

                default:
                    return base.OnKeyDown(e);
            }
        }
    }
}
