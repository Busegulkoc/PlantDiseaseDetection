from fastapi import FastAPI, File, UploadFile
import torch
from ultralytics import YOLO
from io import BytesIO
from PIL import Image
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI()
model = YOLO("best.pt")  # Model dosyanın adını buraya yaz

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Tüm kaynaklara izin ver
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/predict")
async def predict(file: UploadFile = File(...)):
    try:
        image = Image.open(BytesIO(await file.read()))
        results = model.predict(image, imgsz=640)

        # For classification models, results[0].probs contains class probabilities
        # For detection models, results[0].boxes contains detected boxes
        # We'll try to get class probabilities if available, else fallback to detected boxes

        # Try to get class probabilities (for classification models)
        if hasattr(results[0], "probs") and results[0].probs is not None:
            # Get class names and probabilities
            class_names = results[0].names
            probs = results[0].probs.data.tolist()
            response = {
                "predictions": [
                    f"{name}:  {prob:.2f}" for i, (name, prob) in enumerate(zip(class_names, probs))
                ]
            }
            return response

        # If detection model, return detected classes with their confidence, others as 0.00
        class_names = results[0].names
        num_classes = len(class_names)
        scores = [0.0] * num_classes

        if results[0].boxes is not None and results[0].boxes.cls is not None:
            for cls, conf in zip(results[0].boxes.cls, results[0].boxes.conf):
                idx = int(cls)
                if conf > scores[idx]:
                    scores[idx] = float(conf)

        response = {
            "predictions": [
                f"{name}: {score:.2f}" for i, (name, score) in enumerate(zip(class_names, scores))
            ]
        }
        return response

    except Exception as e:
        return {"error": str(e)}
