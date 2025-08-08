import cv2
import mediapipe as mp
import json

mp_pose = mp.solutions.pose
mp_hands = mp.solutions.hands

cap = cv2.VideoCapture('VXPAKOKS240779230.mp4')

pose = mp_pose.Pose(static_image_mode=False, model_complexity=2, enable_segmentation=False, min_detection_confidence=0.5)
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.5)

# 이름 테이블
hand_names = [
    "WRIST",
    "THUMB_CMC", "THUMB_MCP", "THUMB_IP", "THUMB_TIP",
    "INDEX_FINGER_MCP", "INDEX_FINGER_PIP", "INDEX_FINGER_DIP", "INDEX_FINGER_TIP",
    "MIDDLE_FINGER_MCP", "MIDDLE_FINGER_PIP", "MIDDLE_FINGER_DIP", "MIDDLE_FINGER_TIP",
    "RING_FINGER_MCP", "RING_FINGER_PIP", "RING_FINGER_DIP", "RING_FINGER_TIP",
    "PINKY_MCP", "PINKY_PIP", "PINKY_DIP", "PINKY_TIP"
]
pose_names = [
    "NOSE", "LEFT_EYE_INNER", "LEFT_EYE", "LEFT_EYE_OUTER",
    "RIGHT_EYE_INNER", "RIGHT_EYE", "RIGHT_EYE_OUTER",
    "LEFT_EAR", "RIGHT_EAR",
    "MOUTH_LEFT", "MOUTH_RIGHT",
    "LEFT_SHOULDER", "RIGHT_SHOULDER", "LEFT_ELBOW", "RIGHT_ELBOW",
    "LEFT_WRIST", "RIGHT_WRIST", "LEFT_PINKY", "RIGHT_PINKY",
    "LEFT_INDEX", "RIGHT_INDEX", "LEFT_THUMB", "RIGHT_THUMB",
    "LEFT_HIP", "RIGHT_HIP", "LEFT_KNEE", "RIGHT_KNEE",
    "LEFT_ANKLE", "RIGHT_ANKLE", "LEFT_HEEL", "RIGHT_HEEL",
    "LEFT_FOOT_INDEX", "RIGHT_FOOT_INDEX"
]

result_frames = []

while True:
    ret, frame = cap.read()
    if not ret:
        break

    img_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    pose_result = pose.process(img_rgb)
    hands_result = hands.process(img_rgb)

    frame_data = {
        "pose": [],
        "left_hand": [],
        "right_hand": []
    }

    # pose (name 붙이기)
    if pose_result.pose_world_landmarks:
        for idx, lm in enumerate(pose_result.pose_world_landmarks.landmark):
            frame_data["pose"].append({
                "name": pose_names[idx],
                "x": lm.x,
                "y": lm.y,
                "z": lm.z,
                "visibility": pose_result.pose_landmarks.landmark[idx].visibility if pose_result.pose_landmarks else 1.0
            })

    # hands (name 붙이기)
    if hands_result.multi_hand_world_landmarks and hands_result.multi_handedness:
        for hand_idx, hand_landmarks in enumerate(hands_result.multi_hand_world_landmarks):
            handedness = hands_result.multi_handedness[hand_idx].classification[0].label  # 'Left' or 'Right'
            key = "left_hand" if handedness == 'Left' else "right_hand"
            for idx, lm in enumerate(hand_landmarks.landmark):
                frame_data[key].append({
                    "name": hand_names[idx],
                    "x": lm.x,
                    "y": lm.y,
                    "z": lm.z
                })

    result_frames.append(frame_data)

    if len(result_frames) % 10 == 0:
        print(f"{len(result_frames)} frames processed...")

with open('pose_hand_world_landmarks_with_name.json', 'w') as f:
    json.dump(result_frames, f, indent=2)

cap.release()
print("완료! pose_hand_world_landmarks_with_name.json 파일 생성됨.")
