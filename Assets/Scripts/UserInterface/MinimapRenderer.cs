using DotNetMissionSDK.Json;
using OP2MissionHub.Data;
using OP2MissionHub.Systems;
using OP2UtilityDotNet;
using UnityEngine;
using UnityEngine.UI;

namespace OP2MissionHub.UserInterface
{
	[RequireComponent(typeof(RawImage))]
	public class MinimapRenderer : MonoBehaviour
	{
		private RectTransform m_Frame			= default;
		private RawImage m_MinimapImage			= default;


		private void Awake()
		{
			m_Frame = GetComponent<RectTransform>();
			m_MinimapImage = GetComponent<RawImage>();
		}

		/// <summary>
		/// Generates a minimap texture and assigns it as an image.
		/// </summary>
		/// <param name="map">The map to to generate a minimap from.</param>
		/// <param name="missionVariant">The mission variant to generate a minimap from. (optional)</param>
		public void SetMap(Map map, MissionVariant missionVariant=null)
		{
			uint mapWidth = map.GetWidthInTiles();
			uint mapHeight = map.GetHeightInTiles();

			// Create minimap texture
			Texture2D minimapTexture = new Texture2D((int)mapWidth*TextureManager.minimapScale, (int)mapHeight*TextureManager.minimapScale, TextureFormat.ARGB32, false);

			CellTypeMap cellTypeMap = new CellTypeMap(map, missionVariant);

			for (uint x=0; x < mapWidth; ++x)
			{
				for (uint y=0; y < mapHeight; ++y)
				{
					ulong tileMappingIndex = GetTileMappingIndex(map, cellTypeMap, new Vector2Int((int)x,(int)y));
					TileMapping mapping = map.GetTileMapping(tileMappingIndex);
			
					ulong tileSetIndex = mapping.tilesetIndex;
					int tileImageIndex = mapping.tileGraphicIndex;

					string tileSetPath = map.GetTilesetSourceFilename(tileSetIndex);
					int tileSetNumTiles = (int)map.GetTilesetSourceNumTiles(tileSetIndex);

					// Get image offset
					int inverseTileIndex = tileSetNumTiles-tileImageIndex-1;
					
					Vector3Int cellPosition = new Vector3Int((int)x,(int)(mapHeight-y-1),0);

					++cellPosition.y;

					// Set minimap pixel
					Texture2D mTexture = TextureManager.LoadMinimapTileset(tileSetPath, tileSetNumTiles);
					for (int my=0; my < TextureManager.minimapScale; ++my)
					{
						for (int mx=0; mx < TextureManager.minimapScale; ++mx)
						{
							Color color = mTexture.GetPixel(mx, inverseTileIndex*TextureManager.minimapScale + my);
							minimapTexture.SetPixel(cellPosition.x*TextureManager.minimapScale + mx, cellPosition.y*TextureManager.minimapScale + my - 1, color);
						}
					}
				}
			}

			// Apply mission units to minimap
			if (missionVariant != null)
			{
				foreach (GameData.Beacon beacon in missionVariant.tethysGame.beacons)
					SetMinimapTile(minimapTexture, new Vector2Int(beacon.position.x, beacon.position.y) - Vector2Int.one, Color.white);

				foreach (GameData.Marker marker in missionVariant.tethysGame.markers)
					SetMinimapTile(minimapTexture, new Vector2Int(marker.position.x, marker.position.y) - Vector2Int.one, Color.white);

				foreach (GameData.Wreckage wreckage in missionVariant.tethysGame.wreckage)
					SetMinimapTile(minimapTexture, new Vector2Int(wreckage.position.x, wreckage.position.y) - Vector2Int.one, Color.white);

				foreach (PlayerData player in missionVariant.players)
				{
					foreach (UnitData unit in player.resources.units)
					{
						RectInt unitArea = StructureData.GetStructureArea(new Vector2Int(unit.position.x, unit.position.y) - Vector2Int.one, unit.typeID);

						for (int x=unitArea.xMin; x < unitArea.xMax; ++x)
						{
							for (int y=unitArea.yMin; y < unitArea.yMax; ++y)
								SetMinimapTile(minimapTexture, new Vector2Int(x,y), GetPlayerColor(player));
						}
					}
				}
			}

			// Apply minimap texture
			minimapTexture.Apply();

			// Update image
			RefreshImage(map, minimapTexture);
		}

		// Sets the entire tile on the minimap, which spans multiple pixels on a scaled up texture
		private void SetMinimapTile(Texture2D minimapTexture, Vector2Int tilePosition, Color color)
		{
			for (int my=0; my < TextureManager.minimapScale; ++my)
			{
				for (int mx=0; mx < TextureManager.minimapScale; ++mx)
				{
					minimapTexture.SetPixel(tilePosition.x*TextureManager.minimapScale + mx, tilePosition.y*TextureManager.minimapScale + my - 1, color);
				}
			}
		}

		private void RefreshImage(Map map, Texture2D minimapTexture)
		{
			// Update minimap texture
			m_MinimapImage.texture = minimapTexture;
			Vector2Int mapSize = new Vector2Int((int)map.GetWidthInTiles(), (int)map.GetHeightInTiles());

			// Adjust window to match map aspect ratio
			float mapAspect = (float)mapSize.x / mapSize.y;
			m_Frame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_Frame.rect.height * mapAspect);
		}

		private ulong GetTileMappingIndex(Map map, CellTypeMap cellTypeMap, Vector2Int tileXY)
		{
			ulong x = (ulong)tileXY.x;
			ulong y = (ulong)tileXY.y;

			// Get default mapping index
			ulong mappingIndex = map.GetTileMappingIndex(x,y);

			// If no cell type map, return the map index. This will happen if there isn't any .opm mission file.
			if (cellTypeMap == null)
				return mappingIndex;

			// Get TerrainType for mapping index, if available
			TerrainType terrainType;
			if (!GetTerrainTypeForMappingIndex(map, mappingIndex, out terrainType))
				return mappingIndex;

			// Predict starting CellType and remap to terrain type
			int wallTubeIndex;
			switch (TileMapData.GetWallTubeIndexForTile(cellTypeMap, tileXY, out wallTubeIndex))
			{
				case CellType.DozedArea:		mappingIndex = terrainType.bulldozedTileMappingIndex;						break;
				case CellType.NormalWall:		mappingIndex = terrainType.wallTileMappingIndexes[2*16+wallTubeIndex];		break;
				case CellType.LavaWall:			mappingIndex = terrainType.wallTileMappingIndexes[wallTubeIndex];			break;
				case CellType.MicrobeWall:		mappingIndex = terrainType.wallTileMappingIndexes[1*16+wallTubeIndex];		break;
				case CellType.Tube0:
				case CellType.Tube1:
				case CellType.Tube2:
				case CellType.Tube3:
				case CellType.Tube4:
				case CellType.Tube5:
					mappingIndex = terrainType.tubeTileMappingIndexes[wallTubeIndex];
					break;
			}

			return mappingIndex;
		}

		private bool GetTerrainTypeForMappingIndex(Map map, ulong mappingIndex, out TerrainType terrainType)
		{
			ulong count = map.GetTerrainTypeCount();

			// Search terrain types for associated mapping index
			for (ulong i=0; i < count; ++i)
			{
				TerrainType type = map.GetTerrainType(i);
				if (type.tileMappingRange.start <= mappingIndex && mappingIndex <= type.tileMappingRange.end)
				{
					terrainType = type;
					return true;
				}
			}

			terrainType = new TerrainType();
			return false;
		}

		private Color GetPlayerColor(PlayerData player)
		{
			switch (player.color)
			{
				case DotNetMissionSDK.PlayerColor.Blue:		return Color.blue;
				case DotNetMissionSDK.PlayerColor.Red:		return Color.red;
				case DotNetMissionSDK.PlayerColor.Green:	return Color.green;
				case DotNetMissionSDK.PlayerColor.Yellow:	return Color.yellow;
				case DotNetMissionSDK.PlayerColor.Cyan:		return Color.cyan;
				case DotNetMissionSDK.PlayerColor.Magenta:	return Color.magenta;
				case DotNetMissionSDK.PlayerColor.Black:	return Color.black;
			}

			return Color.white;
		}
	}
}
