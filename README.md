# PublicForToolCode

포트폴리오 작성용 Public Git.
* 주의: 유료 에셋 등 몇몇 이유로 인해 해당 cs 만 게재하여 컴파일 에러가 존재함

---

## 목차

- [소개](#소개)
- [기능](#기능)
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
  - completed 콜백에서 네트워크/프로토콜 에러를 체크하고 로그 출력, 응답 텍스트를 읽어 "오류" 문자열 포함 여부에 따라 성공/실패 로그 출력
  - request.Dispose()로 네트워크 리소스를 정리
 
- IsDataValid()
  - 에디터에서 입력된 데이터 검증
    - ex) m_UnitName, m_RCode가 비어있지 않은지
          Rank 값이 Enum 범위 안인지
          공격력/공속/사거리/이속이 음수는 아닌지
          스킬 사용 시 SkillType이 None이 아닌지, AttackSO/SkillAttackSO가 null이 아닌지
  - 조건을 통과하면 true, 아니면 false를 반환

- CreateUnitPrefab()
  - Create 탭에서 Create 버튼을 누르면 실행
    - 선택된 모델 프리팹이 있는지 검사
    - m_RCode 기반 경로(Assets/03. Prefabs/Character/<rcode>.prefab)에 기존 프리팹이 있는지 확인
    - 여부에 따라 삭제 여부를 다이얼로그로 물어보고, 삭제하거나 Edit 탭으로 전환
    - 새 GameObject를 만들고 이름을 RCode로 설정
    - NavMeshAgent, CapsuleCollider, Unit, UnitController 컴포넌트 추가
    - 리플렉션으로 Unit.Data 프로퍼티에 Character 구조체 데이터 기입
    - Canvas 프리팹을 인스턴스화해서 자식으로 추가, UnitCommandUI의 selectedUI를 연결
    - Pivot 오브젝트를 만들고 그 아래에 선택된 모델 프리팹 인스턴스를 추가
    - 스킬 사용 시, Unit.AttackSO, Unit.SkillAttackSO 프로퍼티를 리플렉션으로 설정
    - PrefabUtility.SaveAsPrefabAsset으로 최종 GameObject를 프리팹으로 저장 후, 씬 상의 임시 오브젝트 삭제
    - RegisterUnit() 호출로 서버에도 데이터 등록
   
- EditUnitPrefab()
  - Edit 탭에서 Edit 버튼을 누르면 실행되는 수정 로직
    - 현재 선택된 유닛 프리팹(m_SelectedPrefab) 경로를 통해 프리팹을 다시 로드
    - Unit 컴포넌트를 찾지 못하면 에러 출력
    - 리플렉션으로 Unit.Data에 수정된 Character 데이터를 다시 설정
    - 스킬 사용 중이라면 AttackSO, SkillAttackSO 프로퍼티도 리플렉션으로 갱신
    - PrefabUtility.SavePrefabAsset으로 프리팹을 덮어쓰기
    - RegisterUnit()으로 서버 데이터 갱신

- UpdateRCode()
  - 현재 상태를 바탕으로 유닛의 고유 코드 m_RCode를 계산
    - skillCode = 스킬 사용 여부에 따라 1 또는 0
    - ComputeStableHash(m_UnitName)로 이름 기반 해시를 얻고, 0~9999 범위로 축약
    - un{rank숫자}{skillCode}{4자리 해시} 형태 문자열을 생성
  - Create 모드에서는 GUI 값이 바뀔 때마다 자동으로 호출

- ComputeStableHash(string p_Text)
  - 문자열에 대해 환경에 관계없이 항상 같은 결과를 내는 해시 함수
    - 초기 값 23에서 시작해 각 문자마다 hash = hash * 31 + char 방식으로 누적
  - string.GetHashCode() 대신 사용해서, 에디터/플랫폼/세션이 달라도 동일한 이름 → 동일한 숫자를 보장

---

## 기술 스택

- C#
- .NET BCL
- UnityEditor
- UnityEngine.Networking
- EditorWindow
- IMGUI
- Reflection
- UnityWebRequest

---
