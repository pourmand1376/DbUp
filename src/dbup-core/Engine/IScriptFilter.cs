using System.Collections.Generic;
using DbUp.Engine.Filters;
using DbUp.Support;

namespace DbUp.Engine
{
    public interface IScriptFilter
    {
        IEnumerable<SqlScript> Filter(IEnumerable<SqlScript> sorted, IEnumerable<SqlScript> executedScripts, ScriptNameComparer comparer);
    }
}