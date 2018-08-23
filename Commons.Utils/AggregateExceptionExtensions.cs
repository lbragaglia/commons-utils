using System;

namespace Commons.Utils
{
    //Example usage:
    //_connection.FailsafeDispose().Handle<ItemInfoChangedValidationException>(ex => Console.WriteLine(ex));
    //[...]

    //TODO add chaining, rethrow
    public static class AggregateExceptionExtensions
    {
        public static void Handle<T>(this AggregateException aex, Func<T, bool> predicate) where T : Exception => aex?.Handle(WithTypeSafe(predicate));
        private static Func<Exception, bool> WithTypeSafe<T>(Func<T, bool> predicate) where T : Exception => ex => ex is T && predicate((T) ex);

        public static void Handle<T>(this Exception ex, Func<T, bool> predicate) where T : Exception
        {
            if (WithTypeSafe(predicate)(ex)) return;
            throw ex;
        }

        public static void Handle<T>(this AggregateException aex, Action<T> action) where T : Exception => aex?.Handle(WithTypeSafe(action));

        private static Func<Exception, bool> WithTypeSafe<T>(Action<T> action) where T : Exception => WithTypeSafe<T>(ex =>
        {
            action(ex);
            return true;
        });

        public static void Handle<T>(this Exception ex, Action<T> action) where T : Exception => WithTypeSafe(action)(ex);
    }


}