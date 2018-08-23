using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons.Utils {
    //Example usage:
    //try
    //{
    //    Failsafe.Execute(_jobRegistry.CancelAllJobs, JobManager.StopAndBlock, _rabbitBroker.Disconnect, _api.Dispose);
    //}
    //catch (AggregateException ae)
    //{
    //    Logger.Warn("Error(s) occurred while stopping.", ae);
    //}
    public static class Failsafe {
        public static void DisposeAll (params IDisposable[] disposables) => ForEach (disposables, d => d.Dispose ());

        public static void ForEach<T> (IEnumerable<T> items, Action<T> action) => Execute (items.Select<T, Action> (i => () => action (i)).ToArray ());

        public static void Execute (params Action[] actions) {
            var exceptions = new List<Exception> ();

            foreach (var action in actions) {
                try {
                    action ();
                } catch (Exception ex) {
                    exceptions.Add (ex);
                }
            }

            if (exceptions.Count > 0) {
                throw new AggregateException (exceptions);
            }
        }
    }
}