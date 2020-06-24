using System;
namespace XamarinSunmiPayLibSample
{
    public interface IPayService
    {
        void Read(ReadType readType);
    }

    public enum ReadType //same AidlConstants.CardType for ref
    {
        Magnetic = 1,
        Ic = 2,
        Nfc = 4,
    }
}
