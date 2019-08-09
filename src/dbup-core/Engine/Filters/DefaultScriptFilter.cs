using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Helpers;
using DbUp.Support;

namespace DbUp.Engine.Filters
{
    public class DefaultScriptFilter : IScriptFilter
    {
        public IEnumerable<SqlScript> Filter(IEnumerable<SqlScript> sorted, IEnumerable<SqlScript> executedScripts, ScriptNameComparer comparer)
        {
            var groupedExecutedScripts = executedScripts.GroupBy(s => s.Name).ToDictionary(x => x.Key, x => x.ToList());
            
            return sorted.Where(s=> GetDiff(groupedExecutedScripts,s));
        }

        public bool GetDiff(Dictionary<string,List<SqlScript>> groupedExecutedScripts,SqlScript s)
        {
            if (s.SqlScriptOptions.ScriptType == ScriptType.RunAlways)
            {
                return true;
            }
            if (s.SqlScriptOptions.ScriptType == ScriptType.RunOnChange)
            {
                if (groupedExecutedScripts.ContainsKey(s.Name))
                {
                    // Order the scripts with the same name by Date Applied
                    var orderedScripts = groupedExecutedScripts[s.Name].OrderBy(sc => sc.Applied);
                    // If there are no entries we need to execute the script, or if the contets
                    // Of the last entry are different we also need to execute the script.
                    return orderedScripts.Count() == 0 || orderedScripts.Last().Contents != s.Contents.MD5();
                }
                else
                {
                    return true;
                }
            }
             
            return !groupedExecutedScripts.ContainsKey(s.Name);

        }
    }
    
}