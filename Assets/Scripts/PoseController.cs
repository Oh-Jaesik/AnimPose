using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 단일 랜드마크(관절 위치) 정보를 담는 클래스
/// </summary>
[Serializable]
public class Landmark
{
    public string name;      // 랜드마크 이름 (예: "WRIST", "THUMB_MCP" 등)
    public float x, y, z;    // 3D 위치 좌표
    public float visibility; // 가시성(확률 또는 신뢰도)
}

/// <summary>
/// 한 프레임의 포즈 데이터 (왼손, 오른손, 전체 포즈)
/// </summary>
[Serializable]
public class PoseFrame
{
    public Landmark[] left_hand;  // 왼손 랜드마크 배열
    public Landmark[] right_hand; // 오른손 랜드마크 배열
    public Landmark[] pose;       // 전체 포즈 랜드마크 배열 (필요 시)
}

/// <summary>
/// PoseFrame 배열을 JSON으로부터 읽어와  
/// 애니메이터의 손 본들에 포즈 데이터를 적용하는 클래스  
/// </summary>
public class PoseController : MonoBehaviour
{
    [Header("JSON 데이터")]
    public TextAsset poseJson;        // 포즈 JSON 데이터 텍스트 에셋

    [Header("애니메이터")]
    public Animator animator;         // 손 본들을 제어할 Animator 컴포넌트

    private PoseFrame[] frames;       // 파싱된 전체 포즈 프레임 배열
    private int currentFrame = 0;     // 현재 재생 중인 프레임 인덱스

    // 왼손 랜드마크 이름과 애니메이터 HumanBodyBones 매핑 사전
    private Dictionary<string, HumanBodyBones> leftHandBoneMap = new Dictionary<string, HumanBodyBones>()
    {
        { "WRIST", HumanBodyBones.LeftHand },
        { "THUMB_CMC", HumanBodyBones.LeftThumbProximal },
        { "THUMB_MCP", HumanBodyBones.LeftThumbIntermediate },
        { "THUMB_IP", HumanBodyBones.LeftThumbDistal },
        { "THUMB_TIP", HumanBodyBones.LeftThumbDistal },

        { "INDEX_FINGER_MCP", HumanBodyBones.LeftIndexProximal },
        { "INDEX_FINGER_PIP", HumanBodyBones.LeftIndexIntermediate },
        { "INDEX_FINGER_DIP", HumanBodyBones.LeftIndexDistal },
        { "INDEX_FINGER_TIP", HumanBodyBones.LeftIndexDistal },

        { "MIDDLE_FINGER_MCP", HumanBodyBones.LeftMiddleProximal },
        { "MIDDLE_FINGER_PIP", HumanBodyBones.LeftMiddleIntermediate },
        { "MIDDLE_FINGER_DIP", HumanBodyBones.LeftMiddleDistal },
        { "MIDDLE_FINGER_TIP", HumanBodyBones.LeftMiddleDistal },

        { "RING_FINGER_MCP", HumanBodyBones.LeftRingProximal },
        { "RING_FINGER_PIP", HumanBodyBones.LeftRingIntermediate },
        { "RING_FINGER_DIP", HumanBodyBones.LeftRingDistal },
        { "RING_FINGER_TIP", HumanBodyBones.LeftRingDistal },

        { "PINKY_MCP", HumanBodyBones.LeftLittleProximal },
        { "PINKY_PIP", HumanBodyBones.LeftLittleIntermediate },
        { "PINKY_DIP", HumanBodyBones.LeftLittleDistal },
        { "PINKY_TIP", HumanBodyBones.LeftLittleDistal }
    };

    // 프레임 재생 타이머
    private float frameTimer = 0f;

    // 프레임 간격 (초) - 30fps 기준 (1/30초)
    private float frameInterval = 1f / 30f;

    /// <summary>
    /// 초기화: JSON 데이터 파싱 및 애니메이터 본 매핑 검사
    /// </summary>
    void Start()
    {
        if (poseJson == null)
        {
            Debug.LogError("poseJson에 JSON 파일을 연결하세요!");
            return;
        }
        try
        {
            // JsonHelper를 사용해 JSON 배열 파싱 (JsonHelper는 별도 구현 필요)
            frames = JsonHelper.FromJson<PoseFrame>(poseJson.text);
            Debug.Log("프레임 개수: " + frames.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("JSON 파싱에 실패: " + ex.Message);
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator 컴포넌트를 Inspector에 연결하세요!");
            return;
        }

        // 애니메이터에서 본 매핑 여부 로그 출력 (디버그용)
        foreach (var kvp in leftHandBoneMap)
        {
            var boneTrans = animator.GetBoneTransform(kvp.Value);
            if (boneTrans == null)
                Debug.LogWarning($"Animator에서 본을 찾지 못함: {kvp.Key} ({kvp.Value})");
            else
                Debug.Log($"Animator 본 확인됨: {kvp.Key} ({kvp.Value}) - Transform 이름: {boneTrans.name}");
        }
    }

    /// <summary>
    /// 매 프레임마다 30fps 기준으로 포즈 프레임을 순차 재생한다.
    /// </summary>
    void Update()
    {
        if (frames != null && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameInterval)
            {
                frameTimer -= frameInterval;

                // 마지막 프레임까지 재생 (순환 없음)
                if (currentFrame < frames.Length - 1)
                {
                    currentFrame++;
                    Debug.Log("현재 프레임: " + currentFrame);
                    ApplyFrameToBones(frames[currentFrame]);
                }
            }
        }
    }

    /// <summary>
    /// 현재 프레임의 왼손 랜드마크 데이터를 애니메이터 본에 적용하여 손 관절 회전 변경
    /// </summary>
    /// <param name="frame">적용할 포즈 프레임 데이터</param>
    void ApplyFrameToBones(PoseFrame frame)
    {
        if (animator == null || frame.left_hand == null)
        {
            Debug.LogWarning("Animator가 없거나 왼손 데이터가 없습니다.");
            return;
        }

        Debug.Log("ApplyFrameToBones 호출됨");

        foreach (var lm in frame.left_hand)
        {
            if (!leftHandBoneMap.ContainsKey(lm.name))
            {
                Debug.Log($"{lm.name} 매핑 없음");
                continue;
            }

            HumanBodyBones boneType = leftHandBoneMap[lm.name];
            var bone = animator.GetBoneTransform(boneType);
            if (bone == null)
            {
                Debug.LogWarning($"{lm.name}에 해당하는 본을 찾지 못했습니다.");
                continue;
            }
            Transform parent = bone.parent;
            if (parent == null) continue;

            // 현재 랜드마크 다음에 연결될 랜드마크 이름을 얻음
            string nextName = GetNextLandmarkName(lm.name);
            if (nextName == null)
            {
                // TIP 같은 끝 마디면 다음 마디 없으므로 무시
                continue;
            }

            // 다음 랜드마크를 frame.left_hand에서 찾음
            Landmark nextLm = null;
            foreach (var lm2 in frame.left_hand)
            {
                if (lm2.name == nextName)
                {
                    nextLm = lm2;
                    break;
                }
            }

            if (nextLm == null)
            {
                Debug.LogWarning($"{lm.name} 다음 랜드마크({nextName})를 찾지 못했습니다.");
                continue;
            }

            // 좌표계 변환 (좌표축 반전 포함)
            Vector3 cur = new Vector3(-lm.x, lm.y, -lm.z);
            Vector3 next = new Vector3(-nextLm.x, nextLm.y, -nextLm.z);

            // 현재 랜드마크에서 다음 랜드마크 방향 벡터 계산
            Vector3 dir = (next - cur).normalized;
            Vector3 dirLocal = parent.InverseTransformDirection(dir);

            Quaternion rotation = Quaternion.identity;

            // 엄지손가락과 나머지 손가락 회전 기준 분기 처리
            if (lm.name.StartsWith("THUMB"))
                rotation = Quaternion.FromToRotation(Vector3.right, dirLocal);
            else if (lm.name.StartsWith("INDEX"))
                rotation = Quaternion.FromToRotation(Vector3.forward, dirLocal);
            else
                rotation = Quaternion.FromToRotation(Vector3.forward, dirLocal);

            // 본에 로컬 회전값 적용
            bone.localRotation = rotation;

            Debug.Log($"{lm.name} 본 회전 적용: {rotation.eulerAngles}");
        }
    }

    /// <summary>
    /// 현재 랜드마크 이름을 기준으로 다음 랜드마크 이름을 반환한다.
    /// (ex: MCP -> PIP, PIP -> DIP, DIP -> TIP)
    /// </summary>
    /// <param name="name">현재 랜드마크 이름</param>
    /// <returns>다음 랜드마크 이름 또는 null(끝 마디)</returns>
    string GetNextLandmarkName(string name)
    {
        if (name == "WRIST") return "THUMB_CMC";  // WRIST 다음은 엄지 첫 마디로 임시 지정

        if (name == "THUMB_CMC") return "THUMB_MCP";
        if (name == "THUMB_MCP") return "THUMB_IP";
        if (name == "THUMB_IP") return "THUMB_TIP";

        if (name.Contains("MCP")) return name.Replace("MCP", "PIP");
        if (name.Contains("PIP")) return name.Replace("PIP", "DIP");
        if (name.Contains("DIP")) return name.Replace("DIP", "TIP");

        if (name.Contains("TIP")) return null;

        return null;
    }

    /// <summary>
    /// 씬 뷰에서 현재 프레임 왼손 랜드마크 위치를 빨간색 구체로 시각화
    /// </summary>
    void OnDrawGizmos()
    {
        if (frames == null || frames.Length == 0) return;
        PoseFrame frame = frames[currentFrame];

        if (frame.left_hand != null)
        {
            Gizmos.color = Color.red;
            foreach (var lm in frame.left_hand)
            {
                Gizmos.DrawSphere(new Vector3(lm.x, lm.y, -lm.z), 0.01f);
            }
        }
    }
}
