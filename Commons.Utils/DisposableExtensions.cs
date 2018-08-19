using System;

namespace Commons.Utils
{
    //Example usage:
    //_connection.FailsafeDispose().Handle<ItemInfoChangedValidationException>(ex => Console.WriteLine(ex));
    //[...]
    public static class DisposableExtensions
    {
        public static Exception FailsafeDispose(this IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
    }
}