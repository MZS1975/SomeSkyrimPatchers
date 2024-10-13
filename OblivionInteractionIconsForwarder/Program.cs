using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Activator = Mutagen.Bethesda.Skyrim.Activator;

namespace OblivionInteractionIconsForwarder
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "skymojibase-patcher.esp")
                .Run(args);
        }

        private static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine("Running OblivionInteractionIcons Forwarder...");
            var baseModEntry = state.LoadOrder.TryGetValue(new ModKey("skymojibase", ModType.Light));
            if (baseModEntry?.Mod == null)
            {
                Console.WriteLine("This patcher requires a light mod names 'skymojibase.esl'.");
                return;
            }

            var baseMod = baseModEntry.Mod;
            foreach (var activator in state.LoadOrder.PriorityOrder.WinningOverrides<IActivatorGetter>())
            {
                var referenceActivator = baseMod.Activators.TryGetValue(activator.FormKey);
                if (referenceActivator == null)
                {
                    Console.WriteLine($"Skipping record {activator.FormKey} - not relevant for this mod.");
                    continue;
                }

                if (referenceActivator.ActivateTextOverride == null || Equals(referenceActivator.ActivateTextOverride, activator.ActivateTextOverride))
                {
                    Console.WriteLine($"No patch needed for record {activator.FormKey}");
                    continue;
                }

                var modifiedActivator = state.PatchMod.Activators.GetOrAddAsOverride(activator);
                modifiedActivator.ActivateTextOverride = referenceActivator.ActivateTextOverride.DeepCopy();
                Console.WriteLine($"Patching {activator.ActivateTextOverride} with {referenceActivator.ActivateTextOverride}");
            }
            
            foreach (var flora in state.LoadOrder.PriorityOrder.WinningOverrides<IFloraGetter>())
            {
                var referenceFlora = baseMod.Florae.TryGetValue(flora.FormKey);
                if (referenceFlora == null)
                {
                    Console.WriteLine($"Skipping record {flora.FormKey} - not relevant for this mod.");
                    continue;
                }
                
                if (referenceFlora.ActivateTextOverride == null || Equals(referenceFlora.ActivateTextOverride, flora.ActivateTextOverride))
                {
                    Console.WriteLine($"No patch needed for record {flora.FormKey}");
                    continue;
                }

                var modifiedFlora = state.PatchMod.Florae.GetOrAddAsOverride(flora);
                modifiedFlora.ActivateTextOverride = referenceFlora.ActivateTextOverride.DeepCopy();
                Console.WriteLine($"Patching {flora.ActivateTextOverride} with {referenceFlora.ActivateTextOverride}");
            }
            
            Console.WriteLine("Done.");
        }
    }
}
