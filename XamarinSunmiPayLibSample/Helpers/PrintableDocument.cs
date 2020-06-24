using System.Text;

namespace XamarinSunmiPayLibSample.Helpers
{
    public class PrintableDocument
    {
        public byte ESC = 0x1B;
        public byte[] GenerateDocument(CreditCard creditCard)
        {                
            return ByteMerger(new byte[][]
            {
                new byte[]{ 0x1C, 0x26, 0x1C, 0x43, 0xff},
                new byte[]{0x1d,33,0,0 },
                new byte[]{0x1d,33,0,4 },
                AlignCenter(), BoldOn(),Encoding.UTF8.GetBytes(creditCard.CardHolderName),
                Encoding.UTF8.GetBytes("\n"),
                new byte[]{0x1d,33,1,0 },
                new byte[]{0x1d,33,16,4 },
                AlignCenter(),Encoding.UTF8.GetBytes(creditCard.CardNumber),
                Encoding.UTF8.GetBytes("\n"),
                new byte[]{0x1d,33,0,0 },
                new byte[]{0x1d,33,0,4 },
                BoldOff(),
                new byte[]{0x1d,33,0,0 },
                new byte[]{0x1d,33,0,4 },
                AlignCenter(), Encoding.UTF8.GetBytes(creditCard.ExpireDate)
            });
        }
        public byte[] AlignCenter()
        {
            byte[] result = new byte[3];
            result[0] = ESC;
            result[1] = 97;
            result[2] = 1;
            return result;
        }
        public byte[] BoldOn()
        {
            byte[] result = new byte[3];
            result[0] = ESC;
            result[1] = 69;
            result[2] = 0xF;
            return result;
        }
        public byte[] BoldOff()
        {
            byte[] result = new byte[3];
            result[0] = ESC;
            result[1] = 69;
            result[2] = 0;
            return result;
        }
        public byte[] ByteMerger(byte[][] byteList)
        {
            int length = 0;
            for (int i = 0; i < byteList.Length; i++)
            {
                length += byteList[i].Length;
            }
            byte[] result = new byte[length];
            int index = 0;
            for (int i = 0; i < byteList.Length; i++)
            {
                byte[] nowByte = byteList[i];
                for (int k = 0; k < byteList[i].Length; k++)
                {
                    result[index] = nowByte[k];
                    index++;
                }
            }
            return result;
        }
    }
}
