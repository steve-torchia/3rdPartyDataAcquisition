using System;
using System.Collections.Generic;
using System.Linq;
using DP.Base.Contracts;
using DP.Base.Contracts.Logging;

namespace DP.Base.Context
{
    public class Diagnostic : IDiagnostic
    {
        private List<Tuple<string, DiagnosticLevel, string>> diagnosticTextList = new List<Tuple<string, DiagnosticLevel, string>>();

        public Diagnostic()
        {
        }

        public void Log(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, Exception exception)
        {
            lock (this.diagnosticTextList)
            {
                this.diagnosticTextList.Add(new Tuple<string, DiagnosticLevel, string>(name, diagLevel, string.Format("{0} {1}", message, exception)));
            }
        }

        public void Log(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, params object[] args)
        {
            lock (this.diagnosticTextList)
            {
                this.diagnosticTextList.Add(new Tuple<string, DiagnosticLevel, string>(name, diagLevel, string.Format(message, args)));
            }
        }

        public IEnumerable<Tuple<string, DiagnosticLevel, string>> GetDiagnostics(DiagnosticsSortOrder sortOrder)
        {
            List<Tuple<string, DiagnosticLevel, string>> localList = null;
            //return a copy
            lock (this.diagnosticTextList)
            {
                localList = this.diagnosticTextList.ToList();
            }

            if (sortOrder == DiagnosticsSortOrder.Chronological)
            {
                return localList;
            }

            var retList = new List<Tuple<string, DiagnosticLevel, string>>();

            var values = (DiagnosticLevel[])Enum.GetValues(typeof(DiagnosticLevel)); //GetValues will return in sorted order
            var groupedByLevel = localList.GroupBy(a => a.Item2);
            for (int index = values.Length - 1; index >= 0; index--)
            {
                foreach (var group in groupedByLevel)
                {
                    if (group.Key == (DiagnosticLevel)values[index])
                    {
                        retList.AddRange(group);
                        break;
                    }
                }
            }

            return retList;
        }

        public bool Contains(string name)
        {
            lock (this.diagnosticTextList)
            {
                return this.diagnosticTextList.Any(a => a.Item1 == name);
            }
        }

        public void LogWithReplace(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, params object[] args)
        {
            lock (this.diagnosticTextList)
            {
                for (int i = 0; i < this.diagnosticTextList.Count; i++)
                {
                    if (this.diagnosticTextList[i].Item1 == name)
                    {
                        this.diagnosticTextList[i] = new Tuple<string, DiagnosticLevel, string>(name, diagLevel, string.Format(message, args));
                        return;
                    }
                }

                this.diagnosticTextList.Add(new Tuple<string, DiagnosticLevel, string>(name, diagLevel, string.Format(message, args)));
            }
        }
    }

    public class DiagnosticWrapper : IDiagnosticWrapper
    {
        private IContext innerContext;
        private IDiagnostic innerDiagnostic;

        public void Initialize(IDiagnostic diagnostic, IContext context)
        {
            this.innerDiagnostic = diagnostic;
            this.innerContext = context;
        }

        public void Log(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, Exception exception)
        {
            var diagMessage = this.GetDiagnosticMessage(message);
            this.innerDiagnostic.Log(name, levelForLog, diagLevel, diagMessage, exception);

            if (this.innerContext.Log.IsEnabled(levelForLog))
            {
                this.innerContext.Log.Log(levelForLog, message, exception);
            }
        }

        public void Log(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, params object[] args)
        {
            var diagMessage = this.GetDiagnosticMessage(message);
            this.innerDiagnostic.Log(name, levelForLog, diagLevel, diagMessage, args);

            if (this.innerContext.Log.IsEnabled(levelForLog))
            {
                this.innerContext.Log.Log(levelForLog, message, args);
            }
        }

        private string GetDiagnosticMessage(string message)
        {
            var diagMessage = string.Concat(this.innerContext.Name, ": ", message);
            return diagMessage;
        }

        public IEnumerable<Tuple<string, DiagnosticLevel, string>> GetDiagnostics(DiagnosticsSortOrder sortOrder)
        {
            return this.innerDiagnostic.GetDiagnostics(sortOrder);
        }

        public bool Contains(string name)
        {
            return this.innerDiagnostic.Contains(name);
        }

        public void LogWithReplace(string name, LogLevel levelForLog, DiagnosticLevel diagLevel, string message, params object[] args)
        {
            this.innerDiagnostic.LogWithReplace(name, levelForLog, diagLevel, message, args);
        }
    }

    public static class DiagnosticHelper
    {
        public static void OutputDiagnostics(System.IO.TextWriter textWriter, DiagnosticLevel[] includedLevels)
        {
            var currentAction = ObjectContextActionScope.CurrentObjectContext;
            if (currentAction != null &&
                currentAction.Diagnostic != null)
            {
                string lastCategory = string.Empty;
                DiagnosticLevel? lastDiagLevel = null;
                foreach (var diag in currentAction.Diagnostic.GetDiagnostics(DiagnosticsSortOrder.ByLevel))
                {
                    if (includedLevels != null && includedLevels.Any(a => a == diag.Item2) == false)
                    {
                        continue;
                    }

                    string levelText = string.Empty;
                    if (lastDiagLevel != diag.Item2)
                    {
                        lastDiagLevel = diag.Item2;
                        levelText = string.Concat(Environment.NewLine, "Diagnostic Level: ", diag.Item2);
                    }

                    int delimiter = diag.Item3.IndexOf(':');
                    string category = string.Empty;
                    if (delimiter != -1)
                    {
                        category = diag.Item3.Substring(0, delimiter);
                    }
                    else
                    {
                        category = diag.Item1;
                    }

                    var diagText = string.Empty;
                    if (lastCategory == category.ToUpperInvariant())
                    {
                        diagText = "," + diag.Item3.Substring(delimiter + 1);
                    }
                    else
                    {
                        lastCategory = category.ToUpperInvariant();
                        diagText = string.Concat(levelText, Environment.NewLine, category, Environment.NewLine, ",", diag.Item3.Substring(delimiter + 1));
                    }

                    textWriter.WriteLine(diagText);
                }
            }
        }
    }
}
