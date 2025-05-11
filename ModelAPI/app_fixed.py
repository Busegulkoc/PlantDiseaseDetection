from fastapi import FastAPI, File, UploadFile
import torch
from ultralytics import YOLO
from io import BytesIO
from PIL import Image
from fastapi.middleware.cors import CORSMiddleware
import numpy as np
import logging

# Configure logging
logging.basicConfig(level=logging.INFO, 
                    format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

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
async def predict(file: UploadFile = File(None), imageFile: UploadFile = File(None)):
    try:
        # Use either file or imageFile, whichever is provided
        upload_file = file if file is not None else imageFile
        
        if upload_file is None:
            return {"error": "No file provided. Please upload an image file using 'file' or 'imageFile' parameter.", "predictions": []}
        
        # Read the image file
        contents = await upload_file.read()
        
        # Open as PIL Image directly, ensuring it's in RGB mode
        try:
            image = Image.open(BytesIO(contents)).convert('RGB')
            logger.info(f"Successfully loaded image: {upload_file.filename}, size: {image.size}, mode: {image.mode}")
        except Exception as img_error:
            logger.error(f"Error loading image: {str(img_error)}")
            return {"error": f"Could not load image: {str(img_error)}", "predictions": []}
        
        # Perform prediction with explicit image format
        results = model.predict(source=image, imgsz=640, verbose=False)
        logger.info(f"Prediction completed with result type: {type(results)}")
        
        # Define a helper function to convert numpy arrays to Python native types
        def convert_numpy_to_python(obj):
            # Handle numpy arrays
            if isinstance(obj, np.ndarray):
                return obj.tolist()
            # Handle numpy scalar types
            elif np.isscalar(obj) and isinstance(obj, (np.number, np.bool_)):
                if isinstance(obj, (np.integer, np.int_, np.intc, np.intp, np.int8, np.int16, np.int32, np.int64)):
                    return int(obj)
                elif isinstance(obj, (np.floating, np.float_, np.float16, np.float32, np.float64)):
                    return float(obj)
                elif isinstance(obj, (np.bool_, np.bool8)):
                    return bool(obj)
                else:
                    return obj.item()  # Convert any other numpy scalar to Python scalar
            # Handle dictionaries
            elif isinstance(obj, dict):
                return {k: convert_numpy_to_python(v) for k, v in obj.items()}
            # Handle lists and tuples
            elif isinstance(obj, (list, tuple)):
                return [convert_numpy_to_python(i) for i in obj]
            # Handle objects with a __dict__ attribute (custom objects)
            elif hasattr(obj, "__dict__"):
                try:
                    return convert_numpy_to_python(obj.__dict__)
                except:
                    # If converting __dict__ fails, return a string representation
                    return str(obj)
            # Handle torch tensors
            elif hasattr(obj, "cpu") and hasattr(obj, "numpy"):  # Likely a torch tensor
                try:
                    return convert_numpy_to_python(obj.cpu().numpy())
                except:
                    return str(obj)
            # Return everything else as is
            else:
                return obj

        # Extract and process the YOLO results
        if not results or len(results) == 0:
            logger.warning("No results returned from model")
            return {"predictions": []}
            
        # Log the result structure to help debugging
        logger.info(f"Result structure: {dir(results[0])}")
        
        # For classification models, results[0].probs contains class probabilities
        if hasattr(results[0], "probs") and results[0].probs is not None:
            logger.info("Processing as classification model")
            
            try:
                # Get class names
                if hasattr(results[0], "names") and results[0].names:
                    class_names = convert_numpy_to_python(results[0].names)
                else:
                    logger.warning("No class names found, using indices")
                    class_names = {i: f"Class {i}" for i in range(10)}  # Fallback

                # Extract probabilities safely
                try:
                    if hasattr(results[0].probs, "data"):
                        probs_data = results[0].probs.data
                        if isinstance(probs_data, torch.Tensor):
                            probs_data = probs_data.cpu().numpy()
                        probs = convert_numpy_to_python(probs_data)
                    else:
                        probs = convert_numpy_to_python(results[0].probs)
                    
                    # Build response with class names and probabilities
                    predictions = []
                    for i, (class_id, name) in enumerate(class_names.items()):
                        if i < len(probs):
                            predictions.append({"class": str(name), "probability": float(probs[i])})
                    
                    logger.info(f"Returning {len(predictions)} classification predictions")
                    return {"predictions": predictions}
                except Exception as prob_error:
                    logger.error(f"Error processing probabilities: {str(prob_error)}")
                    return {"error": f"Error processing probabilities: {str(prob_error)}", "predictions": []}
            except Exception as class_error:
                logger.error(f"Error processing classification: {str(class_error)}")
                return {"error": f"Error processing classification: {str(class_error)}", "predictions": []}

        # For detection models, return detected classes with their confidence
        logger.info("Processing as detection model")
        try:
            # Get class names
            if hasattr(results[0], "names") and results[0].names:
                class_names = convert_numpy_to_python(results[0].names)
            else:
                logger.warning("No class names found, using indices")
                class_names = {i: f"Class {i}" for i in range(10)}  # Fallback
                
            # Initialize scores for all classes
            num_classes = len(class_names)
            scores = [0.0] * num_classes
            
            # Extract detection boxes and confidences
            if hasattr(results[0], "boxes") and results[0].boxes is not None:
                try:
                    boxes_cls = convert_numpy_to_python(results[0].boxes.cls) if hasattr(results[0].boxes, "cls") else []
                    boxes_conf = convert_numpy_to_python(results[0].boxes.conf) if hasattr(results[0].boxes, "conf") else []
                    
                    logger.info(f"Found {len(boxes_cls)} boxes with classes: {boxes_cls}")
                    logger.info(f"Confidences: {boxes_conf}")
                    
                    # Update scores with detection confidences
                    for cls, conf in zip(boxes_cls, boxes_conf):
                        idx = int(cls)
                        conf_val = float(conf)
                        if idx < len(scores) and conf_val > scores[idx]:
                            scores[idx] = conf_val
                except Exception as box_error:
                    logger.error(f"Error processing boxes: {str(box_error)}")
                    
            # Build the predictions response
            predictions = []
            for class_id, class_name in class_names.items():
                try:
                    score_idx = int(class_id) if isinstance(class_id, (int, str)) else 0
                    score = scores[score_idx] if score_idx < len(scores) else 0.0
                    predictions.append({"class": str(class_name), "probability": float(score)})
                except Exception as pred_error:
                    logger.error(f"Error processing prediction for class {class_id}: {str(pred_error)}")
            
            logger.info(f"Returning {len(predictions)} detection predictions")
            return {"predictions": predictions}
        except Exception as det_error:
            logger.error(f"Error in detection processing: {str(det_error)}")
            return {"error": f"Error in detection processing: {str(det_error)}", "predictions": []}
            
    except Exception as e:
        import traceback
        import sys
        
        tb = traceback.format_exc()
        error_type = type(e).__name__
        
        logger.error(f"Error in prediction: {error_type}: {str(e)}\n{tb}")
        
        # Check if this is related to JSON serialization or numpy arrays
        if "Object of type" in str(e) and "is not JSON serializable" in str(e):
            # Try to return a safe serializable response
            try:
                if 'numpy' in str(e):
                    # If numpy-related error, attempt to provide a better error message
                    return {
                        "error": f"JSON serialization error with NumPy data: {str(e)}",
                        "error_type": error_type,
                        "predictions": []  # Ensure we always return an empty predictions array
                    }
                return {
                    "error": f"JSON serialization error: {str(e)}",
                    "error_type": error_type,
                    "predictions": []
                }
            except:
                # If all else fails, return a simple error
                return {"error": "Failed to serialize response", "predictions": []}
        
        return {
            "error": str(e),
            "error_type": error_type,
            "traceback": tb,
            "predictions": []  # Always include an empty predictions array for consistent response structure
        }
