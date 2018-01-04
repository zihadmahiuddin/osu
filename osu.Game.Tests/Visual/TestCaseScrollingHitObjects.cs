﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseScrollingHitObjects : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(Playfield) };

        private readonly List<TestPlayfield> playfields = new List<TestPlayfield>();

        public TestCaseScrollingHitObjects()
        {
            playfields.Add(new TestPlayfield(Direction.Vertical));
            playfields.Add(new TestPlayfield(Direction.Horizontal));

            Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.85f),
                Masking = true,
                BorderColour = Color4.White,
                BorderThickness = 2,
                MaskingSmoothness = 1,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Name = "Background",
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.35f,
                    },
                    playfields[0],
                    playfields[1]
                }
            });

            AddSliderStep("Time range", 100, 10000, 5000, v => playfields.ForEach(p => p.TimeRange.Value = v));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            for (int i = 0; i <= 5000; i += 1000)
                addHitObject(Time.Current + i);

            Scheduler.AddDelayed(() => addHitObject(Time.Current + 5000), 1000, true);
        }

        private void addHitObject(double time)
        {
            playfields.ForEach(p =>
            {
                p.Add(new TestDrawableHitObject(new HitObject { StartTime = time })
                {
                    Anchor = p.ScrollingDirection == Direction.Horizontal ? Anchor.CentreRight : Anchor.BottomCentre
                });
            });
        }

        private class ScrollingHitObjectContainer : Playfield.HitObjectContainer
        {
            public readonly BindableDouble TimeRange = new BindableDouble
            {
                MinValue = 0,
                MaxValue = double.MaxValue
            };

            private readonly Direction scrollingDirection;

            public ScrollingHitObjectContainer(Direction scrollingDirection)
            {
                this.scrollingDirection = scrollingDirection;

                RelativeSizeAxes = Axes.Both;
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                foreach (var obj in AliveObjects)
                {
                    var relativePosition = (Time.Current - obj.HitObject.StartTime) / TimeRange;

                    // Todo: We may need to consider scale here
                    var finalPosition = (float)relativePosition * DrawSize;

                    switch (scrollingDirection)
                    {
                        case Direction.Horizontal:
                            obj.X = finalPosition.X;
                            break;
                        case Direction.Vertical:
                            obj.Y = finalPosition.Y;
                            break;
                    }
                }
            }
        }

        private class TestPlayfield : Playfield
        {
            public readonly BindableDouble TimeRange = new BindableDouble(5000);

            public readonly Direction ScrollingDirection;

            public TestPlayfield(Direction scrollingDirection)
            {
                ScrollingDirection = scrollingDirection;

                var scrollingHitObjects = new ScrollingHitObjectContainer(scrollingDirection);
                scrollingHitObjects.TimeRange.BindTo(TimeRange);

                HitObjects = scrollingHitObjects;
            }
        }

        private class TestDrawableHitObject : DrawableHitObject<HitObject>
        {
            public TestDrawableHitObject(HitObject hitObject)
                : base(hitObject)
            {
                Origin = Anchor.Centre;
                AutoSizeAxes = Axes.Both;

                Add(new Box { Size = new Vector2(75) });
            }

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
