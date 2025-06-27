import cv2
from pyzbar.pyzbar import decode
import webbrowser

# Deschide stream-ul ESP32-CAM
cap = cv2.VideoCapture("http://192.168.100.81:81/stream")

while True:
    # 1. Citește un frame
    ret, frame = cap.read()
    if not ret:
        break

    # 2. Decodează orice cod QR din frame
    decoded_objects = decode(frame)
    for obj in decoded_objects:
        # ia cele 4 colțuri ale QR-ului
        points = obj.polygon
        if len(points) == 4:
            pts = [(p.x, p.y) for p in points]
            # (opțional) desenezi un contur peste QR:
            # cv2.polylines(frame, [np.array(pts)], isClosed=True, ...)

        # 3. Extrage textul din QR
        qr_data = obj.data.decode('utf-8')
        print("QR Code detectado:", qr_data)

        # 4. Deschide link-ul în browser
        webbrowser.open(qr_data)

        # eliberează resurse și oprește citirea după primul QR găsit
        cap.release()
        cv2.destroyAllWindows()
        break

    # 5. Afișează fereastra cu video-stream
    cv2.imshow("Leitura", frame)

    # 6. Iese la apăsarea tastei 'q'
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Cleanup final
cap.release()
cv2.destroyAllWindows()
