using System;

namespace BarcodeScanner
{
    public class BarcodeEventArgs : EventArgs
    {
        public BarcodeEventArgs(string barcode)
        {
            Barcode = barcode;
        }

        public string Barcode { get; private set; }
    }
}