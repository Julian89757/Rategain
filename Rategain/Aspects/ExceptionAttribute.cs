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
    public class ExceptionAttribute : OnExceptionAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {

            LogHelper.Write(string.Format("Exception in [{0}],Message:[{1}]", args.Method.Name, args.Exception.Message), LogHelper.LogMessageType.Error,args.Exception);

            args.FlowBehavior = FlowBehavior.Continue;
            
            base.OnException(args);
        }
    }
}
