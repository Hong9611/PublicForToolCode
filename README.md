# PublicForToolCode

포트폴리오 작성용 Public Git.
* 주의: 유료 에셋 등 몇몇 이유로 인해 해당 cs 만 게재하여 컴파일 에러가 존재함

---

## 목차

- [소개](#소개)
- [기능](#기능)
- [사용법](#사용법)
- [기술 스택](#기술-스택)

---

## 소개

3D 캐주얼 랜덤 디펜스 게임에서 Unit 제작의 편의성을 위해 개발

---

## 기능

- ShowWindow(): Unity 상단 메뉴 Tools/Unit Tool 클릭 시 에디터 윈도우를 띄우는 정적 메서드
- LoadAvailablePrefabs(): Assets/06. Assets/PolygonMiniFantasyCharacters/Prefabs/Characters 폴더에서 캐릭터 모델 프리팹들을 찾아서 m_AvailablePrefabs 리스트에 추가
- LoadAttackSOs(): Assets/01. Scripts/Attack/AttackSOs 폴더 안에서 AttackSO 타입 ScriptableObject를 모두 찾아 m_AttackSOs 리스트에 추가
- LoadCreatedPrefabs(): Assets/03. Prefabs/Character 폴더 안에서 .prefab 파일들을 검사해 Unit 컴포넌트를 가진 프리팹만 m_CreatedPrefabs 리스트에 등록, Edit 탭에서 수정할 수 있는 대상 목록을 구성
- OnGUI(): 상단 탭을 그린 뒤, 현재 선택된 탭에 따라 DrawCreateUnitGUI() 또는 DrawEditUnitGUI()를 호출
- DrawTabs(): 상단의 Create Unit Prefab / Edit Unit Prefab 두 개의 토글 버튼(툴바 스타일)을 그린 뒤, 선택된 탭을 m_SelectedTab에 반영
- DrawCreateUnitGUI(): 유닛 생성(Create) 탭의 UI 구성
    - 유닛 이름, 모델 프리팹 선택, Rank, 공격력/속도/사거리/이동 속도 입력
    - 스킬 사용 여부, 스킬 타입, AttackSO/SkillAttackSO 선택 및 자동 매핑
  - 값이 변경되면 UpdateRCode()를 호출해 RCode를 다시 계산
  - Create 버튼 클릭 시 CreateUnitPrefab()을 호출하고, 이후 생성된 프리팹 목록을 갱신
  - 현재 RCode를 화면에 표시
- 

---

## 사용법

---

## 기술 스택

---
