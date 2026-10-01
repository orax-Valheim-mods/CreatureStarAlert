using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreaturesStarsWatcher.Config;

namespace CreaturesStarsWatcher.Detection
{
    /// <summary>
    /// Matches a comma separated list of patterns supporting the '*' wildcard,
    /// for example "Moose, Troll*, Greydwarf*". Patterns are case insensitive and are
    /// only recompiled when the configuration value actually changes.
    /// </summary>
    public sealed class WildcardCreatureFilter : ICreatureFilter
    {
        private readonly ICreatureFilterConfig _config;

        private string _compiledPatterns;
        private List<Regex> _patterns = new List<Regex>();

        public WildcardCreatureFilter(ICreatureFilterConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public bool IsWatched(string creatureName)
        {
            if (string.IsNullOrEmpty(creatureName))
            {
                return false;
            }

            List<Regex> patterns = GetPatterns();
            for (int i = 0; i < patterns.Count; i++)
            {
                if (patterns[i].IsMatch(creatureName))
                {
                    return true;
                }
            }

            return false;
        }

        private List<Regex> GetPatterns()
        {
            string raw = _config.CreaturePatterns;
            if (string.Equals(raw, _compiledPatterns, StringComparison.Ordinal))
            {
                return _patterns;
            }

            _compiledPatterns = raw;
            _patterns = Compile(raw);
            return _patterns;
        }

        private static List<Regex> Compile(string raw)
        {
            var patterns = new List<Regex>();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return patterns;
            }

            foreach (string chunk in raw.Split(','))
            {
                string pattern = chunk.Trim();
                if (pattern.Length == 0)
                {
                    continue;
                }

                // Regex.Escape keeps every other metacharacter literal, then '*' becomes a wildcard.
                string expression = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                patterns.Add(
                    new Regex(
                        expression,
                        RegexOptions.IgnoreCase
                            | RegexOptions.CultureInvariant
                            | RegexOptions.Compiled
                    )
                );
            }

            return patterns;
        }
    }
}
