using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Extensibility;
using RateGain.Util;

namespace RateGainData.Console.Aspects
{
    [Serializable]
    [MulticastAttributeUsage(MulticastTargets.Method)]
    public class TimingAttribute : PostSharp.Aspects.OnMethodBoundaryAspect
    {
        [NonSerialized]
        Stopwatch _StopWatch;

        public override void OnEntry(PostSharp.Aspects.MethodExecutionArgs args)
        {
            _StopWatch = Stopwatch.StartNew();

            base.OnEntry(args);
        }

        public override void OnExit(PostSharp.Aspects.MethodExecutionArgs args)
        {
            var msg = string.Format("[{0}] take {1}ms to execute",
                new StackTrace().GetFrame(1).GetMethod().Name,
                _StopWatch.ElapsedMilliseconds);

            LogHelper.Write(msg, LogHelper.LogMessageType.Info);

            base.OnExit(args);
        }
    }
}
