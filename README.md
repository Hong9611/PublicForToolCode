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

- ShowWindow()
    - Unity 상단 메뉴 Tools/Unit Tool 클릭 시 에디터 윈도우를 띄우는 정적 메서드

- LoadAvailablePrefabs()
    - Assets/06. Assets/PolygonMiniFantasyCharacters/Prefabs/Characters 폴더에서 캐릭터 모델 프리팹들을 찾아서 m_AvailablePrefabs 리스트에 추가

- LoadAttackSOs()
    - Assets/01. Scripts/Attack/AttackSOs 폴더 안에서 AttackSO 타입 ScriptableObject를 모두 찾아 m_AttackSOs 리스트에 추가

- LoadCreatedPrefabs()
    - Assets/03. Prefabs/Character 폴더 안에서 .prefab 파일들을 검사해 Unit 컴포넌트를 가진 프리팹만 m_CreatedPrefabs 리스트에 등록, Edit 탭에서 수정할 수 있는 대상 목록을 구성

- OnGUI()
    - 상단 탭을 그린 뒤, 현재 선택된 탭에 따라 DrawCreateUnitGUI() 또는 DrawEditUnitGUI()를 호출

- DrawTabs()
    - 상단의 Create Unit Prefab / Edit Unit Prefab 두 개의 토글 버튼(툴바 스타일)을 그린 뒤, 선택된 탭을 m_SelectedTab에 반영

- DrawCreateUnitGUI()
  - 유닛 생성(Create) 탭의 UI 구성
    - 유닛 이름, 모델 프리팹 선택, Rank, 공격력/속도/사거리/이동 속도 입력
    - 스킬 사용 여부, 스킬 타입, AttackSO/SkillAttackSO 선택 및 자동 매핑
  - 값이 변경되면 UpdateRCode()를 호출해 RCode를 다시 계산
  - Create 버튼 클릭 시 CreateUnitPrefab()을 호출하고, 이후 생성된 프리팹 목록을 갱신
  - 현재 RCode를 화면에 표시

- DrawEditUnitGUI()
  - 유닛 수정(Edit) 탭의 UI 구성
    - 상단에서 이미 생성된 유닛 프리팹 목록을 드롭다운으로 보여주고, Load 버튼으로 해당 프리팹 데이터를 로드
    - 프리팹이 로드되면 이름/스탯/스킬 여부/스킬 타입/AttackSO/SkillAttackSO를 수정할 수 있게 UI를 노출
  - Edit 버튼 클릭 시 EditUnitPrefab()을 호출
  - 로드된 유닛의 RCode를 표시

- LoadExistingPrefabInfo(GameObject p_ExistingPrefab)
  - 선택된 기존 유닛 프리팹에서 Unit 컴포넌트를 가져와, Character 데이터 (name, rank, atk, atkSpeed, atkRange, moveSpeed, rcode, skill, skillType)를 에디터 내부 필드로 복사
  - 리플렉션을 사용해 Unit의 AttackSO, SkillAttackSO 프로퍼티를 읽어와 에디터 필드에 복사
  - m_SelectedPrefab를 현재 프리팹으로 설정하고, Repaint()로 UI를 갱신

- RegisterUnit()
  - 현재 에디터 필드 값이 유효한지 IsDataValid()로 검증
  - 유효하다면 WWWForm을 만들어 rcode, name, rank, 스탯, useSkill, skillType 등을 폼 필드로 추가한 뒤, Post(form)을 호출해 Google Apps Script로 전송
 
- Post(WWWForm p_Form)
  - UnityWebRequest.Post(Url, p_Form)로 HTTP POST 요청을 생성하고 전송
  - completed 콜백에서 네트워크/프로토콜 에러를 체크하고 로그 출력, 응답 텍스트를 읽어 "오류" 문자열 포함 여부에 따라 성공/실패 로그 출력.
  - request.Dispose()로 네트워크 리소스를 정리
 
- IsDataValid()
  - 
---

## 사용법

---

## 기술 스택

---
