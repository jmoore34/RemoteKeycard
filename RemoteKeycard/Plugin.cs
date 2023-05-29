using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Linq;

namespace RemoteKeycard
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "AFK Replacer";
        public override string Author => "Jon M";
        public override Version Version => new Version(1, 0, 0);

        // Singleton pattern allows easy access to the central state from other classes
        // (e.g. commands)
        public static Plugin Singleton { get; private set; }

        public override void OnEnabled()
        {
            // Set up the Singleton so we can easily get the instance with all the state
            // from another class.
            Singleton = this;
            Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;
            Exiled.Events.Handlers.Player.UnlockingGenerator += OnUnlockingGenerator;
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel += OnActivatingWarheadPanel;

            // Register event handlers
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // Deregister event handlers

            // This will prevent commands and other classes from being able to access
            // any state while the plugin is disabled
            Singleton = null;
            Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;
            Exiled.Events.Handlers.Player.UnlockingGenerator -= OnUnlockingGenerator;
            Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= OnActivatingWarheadPanel;

            base.OnDisabled();
        }

        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            ev.IsAllowed = ev.Player.Items.Any(item =>
                item.IsKeycard
                && ev.Door.RequiredPermissions.CheckPermissions(item.Base, ev.Player.ReferenceHub)
            );
        }

        public void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            ev.IsAllowed = ev.Player.IsBypassModeEnabled || ev.Player.Items.Any(item =>
                item.Base.ItemTypeId
                    is ItemType.KeycardResearchCoordinator
                    or ItemType.KeycardContainmentEngineer
                    or ItemType.KeycardNTFOfficer
                    or ItemType.KeycardNTFLieutenant
                    or ItemType.KeycardNTFCommander
                    or ItemType.KeycardFacilityManager
                    or ItemType.KeycardChaosInsurgency
                    or ItemType.KeycardO5
            );
        }

        public void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev)
        {
            ev.IsAllowed = ev.Player.IsBypassModeEnabled || ev.Player.Items.Any(item =>
                item.Base.ItemTypeId
                    is ItemType.KeycardNTFOfficer
                    or ItemType.KeycardNTFLieutenant
                    or ItemType.KeycardNTFCommander
                    or ItemType.KeycardChaosInsurgency
                    or ItemType.KeycardO5
            );
        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            ev.IsAllowed = ev.Player.IsBypassModeEnabled || ev.Player.Items.Any(item =>
                item.Base.ItemTypeId
                    is ItemType.KeycardContainmentEngineer
                    or ItemType.KeycardFacilityManager
                    or ItemType.KeycardO5
            );
        }


    }
}