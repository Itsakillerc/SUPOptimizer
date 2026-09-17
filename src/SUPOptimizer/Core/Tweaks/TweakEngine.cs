using System;
using System.Collections.Generic;
using System.Linq;

namespace SUPOptimizer.Core.Tweaks
{
    public class BatchApplyResult
    {
        public int Total { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public bool DryRun { get; set; }
        public bool RequiresReboot { get; set; }
        public List<TweakResult> Results { get; set; } = new();
    }

    public static class TweakEngine
    {
        public static TweakResult Apply(string tweakId, bool dryRun)
        {
            var tweak = TweakRegistry.GetTweak(tweakId);
            if (tweak == null)
            {
                return new TweakResult
                {
                    Success = false,
                    Message = $"Tweak with ID '{tweakId}' not found.",
                    DryRun = dryRun
                };
            }

            return tweak.Apply(dryRun);
        }

        public static TweakResult Restore(string tweakId, bool dryRun)
        {
            var tweak = TweakRegistry.GetTweak(tweakId);
            if (tweak == null)
            {
                return new TweakResult
                {
                    Success = false,
                    Message = $"Tweak with ID '{tweakId}' not found.",
                    DryRun = dryRun
                };
            }

            return tweak.Restore(dryRun);
        }

        public static BatchApplyResult ApplyBatch(IEnumerable<string> tweakIds, bool dryRun)
        {
            var batchResult = new BatchApplyResult
            {
                DryRun = dryRun
            };

            var idsList = tweakIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            batchResult.Total = idsList.Count;

            foreach (var id in idsList)
            {
                var tweak = TweakRegistry.GetTweak(id);
                if (tweak == null)
                {
                    batchResult.Skipped++;
                    batchResult.Results.Add(new TweakResult
                    {
                        Success = false,
                        Message = $"Tweak '{id}' not found.",
                        DryRun = dryRun
                    });
                    continue;
                }

                var res = tweak.Apply(dryRun);
                batchResult.Results.Add(res);

                if (res.Success)
                {
                    batchResult.Succeeded++;
                    if (res.RequiresReboot)
                        batchResult.RequiresReboot = true;
                }
                else
                {
                    batchResult.Failed++;
                }
            }

            return batchResult;
        }
    }
}
