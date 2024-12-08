﻿using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Client
{
    public enum DragBoxAccessors
    {
        BoxSize = 16,
        AlignToEdge = 17
    }

    /// <summary>
    ///     A terminal control that uses a draggable window to indicate a position on the screen.
    /// </summary>
    public class TerminalDragBox : TerminalValue<Vector2>
    {
        public TerminalDragBox() : base(MenuControls.DragBox)
        {
        }

        /// <summary>
        ///     Size of the window spawned by the control.
        /// </summary>
        public Vector2 BoxSize
        {
            get { return (Vector2)GetOrSetMember(null, (int)DragBoxAccessors.BoxSize); }
            set { GetOrSetMember(value, (int)DragBoxAccessors.BoxSize); }
        }

        /// <summary>
        ///     Determines whether or not the window will automatically align itself to one side of the screen
        ///     or the other.
        /// </summary>
        public bool AlignToEdge
        {
            get { return (bool)GetOrSetMember(null, (int)DragBoxAccessors.AlignToEdge); }
            set { GetOrSetMember(value, (int)DragBoxAccessors.AlignToEdge); }
        }
    }
}