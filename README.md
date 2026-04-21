# RfidModbusWorker

Worker Service .NET 8 dùng để đọc dữ liệu từ đầu đọc RFID qua Modbus RTU serial và in kết quả ra console/log.

Ứng dụng hiện đọc holding registers từ thiết bị qua `/dev/ttyS0`. Nếu thanh ghi đầu tiên `5300` trả về `1`, worker sẽ in một dòng kết quả. Nếu `5300` trả về `0`, worker vẫn đọc bình thường nhưng không in ra.

## Công nghệ

- .NET 8 Worker Service
- `System.IO.Ports` để đọc cổng serial
- `NModbus` để đọc Modbus RTU
- `DotNetEnv` để load cấu hình từ file `.env`
- systemd để chạy service trên VPS Linux

## Cấu hình `.env`

Tạo file `.env` cạnh file chạy của ứng dụng. Có thể copy từ `.env.example`.

```env
RFID_PORT_NAME=/dev/ttyS0
RFID_BAUD_RATE=115200
RFID_PARITY=None
RFID_DATA_BITS=8
RFID_STOP_BITS=One
RFID_SLAVE_ID=1
RFID_START_ADDRESS=5300
RFID_REGISTER_COUNT=20
RFID_POLL_INTERVAL_MS=100
RFID_READ_TIMEOUT_MS=500
RFID_WRITE_TIMEOUT_MS=500
```

Ý nghĩa cấu hình:

- `RFID_PORT_NAME`: cổng serial kết nối đầu đọc RFID.
- `RFID_BAUD_RATE`: baudrate của Modbus RTU.
- `RFID_PARITY`: parity, hỗ trợ `None`, `Even`, `Odd`, `Mark`, `Space`.
- `RFID_DATA_BITS`: số data bits, thường là `8`.
- `RFID_STOP_BITS`: stop bits, hỗ trợ `One`, `OnePointFive`, `Two`.
- `RFID_SLAVE_ID`: Modbus slave id của đầu đọc.
- `RFID_START_ADDRESS`: địa chỉ holding register bắt đầu đọc.
- `RFID_REGISTER_COUNT`: số lượng register cần đọc.
- `RFID_POLL_INTERVAL_MS`: chu kỳ đọc mong muốn, đơn vị millisecond.
- `RFID_READ_TIMEOUT_MS`: timeout đọc serial/Modbus.
- `RFID_WRITE_TIMEOUT_MS`: timeout ghi serial/Modbus.

Nếu thiếu `.env`, ứng dụng vẫn đọc cấu hình từ environment variables của hệ điều hành. Nếu thiếu biến bắt buộc hoặc giá trị sai, ứng dụng sẽ dừng khi startup và in dòng `CONFIG_ERROR`.

## Chạy local

Restore và build:

```powershell
dotnet restore
dotnet build -c Release
```

Chạy trực tiếp:

```powershell
dotnet run -c Release
```

Lưu ý: máy local cần có cổng serial tương ứng trong `.env`. Nếu không có thiết bị thật, build vẫn kiểm tra được code nhưng chạy sẽ báo lỗi không mở được cổng serial.

## Publish

Publish bản Release:

```powershell
dotnet publish -c Release -f net8.0 -o .\publish
```

Sau khi publish, upload toàn bộ nội dung thư mục `publish` và file `.env` lên VPS, ví dụ:

```text
/home/admin/rfid-worker
```

## Chạy trên VPS

Test foreground:

```bash
cd /home/admin/rfid-worker
sudo dotnet RfidModbusWorker.dll
```

Ứng dụng cần quyền đọc `/dev/ttyS0`. Trong giai đoạn test hiện tại, service chạy bằng `root` để có quyền truy cập serial.

## systemd service

Service đang dùng tên:

```bash
rfid-worker
```

Các lệnh vận hành:

```bash
sudo systemctl status rfid-worker
sudo systemctl restart rfid-worker
sudo systemctl stop rfid-worker
sudo systemctl start rfid-worker
```

Theo dõi log:

```bash
sudo journalctl -u rfid-worker -f
```

Xem log gần nhất:

```bash
sudo journalctl -u rfid-worker -n 100 --no-pager
```

## Format output

Khi đọc thành công và register `5300` bằng `1`, worker in một dòng dạng:

```text
2026-04-21T11:46:37.025+07:00 OK dec=[1,12,43981,61202,0,0,0,8192,...] hex=[0001,000C,ABCD,EF12,0000,0000,0000,2000,...]
```

Trong đó:

- `dec`: giá trị các register dạng decimal.
- `hex`: giá trị các register dạng hexadecimal 4 ký tự.
- Register đầu tiên tương ứng địa chỉ `5300`.

Nếu có lỗi đọc serial hoặc Modbus, worker in:

```text
2026-04-21T11:46:37.025+07:00 ERROR ExceptionType: message
```

Sau lỗi, worker đóng kết nối hiện tại và thử kết nối lại ở vòng đọc tiếp theo.

## Ghi chú vận hành

- Worker chỉ in kết quả khi `register[0] == 1`.
- Nếu `register[0] == 0`, log sẽ im lặng dù app vẫn đang đọc thiết bị.
- Chu kỳ cấu hình là `100ms`, nhưng tốc độ thực tế phụ thuộc thời gian phản hồi của đầu đọc Modbus.
- Không lưu thông tin SSH/VPS/password trong source code hoặc README.
