import json

LEFT_HAND_MAPPING = {
    "mixamorig:LeftHand": "WRIST",
    "mixamorig:LeftHandThumb1": "THUMB_CMC",
    "mixamorig:LeftHandThumb2": "THUMB_MCP",
    "mixamorig:LeftHandThumb3": "THUMB_IP",
    # "mixamorig:LeftHandThumb4": "THUMB_TIP",  # 본 없음

    "mixamorig:LeftHandIndex1": "INDEX_FINGER_MCP",
    "mixamorig:LeftHandIndex2": "INDEX_FINGER_PIP",
    "mixamorig:LeftHandIndex3": "INDEX_FINGER_DIP",
    # "mixamorig:LeftHandIndex4": "INDEX_FINGER_TIP",  # 본 없음

    "mixamorig:LeftHandMiddle1": "MIDDLE_FINGER_MCP",
    "mixamorig:LeftHandMiddle2": "MIDDLE_FINGER_PIP",
    "mixamorig:LeftHandMiddle3": "MIDDLE_FINGER_DIP",

    "mixamorig:LeftHandRing1": "RING_FINGER_MCP",
    "mixamorig:LeftHandRing2": "RING_FINGER_PIP",
    "mixamorig:LeftHandRing3": "RING_FINGER_DIP",

    "mixamorig:LeftHandPinky1": "PINKY_MCP",
    "mixamorig:LeftHandPinky2": "PINKY_PIP",
    "mixamorig:LeftHandPinky3": "PINKY_DIP",
}

# 오른손도 동일하게 TIP 계열은 매핑 제외!
RIGHT_HAND_MAPPING = {
    "mixamorig:RightHand": "WRIST",
    "mixamorig:RightHandThumb1": "THUMB_CMC",
    "mixamorig:RightHandThumb2": "THUMB_MCP",
    "mixamorig:RightHandThumb3": "THUMB_IP",
    # "mixamorig:RightHandThumb4": "THUMB_TIP",

    "mixamorig:RightHandIndex1": "INDEX_FINGER_MCP",
    "mixamorig:RightHandIndex2": "INDEX_FINGER_PIP",
    "mixamorig:RightHandIndex3": "INDEX_FINGER_DIP",

    "mixamorig:RightHandMiddle1": "MIDDLE_FINGER_MCP",
    "mixamorig:RightHandMiddle2": "MIDDLE_FINGER_PIP",
    "mixamorig:RightHandMiddle3": "MIDDLE_FINGER_DIP",

    "mixamorig:RightHandRing1": "RING_FINGER_MCP",
    "mixamorig:RightHandRing2": "RING_FINGER_PIP",
    "mixamorig:RightHandRing3": "RING_FINGER_DIP",

    "mixamorig:RightHandPinky1": "PINKY_MCP",
    "mixamorig:RightHandPinky2": "PINKY_PIP",
    "mixamorig:RightHandPinky3": "PINKY_DIP",
}

POSE_MAPPING = {
    "mixamorig:Head": "NOSE",
    "LeftEye": "LEFT_EYE",
    "LeftEyeInner": "LEFT_EYE_INNER",
    "LeftEyeOuter": "LEFT_EYE_OUTER",
    "RightEye": "RIGHT_EYE",
    "RightEyeInner": "RIGHT_EYE_INNER",
    "RightEyeOuter": "RIGHT_EYE_OUTER",
    "LeftEar": "LEFT_EAR",
    "RightEar": "RIGHT_EAR",
    "MouthLeft": "MOUTH_LEFT",
    "MouthRight": "MOUTH_RIGHT",
    "mixamorig:LeftArm": "LEFT_SHOULDER",
    "mixamorig:RightArm": "RIGHT_SHOULDER",
    "mixamorig:LeftForeArm": "LEFT_ELBOW",
    "mixamorig:RightForeArm": "RIGHT_ELBOW",
    #"mixamorig:LeftHand": "LEFT_WRIST",
    #"mixamorig:RightHand": "RIGHT_WRIST"
    # 필요하면 더 추가
}

FBX_TO_JSON_MAP = {
    "left_hand": LEFT_HAND_MAPPING,
    "right_hand": RIGHT_HAND_MAPPING,
    "pose": POSE_MAPPING
}

with open("FBX_TO_JSON_MAP.json", "w", encoding="utf-8") as f:
    json.dump(FBX_TO_JSON_MAP, f, ensure_ascii=False, indent=2)

print("FBX_TO_JSON_MAP.json 파일이 생성되었습니다!")
