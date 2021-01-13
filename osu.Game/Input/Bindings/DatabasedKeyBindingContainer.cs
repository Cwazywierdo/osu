// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using osu.Game.Rulesets;
using Realms;

namespace osu.Game.Input.Bindings
{
    /// <summary>
    /// A KeyBindingInputManager with a database backing for custom overrides.
    /// </summary>
    /// <typeparam name="T">The type of the custom action.</typeparam>
    public class DatabasedKeyBindingContainer<T> : KeyBindingContainer<T>
        where T : struct
    {
        private readonly RulesetInfo ruleset;

        private readonly int? variant;

        private IDisposable realmSubscription;
        private IQueryable<RealmKeyBinding> realmKeyBindings;

        [Resolved]
        private RealmKeyBindingStore store { get; set; }

        [Resolved]
        private RealmContextFactory realmFactory { get; set; }

        public override IEnumerable<IKeyBinding> DefaultKeyBindings => ruleset.CreateInstance().GetDefaultKeyBindings(variant ?? 0);

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="ruleset">A reference to identify the current <see cref="Ruleset"/>. Used to lookup mappings. Null for global mappings.</param>
        /// <param name="variant">An optional variant for the specified <see cref="Ruleset"/>. Used when a ruleset has more than one possible keyboard layouts.</param>
        /// <param name="simultaneousMode">Specify how to deal with multiple matches of <see cref="KeyCombination"/>s and <typeparamref name="T"/>s.</param>
        /// <param name="matchingMode">Specify how to deal with exact <see cref="KeyCombination"/> matches.</param>
        public DatabasedKeyBindingContainer(RulesetInfo ruleset = null, int? variant = null, SimultaneousBindingMode simultaneousMode = SimultaneousBindingMode.None, KeyCombinationMatchingMode matchingMode = KeyCombinationMatchingMode.Any)
            : base(simultaneousMode, matchingMode)
        {
            this.ruleset = ruleset;
            this.variant = variant;

            if (ruleset != null && variant == null)
                throw new InvalidOperationException($"{nameof(variant)} can not be null when a non-null {nameof(ruleset)} is provided.");
        }

        protected override void LoadComplete()
        {
            var realm = realmFactory.Get();

            if (ruleset == null || ruleset.ID.HasValue)
            {
                var rulesetId = ruleset?.ID;

                realmKeyBindings = realm.All<RealmKeyBinding>()
                                        .Where(b => b.RulesetID == rulesetId && b.Variant == variant);

                realmSubscription = realmKeyBindings
                    .SubscribeForNotifications((sender, changes, error) =>
                    {
                        // first subscription ignored as we are handling this in LoadComplete.
                        if (changes == null)
                            return;

                        ReloadMappings();
                    });
            }

            base.LoadComplete();
        }

        protected override void ReloadMappings()
        {
            if (realmKeyBindings != null)
                KeyBindings = realmKeyBindings.Detach();
            else
                base.ReloadMappings();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }
    }
}
