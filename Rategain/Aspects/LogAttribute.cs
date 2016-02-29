using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostSharp.Aspects;
using RateGain.Util;

namespace RateGainData.Console.Aspects
{
    [Serializable]
    public class LogAttribute : OnMethodBoundaryAspect
    {
        public string UserCaseName { get; set; }

        public override void OnEntry(MethodExecutionArgs args)
        {
            var msg = string.Format("Entering [{0}]", args.Method.Name);
            if (!string.IsNullOrEmpty(UserCaseName))
                msg = UserCaseName + " start";

            LogHelper.Write(msg, LogHelper.LogMessageType.Info);
            base.OnEntry(args);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var msg = string.Format("Leaving [{0}]", args.Method.Name);
            if (!string.IsNullOrEmpty(UserCaseName))
                msg = UserCaseName + " async download completed，nice.";

            LogHelper.Write(msg, LogHelper.LogMessageType.Info);
            base.OnExit(args);
        }

        //   Called only when a method has stopped executing due to an unhandled exception
        public override void OnException(MethodExecutionArgs args)
        {
            var msg = args.Exception.Message;
            LogHelper.Write(msg, LogHelper.LogMessageType.Error);
            base.OnExit(args);
        }
    }

    [Serializable]
    public class ImportLogAttribute : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs args)
        {
            var filename = args.Arguments[0];
            if (filename != null)
            {
                var msg = filename + " download completed, Now start to import redis server... ";
                LogHelper.Write(msg, LogHelper.LogMessageType.Info);
            }
            base.OnEntry(args);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            var filename = (args.Arguments[0]);
            var handleResp = (HandleResp)(args.ReturnValue);
            if (filename != null && handleResp != null)
            {
                var msg = filename + " " + handleResp.EffectiveRecord + " effective record. ";
                if (handleResp.Status == 1)
                {
                    LogHelper.Write(msg, LogHelper.LogMessageType.Info);
                }
                else
                {
                    msg += handleResp.Desc;
                    LogHelper.Write(msg, LogHelper.LogMessageType.Info);
                }   
            }
            base.OnExit(args);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            var msg = args.Exception.Message;
            LogHelper.Write(msg, LogHelper.LogMessageType.Error);
            base.OnExit(args);
        }
    }
}
