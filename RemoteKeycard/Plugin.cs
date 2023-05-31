using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using System;
using System.Linq;

namespace RemoteKeycard
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "Remote Keycard";
        public override string Author => "Jonathan Moore";
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
            //Exiled.Events.Handlers.Player.ActivatingWarheadPanel += OnActivatingWarheadPanel;

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
            //Exiled.Events.Handlers.Player.ActivatingWarheadPanel -= OnActivatingWarheadPanel;

            base.OnDisabled();
        }

        private CustomItem? _hackDevice = null;
        private CustomItem? HackDevice
        {
            get
            {
                if (_hackDevice == null && CustomItem.TryGet(208u, out var hackDevice))
                {
                    _hackDevice = hackDevice;
                }
                return _hackDevice;
            }
        }


        public void OnInteractingDoor(InteractingDoorEventArgs ev)
        {
            if (HackDevice != null && HackDevice.Check(ev.Player.CurrentItem))
            {
                return;
            }
            ev.IsAllowed |= ev.Player.Items.Any(item =>
                item.IsKeycard
                && ev.Door.RequiredPermissions.CheckPermissions(item.Base, ev.Player.ReferenceHub)
            );
        }

        public void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            ev.IsAllowed |= ev.Player.Items.Any(item =>
            {
                if (item.Base is KeycardItem keycard)
                {
                    bool allowed = keycard.Permissions.HasFlagFast(ev.Chamber.RequiredPermissions);
                    //Log.Debug($"Keycard with permissions {keycard.Permissions} attempted to open locker with required permissions {ev.Chamber.RequiredPermissions} (allowed: {allowed})");
                    return allowed;
                }
                return false;
            });
        }

        public void OnUnlockingGenerator(UnlockingGeneratorEventArgs ev)
        {
            if (ev.Player.GetCustomRoles().Any(r => r.Name.StartsWith("SH")))
            { // serpents' hand
                ev.IsAllowed = false;
            }
            else
            {
                ev.IsAllowed |= ev.Player.IsBypassModeEnabled || ev.Player.Items.Any(item =>
                {
                    if (item.Base is KeycardItem keycard)
                    {
                        bool allowed = keycard.Permissions.HasFlagFast(KeycardPermissions.ArmoryLevelTwo);
                        //Log.Info($"Keycard with permissions {keycard.Permissions} attempted to unlock generator (allowed: {allowed})");
                        return allowed;
                    }
                    return false;
                }
            );
            }

        }

        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            ev.IsAllowed |= ev.Player.IsBypassModeEnabled || ev.Player.Items.Any(item =>
                item.Base.ItemTypeId
                    is ItemType.KeycardContainmentEngineer
                    or ItemType.KeycardFacilityManager
                    or ItemType.KeycardO5
            );
        }


    }
}