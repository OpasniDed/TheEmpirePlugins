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

        private static readonly string AudioDirectory = Path.Combine(Paths.Configs, "FireAudio");



        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon += OnReloading;
            Exiled.Events.Handlers.Item.ChangingAttachments += ChangingAttachent;
            Exiled.Events.Handlers.Player.Shooting += OnShooting;
            base.SubscribeEvents();

            InitializeAudio();
        }
        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.ReloadingWeapon -= OnReloading;
            Exiled.Events.Handlers.Item.ChangingAttachments -= ChangingAttachent;
            Exiled.Events.Handlers.Player.Shooting -= OnShooting;


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




        protected void ChangingAttachent(ChangingAttachmentsEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;
            ev.IsAllowed = false;
        }

        
        


  

        protected override void OnReloading(ReloadingWeaponEventArgs ev)
        {
            if (!Check(ev.Player.CurrentItem)) return;
            ev.IsAllowed = false;
            base.OnReloading(ev);
        }

        private void OnShooting(ShootingEventArgs ev)
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

                


                var owner = ev.Player != null ? new List<ReferenceHub> { ev.Player.ReferenceHub } : null;

                try
                {
                    var audioPlayer = AudioPlayer.CreateOrGet($"{UnityEngine.Random.Range(1, 10000)}", soundPath, onIntialCreation: p =>
                    {
                        var speaker = p.AddSpeaker($"Main{UnityEngine.Random.Range(1, 10000)}", isSpatial: true, minDistance: 1f, maxDistance: 15f);
                        speaker.transform.SetParent(ev.Player.Transform, false);
                    });

                    if (audioPlayer != null)
                    {
                        Log.Info(audioPlayer.transform.parent.position);
                        audioPlayer.AddClip(soundPath, loop: false, volume: 1.5f, destroyOnEnd: true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error creating audio: {ex}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in OnShooting: {ex}");
            }

        }

    }
}
