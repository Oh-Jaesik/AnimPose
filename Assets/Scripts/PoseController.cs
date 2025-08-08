using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public class Landmark
{
    public string name;
    public float x, y, z;
    public float visibility;
}

[Serializable]
public class PoseFrame
{
    public Landmark[] left_hand;
    public Landmark[] right_hand;
    public Landmark[] pose;
}

public class PoseController : MonoBehaviour
{
    public TextAsset poseJson;
    public Animator animator;
    private PoseFrame[] frames;
    private int currentFrame = 0;

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

    private float frameTimer = 0f;
    private float frameInterval = 1f / 30f;  // 30 FPS

    void Start()
    {
        if (poseJson == null)
        {
            Debug.LogError("poseJson에 JSON 파일을 연결하세요!");
            return;
        }
        try
        {
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

        // 본 매핑 확인용 로그 추가
        foreach (var kvp in leftHandBoneMap)
        {
            var boneTrans = animator.GetBoneTransform(kvp.Value);
            if (boneTrans == null)
                Debug.LogWarning($"Animator에서 본을 찾지 못함: {kvp.Key} ({kvp.Value})");
            else
                Debug.Log($"Animator 본 확인됨: {kvp.Key} ({kvp.Value}) - Transform 이름: {boneTrans.name}");
        }
    }

    void Update()
    {
        if (frames != null && frames.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameInterval)
            {
                frameTimer -= frameInterval;

                if (currentFrame < frames.Length - 1)
                {
                    currentFrame++;
                    Debug.Log("현재 프레임: " + currentFrame);
                    ApplyFrameToBones(frames[currentFrame]);
                }
            }
        }
    }

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

            string nextName = GetNextLandmarkName(lm.name);
            if (nextName == null)
            {
                // TIP 같은 끝 마디라서 다음 마디 없으면 그냥 넘어가기 (경고 로그 생략)
                continue;
            }

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

            Vector3 cur = new Vector3(-lm.x, lm.y, -lm.z);
            Vector3 next = new Vector3(-nextLm.x, nextLm.y, -nextLm.z);

            Vector3 dir = (next - cur).normalized;
            Vector3 dirLocal = parent.InverseTransformDirection(dir);

            Quaternion rotation = Quaternion.identity;
            if (lm.name.StartsWith("THUMB"))
                rotation = Quaternion.FromToRotation(Vector3.right, dirLocal);
            else if (lm.name.StartsWith("INDEX"))
                rotation = Quaternion.FromToRotation(Vector3.forward, dirLocal);
            else
                rotation = Quaternion.FromToRotation(Vector3.forward, dirLocal);

            bone.localRotation = rotation;

            Debug.Log($"{lm.name} 본 회전 적용: {rotation.eulerAngles}");
        }
    }

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
