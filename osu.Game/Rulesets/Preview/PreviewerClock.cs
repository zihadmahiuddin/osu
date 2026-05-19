// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Preview
{
    // TODO: this is mostly just EditorClock, mainly the seeking code is copied from there
    // TODO: maybe it would be better to have a "SeekableClock" that both of these derive from instead?
    // TODO: add audio adjustments for playback control during preview
    public partial class PreviewerClock : CompositeComponent, IAdjustableClock, IFrameBasedClock, ISourceChangeableClock
    {
        public IBeatmap Beatmap { get; }

        private readonly Bindable<Track> track = new Bindable<Track>();

        public double TrackLength => track.Value?.IsLoaded == true ? track.Value.Length : 60000;

        public ControlPointInfo ControlPointInfo => Beatmap.ControlPointInfo;

        private readonly BindableBeatDivisor beatDivisor;

        private readonly FramedBeatmapClock underlyingClock;

        private const double transform_time = 300;

        public PreviewerClock(IBeatmap? beatmap, BindableBeatDivisor? beatDivisor)
        {
            Beatmap = beatmap ?? new Beatmap();

            this.beatDivisor = beatDivisor ?? new BindableBeatDivisor();

            underlyingClock = new FramedBeatmapClock(applyOffsets: true, requireDecoupling: true);
            AddInternal(underlyingClock);
        }

        #region Clock interfaces

        public double CurrentTime => underlyingClock.CurrentTime;

        double IAdjustableClock.Rate
        {
            get => underlyingClock.Rate;
            set => underlyingClock.Rate = value;
        }

        double IClock.Rate => underlyingClock.Rate;

        public bool IsRunning => underlyingClock.IsRunning;

        public double ElapsedFrameTime => underlyingClock.ElapsedFrameTime;

        public double FramesPerSecond => underlyingClock.FramesPerSecond;

        public IClock Source => underlyingClock.Source;

        public void Reset()
        {
            underlyingClock.Reset();
        }

        public void Start()
        {
            underlyingClock.Start();
        }

        public void Stop()
        {
            seekingOrStopped.Value = true;
            underlyingClock.Stop();
        }

        public bool Seek(double position)
        {
            seekingOrStopped.Value = IsSeeking = true;

            ClearTransforms();

            // Ensure the sought point is within the boundaries
            position = Math.Clamp(position, 0, TrackLength);
            return underlyingClock.Seek(position);
        }

        public void ResetSpeedAdjustments()
        {
            underlyingClock.ResetSpeedAdjustments();
        }

        public void ProcessFrame()
        {
        }

        public void ChangeSource(IClock source)
        {
            track.Value = (source as Track)!;
            underlyingClock.ChangeSource(source);
        }

        #endregion

        #region Seeking

        public IBindable<bool> SeekingOrStopped => seekingOrStopped;

        private readonly Bindable<bool> seekingOrStopped = new Bindable<bool>(true);

        /// <summary>
        /// Whether a seek is currently in progress. True for the duration of a seek performed via <see cref="SeekSmoothlyTo"/>.
        /// </summary>
        public bool IsSeeking { get; private set; }

        /// <summary>
        /// Seeks backwards by one beat length.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat after seeking.</param>
        /// <param name="amount">The relative amount (magnitude) which should be seeked.</param>
        public void SeekBackward(bool snapped = false, double amount = 1) => seek(-1, snapped, amount);

        /// <summary>
        /// Seeks forwards by one beat length.
        /// </summary>
        /// <param name="snapped">Whether to snap to the closest beat after seeking.</param>
        /// <param name="amount">The relative amount (magnitude) which should be seeked.</param>
        public void SeekForward(bool snapped = false, double amount = 1) => seek(1, snapped, amount);

        private void seek(int direction, bool snapped, double amount = 1)
        {
            double current = CurrentTimeAccurate;

            if (amount <= 0) throw new ArgumentException("Value should be greater than zero", nameof(amount));

            var timingPoint = ControlPointInfo.TimingPointAt(current);

            if (IsRunning)
            {
                // when track is playing, seek a bit more than usual.
                // the amount adjusted matches stable, because don't-break-what-works.
                // https://github.com/peppy/osu-stable-reference/blob/7519cafd1823f1879c0d9c991ba0e5c7fd3bfa02/osu!/GameModes/Edit/Editor.cs#L1639-L1640
                //
                // ReSharper disable once PossibleLossOfFraction
                amount *= 1 + 250 / (int)timingPoint.BeatLength;
            }

            if (direction < 0 && timingPoint.Time == current)
                // When going backwards and we're at the boundary of two timing points, we compute the seek distance with the timing point which we are seeking into
                timingPoint = ControlPointInfo.TimingPointAt(current - 1);

            double seekAmount = timingPoint.BeatLength / beatDivisor.Value * amount;
            double seekTime = current + seekAmount * direction;

            if (!snapped || ControlPointInfo.TimingPoints.Count == 0)
            {
                SeekSmoothlyTo(seekTime);
                return;
            }

            // We will be snapping to beats within timingPoint
            seekTime -= timingPoint.Time;

            // Determine the index from timingPoint of the closest beat to seekTime, accounting for scrolling direction
            int closestBeat;
            if (direction > 0)
                closestBeat = (int)Math.Floor(seekTime / seekAmount);
            else
                closestBeat = (int)Math.Ceiling(seekTime / seekAmount);

            seekTime = timingPoint.Time + closestBeat * seekAmount;

            // limit forward seeking to only up to the next timing point's start time.
            var nextTimingPoint = ControlPointInfo.TimingPointAfter(timingPoint.Time);
            if (seekTime > nextTimingPoint?.Time)
                seekTime = nextTimingPoint.Time;

            // Due to the rounding above, we may end up on the current beat. This will effectively cause 0 seeking to happen, but we don't want this.
            // Instead, we'll go to the next beat in the direction when this is the case
            if (Precision.AlmostEquals(current, seekTime, 0.5f))
            {
                closestBeat += direction > 0 ? 1 : -1;
                seekTime = timingPoint.Time + closestBeat * seekAmount;
            }

            if (seekTime < timingPoint.Time && !ReferenceEquals(timingPoint, ControlPointInfo.TimingPoints.First()))
                seekTime = timingPoint.Time;

            SeekSmoothlyTo(seekTime);
        }

        /// <summary>
        /// Seek smoothly to the provided destination, if within a certain proximity to the current viewport.
        /// Use <see cref="Seek"/> to perform an immediate seek.
        /// </summary>
        /// <param name="seekDestination"></param>
        public void SeekSmoothlyTo(double seekDestination)
        {
            seekingOrStopped.Value = true;

            // The whole point of seeking smoothly is to maintain continuity for the user.
            // Above a certain proximity, there's little reason to do this as the jump is already huge.
            const double smooth_seek_max_proximity = 5000;

            if (IsRunning || Math.Abs(seekDestination - currentTime) > smooth_seek_max_proximity)
            {
                Seek(seekDestination);
                return;
            }

            transformSeekTo(seekDestination, transform_time, Easing.OutQuint);
        }

        private void transformSeekTo(double seek, double duration = 0, Easing easing = Easing.None)
            => this.TransformTo(this.PopulateTransform(new TransformSeek(), Math.Clamp(seek, 0, TrackLength), duration, easing));

        private void updateSeekingState()
        {
            if (seekingOrStopped.Value)
            {
                IsSeeking &= Transforms.Any();

                if (!IsRunning)
                {
                    // seeking in the editor can happen while the track isn't running.
                    // in this case we always want to expose ourselves as seeking (to avoid sample playback).
                    return;
                }

                // we are either running a seek tween or doing an immediate seek.
                // in the case of an immediate seek the seeking bool will be set to false after one update.
                // this allows for silencing hit sounds and the likes.
                seekingOrStopped.Value = IsSeeking;
            }
        }

        #endregion

        /// <summary>
        /// The current time of this clock, include any active transform seeks performed via <see cref="SeekSmoothlyTo"/>.
        /// </summary>
        public double CurrentTimeAccurate =>
            Transforms.OfType<TransformSeek>().LastOrDefault()?.EndValue ?? CurrentTime;

        protected override void Update()
        {
            base.Update();

            updateSeekingState();
        }

        private double currentTime
        {
            get => underlyingClock.CurrentTime;
            set => underlyingClock.Seek(value);
        }

        private class TransformSeek : Transform<double, PreviewerClock>
        {
            public override string TargetMember => nameof(currentTime);

            protected override void Apply(PreviewerClock clock, double time) => clock.currentTime = valueAt(time);

            private double valueAt(double time)
            {
                if (time < StartTime) return StartValue;
                if (time >= EndTime) return EndValue;

                return Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);
            }

            protected override void ReadIntoStartValue(PreviewerClock clock) => StartValue = clock.currentTime;
        }
    }
}
