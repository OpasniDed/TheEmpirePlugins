using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Item;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using PlayerStatsSystem;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace TheEmpirePlugins.Items
{
    [CustomItem(ItemType.GunCOM15)]
    public class Albert_01R : CustomWeapon
    {
        public override uint Id { get; set; } = 255;
        public override string Name { get; set; } = "Albert-01R";
        public override string Description { get; set; } = "Albert-01R, 13 ammo, cant reload, 225 damage";
        public override float Weight { get; set; } = 1f;
        public override byte ClipSize { get; set; } = 13;
        public override ItemType Type { get; set; } = ItemType.GunCOM15;
        public override float Damage { get; set; } = 225f;
        public override SpawnProperties SpawnProperties { get; set; } = new()
        {
            Limit = 1,
            DynamicSpawnPoints = new List<DynamicSpawnPoint>
            {
                new DynamicSpawnPoint
                {
                    Chance = 100,
                    Location = SpawnLocationType.Inside079Armory,
                },
            },
            
            
        };

        private static readonly string AudioDirectory = Path.Combine(Paths.Config, "FireAudio");
        private readonly Dictionary<Player, AudioPlayer> _audioPlayers = new();
        private readonly Dictionary<Player, SchematicObject> _schematicsPlayers = new();



        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon += OnReloading;
            Exiled.Events.Handlers.Item.ChangingAttachments += ChangingAttachent;
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            Exiled.Events.Handlers.Player.Destroying += OnPlayerDestroyed;
            Exiled.Events.Handlers.Player.ChangingItem += OnChanging;
            base.SubscribeEvents();

            InitializeAudio();
        }
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon -= OnReloading;
            Exiled.Events.Handlers.Item.ChangingAttachments -= ChangingAttachent;
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;
            Exiled.Events.Handlers.Player.Destroying -= OnPlayerDestroyed;
            Exiled.Events.Handlers.Player.ChangingItem -= OnChanging;

            foreach (var audioPlayer in _audioPlayers.Values)
            {
                audioPlayer?.Destroy();
            }
            _audioPlayers.Clear();

            base.UnsubscribeEvents();
        }
        private void InitializeAudio()
        {
            if (!Directory.Exists(AudioDirectory))
            {
                Directory.CreateDirectory(AudioDirectory);
                Log.Info($"Created directory: {AudioDirectory}");
            }
        }

        protected void D(SpawningItemEventArgs ev)
        {
            if (Check(ev.))
        }

        private void OnPlayerDestroyed(DestroyingEventArgs ev)
        {
            if (_audioPlayers.TryGetValue(ev.Player, out var audioPlayer))
            {
                audioPlayer?.Destroy();
                _audioPlayers.Remove(ev.Player);
            }
        }

        protected void ChangingAttachent(ChangingAttachmentsEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;
            ev.IsAllowed = false;
        }

        protected override void OnChanging(ChangingItemEventArgs ev)
        {
            
            if (_schematicsPlayers.ContainsKey(ev.Player))
            {
                _schematicsPlayers[ev.Player].Destroy();
                _schematicsPlayers.Remove(ev.Player);
            }
            if (!Check(ev.Item)) return;
            
            var schematic = ObjectSpawner.SpawnSchematic("ForFire", position: ev.Player.Position);
            if (schematic == null)
            {
                Log.Error("Schematic with name 'ForFire' not found");
                return;
            }
            _schematicsPlayers[ev.Player] = schematic;
            schematic.transform.parent = ev.Player.Transform;
            base.OnChanging(ev);
        }
        


  

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;
            ev.IsAllowed = false;
            base.OnReloading(ev);
        }

        protected override void OnShooting(ShootingEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem))
                return;

            try
            {
                var soundPath = Path.Combine(AudioDirectory, "Fire.ogg");
                if (!File.Exists(soundPath))
                {
                    Log.Error($"Sound file not exist: {soundPath}");
                    return;
                }

                if (_audioPlayers.TryGetValue(ev.Player, out var oldAudio))
                {
                    try
                    {
                        if (oldAudio != null && oldAudio.gameObject != null)
                        {
                            oldAudio.Destroy();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error destroying old audio: {ex}");
                    }
                    finally
                    {
                        _audioPlayers.Remove(ev.Player);
                    }
                }

                if (!_schematicsPlayers.TryGetValue(ev.Player, out var schematic) || schematic == null)
                {
                    Log.Error("Schematic not found for player");
                    return;
                }

                var owner = ev.Player != null ? new List<ReferenceHub> { ev.Player.ReferenceHub } : null;
                AudioPlayer audio = null;

                try
                {
                    audio = AudioPlayer.Create(
                        $"GunFire_{ev.Player.Id}_{DateTime.UtcNow.Ticks}",
                        "Fire",
                        owners: owner,
                        onIntialCreation: p =>
                        {
                            if (p != null && schematic != null && schematic.transform != null)
                            {
                                p.transform.parent = schematic.transform;
                                var speaker = p.AddSpeaker("Fire", minDistance: 2.5f, maxDistance: 20f, volume: 1f);
                                if (speaker != null)
                                {
                                    speaker.transform.parent = schematic.transform;
                                    speaker.transform.localPosition = Vector3.zero;
                                }
                            }
                        });

                    if (audio != null)
                    {
                        _audioPlayers[ev.Player] = audio;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error creating audio: {ex}");
                    audio?.Destroy();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnShooting: {ex}");
            }

            base.OnShooting(ev);
        }

    }
}
