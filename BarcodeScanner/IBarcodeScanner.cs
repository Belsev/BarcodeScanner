using System;

namespace BarcodeScannerInterface
{
	public interface IBarcodeScanner : IDisposable
	{
		event EventHandler<BarcodeEventArgs> BarcodeEvent;
		bool Alive { get; }
        bool IsEmpty();

	    //object Stop();
	}
}