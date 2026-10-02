using System;
using System.Collections.Generic;
using BepInEx.Logging;
using CreaturesStarsWatcher.Config;
using CreaturesStarsWatcher.Detection;
using UnityEngine;

namespace CreaturesStarsWatcher.UI
{
    /// <summary>
    /// Screens one alert at a time behind a fading banner and keeps the winning
    /// creature on screen: the pending alerts are picked on the selection criterion
    /// (most stars or nearest) and a pending creature takes the banner over as soon
    /// as it beats the one displayed. It only depends on
    /// <see cref="IBannerConfig"/>, everything it needs to draw is carried by the
    /// <see cref="CreatureAlert"/> itself.
    /// </summary>
    public sealed class BannerUI : IAlertSink
    {
        private const float FadeIn = 0.25f;
        private const float FadeOut = 0.5f;
        private const float Padding = 14f;

        /// <summary>Share of the screen width kept between the banner and a side edge.</summary>
        private const float SideInsetRatio = 0.05f;

        /// <summary>Screen height share used by the automatic font size.</summary>
        private const float ScreenHeightRatio = 0.035f;

        /// <summary>Largest lead in metres a pending creature must win by to take the banner over.</summary>
        private const float MaxLead = 5f;

        /// <summary>Smallest lead at close range, so idle animations cannot flicker the banner.</summary>
        private const float MinLead = 0.5f;

        /// <summary>Share of the displayed distance the candidate must win by, up to <see cref="MaxLead"/>.</summary>
        private const float LeadRatio = 0.25f;

        private const string NameColor = "#FFFFFF";
        private const string StarColor = "#FFC53B";
        private const string DistanceColor = "#A8A8A8";

        private readonly IBannerConfig _config;
        private readonly ManualLogSource _logger;
        private readonly List<CreatureAlert> _pending = new List<CreatureAlert>();

        private CreatureAlert _current;
        private float _elapsed;
        private GUIStyle _labelStyle;
        private int _labelFontSize = -1;

        public BannerUI(IBannerConfig config, ManualLogSource logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Enqueue(CreatureAlert alert)
        {
            if (alert == null)
            {
                return;
            }

            Player player = Player.m_localPlayer;
            ForgetUnavailable();
            if (_pending.Count >= _config.MaxQueuedAlerts)
            {
                // Only the winning creatures are worth a slot: a better newcomer takes
                // the place of the worst pending alert instead of being dropped.
                int worst = FindWorst(player);
                if (worst < 0 || !Ranks(alert, _pending[worst], player))
                {
                    _logger.LogDebug(
                        $"[StarsWatcher] Banner queue is full, dropping {alert.DisplayName}"
                    );
                    return;
                }

                _pending.RemoveAt(worst);
            }

            _pending.Add(alert);
        }

        /// <summary>Drops the visible banner and every pending alert.</summary>
        public void Clear()
        {
            _pending.Clear();
            _current = null;
            _elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (Player.m_localPlayer == null)
            {
                Clear();
                return;
            }

            // The creature was unloaded or destroyed: its alert must leave the screen.
            if (_current != null && !_current.IsAvailable)
            {
                _current = null;
                _elapsed = 0f;
            }

            float duration = GetDuration();
            if (_current != null)
            {
                _elapsed += deltaTime;
                if (_elapsed >= duration)
                {
                    _current = null;
                    _elapsed = 0f;
                }
            }

            ShowBest();
        }

        /// <summary>
        /// Keeps the winning creature on screen: promotes the best pending alert when
        /// the banner is free and swaps for a pending one as soon as it beats the
        /// creature displayed (more stars with the option, clearly nearer otherwise).
        /// </summary>
        private void ShowBest()
        {
            Player player = Player.m_localPlayer;
            ForgetUnavailable();

            int best = FindBest(player);
            if (_current == null)
            {
                Promote(best);
                return;
            }

            if (best < 0 || !Beats(_pending[best], _current, player))
            {
                return;
            }

            CreatureAlert previous = _current;
            if (Promote(best))
            {
                _pending.Add(previous);
            }
        }

        /// <summary>Displays the pending alert at the given index. False when there is none.</summary>
        private bool Promote(int index)
        {
            if (index < 0 || index >= _pending.Count)
            {
                return false;
            }

            _current = _pending[index];
            _pending.RemoveAt(index);

            // The elapsed time carries over: swapping must not extend the banner forever.
            return true;
        }

        private int FindBest(Player player)
        {
            int best = -1;
            for (int i = 0; i < _pending.Count; i++)
            {
                if (best < 0 || Ranks(_pending[i], _pending[best], player))
                {
                    best = i;
                }
            }

            return best;
        }

        private int FindWorst(Player player)
        {
            int worst = -1;
            for (int i = 0; i < _pending.Count; i++)
            {
                if (worst < 0 || Ranks(_pending[worst], _pending[i], player))
                {
                    worst = i;
                }
            }

            return worst;
        }

        /// <summary>Whether the left alert ranks before the right one on the criterion.</summary>
        private bool Ranks(CreatureAlert left, CreatureAlert right, Player player)
        {
            return AlertPriority.Compare(
                    left.Stars,
                    DistanceOf(left, player),
                    right.Stars,
                    DistanceOf(right, player),
                    _config.PreferMostStars
                ) < 0;
        }

        /// <summary>
        /// Whether the candidate takes the banner from the creature displayed: more stars
        /// always win with the option, otherwise (and on equal stars) the candidate must
        /// lead by <see cref="RequiredLead"/> metres, which keeps a herd from making the
        /// banner flicker while still letting you switch it by stepping up to a creature.
        /// </summary>
        private bool Beats(CreatureAlert candidate, CreatureAlert current, Player player)
        {
            if (_config.PreferMostStars && candidate.Stars != current.Stars)
            {
                return candidate.Stars > current.Stars;
            }

            float candidateDistance = DistanceOf(candidate, player);
            float currentDistance = DistanceOf(current, player);
            return candidateDistance + RequiredLead(currentDistance) < currentDistance;
        }

        /// <summary>
        /// Metres the candidate must win by: a quarter of the displayed distance, kept
        /// between <see cref="MinLead"/> and <see cref="MaxLead"/>. Two creatures less
        /// than 5 m apart can therefore swap the banner, while the 5 m hysteresis still
        /// holds at range.
        /// </summary>
        private static float RequiredLead(float distance)
        {
            return Mathf.Clamp(distance * LeadRatio, MinLead, MaxLead);
        }

        private void ForgetUnavailable()
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                if (!_pending[i].IsAvailable)
                {
                    _pending.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Distance to the local player in metres: live while the creature exists, the
        /// recorded one otherwise, and as far as possible once the creature is gone.
        /// </summary>
        private static float DistanceOf(CreatureAlert alert, Player player)
        {
            if (!alert.IsAvailable)
            {
                return float.MaxValue;
            }

            if (player == null)
            {
                return alert.Distance;
            }

            return Vector3.Distance(player.transform.position, alert.Character.transform.position);
        }

        public void OnGui()
        {
            Player player = Player.m_localPlayer;
            if (_current == null || player == null)
            {
                return;
            }

            float duration = GetDuration();
            float alpha = ComputeAlpha(duration);
            if (alpha <= 0f)
            {
                return;
            }

            string text = BuildText(player, _current);
            int fontSize = GetFontSize();
            GUIStyle style = GetLabelStyle(fontSize);
            Vector2 size = style.CalcSize(new GUIContent(text));

            float width = size.x + Padding * 2f;
            float height = size.y + Padding * 2f;
            float x = GetX(_config.Position, width);
            float y = Mathf.Round(GetTop(_config.Position, height));

            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.65f * alpha);
            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(x, y, width, height), text, style);
            GUI.color = previousColor;
        }

        private string BuildText(Player player, CreatureAlert alert)
        {
            string name = string.IsNullOrEmpty(alert.DisplayName)
                ? alert.PrefabName
                : alert.DisplayName;
            // Bounded like the StarsMinimum setting, which accepts up to 9 stars.
            int stars = Mathf.Clamp(alert.Stars, 1, 9);
            string text =
                $"<color={NameColor}>{name}</color>  <color={StarColor}>{new string('★', stars)}</color>";

            // The distance is only meaningful while the creature still exists.
            if (_config.ShowDistance && alert.IsAvailable)
            {
                float distance = Vector3.Distance(
                    player.transform.position,
                    alert.Character.transform.position
                );
                text += $"  <color={DistanceColor}>({distance:0} m)</color>";
            }

            return text;
        }

        /// <summary>Configured font size, or a screen-proportional one when set to automatic.</summary>
        private int GetFontSize()
        {
            if (_config.FontSize > 0)
            {
                return _config.FontSize;
            }

            return Mathf.Max(14, Mathf.RoundToInt(Screen.height * ScreenHeightRatio));
        }

        /// <summary>
        /// Seconds the banner lasts. A duration of 0 keeps it on screen until the
        /// creature is unloaded or dies, which is obtained by asking for an endless
        /// duration: neither the expiry nor the fade out can ever trigger.
        /// </summary>
        private float GetDuration()
        {
            if (_config.DurationSeconds <= 0f)
            {
                return float.MaxValue;
            }

            return _config.DurationSeconds;
        }

        private float ComputeAlpha(float duration)
        {
            float fadeIn = Mathf.Min(FadeIn, duration * 0.25f);
            float fadeOut = Mathf.Min(FadeOut, duration * 0.25f);
            float alpha =
                Mathf.Min(1f, _elapsed / fadeIn) * Mathf.Min(1f, (duration - _elapsed) / fadeOut);
            return Mathf.Clamp01(alpha);
        }

        private static float GetTop(BannerPosition position, float height)
        {
            switch (position)
            {
                case BannerPosition.Left:
                case BannerPosition.Center:
                case BannerPosition.Right:
                    return (Screen.height - height) * 0.5f;
                case BannerPosition.BottomLeft:
                case BannerPosition.BottomCenter:
                case BannerPosition.BottomRight:
                    return Screen.height * 0.78f - height;
                default: // top row
                    return Screen.height * 0.18f;
            }
        }

        private static float GetX(BannerPosition position, float width)
        {
            switch (position)
            {
                case BannerPosition.TopLeft:
                case BannerPosition.Left:
                case BannerPosition.BottomLeft:
                    return Mathf.Round(Screen.width * SideInsetRatio);
                case BannerPosition.TopRight:
                case BannerPosition.Right:
                case BannerPosition.BottomRight:
                    return Mathf.Round(Screen.width * (1f - SideInsetRatio) - width);
                default:
                    return Mathf.Round((Screen.width - width) * 0.5f);
            }
        }

        private GUIStyle GetLabelStyle(int fontSize)
        {
            if (_labelStyle != null && _labelFontSize == fontSize)
            {
                return _labelStyle;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = fontSize,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
            };
            _labelFontSize = fontSize;
            return _labelStyle;
        }
    }
}
