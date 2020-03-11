using System;

namespace BarcodeScannerInterface
{
    public class BarcodeEventArgs : EventArgs
    {
        public BarcodeEventArgs(string scanData)
        {
            ScanData = scanData;
        }

        public string ScanData { get; private set; }
    }
}