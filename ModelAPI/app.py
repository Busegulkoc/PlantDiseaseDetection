from fastapi import FastAPI, File, UploadFile
from ultralytics import YOLO
from io import BytesIO
from PIL import Image
from fastapi.middleware.cors import CORSMiddleware
import logging
import traceback
import numpy as np
import torch
import tempfile
import os

# Configure logging
logging.basicConfig(level=logging.INFO, 
                    format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = FastAPI()

# Ensure the model path is correct and the model file is accessible in the Docker container.
try:
    model = YOLO("best.pt")
    logger.info("YOLO model loaded successfully.")
    
    # Log model information
    model_type = model.task if hasattr(model, 'task') else type(model).__name__
    logger.info(f"Model type: {model_type}")
except Exception as model_load_error:
    logger.error(f"Failed to load YOLO model: {model_load_error}", exc_info=True)
    model = None

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Helper function to convert any numpy values to Python native types
def convert_numpy_to_python(obj):
    if isinstance(obj, np.ndarray):
        return obj.tolist()
    elif isinstance(obj, np.generic):
        return obj.item()  # Convert numpy scalars to Python scalars
    elif isinstance(obj, dict):
        return {k: convert_numpy_to_python(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [convert_numpy_to_python(item) for item in obj]
    else:
        return obj

@app.post("/predict")
async def predict(file: UploadFile = File(None), imageFile: UploadFile = File(None)):
    if model is None:
        logger.error("Model not loaded, cannot predict.")
        return {"predictions": [], "error": "Model not loaded."}
        
    try:
        upload_file = file if file is not None else imageFile
        
        if upload_file is None:
            logger.warning("No file provided in the request.")
            return {"predictions": [], "error": "No file provided. Please upload an image file using 'file' or 'imageFile' parameter."}
        
        contents = await upload_file.read()
        
        # For classification models, we'll save the image to a temporary file
        # This is a workaround for the issue with PIL/numpy conversion in YOLO classify
        with tempfile.NamedTemporaryFile(delete=False, suffix=".jpg") as temp_file:
            temp_file.write(contents)
            temp_file_path = temp_file.name
        
        try:
            # Log the temp file path
            logger.info(f"Saved image to temporary file: {temp_file_path}")
            
            # Open image just to log the dimensions and format
            with Image.open(BytesIO(contents)) as img:
                logger.info(f"Successfully loaded image: {upload_file.filename}, size: {img.size}, mode: {img.mode}")
            
            # Call predict with the file path instead of the PIL Image object
            results = model.predict(source=temp_file_path, imgsz=640, verbose=False)
            logger.info(f"Prediction completed. Results type: {type(results)}")
            
            # Delete the temporary file after prediction
            try:
                os.unlink(temp_file_path)
                logger.info(f"Deleted temporary file: {temp_file_path}")
            except Exception as delete_error:
                logger.warning(f"Failed to delete temporary file: {delete_error}")
                
        except Exception as predict_error:
            # If prediction fails, clean up and log error
            logger.error(f"Prediction error: {predict_error}", exc_info=True)
            try:
                os.unlink(temp_file_path)
                logger.info(f"Deleted temporary file after error: {temp_file_path}")
            except:
                pass
            return {"predictions": [], "error": f"Error during prediction: {str(predict_error)}"}

        # Process results
        if not results:
            logger.info("Model returned no results.")
            return {"predictions": [], "error": "Model returned no results."}

        processed_predictions = []
        
        # Determine if it's a classification model
        is_classification = hasattr(model, 'task') and model.task == 'classify'
        logger.info(f"Processing results for model task type: {'classification' if is_classification else 'detection'}")
        
        # Log the structure of the results object
        for i, result in enumerate(results):
            logger.info(f"Result {i} type: {type(result)}")
            logger.info(f"Result {i} attributes: {dir(result)}")
        
        # Process results based on model type
        for result_item in results:
            if is_classification or (hasattr(result_item, 'probs') and result_item.probs is not None):
                # Handle classification results
                logger.info("Processing classification results")
                
                if hasattr(result_item, 'probs') and result_item.probs is not None:
                    logger.info(f"Probs attributes: {dir(result_item.probs)}")
                    names = result_item.names
                    logger.info(f"Class names: {names}")
                    
                    # Different ways to access probabilities based on the structure
                    if hasattr(result_item.probs, 'top5'):
                        # Access top 5 predictions
                        logger.info("Using top5 method")
                        top5_indices = result_item.probs.top5.cpu().numpy().tolist() if hasattr(result_item.probs.top5, 'cpu') else convert_numpy_to_python(result_item.probs.top5)
                        top5_conf = result_item.probs.top5conf.cpu().numpy().tolist() if hasattr(result_item.probs.top5conf, 'cpu') else convert_numpy_to_python(result_item.probs.top5conf)
                        
                        logger.info(f"Top 5 indices: {top5_indices}")
                        logger.info(f"Top 5 confidences: {top5_conf}")
                        
                        for idx, conf in zip(top5_indices, top5_conf):
                            if idx in names:
                                processed_predictions.append({
                                    "class": names[idx],
                                    "probability": float(conf)
                                })
                    elif hasattr(result_item.probs, 'data'):
                        # Access all class probabilities
                        logger.info("Using data method")
                        probs_data = result_item.probs.data
                        
                        # Convert to Python list depending on data type
                        if hasattr(probs_data, 'cpu'):
                            probs_list = probs_data.cpu().numpy().tolist()
                        else:
                            probs_list = convert_numpy_to_python(probs_data)
                        
                        logger.info(f"Probabilities length: {len(probs_list)}")
                        
                        # Add each class prediction
                        for i, prob in enumerate(probs_list):
                            if i in names:
                                processed_predictions.append({
                                    "class": names[i],
                                    "probability": float(prob)
                                })
                    else:
                        # Fallback method - try to access probs directly
                        logger.warning("Using fallback method for probabilities")
                        try:
                            # Try different attributes that might contain probabilities
                            for attr_name in dir(result_item.probs):
                                if attr_name.startswith("__"):
                                    continue
                                    
                                logger.info(f"Checking attribute: {attr_name}")
                                try:
                                    attr_value = getattr(result_item.probs, attr_name)
                                    if isinstance(attr_value, (torch.Tensor, np.ndarray, list)) and not callable(attr_value):
                                        logger.info(f"Found potential probability data in {attr_name}")
                                except:
                                    pass
                        except Exception as prob_error:
                            logger.error(f"Error accessing probabilities: {prob_error}")
            elif hasattr(result_item, 'boxes') and result_item.boxes is not None:
                # Handle object detection results
                logger.info("Processing detection results")
                boxes = result_item.boxes
                names = result_item.names
                
                for i in range(len(boxes.cls)):
                    class_id = int(boxes.cls[i].item())
                    confidence = float(boxes.conf[i].item())
                    
                    box_coords = boxes.xyxyn[i].cpu().numpy().tolist() if hasattr(boxes.xyxyn[i], 'cpu') else convert_numpy_to_python(boxes.xyxyn[i])
                    
                    prediction_item = {
                        "class": names[class_id],
                        "confidence": confidence,
                        "box": box_coords
                    }
                    processed_predictions.append(prediction_item)
            else:
                logger.warning(f"Unrecognized result type or missing expected attributes")

        # Final safety check to ensure all predictions are JSON serializable
        processed_predictions = convert_numpy_to_python(processed_predictions)
        
        logger.info(f"Successfully processed {len(processed_predictions)} predictions.")
        return {"predictions": processed_predictions, "error": None}

    except Exception as e:
        tb = traceback.format_exc()
        error_type = type(e).__name__
        logger.error(f"Error in prediction endpoint: {error_type}: {str(e)}\nTraceback: {tb}")
        return {
            "predictions": [], 
            "error": f"{error_type}: {str(e)}"
        }

@app.get("/ping")
async def ping():
    """Health check endpoint"""
    return {"status": "ok", "model_loaded": model is not None}