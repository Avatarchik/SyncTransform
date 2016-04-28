using System.Runtime.InteropServices;
using UnityEngine;
using DataUI;

namespace Gist {

    public class WindowControl : MonoBehaviour {
        public const int GWL_STYLE = -16;
        public const int WS_CHILD = 0x40000000; //child window
        public const int WS_BORDER = 0x00800000; //window with border
        public const int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public const int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar
        public const int WP_SWP_NOSIZE = 0x0001;

        public Data data;
        public bool guiVisibility;
        public KeyCode guiToggle = KeyCode.W;

        FieldEditor _editor;
        Rect _window = new Rect(10, 10, 200, 200);

        void OnEnable() {
            if (data.applyOnEnable)
                Apply();
            _editor = new FieldEditor (data);
        }
        void Update() {
            if (Input.GetKeyDown (guiToggle))
                guiVisibility = !guiVisibility;
        }
        void OnGUI() {
            if (guiVisibility && _editor != null)
                _window = GUILayout.Window (GetInstanceID (), _window, Window, name);
        }

        void Window(int id) {
            GUILayout.BeginVertical ();

            _editor.OnGUI ();
            if (GUILayout.Button ("Apply"))
                Apply ();
            if (GUILayout.Button ("Load Current"))
                Current ();

            GUILayout.EndVertical ();
            GUI.DragWindow ();
        }
        bool Apply() {
            if (Application.isEditor) {
                //Debug.LogError ("Application shouldn't be running in Editor");
                return false;
            }
            
            System.IntPtr hwnd;
            if (!GetActiveWindow(out hwnd))
                return false;

            if (data.hideTitleBar) {
                var style = GetWindowLong (hwnd, GWL_STYLE);
                if (SetWindowLong (hwnd, GWL_STYLE, (style & ~WS_CAPTION)) == 0)
                    Debug.LogError ("Failed to Hide Title Bar");
            }
            var flags = data.width * data.height == 0 ? WP_SWP_NOSIZE : 0;
            if (!SetWindowPos (hwnd, 0, data.x, data.y, data.width, data.height, flags))
                Debug.LogFormat ("Failed to Sset Window Position");
            return true;
        }
        bool Current() {
            System.IntPtr hwnd;
            if (!GetActiveWindow (out hwnd))
                return false;
            NativeRect rect;
            if (!GetClientRect (hwnd, out rect)) {
                Debug.LogError ("Failed to Get Client Rect");
                return false;
            }

            data.x = rect.left;
            data.y = rect.top;
            data.width = rect.right - rect.left;
            data.height = rect.bottom - rect.top;
            _editor.Load ();
            return true;
        }

        static bool GetActiveWindow (out System.IntPtr hwnd) {
            hwnd = GetActiveWindow ();
            if (hwnd == System.IntPtr.Zero) {
                Debug.LogError ("Failed to Get Active Window Handle");
                return false;
            }
            return true;
        }

        [System.Serializable]
        public class Data {
            public int x = 0;
            public int y = 0;
            public int width = 1920;
            public int height = 1080;
            public bool applyOnEnable = false;
            public bool hideTitleBar = true;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeRect {
            public System.Int32 left;
            public System.Int32 top;
            public System.Int32 right;
            public System.Int32 bottom;
        }

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(System.IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        public static extern System.IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll")]
        public static extern System.IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(System.IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(System.IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(System.IntPtr hWnd, out NativeRect rect);
    }
}
