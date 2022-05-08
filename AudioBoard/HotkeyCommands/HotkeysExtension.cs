using System;
using System.Windows;
using System.Windows.Interop;

namespace PCAFFINITY
{
    /// <exclude />
    public class HotkeysExtensionWindow : Window
    {
        public IntPtr _windowHandle;
        private const int WM_HOTKEY = 0x0312;

        private HwndSource _source;

        /// <exclude />
        public delegate void KeyPressedCallEventHandler(Window win, short k);

        /// <exclude />
        public event KeyPressedCallEventHandler KeyPressedCall;

        /// <exclude />
        public virtual void OnKeyPressedCall(IntPtr k)
        {
            KeyPressedCall?.Invoke(this, (short)k.ToInt32());
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
        }

        /// <exclude />
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_HOTKEY:
                    OnKeyPressedCall(wParam);
                    break;
            }

            return IntPtr.Zero;
        }
    }
}