
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Params.Data;


namespace MediaPipeWebcamModule
{
    public class MediaPipeWebcamTracker : ExtTrackingModule
    {





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

        private TcpMessageReceiver _reciever;


        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            var state = (eyeAvailable, expressionAvailable);
            ModuleInformation.Name = "Mediapipe Webcam Module";
            var stream =GetType().Assembly.GetManifestResourceStream("assets\\logo.png");
            ModuleInformation.StaticImages = stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;
            _reciever = new TcpMessageReceiver(5555);

            //UnifiedTracking.Mutator.Enabled = true;
            //UnifiedTracking.Mutator.SmoothingMode = true;
            //UnifiedTracking.Mutator.SetSmoothness(0.1f);
            return state;
        }

        // Polls data from the tracking interface.
        // VRCFaceTracking will run this function in a separate thread;
        public override void Update()
        {
                var trackingData = _reciever.GetOldestMessage();
                if (trackingData != null)
                {
                    foreach (var parameter in UEtranslation)
                    {
                        int p = parameter.Key;
                        string mpkey = parameter.Value.Item1;
                        float defaultVal = parameter.Value.Item2;
                        if (trackingData[mpkey] is not 0)
                        {
                            UnifiedTracking.Data.Shapes[p].Weight = (float)trackingData[mpkey];
                        }
                        else
                        {
                            UnifiedTracking.Data.Shapes[p].Weight = (float) defaultVal;
                        }
                    
                    UnifiedTracking.Data.Eye.Left.Openness = (float)trackingData["EyeOpennessLeft"];
                    UnifiedTracking.Data.Eye.Right.Openness = (float)trackingData["EyeOpennessRight"];


                    bool leftEyeUp = (trackingData["EyeLookUpLeft"] > trackingData["EyeLookDownLeft"]);
                    float lefty = (leftEyeUp) ? trackingData["EyeLookUpLeft"] : -1 * trackingData["EyeLookDownLeft"];
                    bool rightEyeUp = (trackingData["EyeLookUpRight"] > trackingData["EyeLookDownRight"]);
                    float righty = (rightEyeUp) ? trackingData["EyeLookUpRight"] : -1 * trackingData["EyeLookDownRight"];
                    bool leftEyeOut = (trackingData["EyeLookInLeft"] < trackingData["EyeLookOutLeft"]);
                    float leftx = (leftEyeOut) ? trackingData["EyeLookOutLeft"] * -1 : trackingData["EyeLookInLeft"];
                    bool rightEyeIn = (trackingData["EyeLookInRight"] > trackingData["EyeLookOutRight"]);
                    float rightx = (rightEyeIn) ? trackingData["EyeLookInRight"] * -1 : trackingData["EyeLookOutRight"];
                    var avgx = (leftx + rightx) / 2;
                    var avgy = (lefty + righty) / 2;
                    UnifiedTracking.Data.Eye.Left.Gaze = new VRCFaceTracking.Core.Types.Vector2(avgx, avgy);
                    UnifiedTracking.Data.Eye.Right.Gaze = new VRCFaceTracking.Core.Types.Vector2(avgx, avgy);


                    //UnifiedTracking.Mutator.MutateData(UnifiedTracking.Data);
                    //UnifiedTracking.UpdateData();


                    // if you don't want to simplify it
                    //bool leftEyeOut = (trackingData["EyeLookInLeft"] < trackingData["EyeLookOutLeft"]);
                    //float leftx = (leftEyeOut) ? trackingData["EyeLookInLeft"] * -1 : trackingData["EyeLookOutLeft"];
                    //bool rightEyeIn = (trackingData["EyeLookInRight"] > trackingData["EyeLookOutRight"]);
                    //float rightx = (rightEyeIn) ? trackingData["EyeLookInRight"] * -1 : trackingData["EyeLookOutRight"]
                    //;
                    //UnifiedTracking.Data.Eye.Left.Gaze = new VRCFaceTracking.Core.Types.Vector2(leftx, lefty);
                    //UnifiedTracking.Data.Eye.Right.Gaze = new VRCFaceTracking.Core.Types.Vector2(rightx, righty);



                }

                Thread.Sleep(1000 / 100);
            }
            else
            {
                Thread.Sleep(10);

            }
            
                

        }

        // Called when the module is unloaded or VRCFaceTracking itself tears down.
        public override void Teardown()
        {
            _reciever.Stop();
        }

    }
}
