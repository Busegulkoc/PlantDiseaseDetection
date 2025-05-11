FROM python:3.9-slim

WORKDIR /app

# Install system dependencies required for OpenCV
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1-mesa-glx \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements file
COPY requirements.txt .

# Install Python dependencies with specific versions
RUN pip install --no-cache-dir numpy==1.24.3 \
    && pip install --no-cache-dir torch==2.1.0 \
    && pip install --no-cache-dir -r requirements.txt

# Copy the application code
COPY . .

# Expose the API port
EXPOSE 8000

# Run the FastAPI application
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8000"]