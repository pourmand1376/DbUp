using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Support;

namespace DbUp.Engine.Filters
{
    public class DefaultScriptFilter : IScriptFilter
    {
        public IEnumerable<SqlScript> Filter(IEnumerable<SqlScript> sorted, IEnumerable<SqlScript> executedScripts, ScriptNameComparer comparer)
        {
            HashSet<String> executedScriptNames = new HashSet<string>(executedScripts.Select(s => s.Name));
            return sorted.Where(s => s.SqlScriptOptions.ScriptType == ScriptType.RunAlways || !executedScriptNames.Contains(s.Name, comparer));
        }
    }
}