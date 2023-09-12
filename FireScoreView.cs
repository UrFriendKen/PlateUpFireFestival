using Kitchen;
using KitchenMods;
using MessagePack;
using System;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FireEvent
{
    public class FireScoreView : UpdatableObjectView<FireScoreView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();

                Views = GetEntityQuery(new QueryHelper()
                    .All(typeof(CDayDisplay), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                SDay sDay = GetOrDefault<SDay>();
                int currentDay = sDay.Day + (Has<SIsNightTime>() ? 1 : 0);

                bool hasSFireScore = Require(out SFireScore fireScore);
                float score = 0f;
                int target = fireScore.Target;
                if (hasSFireScore)
                {
                    switch (fireScore.GameMode)
                    {
                        case FireGameMode.CountDown:
                            score = fireScore.Score;
                            target = fireScore.Target; 
                            break;
                        case FireGameMode.CountUp:
                            score = fireScore.Score;
                            switch (fireScore.CountUpScoringMode)
                            {
                                case UpdateFireScoreCountUp.ScoringMode.GrossEarnings:
                                case UpdateFireScoreCountUp.ScoringMode.Savings:
                                    if (Has<SIsDayTime>())
                                    {
                                        score += Require(out SMoney money) ? money - fireScore.StartOfDayCoins : 0f;
                                    }
                                    break;
                            }
                            target = fireScore.Target;
                            break;
                    }
                }

                for (var i = 0; i < views.Length; i++)
                {
                    var view = views[i];
                    SendUpdate(view, new ViewData()
                    {
                        ShowScore = hasSFireScore,
                        GameMode = fireScore.GameMode,
                        Score = score,
                        Multiplier = fireScore.FrameMultiplier,
                        CurrentDay = currentDay,
                        Target = target
                    });
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool ShowScore;
            [Key(1)] public FireGameMode GameMode;
            [Key(2)] public float Score;
            [Key(3)] public float Multiplier;
            [Key(4)] public int CurrentDay;
            [Key(5)] public int Target;    // Update display to show last day or if game ended

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<FireScoreView>();

            public bool IsChangedFrom(ViewData check) =>
                ShowScore != check.ShowScore ||
                GameMode != check.GameMode ||
                Score.GetHashCode() != check.Score.GetHashCode() ||
                Multiplier != check.Multiplier ||
                CurrentDay != check.CurrentDay ||
                Target != check.Target;
        }

        private bool active = false;
        private FireGameMode gameMode = FireGameMode.CountUp;
        private string spriteName = "coin";
        private float score = 0;
        private float lastUpdateMultiplier = 0f;
        private int target = 0;
        private bool exceedTarget = false;

        public TextMeshPro Score;
        public TextMeshPro FireMultiplier;
        public TextMeshPro Target;

        protected override void UpdateData(ViewData data)
        {
            active = data.ShowScore;
            gameMode = data.GameMode;
            score = data.Score;
            lastUpdateMultiplier = data.Multiplier;
            target = data.Target;
            switch (gameMode)
            {
                case FireGameMode.CountDown:
                    spriteName = "extinguish";
                    exceedTarget = data.Target != 0 && data.CurrentDay > data.Target;
                    break;
                case FireGameMode.CountUp:
                    spriteName = "coin";
                    exceedTarget = data.Target != 0 && data.Score >= data.Target;
                    break;
            }
        }

        void Update()
        {
            Score?.gameObject.SetActive(active);
            FireMultiplier?.gameObject.SetActive(active);
            Target?.gameObject.SetActive(active && target != 0);

            if (!active)
                return;

            if (gameMode == FireGameMode.CountDown)
            {
                TimeSpan duration = score > 0f? new TimeSpan((long)(score * 1E+07f)) : TimeSpan.Zero;
                if (Score)
                {
                    string text = $"Score: {Math.Floor(duration.TotalHours):00}:{duration.Minutes:00}:{duration.Seconds:00}{(Main.PrefManager?.Get<bool>(Main.SHOW_DECIMALS_ID) ?? false ? $".{duration.Milliseconds}" : "")}";
                    Score.text = text;
                    float gb = Mathf.Clamp(((float)duration.TotalSeconds)/3600f, 0f, 1f);
                    Score.color = exceedTarget ? new Color(0f, 1f, 0f) : new Color(1f, gb, gb);
                }
                if (FireMultiplier)
                {
                    string text = $"Combo: {lastUpdateMultiplier:0.#}x <sprite name=\"{spriteName}\">";
                    FireMultiplier.text = string.Join("\n", text);
                    Target.color = Color.white;
                }
                if (Target)
                {
                    string text = $"Target: Day {target}";
                    Target.text = string.Join("\n", text);
                    Target.color = exceedTarget ? new Color(0f, 1f, 0f) : Color.white;
                }
            }
            else if (gameMode == FireGameMode.CountUp)
            {
                if (Score)
                {
                    string text = string.Empty;
                    if (exceedTarget)
                        text = $"Target Score Reached!";
                    Score.text = text;
                    Score.color = new Color(0f, 1f, 0f);
                }
                if (FireMultiplier)
                {
                    string text = $"Score: <sprite name=\"{spriteName}\"> {score}";
                    FireMultiplier.text = text;
                    FireMultiplier.color = exceedTarget ? new Color(0f, 1f, 0f) : Color.white;
                }
                if (Target)
                {
                    string text = $"Target: <sprite name=\"{spriteName}\"> {target}";
                    Target.text = string.Join("\n", text);
                    Target.color = Color.white;
                }
            }
        }
    }
}
