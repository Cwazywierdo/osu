﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using OpenTK;

namespace osu.Game.Screens.Multi.Screens
{
    public class Lounge : MultiplayerScreen
    {
        private readonly Container content;
        private readonly FillFlowContainer<DrawableRoom> roomsFlow;
        private readonly RoomInspector roomInspector;

        protected override Container<Drawable> TransitionContent => content;

        public override string Title => "lounge";
        public override string Name => "Lounge";

        private IEnumerable<Room> rooms;
        public IEnumerable<Room> Rooms
        {
            get { return rooms; }
            set
            {
                if (Equals(value, rooms)) return;
                rooms = value;

                var enumerable = rooms.ToList();

                roomsFlow.Children = enumerable.Select(r => new DrawableRoom(r)
                {
                    Action = didSelect,
                }).ToList();

                if (!enumerable.Contains(roomInspector.Room))
                    roomInspector.Room = null;
            }
        }

        public Lounge()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.55f,
                            Padding = new MarginPadding
                            {
                                Vertical = 35 - DrawableRoom.SELECTION_BORDER_WIDTH,
                                Right = 20 - DrawableRoom.SELECTION_BORDER_WIDTH
                            },
                            Child = roomsFlow = new FillFlowContainer<DrawableRoom>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                LayoutEasing = Easing.OutQuint,
                                LayoutDuration = 200,
                                Spacing = new Vector2(10 - DrawableRoom.SELECTION_BORDER_WIDTH * 2),
                            },
                        },
                        roomInspector = new RoomInspector
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.45f,
                        },
                    },
                },
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            content.Padding = new MarginPadding
            {
                Left = SearchableListOverlay.WIDTH_PADDING - DrawableRoom.SELECTION_BORDER_WIDTH,
                Right = SearchableListOverlay.WIDTH_PADDING,
            };
        }

        private void didSelect(DrawableRoom room)
        {
            roomsFlow.Children.ForEach(c =>
            {
                if (c != room)
                    c.State = SelectionState.NotSelected;
            });

            roomInspector.Room = room.Room;

            // open the room if its selected and is clicked again
            if (room.State == SelectionState.Selected)
                Push(new Match(room.Room));
        }
    }
}
