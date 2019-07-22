namespace Commons.Utils.Cache
{
    public class GlobalScope : IScopeProvider
    {
        public string ApplyCurrentScope(string key) => key;
    }
}