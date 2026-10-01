using System;
using System.Collections.Generic;
using BepInEx.Logging;
using CreaturesStarsWatcher.Config;
using CreaturesStarsWatcher.Detection;
using UnityEngine;

namespace CreaturesStarsWatcher.UI
{
    /// <summary>
    /// Screens one alert at a time behind a fading banner and queues the extra ones.
    /// It only depends on <see cref="IBannerConfig"/>, everything it needs to draw is
    /// carried by the <see cref="CreatureAlert"/> itself.
    /// </summary>
    public sealed class BannerUI : IAlertSink
    {
        private const float FadeIn = 0.25f;
        private const float FadeOut = 0.5f;
        private const float Padding = 14f;
        private const float ScreenHeightRatio = 0.035f;

        private const string NameColor = "#FFFFFF";
        private const string StarColor = "#FFC53B";
        private const string DistanceColor = "#A8A8A8";

        private readonly IBannerConfig _config;
        private readonly ManualLogSource _logger;
        private readonly Queue<CreatureAlert> _queue = new Queue<CreatureAlert>();

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

            if (_queue.Count >= _config.MaxQueuedAlerts)
            {
                _logger.LogDebug(
                    $"[StarsWatcher] Banner queue is full, dropping {alert.DisplayName}"
                );
                return;
            }

            _queue.Enqueue(alert);
        }

        /// <summary>Drops the visible banner and every queued alert.</summary>
        public void Clear()
        {
            _queue.Clear();
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

            if (_current == null && _queue.Count > 0)
            {
                _current = _queue.Dequeue();
                _elapsed = 0f;
            }
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
            int fontSize = Mathf.Max(14, Mathf.RoundToInt(Screen.height * ScreenHeightRatio));
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
            if (_config.ShowDistance && alert.Character != null)
            {
                float distance = Vector3.Distance(
                    player.transform.position,
                    alert.Character.transform.position
                );
                text += $"  <color={DistanceColor}>({distance:0} m)</color>";
            }

            return text;
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
