using System;
using System.Collections.Generic;

namespace Bluehands.Repository.Diagnostics
{
    [Serializable]
    public class LogViewerState
    {
        public List<string> OpenedFiles { get; set; }

        public LogViewerState()
        {
            OpenedFiles = new List<string>();    
        }
    }
}