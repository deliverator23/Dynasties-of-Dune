//modPlatform=Modio
using System.Collections.Generic;
using Mohawk.SystemCore;
using TenCrowns.GameCore;
using UnityEngine;

public class MapScriptArrakipelago : DefaultMapScript
{
    readonly int HillsPercent = 16;
    readonly int MinIslandSizeForPlayerStart = 30;

    MapOptionType eLandmassOption = MapOptionType.NONE;

    readonly MapOptionType LANDMASS_LARGE = MapOptionType.NONE;
    readonly MapOptionType LANDMASS_MEDIUM = MapOptionType.NONE;
    readonly MapOptionType LANDMASS_SMALL = MapOptionType.NONE;

    protected override bool ShapeBoundaryToMap => false;
    protected override float ElevationNoiseAmplitude => 2;
    protected override short MinContinentSize => 30;
    protected override short LakePercent => 0;
    protected override short CoastPercent => 10;
    protected override short OceanPercent
    {
        get
        {
            if (eLandmassOption == LANDMASS_SMALL)
            {
                return 70;
            }
            else if (eLandmassOption == LANDMASS_MEDIUM)
            {
                return 60;
            }
            else if (eLandmassOption == LANDMASS_LARGE)
            {
                return 50;
            }
            return base.OceanPercent;
        }
    }

    List<List<int>> islandAreas = new List<List<int>>();
    List<List<int>> waterAreas = new List<List<int>>();

    public static string GetName()
    {
        return "Arrakis Archipelago";
    }

    public static string GetHelp()
    {
        return "Arrakis Archipelago";
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
    }

    protected override void InitMapData()
    {
        base.InitMapData();

        MapOptionsMultiType eLandmass = infos.getType<MapOptionsMultiType>("MAP_OPTIONS_ARCHIPELAGO_LANDMASS");
        if (eLandmass != MapOptionsMultiType.NONE)
        {
            if (!mapParameters.gameParams.mapMapMultiOptions.TryGetValue(eLandmass, out eLandmassOption))
            {
                eLandmassOption = infos.mapOptionsMulti(eLandmass).meDefault;
            }
        }
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

        // Scattered hills, not part of mountain chains.
        hillsGen.AddNoise(MapWidth / 25.0f, 1);
        hillsGen.Normalize();

        tiles = hillsGen.GetPercentileRange(100 - HillsPercent, 100);
        foreach (NoiseGenerator.TileValue tile in tiles)
        {
            TileData loopTile = GetTile(tile.x, tile.y);
            if (loopTile.meHeight == infos.Globals.DEFAULT_HEIGHT)
            {
                loopTile.meHeight = infos.Globals.HILL_HEIGHT;
            }
        }

        ResetDistances();
    }

    protected override void GenerateDeserts()
    {
        // do nothing
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

    protected override void BuildVegetation()
    {
        // do nothing
    }

    protected override void AddSmallLakeToStartLocation(TileData tile, int iMaxTargetLakeTiles = 4, int iMinTargetLakeTiles = 1, bool bAddLakeEffectRains = true)
    {
        // do nothing
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
