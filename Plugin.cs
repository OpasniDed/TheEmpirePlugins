using Exiled.API.Features;
using Exiled.CustomItems.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheEmpirePlugins.Items;

namespace TheEmpirePlugins
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "OpasniDed";
        public override string Name => "TheEmpirePlugins";
        public override string Prefix => "TheEmpirePlugins";

        public static Plugin Instance;
        private Albert_01R _albert;

        public override void OnEnabled()
        {
            Instance = this;
            Subscribe();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Unsubscribe();
            Instance = null;
            base.OnDisabled();
        }

        private void Subscribe()
        {
            _albert = new Albert_01R();
            _albert.Register();
        }

        private void Unsubscribe()
        {
            _albert = null;
            _albert.Unregister();
        }
    }
}
