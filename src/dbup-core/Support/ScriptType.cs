using System;

namespace DbUp.Support
{
    public enum ScriptType
    {
        /// <summary>
        /// Default script type.  The script will run only once.
        /// </summary>
        RunOnce = 0,

        /// <summary>
        /// The script will always be run.  Useful for setting permissions.  Please note, the script should be written so it can always be ran.
        /// </summary>
        RunAlways = 1,

        /// <summary>
        /// The script will run if it has not been run before, and there after whenever the contents of the script changes.
        /// Useful for Stored Procedures.
        /// </summary>
        RunOnChange = 2
    }
}
