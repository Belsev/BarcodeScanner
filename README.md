# BarcodeScanner

## Description
Asynchronous barcode scanner. Splits all data read from COM port into barcodes. Then generates one BarcodeEvent for every barcode. If scanner gets disconnected, tries to restore connection.

## How to use
```cs
using BarcodeScanner;
using System;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            //Get port number
            var portNumber = int.Parse(Console.ReadLine());
            //Create new instance of AutoBarcodeScanner
            var barcodeScanner = new AutoBarcodeScanner(portNumber);
            //Subscribe to BarcodeEvent
            barcodeScanner.BarcodeEvent += (object _, BarcodeEventArgs args) =>
            {
                Console.WriteLine(args.Barcode);
            };

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}

```
