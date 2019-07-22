using System.Threading;
using Commons.Utils.Cache;

namespace Commons.Utils.Tests
{
    public class MessageScope : IScopeProvider, IScopeFactory
    {
        private readonly AsyncLocal<string> _currentScope = new AsyncLocal<string>();

        public MessageScope()
        {
            _currentScope.Value = "N/D";
        }

        public string ApplyCurrentScope(string key) => $"{_currentScope.Value};{key}";

        public void StartNew(string scopeName) => _currentScope.Value = scopeName;
    }
}