# AGENTS.md

This document provides instructions for AI coding agents working on this Unity project.

---

## Project Overview

- **Project Name**: treasure
- **Engine**: Unity 6000.3.24f1 with URP 2D (`Assets/Settings/Renderer2D.asset`)
- **Game Type**: 동성로 트레저헌터 — 세로 화면 모바일 위치 기반 수집 게임 (GPS 지도 + 탭 추격 포획 + 지도 퍼즐)
- **Language**: C#

주요 패키지 (`Packages/manifest.json`):
- Input System 1.20.0 (액션 에셋: `Assets/Settings/InputSystem_Actions.inputactions`)
- Universal RP 17.3.0
- 2D Animation, Aseprite Importer, PSD Importer, SpriteShape, Tilemap Extras
- Test Framework 1.6.0

---

## Agent Workflow

- 생성물/로컬 폴더는 수정하지 않는다: `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`, `Build/`, `Builds/`, `*.csproj`, `*.slnx` (Unity가 재생성한다)
- `Assets/` 아래 파일을 옮기거나 이름을 바꾸거나 지울 때는 같은 이름의 `.meta` 파일도 함께 처리한다
- 새 에셋의 `.meta` 파일과 GUID는 직접 만들지 않고 Unity가 생성하게 둔다
- `.unity`, `.prefab`, `.asset` YAML 직접 수정은 최소화하고, 필요하면 Editor 스크립트나 Unity 에디터 작업 절차로 안내한다
- 코드 변경 후 Unity 컴파일 에러가 없는지 확인하고, `Assets/Tests/EditMode/`에 테스트가 있으면 EditMode 테스트를 실행한다

---

## Critical Coding Rules

### 1. Comments Must Be in Korean (한국어)

```csharp
// Good: 플레이어 이동 속도
public float moveSpeed = 5f;

// Bad: Player movement speed (DO NOT USE ENGLISH)
```

### 2. NO XML Documentation Comments

```csharp
// WRONG - Do not use XML comments
/// <summary>
/// This is wrong
/// </summary>

// CORRECT - Use simple comments
// 플레이어의 현재 체력을 반환한다
public int GetHealth() { ... }
```

### 3. Keep Code Simple and Readable

This project prioritizes **human maintainability** over clever abstractions.

- Prefer explicit code over "magic" patterns
- Avoid deep inheritance hierarchies (max 2 levels)
- Avoid excessive generics or reflection
- One class = one responsibility
- Methods should be under 30 lines when possible

```csharp
// Good: 간단하고 명확함
public void TakeDamage(int damage)
{
    currentHealth -= damage;
    if (currentHealth <= 0)
    {
        Die();
    }
}

// Bad: 과도한 엔지니어링
public void TakeDamage<T>(T damage) where T : IDamageType =>
    _healthSystem.ApplyDamageModifier(damage.GetModifiedValue(_resistances));
```

### 4. Frame-Rate Independent Code (프레임 독립적 코드)

All time-based logic must be frame-rate independent (60FPS, 120FPS, 144FPS all behave the same):

```csharp
// ✅ Good: 시간 기반 (프레임 독립적)
_timer -= Time.deltaTime;           // Update에서 사용
movement *= Time.fixedDeltaTime;    // FixedUpdate에서 사용

// ❌ Bad: 프레임 기반 (FPS에 따라 다르게 동작)
_timer -= 1;  // 프레임마다 1씩 감소 - 위험!
```

**Rules:**
- `Update()`: Input handling, animation, UI → Use `Time.deltaTime`
- `FixedUpdate()`: Physics, Rigidbody manipulation → Use `Time.fixedDeltaTime`
- Animation: Use "duration in seconds" instead of FPS

---

## Project Structure

> **Note**: 아래는 권장 기본 구조다. 2D/3D 여부와 프로젝트 규모에 따라 불필요한 폴더는 빼고, 필요한 폴더만 추가해서 사용한다. 현재 `Assets/`에는 `Art/`(`Sprites/`, `Shaders/`), `Editor/`, `Fonts/`, `Prefabs/`, `Scenes/`, `ScriptableObjects/`, `Scripts/`(`Core/`, `Data/`, `Location/`, `Map/`, `Capture/`, `UI/`), `Settings/`, `Tests/`, `TextMesh Pro/`가 있다.

```text
Assets/
├── Animations/                # 애니메이션 클립 및 컨트롤러
│   └── {Category}/            # 대상별 하위 폴더 (예: Player/, Enemies/)
│
├── Art/                       # 아트 리소스
│   ├── Sprites/               # 스프라이트 (2D)
│   ├── Models/                # 모델/머티리얼 (3D)
│   └── Tilemaps/              # 타일맵 관련 리소스 (2D)
│
├── Audio/                     # 오디오 리소스
│   ├── BGM/                   # 배경 음악
│   └── SFX/                   # 효과음
│
├── Editor/                    # 에디터 전용 유틸리티
│
├── Fonts/                     # 프로젝트 폰트 리소스 (한글 폰트, TMP 자산 등)
│
├── Prefabs/                   # 씬에서 재사용하는 프리팹
│   ├── Characters/            # 플레이어/적 캐릭터
│   ├── Environment/           # 환경 오브젝트
│   ├── Items/                 # 드랍/상호작용 아이템
│   └── UI/                    # UI 관련 프리팹
│
├── Resources/                 # 런타임 로드 대상 리소스 (최소한으로 유지)
│
├── Scenes/                    # 주요 게임 씬
│   ├── BootScene.unity        # (예시) 초기 부트스트랩
│   ├── MainMenuScene.unity    # (예시) 메인 메뉴
│   └── GameScene.unity        # (예시) 메인 게임플레이
│
├── ScriptableObjects/         # 게임 데이터 에셋
│   └── {Category}/            # 데이터 종류별 하위 폴더 (예: Items/, Enemies/, Upgrades/)
│
├── Scripts/                   # 런타임 코드
│   ├── Core/                  # 공통 핵심 시스템
│   │   ├── Events/            # 이벤트 관련 코드
│   │   ├── Input/             # 입력 처리
│   │   ├── Interfaces/        # 공용 인터페이스
│   │   └── Runtime/           # 공용 런타임 헬퍼
│   ├── Data/                  # ScriptableObject/데이터 바인딩 코드
│   ├── {Feature}/             # 도메인별 게임플레이 로직 (예: Combat/, Stage/, Shop/)
│   └── UI/                    # HUD 및 화면 UI 코드
│
├── Settings/                  # 렌더링(URP 등) 및 관련 설정 에셋
│
└── Tests/                     # 테스트 코드
    └── EditMode/              # EditMode 단위 테스트

Docs/                          # 프로젝트 문서
Plans/                         # 마일스톤/기획 문서
Packages/                      # Unity 패키지 설정 및 로컬 패키지
ProjectSettings/               # Unity 프로젝트 설정
```

> **Note**: 신규 파일은 가능한 한 기존 축(`Core`, `Data`, `{Feature}`, `UI`) 안에 배치하고, 새 최상위 폴더 추가는 구조상 꼭 필요할 때만 한다.

---

## Naming Conventions

### Files and Folders

| Type | Convention | Example |
|------|------------|---------|
| Folders | PascalCase | `Scripts/Player/` |
| C# Scripts | PascalCase | `PlayerController.cs` |
| Prefabs | PascalCase | `PlayerCharacter.prefab` |
| Scenes | PascalCase_Underscore | `Level_01_Forest.unity` |
| ScriptableObjects | PascalCase | `EnemyData.asset` |

### Code

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `GameManager` |
| Public Methods | PascalCase | `TakeDamage()` |
| Private Methods | PascalCase | `CalculateDistance()` |
| Public Fields | camelCase | `public float moveSpeed` |
| Private Fields | _camelCase | `private int _currentHealth` |
| Constants | UPPER_SNAKE | `public const int MAX_HEALTH = 100` |
| Enums | PascalCase | `GameState.Playing` |
| Interfaces | IPascalCase | `IDamageable` |

---

## Project-Specific Systems

씬은 `Assets/Scenes/GameScene.unity` 하나이고, **메뉴 `Treasure/게임 씬 다시 만들기`**(코드 진입점 `Treasure.EditorTools.GameSceneBuilder.BuildAll()`)가 매번 새로 만든다. 씬을 직접 고치지 말고 `Assets/Editor/` 빌더(`HudBuilder`, `CaptureUIBuilder`, `MenuScreensBuilder`, `DebugUIBuilder`, `UIFactory`)를 고친 뒤 다시 만든다. 데이터·아트·프리팹·폰트 에셋은 **없을 때만** 만들므로 에셋에서 직접 수정한 값과 교체한 PNG는 유지된다.

|영역|클래스|역할|
|---|---|---|
|설정/데이터|`GameConfig`, `SpotData`, `CharacterData`, `ShopItemData` (`Assets/ScriptableObjects/`)|스팟 좌표·반경·퍼즐 조각, 캐릭터 난이도, 아이템, 코인/쿨다운 값|
|진행/저장|`GameManager`(실행 순서 -100), `PlayerProgress`, `SaveSystem`|`PlayerProgress.Changed`마다 PlayerPrefs(`dongseongno_save_v1`)에 JSON 저장|
|스팟 규칙|`SpotRules.Evaluate`|Cleared > Locked(최종 스팟·퍼즐 미완성) > Cooldown > Capturable(반경 안) > OutOfRange|
|위치|`LocationProvider`, `GeoUtil`, `DebugSettings`|기기는 GPS/시뮬레이션 즉시 전환(설정은 PlayerPrefs), 에디터는 항상 시뮬레이션(WASD/방향키, Shift 5배, 화면 조이스틱). 방향은 나침반 우선, 없으면 시뮬레이션 이동 방향. 좌표는 동성로 광장 원점 기준 로컬 미터|
|지도|`MapView`, `VectorMapLoader`, `VectorTileMeshBuilder`, `MvtReader`, `VectorMapStyle`, `SpotMarker`|에디터·기기 모두 OpenFreeMap 벡터 타일(z14, 보이는 범위만, 30일 캐시 `persistentDataPath/VectorTiles`)을 받아 도로·건물·공원·물만 단색 메시로 그린다(셰이더 `Assets/Art/Shaders/MapFlatColor.shader`). 글자·POI·지번은 그리지 않는다. 해석·삼각분할은 백그라운드 스레드. 핀치/휠 줌, 드래그하면 따라가기 해제(“내 위치”로 복귀), 북쪽 고정/방향 따라 회전|
|디버그|`DebugPanel`, `VirtualJoystick`|개발 빌드·에디터에서만 “설정” 버튼 표시. 위치 모드, 따라가기, 지도 회전, 쿨다운/저장 초기화, 상태 정보|
|포획|`CaptureFlow` → `CameraScreen` → `CaptureActionScreen`(`TapChaseGame`)|성공: 코인·스팟 고정 조각·도감 카드 + 60초 대기. 실패/포기: 180초 대기|
|화면 전환|`ScreenRouter`, `Popup`|`Show(ScreenId)`, 팝업 스택, 뒤로가기(Escape = Android 뒤로 키)|

- 버튼 리스너는 각 화면 스크립트의 `Awake`에서 코드로 연결한다(영구 리스너 사용 안 함).
- 저장 초기화: 메뉴 `Treasure/저장 데이터 초기화`(플레이 모드 밖에서).
- 지도 출처 표기(`OpenFreeMap © OpenMapTiles Data from OpenStreetMap`)는 필수다. 타일 주소는 실행마다 `GameConfig.vectorTileJsonUrl`(TileJSON)의 `tiles[0]`으로 다시 얻는다. 다른 OpenMapTiles 스키마 서버로 바꾸려면 이 값만 바꾼다.
- 지도 색·그리는 순서는 `VectorMapStyle`, 무엇을 그릴지(레이어·도로 폭)는 `VectorTileMeshBuilder`의 분류에서 정한다.

---

## Do's and Don'ts

### ✅ Do

- Write clear, simple code
- Add Korean comments explaining "why", not "what"
- Use meaningful variable names
- Keep methods focused and short
- Use Unity's built-in systems (Input System, Cinemachine, etc.)
- Use `Time.deltaTime` / `Time.fixedDeltaTime` for all time-based logic
- Use time (seconds) instead of frames for animation/timing

### ❌ Don't

- Write XML documentation comments
- Create complex inheritance chains
- Use English comments
- Over-engineer solutions
- Create "clever" code that's hard to understand
- Use magic numbers without comments
- Use frame-based timing (프레임 기반 타이밍 사용 금지)
- Use emojis in Editor UI (에디터 UI에 이모지 사용 금지)
- GitHub 저장소에 AI 에이전트(claude 등)를 공동작업자(collaborator)로 추가하지 말 것
- 커밋 메시지에 `Co-Authored-By` 등 AI 작성자 트레일러를 추가하지 말 것

---

## Git Commit Convention

```
<type>: <description in Korean>
```

### Types

| Type | Description | Example |
|------|-------------|---------|
| `feat` | New feature | `feat: 플레이어 대시 구현` |
| `fix` | Bug fix | `fix: 적 벽 통과 버그 수정` |
| `docs` | Documentation | `docs: 입력시스템 가이드 업데이트` |
| `refactor` | Code refactoring | `refactor: 플레이어 코드 정리` |
| `style` | Code style (formatting) | `style: 들여쓰기 수정` |
| `chore` | Build, config, etc. | `chore: .gitignore 업데이트` |
| `art` | Art resources | `art: 플레이어 스프라이트 추가` |

> Type is in English, description is in Korean.
