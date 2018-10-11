// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTSTRUCT
        {
            public int X;
            public int Y;
        }

        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINTSTRUCT pt);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        internal static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, out POINTSTRUCT pt, int cPoints);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int key);

        private static IntPtr _windowHandle;
        private static MouseInputWnd _mouseInputWnd = new MouseInputWnd();

        private static IntPtr PlatformGetWindowHandle()
        {
            return _windowHandle;
        }

        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            // Release old Handle
            if (_mouseInputWnd.Handle != IntPtr.Zero)
                _mouseInputWnd.ReleaseHandle();

            _windowHandle = windowHandle;
            _mouseInputWnd.AssignHandle(windowHandle);
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            return window.MouseState;
        }

        private static MouseState PlatformGetState()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                POINTSTRUCT pos;
                GetCursorPos(out pos);
                MapWindowPoints(IntPtr.Zero, _windowHandle, out pos, 1);
                var clientPos = new System.Drawing.Point(pos.X, pos.Y);

                //Use GetAsyncKeyState instead of Control.MouseButtons since this also works from other threads than the main thread

                bool leftButton = GetAsyncKeyState(0x01) < 0;
                bool rightButton = GetAsyncKeyState(0x02) < 0;
                bool middleButton = GetAsyncKeyState(0x04) < 0;
                bool xButton1 = GetAsyncKeyState(0x05) < 0;
                bool xButton2 = GetAsyncKeyState(0x06) < 0;

                return new MouseState(
                    clientPos.X,
                    clientPos.Y,
                    _mouseInputWnd.ScrollWheelValue,
                    leftButton ? ButtonState.Pressed : ButtonState.Released,
                    middleButton ? ButtonState.Pressed : ButtonState.Released,
                    rightButton ? ButtonState.Pressed : ButtonState.Released,
                    xButton1 ? ButtonState.Pressed : ButtonState.Released,
                    xButton2 ? ButtonState.Pressed : ButtonState.Released,
                    _mouseInputWnd.HorizontalScrollWheelValue
                    );
            }

            return _defaultState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            if (PrimaryWindow != null)
            {
                PrimaryWindow.MouseState.X = x;
                PrimaryWindow.MouseState.Y = y;
            }

            SetCursorPos(x, y);

            //var pt = _window.PointToScreen(new System.Drawing.Point(x, y));
            //SetCursorPos(pt.X, pt.Y);
        }

        public static void PlatformSetCursor(MouseCursor cursor)
        {
            //_window.Cursor = cursor.Cursor;
        }

        #region Nested class MouseInputWnd

        /// <remarks>
        /// Subclass WindowHandle to read WM_MOUSEWHEEL and WM_MOUSEHWHEEL messages
        /// </remarks>
        class MouseInputWnd : NativeWindow
        {
            const int WM_MOUSEWHEEL = 0x020A;
            const int WM_MOUSEHWHEEL = 0x020E;

            public int ScrollWheelValue = 0;
            public int HorizontalScrollWheelValue = 0;

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_MOUSEWHEEL:
                        var delta = (short)(((ulong)m.WParam >> 16) & 0xffff);
                        ScrollWheelValue += delta;
                        break;
                    case WM_MOUSEHWHEEL:
                        var deltaH = (short)(((ulong)m.WParam >> 16) & 0xffff);
                        HorizontalScrollWheelValue += deltaH;
                        break;
                }

                base.WndProc(ref m);
            }
        }

        #endregion Nested class MouseInputWnd
    }
}
