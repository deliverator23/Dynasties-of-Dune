//modPlatform=Modio
using System.Collections.Generic;
using System.Linq;
using System;
using Mohawk.SystemCore;
using UnityEngine;
using TenCrowns.GameCore;


public class MapScriptArrakipelago : DefaultMapScript
{
    readonly int HillsPercent = 16;
    readonly int MinIslandSizeForPlayerStart = 30;

    List<List<int>> islands = new List<List<int>>();
    Dictionary<int, int> tileIslands = new Dictionary<int, int>();

    MapOptionType eLandmassOption = MapOptionType.NONE;

    readonly MapOptionType LANDMASS_LARGE = MapOptionType.NONE;
    readonly MapOptionType LANDMASS_MEDIUM = MapOptionType.NONE;
    readonly MapOptionType LANDMASS_SMALL = MapOptionType.NONE;

    public static string GetName()
    {
        return "Arrakipelago";
    }

    public static string GetHelp()
    {
        return "TEXT_MAP_HELP_ARRAKIPELAGO";
    }

    public static bool IncludeInRandom()
    {
        return true;
    }

    public static bool IsHidden()
    {
        return false;
    }

    public new static void GetCustomOptionsMulti(List<MapOptionsMultiType> options, Infos infos)
    {
        options.Add(infos.getType<MapOptionsMultiType>("MAP_OPTIONS_ARCHIPELAGO_LANDMASS"));

        DefaultMapScript.GetCustomOptionsMulti(options, infos);
    }

    public MapScriptArrakipelago(ref MapParameters mapParameters, Infos infos) : base(ref mapParameters, infos)
    {
        LANDMASS_LARGE = infos.getType<MapOptionType>("MAP_OPTION_ARCHIPELAGO_LANDMASS_LARGE");
        LANDMASS_MEDIUM = infos.getType<MapOptionType>("MAP_OPTION_ARCHIPELAGO_LANDMASS_MEDIUM");
        LANDMASS_SMALL = infos.getType<MapOptionType>("MAP_OPTION_ARCHIPELAGO_LANDMASS_SMALL");

        ShapeBoundaryToMap = false;
        ElevationNoiseAmplitude = 2;
    }

    protected override void InitMapData()
    {
        base.InitMapData();

		OceanPercent = 66;
		CoastPercent = 10;
	
    }

    protected override void SetMapSize()
    {
        base.SetMapSize();
        mapParameters.iWidth = mapParameters.iWidth * 7 / 5; // increased map dimensions due to its low percentage of workable tiles. (approximately twice as many tiles - sqrt(2) ~ 7/5)
        mapParameters.iHeight = mapParameters.iHeight * 7 / 5;
    }

    protected override void GenerateLand()
    {
        int iBoundaryWidth = infos.Globals.MAP_BOUNDARY_IMMUTABLE_OUTER_WIDTH + 7;
        for (int x = 0; x < MapWidth; ++x)
        {
            for (int y = 0; y < iBoundaryWidth; ++y)
            {
                heightGen.SetValue(x, y, y - iBoundaryWidth);
                heightGen.SetValue(x, MapHeight - y - 1, y - iBoundaryWidth);
            }
        }

        for (int y = 0; y < MapHeight; ++y)
        {
            for (int x = 0; x < iBoundaryWidth; ++x)
            {
                heightGen.SetValue(x, y, x - iBoundaryWidth);
                heightGen.SetValue(MapWidth - x - 1, y, x - iBoundaryWidth);
            }
        }

        heightGen.Normalize();
        heightGen.AddNoise(16, ElevationNoiseAmplitude);
        heightGen.Normalize();

        // Set the lowest tiles to be water
        List<NoiseGenerator.TileValue> tiles = heightGen.GetPercentileRange(0, OceanPercent);
        foreach (NoiseGenerator.TileValue tile in tiles)
        {
            TileData loopTile = GetTile(tile.x, tile.y);
            loopTile.meTerrain = DESERT_TERRAIN;
            loopTile.meHeight = infos.Globals.HILL_HEIGHT;
        }

        // Set the next lowest tiles to be coast
        tiles = heightGen.GetPercentileRange(OceanPercent, OceanPercent + CoastPercent);
        foreach (NoiseGenerator.TileValue tile in tiles)
        {
            TileData loopTile = GetTile(tile.x, tile.y);
            loopTile.meTerrain = DESERT_TERRAIN;
            loopTile.meHeight = infos.Globals.DEFAULT_HEIGHT;
        }

        ResetDistances();
    }
	
	protected override void GenerateDeserts()
	{
		// do nothing
	}

	protected override void GenerateMountains()
	{
        foreach (TileData tile in Tiles.Where(x => !IsDesert(x) && CountAdjacent(x, IsDesert) > 0))
        {	
			if (random.Next(2) == 1) {
				tile.meHeight = infos.Globals.MOUNTAIN_HEIGHT;
			} else {
				tile.meHeight = infos.Globals.HILL_HEIGHT;
			}
        }
	}
	
	protected virtual bool IsNotDesert(TileData tile)
	{
		if (tile == null)
		{
			return false;
		}
		if (tile.meTerrain == DESERT_TERRAIN)
		{
			return false;
		}
		return true;
	}
	
	protected override void EliminateSingletonMountains()
	{
		// do nothing
	}
	
	protected override void GenerateElevations()
	{
		// do nothing
	}

	protected override void FixCoast()
	{
		// do nothing
	}

	protected override void GenerateRivers()
	{
		// do nothing
	}
	
	protected override void ConvertOverloadedRiverTilesToLakes()
	{
		// do nothing
	}

	protected override void FillLakes()
	{
		// do nothing
	}
                
 	protected override void BuildWaterAreas()
	{
		// do nothing
	}    

 	protected override void ModifyTerrain()
	{
		// do nothing
	}	
    
 	protected override void SmoothTerrain()
	{
		// do nothing
	}		
	
	protected override void HandleRainEffects()
	{
		using (new UnityProfileScope("DefaultMapScript.HandleRainEffects"))
		{

			MeasureMountainStrength();

			ApplyOceanicRains();

			SoftenDesertsAlongRivers();

			//ApplySeaBreezeRainsToIrregularCoastline();

			//ApplyLakeEffectRains();

			SmoothDeserts();
			
			foreach (TileData tile in Tiles.Where(x => IsDesert(x) && CountAdjacent(x, IsNotDesert) > 0))
			{	
				tile.meHeight = infos.Globals.DEFAULT_HEIGHT;
			}
		}
	}
	
	protected override void MakePlayerStart(PlayerType player, TileData tile, bool freshWaterCheck = true)
	{
		tile.meCitySite = CitySiteType.ACTIVE_START;
		playerStarts.Add(player, tile.ID);
		mValidCitySite.Clear();

		//if (!MirrorMap && freshWaterCheck && !IsFreshWaterAccess(tile)) // Best available legal site lacks fresh water. Fudge in a lake.
		//{
		//	AddSmallLakeToStartLocation(tile);
		//}
	}
	
 	protected override void BuildVegetation()
	{
		// do nothing
	}

    protected override bool AddPlayerStarts()
    {
        BuildAreas(islands, null, x => IsLand(x) && !x.isImpassable(infos));
        for (int area = islands.Count - 1; area >= 0; --area)
        {
            foreach (int tileID in islands[area])
            {
                tileIslands[tileID] = area;
            }
            if (islands[area].Count <= MinIslandSizeForPlayerStart)
            {
                islands.RemoveAt(area);
            }
        }

        return base.AddPlayerStarts();
    }

    protected override bool IsValidPlayerStart(TileData tile, PlayerType player, bool bDoMinDistanceCheck = true)
    {
        if (!base.IsValidPlayerStart(tile, player, bDoMinDistanceCheck))
        {
            return false;
        }

        if (bDoMinDistanceCheck)
        {
            bool islandTaken = false;
            foreach (var kvp in playerStarts)
            {
                PlayerType loopPlayer = kvp.Key;
                int startTileID = kvp.Value;
                if (startTileID != -1)
                {
                    if (tileIslands[startTileID] == tileIslands[tile.ID])
                    {
                        islandTaken = true;
                    }
                }
            }

            if (islands.Count >= players.Count && islandTaken)
            {
                return false;
            }
        }

        return true;
    }

    protected override bool AddPlayerStartsTwoTeamMP()
    {
        if (!MirrorMap)
        {
            return base.AddPlayerStartsDefault(); // so each team starts on a different continent, if possible, instead of forcing left and right
        }
        else
        {
            return base.AddPlayerStartsTwoTeamMP();
        }
    }
}
