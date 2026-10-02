using System;
using System.Collections.Generic;
using BepInEx.Logging;
using CreaturesStarsWatcher.Config;
using CreaturesStarsWatcher.Detection;
using UnityEngine;

namespace CreaturesStarsWatcher.UI
{
    /// <summary>
    /// Screens one alert at a time behind a fading banner and keeps the closest
    /// detected creature on screen: the pending alerts are picked by distance and a
    /// clearly nearer creature takes the banner over. It only depends on
    /// <see cref="IBannerConfig"/>, everything it needs to draw is carried by the
    /// <see cref="CreatureAlert"/> itself.
    /// </summary>
    public sealed class BannerUI : IAlertSink
    {
        private const float FadeIn = 0.25f;
        private const float FadeOut = 0.5f;
        private const float Padding = 14f;

        /// <summary>Screen height share used by the automatic font size.</summary>
        private const float ScreenHeightRatio = 0.035f;

        /// <summary>Metres a pending creature must win by to take the banner over.</summary>
        private const float PreemptionMargin = 5f;

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
                // Only the closest creatures are worth a slot: a nearer newcomer takes
                // the place of the farthest pending alert instead of being dropped.
                int farthest = FindFarthest(player);
                if (
                    farthest < 0
                    || DistanceOf(alert, player) >= DistanceOf(_pending[farthest], player)
                )
                {
                    _logger.LogDebug(
                        $"[StarsWatcher] Banner queue is full, dropping {alert.DisplayName}"
                    );
                    return;
                }

                _pending.RemoveAt(farthest);
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

            ShowClosest();
        }

        /// <summary>
        /// Keeps the closest detected creature on screen: promotes the nearest pending
        /// alert when the banner is free and swaps for a pending one as soon as it is
        /// clearly nearer than the creature displayed.
        /// </summary>
        private void ShowClosest()
        {
            Player player = Player.m_localPlayer;
            ForgetUnavailable();

            int closest = FindClosest(player);
            if (_current == null)
            {
                Promote(closest);
                return;
            }

            if (closest < 0)
            {
                return;
            }

            // The margin keeps two creatures of a herd from fighting over the banner.
            if (
                DistanceOf(_pending[closest], player) + PreemptionMargin
                >= DistanceOf(_current, player)
            )
            {
                return;
            }

            CreatureAlert previous = _current;
            if (Promote(closest))
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

        private int FindClosest(Player player)
        {
            int closest = -1;
            for (int i = 0; i < _pending.Count; i++)
            {
                if (
                    closest < 0
                    || DistanceOf(_pending[i], player) < DistanceOf(_pending[closest], player)
                )
                {
                    closest = i;
                }
            }

            return closest;
        }

        private int FindFarthest(Player player)
        {
            int farthest = -1;
            for (int i = 0; i < _pending.Count; i++)
            {
                if (
                    farthest < 0
                    || DistanceOf(_pending[i], player) > DistanceOf(_pending[farthest], player)
                )
                {
                    farthest = i;
                }
            }

            return farthest;
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
            float x = Mathf.Round((Screen.width - width) * 0.5f);
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
            int stars = Mathf.Clamp(alert.Stars, 1, 5);
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

        private float GetDuration()
        {
            return Mathf.Max(1f, _config.DurationSeconds);
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
                case BannerPosition.Center:
                    return (Screen.height - height) * 0.5f;
                case BannerPosition.BottomCenter:
                    return Screen.height * 0.78f - height;
                default:
                    return Screen.height * 0.18f;
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
