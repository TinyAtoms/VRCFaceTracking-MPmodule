using System.Runtime.InteropServices;
using System;

namespace MediaPipeWebcam
{
    internal class DllLoader
    {

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FreeLibrary(IntPtr hModule);

            private static IntPtr module = IntPtr.Zero;

            public static void InitialRuntime()
            {
                if (module != IntPtr.Zero)
                    ReleaseRuntime();

            module = LoadLibrary("NetMQ.dll");
            //module = LoadLibrary("D:\\Desktop\\VRtrackingmodeule\\MediaPipeWebcam\\dependencies\\NetMQ.dll");
            Console.WriteLine("loaded library");
        }

            public static void ReleaseRuntime()
            {
                if (!FreeLibrary(module)) // ideally should never happen.
                    throw new Exception($"Failed to release Lip module DLL.");
                module = IntPtr.Zero;
            }

        //[DllImport("D:\\Desktop\\VRtrackingmodeule\\MediaPipeWebcam\\dependencies\\NetMQ.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("NetMQ.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int Initial(int anipalType, IntPtr config);
    }
}





