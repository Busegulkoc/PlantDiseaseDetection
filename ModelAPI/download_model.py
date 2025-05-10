import gdown
from ultralytics import YOLO

# Google Drive dosya linkini buraya ekle (dosya ID'sini kullan)
url = 'https://drive.google.com/uc?id=1-e1j5cKvcRbElCDHqeqrndbyf9zo7Jmi'  # Model ID'si
output = 'best.pt'  # İndirilen model dosyasının adı

# Google Drive'dan dosyayı indir
gdown.download(url, output, quiet=False)

# Modeli yükle
model = YOLO(output)  # İndirilen model dosyasını burada kullan

print("Model başarıyla yüklendi!")
