namespace Commons.Utils.Cache
{
    public interface IScopeProvider
    {
        string ApplyCurrentScope(string key);
    }
}