using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace BarcodeScanner
{
    public class AutoBarcodeScanner : IDisposable
    {
        /// <summary>
        /// SerialPort Object
        /// </summary>
        private readonly SerialPort serialPort;
        /// <summary>
        /// COM Port number
        /// </summary>
        private readonly int port;
        /// <summary>
        /// Flag, signaling to stop all threads
        /// </summary>
        private bool disposedFlag = false;
        /// <summary>
        /// A Buffer containing every read data from the COM port
        /// </summary>
        private List<string> buffer = new List<string>();

        /// <summary>
        /// Array of symbols, separating barcodes from each other.
        /// By default \r and \n
        /// </summary>
        public char[] Separators { get; set; } = { '\r', '\n' };

        /// <summary>
        /// Async event containing one Barcode. Invokes after barcode being read
        /// </summary>
        public event EventHandler<BarcodeEventArgs> BarcodeEvent;

        /// <summary>
        /// Creates new AutoBarcodeScanner object and instantly occupies COM port
        /// </summary>
        /// <param name="port">COM port</param>
        public AutoBarcodeScanner(int port)
        {
            this.port = port;
            serialPort = new SerialPort($"COM{port}");
            serialPort.Open();

            var portListenThread = new Thread(PortListenWorker);
            portListenThread.Start();

            var healthRestoreThread = new Thread(HealthRestoreWorker);
            healthRestoreThread.Start();

            var bufferThread = new Thread(ReadBufferWorker);
            bufferThread.Start();
        }

        /// <summary>
        /// Retrieving data from a buffer and then flushing it
        /// </summary>
        private void ReadBufferWorker()
        {
            while (!disposedFlag)
            {
                var newList = new List<string>();
                lock (buffer)
                {
                    newList = buffer.ToList();
                    buffer.Clear();
                }
                ProcessData(newList);
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Attempting to reconnect Barcode scanner in case of sudden disconnects
        /// </summary>
        private void HealthRestoreWorker()
        {
            while (!disposedFlag)
            {
                if (!serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.Close();
                        //Referring to documentations need to pause
                        Thread.Sleep(500);
                        serialPort.Open();
                    }
                    catch { }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Listening to COM port
        /// </summary>
        private void PortListenWorker()
        {
            while (!disposedFlag)
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
        /// Separating data from buffer into barcodes. Invokes BarcodeEvent for every barcode
        /// </summary>
        /// <param name="dataToProcess">Data to process</param>
        private void ProcessData(List<string> dataToProcess)
        {
            dataToProcess.ForEach(data =>
            {
                var barcodes = data.Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                barcodes.ForEach(barcode =>
                {
                    var eventArgs = new BarcodeEventArgs(barcode);
                    BarcodeEvent?.Invoke(this, eventArgs);
                });
            });
        }

        /// <summary>
        /// Scanner state
        /// </summary>
        public bool Connected
        {
            get
            {
                return serialPort?.IsOpen ?? false;
            }
        }

        public void Dispose()
        {
            disposedFlag = true;
            serialPort?.Dispose();
        }

        ~AutoBarcodeScanner()
        {
            Dispose();
        }
    }
}
