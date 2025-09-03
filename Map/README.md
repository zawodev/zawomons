# Hexagonal Map System

## Przegląd
System hexagonalnej mapy dla gry zawomons zawiera:
- Deterministyczną generację mapy na bazie seed'u
- System biomów z ScriptableObjects
- Kontroler kamery z drag & zoom
- Optymalizację culling dla dużych map
- Wsparcie dla budynków i stworek na mapie

## Struktura Plików

### Główne Systemy
- `MapSystem.cs` - Główny kontroler mapy
- `HexTile.cs` - Pojedynczy hexagon
- `CameraController.cs` - Kontrola kamery
- `HexMath.cs` - Matematyka hexagonalna

### Modele Danych
- `BiomeData.cs` - ScriptableObject dla biomów
- `HexTileData.cs` - dane pojedynczego kafla
- `CreatureMapData.cs` - dane stworków na mapie

### Edytory
- `MapSystemEditor.cs` - Custom editor dla MapSystem
- `HexTileEditor.cs` - Custom editor dla HexTile
- `BiomeCreator.cs` - Tworzenie przykładowych biomów

### Testy
- `HexMapTester.cs` - Skrypt testowy

## Konfiguracja

### 1. Szybka konfiguracja (ZALECANE)
1. Idź do `Tools -> Map System -> Create Complete Hex Tile Prefab`
   - To utworzy kompletny prefab w `Assets/Prefabs/Map/HexTile_Prefab.prefab`
   - Wraz z materiałami w `Assets/Materials/Map/`
2. Idź do `Tools -> Map System -> Create Default Biomes` 
   - To utworzy 5 podstawowych biomów w `Assets/Resources/Biomes/`

### 2. Alternatywna konfiguracja
1. **Tworzenie HexTile prefabu:**
   - Idź do `GameObject -> 3D Object -> Hex Tile` w menu Unity
   - To utworzy HexTile z podstawowymi komponentami
   - Użyj przycisku "Create Default Materials" w inspektorze
   - Zapisz jako prefab
   
2. **Tworzenie Biomów:**
   - Użyj `Create -> Map -> Biome Data` do tworzenia pojedynczych biomów
   - Lub `Tools -> Create Default Biomes` dla zestawu podstawowych

### 3. Konfiguracja Sceny
1. Utwórz pusty GameObject i dodaj komponent `MapSystem`
2. Przypisz utworzony HexTile prefab do pola `hexTilePrefab`
3. Dla kamery:
   - Dodaj `CameraController` do głównej kamery
   - Skonfiguruj ustawienia zoom i pan

## Edycja HexTile Prefabu

### Live Edycja
- Wszystkie parametry HexTile można edytować na żywo w inspektorze
- **Hex Settings:** rozmiar, orientacja (pointy top)
- **Outline Settings:** 
  - `showOutline` - pokaż outline zawsze
  - `outlineThickness` - grubość outline (0.02-0.2)
  - `outlineColor` - kolor outline

### Przyciski Edytora
- **Generate Mesh** - regeneruj mesh hexagonu
- **Create Default Materials** - utworz podstawowe materiały
- **Apply/Test Materials** - testuj różne materiały (base, hover, selected)

### 3. Generowanie Mapy
- Użyj przycisku "Generate Map" w inspektorze MapSystem
- "Random Seed" wygeneruje nowy losowy seed
- Możesz testować używając `HexMapTester` (klawisz G generuje mapę)

## Klawisze Testowe (HexMapTester)
- `G` - Generuj mapę
- `R` - Randomizuj seed
- `H` - Test matematyki hexagonalnej
- `B` - Test ładowania biomów

## Kontrola Kamery
- `LPM + przeciągnij` - przesuwanie kamery
- `Scroll` - zoom in/out
- `Środkowy przycisk myszy + przeciągnij` - alternatywne przesuwanie

## Ustawienia MapSystem

### Map Generation Settings
- `mapWidth/Height` - rozmiar mapy w kaflach
- `seed` - seed dla deterministycznej generacji
- `hexSize` - rozmiar pojedynczego hexagonu
- `pointyTop` - orientacja hexagonów (pionowa vs pozioma)

### Optimization
- `enableCulling` - włącz/wyłącz culling
- `cullingDistance` - dystans renderowania

## API Integration
System przygotowany na integrację z API:
- `LoadBuildingData()` - ładowanie budynków z API
- `LoadCreatureData()` - ładowanie stworków z API
- Na razie używa mock danych

## Rozszerzanie Systemu
- Dodaj nowe biomy tworząc ScriptableObject z `BiomeData`
- Modyfikuj `SelectBiomeForPosition()` dla innych algorytmów generacji
- Rozszerz `CreatureMapData` o więcej właściwości stworków
- Dodaj nowe typy budynków w `BuildingData`
