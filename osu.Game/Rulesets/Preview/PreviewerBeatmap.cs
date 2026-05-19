// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Preview
{
    // TODO: maybe this is not even required? will find out when implementing practice screen I guess
    public partial class PreviewerBeatmap : Component, IBeatmap, IBeatSnapProvider
    {
        private readonly BeatmapInfo beatmapInfo;
        public readonly IBeatmap PlayableBeatmap;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; } = null!;

        public PreviewerBeatmap(IBeatmap playableBeatmap, BeatmapInfo? beatmapInfo = null)
        {
            PlayableBeatmap = playableBeatmap;
            this.beatmapInfo = beatmapInfo ?? playableBeatmap.BeatmapInfo;
            BeatmapVersion = PlayableBeatmap.BeatmapVersion;
        }

        public BeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set => throw new InvalidOperationException($"Can't set {nameof(BeatmapInfo)} on {nameof(PreviewerBeatmap)}");
        }

        public BeatmapMetadata Metadata => beatmapInfo.Metadata;

        public BeatmapDifficulty Difficulty
        {
            get => PlayableBeatmap.Difficulty;
            set => PlayableBeatmap.Difficulty = value;
        }

        public ControlPointInfo ControlPointInfo
        {
            get => PlayableBeatmap.ControlPointInfo;
            set => PlayableBeatmap.ControlPointInfo = value;
        }

        SortedList<BreakPeriod> IBeatmap.Breaks
        {
            get => PlayableBeatmap.Breaks;
            set => PlayableBeatmap.Breaks = value;
        }

        public List<string> UnhandledEventLines => PlayableBeatmap.UnhandledEventLines;

        public double TotalBreakTime => PlayableBeatmap.TotalBreakTime;

        public IReadOnlyList<HitObject> HitObjects => PlayableBeatmap.HitObjects;

        public IEnumerable<BeatmapStatistic> GetStatistics() => PlayableBeatmap.GetStatistics();

        public double GetMostCommonBeatLength() => PlayableBeatmap.GetMostCommonBeatLength();

        public double AudioLeadIn
        {
            get => PlayableBeatmap.AudioLeadIn;
            set => PlayableBeatmap.AudioLeadIn = value;
        }

        public float StackLeniency
        {
            get => PlayableBeatmap.StackLeniency;
            set => PlayableBeatmap.StackLeniency = value;
        }

        public bool SpecialStyle
        {
            get => PlayableBeatmap.SpecialStyle;
            set => PlayableBeatmap.SpecialStyle = value;
        }

        public bool LetterboxInBreaks
        {
            get => PlayableBeatmap.LetterboxInBreaks;
            set => PlayableBeatmap.LetterboxInBreaks = value;
        }

        public bool WidescreenStoryboard
        {
            get => PlayableBeatmap.WidescreenStoryboard;
            set => PlayableBeatmap.WidescreenStoryboard = value;
        }

        public bool EpilepsyWarning
        {
            get => PlayableBeatmap.EpilepsyWarning;
            set => PlayableBeatmap.EpilepsyWarning = value;
        }

        public bool SamplesMatchPlaybackRate
        {
            get => PlayableBeatmap.SamplesMatchPlaybackRate;
            set => PlayableBeatmap.SamplesMatchPlaybackRate = value;
        }

        public double DistanceSpacing
        {
            get => PlayableBeatmap.DistanceSpacing;
            set => PlayableBeatmap.DistanceSpacing = value;
        }

        public int GridSize
        {
            get => PlayableBeatmap.GridSize;
            set => PlayableBeatmap.GridSize = value;
        }

        public double TimelineZoom
        {
            get => PlayableBeatmap.TimelineZoom;
            set => PlayableBeatmap.TimelineZoom = value;
        }

        public CountdownType Countdown
        {
            get => PlayableBeatmap.Countdown;
            set => PlayableBeatmap.Countdown = value;
        }

        public int CountdownOffset
        {
            get => PlayableBeatmap.CountdownOffset;
            set => PlayableBeatmap.CountdownOffset = value;
        }

        int[] IBeatmap.Bookmarks
        {
            get => PlayableBeatmap.Bookmarks;
            set => PlayableBeatmap.Bookmarks = value;
        }

        public int BeatmapVersion { get; }

        public IBeatmap Clone() => (PreviewerBeatmap)MemberwiseClone();

        public double SnapTime(double time, double? referenceTime) => ControlPointInfo.GetClosestSnappedTime(time, BeatDivisor, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => ControlPointInfo.TimingPointAt(referenceTime).BeatLength / BeatDivisor;

        public int BeatDivisor => beatDivisor.Value;
    }
}
