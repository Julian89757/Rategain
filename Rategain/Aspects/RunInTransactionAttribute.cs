using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using RateGain.Util;

namespace RateGainData.Console.Aspects
{
    [Serializable]
    [AspectTypeDependency(AspectDependencyAction.Order,
                          AspectDependencyPosition.After, typeof(LogAttribute))]
    public class RunInTransactionAttribute : OnMethodBoundaryAspect
    {
        [NonSerialized]
        TransactionScope TransactionScope;

        public override void OnEntry(MethodExecutionArgs args)
        {
            this.TransactionScope = new TransactionScope(TransactionScopeOption.RequiresNew);
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            this.TransactionScope.Complete();
        }

        public override void OnException(MethodExecutionArgs args)
        {
            args.FlowBehavior = FlowBehavior.Continue;
            Transaction.Current.Rollback();

            LogHelper.Write(string.Format("[{0}] Transaction Was Unsuccessful!", args.Method.Name), LogHelper.LogMessageType.Info);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            TransactionScope.Dispose();
        }
    }
}
