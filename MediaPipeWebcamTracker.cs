
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;
using NetMQ;
using NetMQ.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace MediaPipeWebcam
{
    public class MediaPipeWebcamTracker : ExtTrackingModule
    {


        // Kernel32 SetDllDirectory
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);
        

        private Dictionary<int, (string, float)> UEtranslation =
       new Dictionary<int, (string, float)>
       {

           { (int)UnifiedExpressions.BrowInnerUpRight, ("BrowInnerUpRight", 0) },
           { (int)UnifiedExpressions.BrowOuterUpLeft, ("BrowOuterUpLeft", 0) },
           { (int)UnifiedExpressions.BrowOuterUpRight, ("BrowOuterUpRight", 0) },
           { (int)UnifiedExpressions.CheekPuffLeft, ("CheekPuff", 0) },
           { (int)UnifiedExpressions.CheekPuffRight, ("CheekPuff", 0) },
           { (int)UnifiedExpressions.CheekSquintLeft, ("CheekSquintLeft", 0) },
           { (int)UnifiedExpressions.CheekSquintRight, ("CheekSquintRight", 0) },
           { (int)UnifiedExpressions.EyeSquintLeft, ("EyeSquintLeft", 0) },
           { (int)UnifiedExpressions.EyeSquintRight, ("EyeSquintRight", 0) },
           { (int)UnifiedExpressions.EyeWideLeft, ("EyeWideLeft", 0) },
           { (int)UnifiedExpressions.EyeWideRight, ("EyeWideRight", 0) },
           { (int)UnifiedExpressions.JawForward, ("JawForward", 0) },
           { (int)UnifiedExpressions.JawLeft, ("JawLeft", 0) },
           { (int)UnifiedExpressions.JawOpen, ("JawOpen", 0) },
           { (int)UnifiedExpressions.JawRight, ("JawRight", 0) },
           { (int)UnifiedExpressions.MouthClosed, ("MouthClosed", 0) },
           { (int)UnifiedExpressions.MouthDimpleLeft, ("MouthDimpleLeft", 0) },
           { (int)UnifiedExpressions.MouthDimpleRight, ("MouthDimpleRight", 0) },
           { (int)UnifiedExpressions.MouthFrownLeft, ("MouthFrownLeft", 0) },
           { (int)UnifiedExpressions.MouthFrownRight, ("MouthFrownRight", 0) },
           { (int)UnifiedExpressions.MouthLowerDownLeft, ("MouthLowerDownLeft", 0) },
           { (int)UnifiedExpressions.MouthLowerDownRight, ("MouthLowerDownRight", 0) },
           { (int)UnifiedExpressions.MouthPressLeft, ("MouthPressLeft", 0) },
           { (int)UnifiedExpressions.MouthPressRight, ("MouthPressRight", 0) },
           { (int)UnifiedExpressions.MouthStretchLeft, ("MouthStretchLeft", 0) },
           { (int)UnifiedExpressions.MouthStretchRight, ("MouthStretchRight", 0) },
           { (int)UnifiedExpressions.MouthUpperUpLeft, ("MouthUpperUpLeft", 0) },
           { (int)UnifiedExpressions.MouthUpperUpRight, ("MouthUpperUpRight", 0) },
           { (int)UnifiedExpressions.NoseSneerLeft, ("NoseSneerLeft", 0) },
           { (int)UnifiedExpressions.NoseSneerRight, ("NoseSneerRight", 0) },
           { (int)UnifiedExpressions.LipFunnelLowerLeft, ("LipFunnel", 0) },
           { (int)UnifiedExpressions.LipFunnelLowerRight, ("LipFunnel", 0) },
           { (int)UnifiedExpressions.LipFunnelUpperLeft, ("LipFunnel", 0) },
           { (int)UnifiedExpressions.LipFunnelUpperRight, ("LipFunnel", 0) },
           { (int)UnifiedExpressions.LipPuckerLowerLeft, ("LipPucker", 0) },
           { (int)UnifiedExpressions.LipPuckerLowerRight, ("LipPucker", 0) },
           { (int)UnifiedExpressions.LipPuckerUpperLeft, ("LipPucker", 0) },
           { (int)UnifiedExpressions.LipPuckerUpperRight, ("LipPucker", 0) },

       };

        private Dictionary<int, (string, float)> USEtranslation =
        new Dictionary<int, (string, float)>
        {
           { (int)UnifiedSimpleExpressions.BrowDownLeft, ("BrowDownLeft", 0) },
           { (int)UnifiedSimpleExpressions.BrowDownRight, ("BrowDownRight", 0) },
           { (int)UnifiedSimpleExpressions.MouthSmileLeft, ("MouthSmileLeft", 0) },
           { (int)UnifiedSimpleExpressions.MouthSmileRight, ("MouthSmileRight", 0) },
         };


        public SubSocketManager _subSocketManager;
        // interface can send eye and expression data
        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            Console.WriteLine("came to this init");

            //var currentDllDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            SetDllDirectory("D:\\Desktop\\VRtrackingmodeule\\MediaPipeWebcam\\dependencies");
            Console.WriteLine("set the dll dir");
            DllLoader.InitialRuntime();


            var state = (eyeAvailable, expressionAvailable);
            _subSocketManager = new SubSocketManager("tcp://localhost:5555", 60);

            ModuleInformation.Name = "Mediapipe Webcam Module";

            ////// Example of an embedded image stream being referenced as a stream
            //var stream =
            //    GetType()
            //    .Assembly
            //    .GetManifestResourceStream("MediaPipeWebcam.logo.png");

            //// Setting the stream to be referenced by VRCFaceTracking.
            //ModuleInformation.StaticImages =
            //    stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;
            return state;
        }

        // Polls data from the tracking interface.
        // VRCFaceTracking will run this function in a separate thread;
        public override void Update()
        {
            // Get latest tracking data from interface and transform to VRCFaceTracking data.

            //if (Status == ModuleState.Active) // Module Status validation
            {
                // ... Execute update cycle.
                var trackingData = _subSocketManager.GetOldestData();


                if (trackingData != null)
                {
                    Console.WriteLine("got values!!");
                    foreach (var parameter in UEtranslation)
                    {
                        int p = parameter.Key;
                        string mpkey = parameter.Value.Item1;
                        float defaultVal = parameter.Value.Item2;
                        if (trackingData[mpkey] is not null)
                        {
                            UnifiedTracking.Data.Shapes[p].Weight = (float)trackingData[mpkey];
                        }
                        else
                        {
                            UnifiedTracking.Data.Shapes[p].Weight = (float) defaultVal;
                        }
                       

                    }

                    

                }
                else
                {
                }
                Thread.Sleep(1000 / 30);


                //UnifiedTracking.Data.Eye.Left.Openness = 1;
                //UnifiedTracking.Data.Shapes[(int)UnifiedExpressions.JawOpen] = ExampleTracker.Mouth.JawOpen;
            }

            // Add a delay or halt for the next update cycle for performance. eg: 
        }

        // Called when the module is unloaded or VRCFaceTracking itself tears down.
        public override void Teardown()
        {
            //... Deinitialize tracking interface; dispose any data created with the module.
        }

    }
}
