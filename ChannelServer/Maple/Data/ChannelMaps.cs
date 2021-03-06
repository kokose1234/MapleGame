﻿using System.Collections.ObjectModel;
using Loki.Data;
using Loki.Maple.Maps;
using Loki.Maple.Shops;
using System;
using System.Reflection;
using Loki.Maple.Life;

namespace Loki.Maple.Data
{
    public class ChannelMaps : KeyedCollection<int, Map>
    {
        public ChannelMaps()
            : base()
        {
            using (Log.Load("Maps"))
            {
                foreach (dynamic mapDatum in new Datums("map_data").Populate())
                {
                    this.Add(new Map(mapDatum));
                }
            }

            using (Log.Load("Portals"))
            {
                foreach (dynamic portalDatum in new Datums("map_portals").Populate())
                {
                    Type implementedType = Assembly.GetExecutingAssembly().GetType("Loki.Maple.Maps.Portals." + portalDatum.script.ToString());

                    if (implementedType != null)
                    {
                        this[portalDatum.mapid].Portals.Add((Portal)Activator.CreateInstance(implementedType, portalDatum));
                    }
                    else
                    {
                        this[portalDatum.mapid].Portals.Add(new Portal(portalDatum));
                    }
                }
            }

            using (Log.Load("Life"))
            {
                foreach (dynamic lifeDatum in new Datums("map_life").Populate())
                {
                    switch ((string)lifeDatum.life_type)
                    {
                        case "npc":
                            Type implementedType = Assembly.GetExecutingAssembly().GetType("Loki.Maple.Life.Implementation.Npc" + lifeDatum.lifeid.ToString());

                            if (implementedType != null)
                            {
                                this[lifeDatum.mapid].Npcs.Add((Npc)Activator.CreateInstance(implementedType, lifeDatum));
                            }
                            else
                            {
                                this[lifeDatum.mapid].Npcs.Add(new Npc(lifeDatum));
                            }

                            break;

                        case "mob":
                        case "reactor":
                            this[lifeDatum.mapid].SpawnPoints.Add(new SpawnPoint(lifeDatum));
                            break;
                    }
                }

                foreach (dynamic npcDatum in new Datums("npc_data").Populate())
                {
                    if (npcDatum.storage_cost > 0)
                    {
                        foreach (Map loopMap in this)
                        {
                            foreach (Npc loopNpc in loopMap.Npcs)
                            {
                                if (loopNpc.MapleID == npcDatum.npcid)
                                {
                                    loopNpc.StorageCost = npcDatum.storage_cost;
                                }
                            }
                        }
                    }
                }
            }

            using (Log.Load("Shops"))
            {
                Shop.LoadRechargeTiers();

                foreach (dynamic shopDatum in new Datums("shop_data").Populate())
                {
                    foreach (Map loopMap in this)
                    {
                        foreach (Npc loopNpc in loopMap.Npcs)
                        {
                            if (loopNpc.MapleID == shopDatum.npcid)
                            {
                                loopNpc.Shop = new Shop(loopNpc, shopDatum);
                            }
                        }
                    }
                }
            }
        }

        protected override int GetKeyForItem(Map item)
        {
            return item.MapleID;
        }
    }
}
