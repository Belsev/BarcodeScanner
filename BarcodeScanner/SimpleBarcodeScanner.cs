using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace BarcodeScannerInterface
{
    public class SimpleBarcodeScanner : IDisposable
    {
        private readonly SerialPort serialPort;
        private readonly int port;
        private bool keepAlive = true;
        private List<string> buffer = new List<string>();

        /// <summary>
        /// Событие с одним штрих-кодом
        /// </summary>
        public event EventHandler<BarcodeEventArgs> BarcodeEvent;

        /// <summary>
        /// Создаёт новый экземпляр Сканера и сразу занимает порт
        /// </summary>
        /// <param name="port">COM порт</param>
        public SimpleBarcodeScanner(int port)
        {
            this.port = port;
            serialPort = new SerialPort($"COM{port}");
            serialPort.Open();

            var portListenThread = new Thread(PortListenThreadWorker);
            portListenThread.Start();

            var healthRestoreThread = new Thread(HealthRestoreThreadWorker);
            healthRestoreThread.Start();

            var bufferThread = new Thread(BufferThreadWorker);
            bufferThread.Start();
        }

        /// <summary>
        /// ПДальнейшая обработка зачитанных в буфер данных
        /// </summary>
        private void BufferThreadWorker()
        {
            while (keepAlive)
            {
                var newList = new List<string>();
                lock (buffer)
                {
                    newList = buffer.ToList();
                    buffer.Clear();
                }
                ReadData(newList);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Переподключение сканера при его отключении
        /// </summary>
        private void HealthRestoreThreadWorker()
        {
            while (keepAlive)
            {
                if (!serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.Close();
                        Thread.Sleep(500);
                        serialPort.Open();
                    }
                    catch { }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Прослушивание порта
        /// </summary>
        private void PortListenThreadWorker()
        {
            while (keepAlive)
            {
                var data = serialPort.ReadExisting();
                if (!string.IsNullOrWhiteSpace(data))
                {
                    lock (buffer)
                    {
                        buffer.Add(data);
                    }
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Разделение считанных данных на штрихкоды и дальнейшая их отправка
        /// </summary>
        /// <param name="readedData">Считанные данные</param>
        private void ReadData(List<string> readedData)
        {
            readedData.ForEach(data =>
            {
                char[] separators = { '\r', '\n' };
                var barcodes = data.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                barcodes.ForEach(barcode =>
                {
                    var eventArgs = new BarcodeEventArgs(barcode);
                    BarcodeEvent?.Invoke(this, eventArgs);
                });
            });
        }

        /// <summary>
        /// Состояние сканера
        /// </summary>
        public bool Alive
        {
            get
            {
                return serialPort?.IsOpen ?? false;
            }
        }

        public void Dispose()
        {
            keepAlive = false;
            serialPort?.Dispose();
        }

        ~SimpleBarcodeScanner()
        {
            Dispose();
        }
    }
}
